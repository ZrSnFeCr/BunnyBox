using Dalamud.Configuration;
using System;

namespace BunnyBox;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool active { get; set; } = false;
    private float _displacementFactor { get; set; } = 0.1f;

    public float displacementFactor
    {
        get => _displacementFactor;
        set => _displacementFactor = Math.Clamp(value, 0.0f, 1.0f);
    }

    public void Save()
    {
        BunnyBox.PluginInterface.SavePluginConfig(this);
    }
}
