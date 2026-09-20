using System;
using System.Collections.Generic;

namespace Hafiz.Application.Models.Matn;

public class MatnDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string? Category { get; set; }
    public IReadOnlyList<string> Aliases { get; set; } = Array.Empty<string>();
    public IReadOnlyList<MatnChapterDefinition> Chapters { get; set; } = Array.Empty<MatnChapterDefinition>();
    public IReadOnlyList<MatnUnitDefinition> Units { get; set; } = Array.Empty<MatnUnitDefinition>();
}

public class MatnChapterDefinition
{
    public int Number { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class MatnUnitDefinition
{
    public int Number { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Singular { get; set; } = string.Empty;
}
