using Microsoft.JSInterop;

namespace FarmBot;

public sealed class ThemeOption
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string SwatchA { get; init; }
    public required string SwatchB { get; init; }
    public required string SwatchC { get; init; }
}

public sealed class ThemeService
{
    public static readonly IReadOnlyList<ThemeOption> All = new[]
    {
        new ThemeOption { Id = "night", Name = "Night", SwatchA = "#13151c", SwatchB = "#1c202b", SwatchC = "#c4783a" },
        new ThemeOption { Id = "day", Name = "Day", SwatchA = "#f4efe6", SwatchB = "#fffdf8", SwatchC = "#b56a2c" },
        new ThemeOption { Id = "meta", Name = "Meta", SwatchA = "#0b141a", SwatchB = "#172633", SwatchC = "#3d9ad6" },
        new ThemeOption { Id = "wine", Name = "Wine", SwatchA = "#1a1014", SwatchB = "#321c24", SwatchC = "#d4a017" },
        new ThemeOption { Id = "forest", Name = "Forest", SwatchA = "#101612", SwatchB = "#1c2820", SwatchC = "#7d9a5a" }
    };

    public string CurrentId { get; private set; } = "night";
    public ThemeOption Current => All.FirstOrDefault(item => item.Id == CurrentId) ?? All[0];
    public event Action? Changed;

    public async Task SelectAsync(string id, IJSRuntime js)
    {
        CurrentId = All.Any(item => item.Id == id) ? id : "night";
        await js.InvokeVoidAsync("adminTheme.apply", CurrentId);
        Changed?.Invoke();
    }
}
