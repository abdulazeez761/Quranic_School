using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hafiz.Application.Interfaces.Data;
using Hafiz.Application.Interfaces.Services;
using Hafiz.Application.Models.Matn;
using Hafiz.Domain.Enums;
using Hafiz.Repositories.Interfaces;

namespace Hafiz.Application.Services;

public class MatnReferenceService : IMatnReferenceService
{
    private readonly IMatnDataProvider _dataProvider;
    private readonly IMatnRepository _matnRepository;

    public MatnReferenceService(IMatnDataProvider dataProvider, IMatnRepository matnRepository)
    {
        _dataProvider = dataProvider;
        _matnRepository = matnRepository;
    }

    public async Task<IReadOnlyList<MatnDefinition>> GetAllAsync()
    {
        return await _dataProvider.GetAllAsync();
    }

    public async Task<MatnDefinition?> GetByIdOrNameAsync(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return null;

        var trimmed = identifier.Trim();

        // 1. Direct match by static Id, Name, or Alias in static provider
        var directMatch = await _dataProvider.GetByIdOrNameAsync(trimmed);
        if (directMatch != null)
            return directMatch;

        // 2. If it is a GUID, resolve database Matn Title
        if (Guid.TryParse(trimmed, out var guid))
        {
            var dbMatn = await _matnRepository.GetByIdAsync(guid);
            if (dbMatn != null)
            {
                var titleMatch = await _dataProvider.GetByIdOrNameAsync(dbMatn.Title);
                if (titleMatch != null)
                {
                    // Return definition mapped with the database GUID
                    return new MatnDefinition
                    {
                        Id = dbMatn.Id.ToString(),
                        Name = dbMatn.Title,
                        Author = string.IsNullOrWhiteSpace(dbMatn.Author) ? titleMatch.Author : dbMatn.Author,
                        Category = titleMatch.Category,
                        Aliases = titleMatch.Aliases,
                        Chapters = titleMatch.Chapters,
                        Units = titleMatch.Units
                    };
                }

                // If not found in reference catalog, return clean empty definition without inventing chapters
                var fallbackUnits = new List<MatnUnitDefinition>();
                if (dbMatn.DefaultUnit > 0)
                {
                    var unitName = dbMatn.DefaultUnit switch
                    {
                        MatnUnit.Verses => "أبيات",
                        MatnUnit.Lines => "سطور",
                        MatnUnit.Pages => "صفحات",
                        MatnUnit.Chapters => "أبواب",
                        MatnUnit.Hadiths => "أحاديث",
                        _ => "وحدات"
                    };
                    var unitSingular = dbMatn.DefaultUnit switch
                    {
                        MatnUnit.Verses => "بيت",
                        MatnUnit.Lines => "سطر",
                        MatnUnit.Pages => "صفحة",
                        MatnUnit.Chapters => "باب",
                        MatnUnit.Hadiths => "حديث",
                        _ => "وحدة"
                    };
                    fallbackUnits.Add(new MatnUnitDefinition
                    {
                        Number = (int)dbMatn.DefaultUnit,
                        Name = unitName,
                        Singular = unitSingular
                    });
                }

                return new MatnDefinition
                {
                    Id = dbMatn.Id.ToString(),
                    Name = dbMatn.Title,
                    Author = dbMatn.Author,
                    Chapters = Array.Empty<MatnChapterDefinition>(),
                    Units = fallbackUnits
                };
            }
        }

        return null;
    }
}
