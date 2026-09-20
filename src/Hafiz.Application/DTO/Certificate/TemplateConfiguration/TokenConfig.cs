using System.Collections.Generic;

namespace Hafiz.Application.DTO.Certificate.TemplateConfiguration;

public class TokenConfig
{
    public string ClosingText { get; set; } = "اللهم بارك لهم فيما علمتهم وانفعهم به";
    public Dictionary<string, string> CustomFields { get; set; } = new();
}
