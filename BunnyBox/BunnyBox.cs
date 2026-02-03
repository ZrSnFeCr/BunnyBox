using BunnyBox.Windows;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using System;
using System.IO;
using System.Numerics;

namespace BunnyBox;

public sealed class BunnyBox : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IKeyState KeyState { get; private set; } = null!;
    [PluginService] internal static ISigScanner SigScanner { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;

    private const string CommandName = "/telewalk";
    public Configuration Configuration { get; init; }
    public readonly WindowSystem WindowSystem = new("BunnyBox");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    public BunnyBox()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        var bunnyImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "bunny.png");

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, bunnyImagePath);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = CommandName
        });

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;

        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    public void SetEnabled(bool enabled)
    {
        if (Framework == null)
        {
            Log.Error("BunnyBox error: Framework is null!");
            return;
        }
        if (enabled)
        {
            Framework.Update += ModifyPOS;
            Log.Info("Enabling Telewalk");
        }
        else
        {
            Framework.Update -= ModifyPOS;
            Log.Info("Disabling Telewalk");
        }
    }

    private void OnCommand(string command, string args)
    {
        ToggleConfigUi();
    }

    private unsafe void ModifyPOS(IFramework framework)
    {
        if (!Configuration.active)
        {
            return;
        }
        var camMgr = FFXIVClientStructs.FFXIV.Client.Game.Control.CameraManager.Instance();
        if (camMgr == null) return;

        var camera = (Camera*)camMgr->GetActiveCamera();
        var c = camera->ViewMatrix;
        var forward = new Vector3(c.M41, 0f, -c.M21);
        var right = new Vector3(-c.M21, 0f, -c.M41);
        var up = new Vector3(0f, Configuration.displacementFactor, 0f);

        var player = ObjectTable.LocalPlayer;
        if (player != null)
        {
            var movement = Vector3.Zero;
            if (KeyState[VirtualKey.W]) movement += forward;
            if (KeyState[VirtualKey.S]) movement -= forward;
            if (KeyState[VirtualKey.A]) movement += right;
            if (KeyState[VirtualKey.D]) movement -= right;
            if (KeyState[VirtualKey.SPACE]) movement += up;
            if (KeyState[VirtualKey.SHIFT]) movement -= up;

            if (movement != Vector3.Zero)
            {
                movement = Vector3.Normalize(movement) * Configuration.displacementFactor;
                SetPos(player.Position + movement);
            }
        }
    }

    private static nint SetPosFunPtr
    {
        get
        {
            if (!SigScanner.TryScanText("E8 ?? ?? ?? ?? 83 4B 70 01", out var ptr))
            {
                return IntPtr.Zero;
            }
            return ptr;
        }
    }

    public static void SetPos(Vector3 pos)
    {
        SetPos(pos.X, pos.Z, pos.Y);
    }

    public unsafe static void SetPos(float x, float y, float z)
    {
        var player = ObjectTable.LocalPlayer;
        if (SetPosFunPtr != IntPtr.Zero && player != null)
        {
            ((delegate* unmanaged[Stdcall]<long, float, float, float, long>)SetPosFunPtr)(player.Address, x, z, y);
        }
    }

    public void ToggleConfigUi() => ConfigWindow.Toggle();
    public void ToggleMainUi() => MainWindow.Toggle();
}
