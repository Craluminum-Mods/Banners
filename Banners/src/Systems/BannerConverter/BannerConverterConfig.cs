using System.Collections.Generic;

namespace Flags;

public class BannerConverterConfig
{
    public Dictionary<string, string> BaseColorsToColors { get; set; } = new();
    public Dictionary<string, string> IdsToColors { get; set; } = new();
    public Dictionary<string, string> Patterns { get; set; } = new();
}