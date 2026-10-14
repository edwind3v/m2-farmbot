using System.Runtime.InteropServices;
using FarmBot.Core.Configuration;
using FarmBot.Core.Models;
using FarmBot.Platform.Input;
using FarmBot.Platform.Native;
using FarmBot.Platform.Window;

namespace FarmBot.Tests;

public class PlatformTests
{
    [Fact]
    public void AttachByPidFailsClosedWhenTheProcessIsMissing()
    {
        Assert.False(FarmBot.Platform.Client.ProcessMemory.TryAttach(int.MaxValue - 7, out var memory, out var status));
        Assert.Null(memory);
        Assert.Contains("not running", status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MapsAttackAndSkillKeys()
    {
        Assert.Equal(0x20, VirtualKeys.FromName("space"));
        Assert.Equal((ushort)'Z', VirtualKeys.FromName("z"));
        Assert.Equal((ushort)'1', VirtualKeys.FromName("1"));
        Assert.Equal(0x70, VirtualKeys.FromName("F1"));
        Assert.Equal(0x79, VirtualKeys.FromName("f10"));
        Assert.Equal(0, VirtualKeys.FromName("not-a-key"));
    }

    [Fact]
    public void InputStructMatchesTheWin32Size()
    {
        if (!Environment.Is64BitProcess)
            return;
        Assert.Equal(40, Marshal.SizeOf<Win32.INPUT>());
    }

    [Fact]
    public void ConfigRoundTripPreservesRules()
    {
        var path = Path.Combine(Path.GetTempPath(), "farmbot-config-" + Guid.NewGuid().ToString("N") + ".json");
        var config = BotConfig.CreateDefault();
        config.FarmCenterX = 1234.5f;
        config.NameBlacklist.Add("training");
        config.PipeName = @"\\.\pipe\M2FarmBot";
        ConfigStore.Save(path, config);

        var loaded = ConfigStore.LoadOrCreate(path);
        Assert.Equal(EntityKind.Metin, loaded.TargetRules[0].Kind);
        Assert.Equal(100, loaded.TargetRules[0].Priority);
        Assert.Equal("M2FarmBot", loaded.PipeName);
        Assert.Contains("training", loaded.NameBlacklist);
        Assert.Equal(1234.5f, loaded.FarmCenterX, 3);
        File.Delete(path);
    }

    [Fact]
    public void FindsAWindowByTitle()
    {
        const string title = "METIN2 FarmBotProbe9C2A";
        GameWindowInfo? found = null;
        var error = 0;
        var thread = new Thread(() =>
        {
            var hwnd = CreateWindowExW(
                0,
                "Static",
                title,
                unchecked((int)0x80000000) | 0x10000000,
                -3000,
                -3000,
                40,
                20,
                IntPtr.Zero,
                IntPtr.Zero,
                GetModuleHandle(null),
                IntPtr.Zero);
            if (hwnd == IntPtr.Zero)
            {
                error = Marshal.GetLastWin32Error();
                return;
            }

            try
            {
                found = GameWindowFinder.Find(new[] { "FarmBotProbe9C2A" });
            }
            finally
            {
                DestroyWindow(hwnd);
            }
        });
        thread.IsBackground = true;
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(5)), "window probe did not finish");
        Assert.Equal(0, error);
        Assert.NotNull(found);
        Assert.Contains("FarmBotProbe9C2A", found!.Title);
        Assert.Null(GameWindowFinder.Find(new[] { "this-window-should-not-exist-9c2a" }));
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateWindowExW(
        int exStyle,
        string className,
        string windowName,
        int style,
        int x,
        int y,
        int width,
        int height,
        IntPtr parent,
        IntPtr menu,
        IntPtr instance,
        IntPtr param);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(IntPtr hwnd);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr GetModuleHandle(string? name);
}
