using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System;
using System.Numerics;

namespace BunnyBox.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;
    private readonly BunnyBox telewalk;

    public ConfigWindow(BunnyBox plugin) : base("Telewalk settings###ujinolive")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(280, 100);
        SizeCondition = ImGuiCond.Always;

        configuration = plugin.Configuration;
        telewalk = plugin;
    }

    public void Dispose() { }

    public override void PreDraw() { }

    public override void Draw()
    {
        var active = configuration.active;
        if (ImGui.Checkbox("Active", ref active))
        {
            configuration.active = active;
            configuration.Save();
            telewalk.SetEnabled(active);
        }

        var displacementFactor = configuration.displacementFactor;
        if (ImGui.InputFloat("Speed", ref displacementFactor, 0.01f, 0.02f, "%.02f"))
        {
            configuration.displacementFactor = displacementFactor;
            configuration.Save();
        }
    }
}
