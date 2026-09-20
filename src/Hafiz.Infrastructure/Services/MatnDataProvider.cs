using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hafiz.Application.Interfaces.Data;
using Hafiz.Application.Models.Matn;

namespace Hafiz.Infrastructure.Services;

public class MatnDataProvider : IMatnDataProvider
{
    private readonly Lazy<Task<IReadOnlyList<MatnDefinition>>> _cachedMatns;

    public MatnDataProvider()
    {
        _cachedMatns = new Lazy<Task<IReadOnlyList<MatnDefinition>>>(LoadMatnsDataAsync);
    }

    public async Task<IReadOnlyList<MatnDefinition>> GetAllAsync()
    {
        return await _cachedMatns.Value;
    }

    public async Task<MatnDefinition?> GetByIdOrNameAsync(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return null;

        var matns = await GetAllAsync();
        var cleanId = identifier.Trim();

        // 1. Exact match by Id (case-insensitive)
        var exactIdMatch = matns.FirstOrDefault(m => string.Equals(m.Id, cleanId, StringComparison.OrdinalIgnoreCase));
        if (exactIdMatch != null)
            return exactIdMatch;

        // 2. Normalized search across Name and Aliases
        var normalizedQuery = NormalizeArabic(cleanId);
        if (string.IsNullOrEmpty(normalizedQuery))
            return null;

        // Exact normalized name match
        var exactNameMatch = matns.FirstOrDefault(m => NormalizeArabic(m.Name) == normalizedQuery);
        if (exactNameMatch != null)
            return exactNameMatch;

        // Exact normalized alias match
        var aliasMatch = matns.FirstOrDefault(m => m.Aliases.Any(a => NormalizeArabic(a) == normalizedQuery));
        if (aliasMatch != null)
            return aliasMatch;

        // Substring / fuzzy match (query contains matn name/alias or vice-versa)
        var fuzzyMatch = matns.FirstOrDefault(m =>
        {
            var normName = NormalizeArabic(m.Name);
            if (normalizedQuery.Contains(normName) || normName.Contains(normalizedQuery))
                return true;

            return m.Aliases.Any(a =>
            {
                var normAlias = NormalizeArabic(a);
                return normalizedQuery.Contains(normAlias) || normAlias.Contains(normalizedQuery);
            });
        });

        return fuzzyMatch;
    }

    private static async Task<IReadOnlyList<MatnDefinition>> LoadMatnsDataAsync()
    {
        string jsonContent = string.Empty;

        // Strategy 1: Check AppContext.BaseDirectory
        var basePath = AppContext.BaseDirectory;
        var filePath = Path.Combine(basePath, "Data", "Matns", "matns.json");

        if (File.Exists(filePath))
        {
            jsonContent = await File.ReadAllTextAsync(filePath);
        }
        else
        {
            // Strategy 2: Check current working directory
            var localPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Matns", "matns.json");
            if (File.Exists(localPath))
            {
                jsonContent = await File.ReadAllTextAsync(localPath);
            }
            else
            {
                // Strategy 3: Read Embedded Resource
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = assembly.GetManifestResourceNames()
                    .FirstOrDefault(r => r.EndsWith("matns.json", StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(resourceName))
                {
                    await using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    {
                        using var reader = new StreamReader(stream);
                        jsonContent = await reader.ReadToEndAsync();
                    }
                }
            }
        }

        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            return Array.Empty<MatnDefinition>();
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var parsed = JsonSerializer.Deserialize<List<MatnDefinition>>(jsonContent, options)
                     ?? new List<MatnDefinition>();

        // Ensure non-null collections for clean API compliance
        foreach (var m in parsed)
        {
            m.Aliases ??= Array.Empty<string>();
            m.Chapters ??= Array.Empty<MatnChapterDefinition>();
            m.Units ??= Array.Empty<MatnUnitDefinition>();
        }

        return parsed.AsReadOnly();
    }

    private static string NormalizeArabic(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Remove Harakat (tashkeel)
        var s = Regex.Replace(text, @"[\u064B-\u065F\u0670]", "");

        // Normalize Alefs: أ, إ, آ, ٱ -> ا
        s = Regex.Replace(s, @"[أإآٱ]", "ا");

        // Normalize Taa Marbuta: ة -> ه
        s = Regex.Replace(s, @"ة", "ه");

        // Normalize Yaa: ى -> ي
        s = Regex.Replace(s, @"ى", "ي");

        // Remove Quranic punctuation and special marks
        s = Regex.Replace(s, @"[\u0600-\u061F\u06D6-\u06ED]", "");

        // Remove non-alphanumeric Arabic/English characters
        s = Regex.Replace(s, @"[^\u0621-\u064A0-9a-zA-Z\s]", " ");

        // Collapse multiple whitespaces
        s = Regex.Replace(s, @"\s+", " ").Trim().ToLowerInvariant();

        return s;
    }
}
