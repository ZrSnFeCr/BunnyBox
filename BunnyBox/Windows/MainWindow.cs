using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

namespace BunnyBox.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly string bunnyImagePath;
    private readonly BunnyBox plugin;

    public MainWindow(BunnyBox plugin, string bunnyImagePath)
        : base("Telewalk##telewalkmainwindow", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(380, 380),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.bunnyImagePath = bunnyImagePath;
        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        if (ImGui.Button("Show Settings"))
        {
            plugin.ToggleConfigUi();
        }

        ImGui.Spacing();

        using (var child = ImRaii.Child("SomeChildWithAScrollbar", Vector2.Zero, true))
        {
            if (child.Success)
            {
                ImGui.Text("Have a bunny:");
                var bunnyImage = BunnyBox.TextureProvider.GetFromFile(bunnyImagePath).GetWrapOrDefault();
                if (bunnyImage != null)
                {
                    using (ImRaii.PushIndent(55f))
                    {
                        ImGui.Image(bunnyImage.Handle, bunnyImage.Size);
                    }
                }
                else
                {
                    ImGui.Text("Image not found.");
                }

                ImGuiHelpers.ScaledDummy(20.0f);
            }
        }
    }
}
