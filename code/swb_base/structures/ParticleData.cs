using System.IO;
using Sandbox;

namespace SWB_Base;

public partial class ParticleData : BaseNetworkable
{
    /// <summary>Path to particle</summary>
    [Net] public string Path { get; set; }

    /// <summary>View model scale</summary>
    [Net] public float VMScale { get; set; } = 1f;

    /// <summary>World model scale</summary>
    [Net] public float WMScale { get; set; } = 1f;

    public ParticleData() { }

    public ParticleData(string path)
    {
        this.Path = path;
    }

    public ParticleData(string path, float scale)
    {
        this.Path = path;
        this.VMScale = scale;
        this.WMScale = scale;
    }

    public ParticleData(string path, float vmScale, float wmScale)
    {
        this.Path = path;
        this.VMScale = vmScale;
        this.WMScale = wmScale;
    }

    public byte[] Serialize()
    {
        if (Path == null) return default;

        using var ms = new MemoryStream();
        using (var writer = new BinaryWriter(ms))
        {
            writer.Write(Path);
            writer.Write(VMScale);
            writer.Write(WMScale);
        }

        return ms.ToArray();
    }

    public static ParticleDataS Deserialize(byte[] bytes)
    {
        if (bytes == null) return null;

        var particleData = new ParticleDataS();
        using var ms = new MemoryStream(bytes);
        using (var reader = new BinaryReader(ms))
        {
            particleData.Path = reader.ReadString();
            particleData.VMScale = reader.ReadSingle();
            particleData.WMScale = reader.ReadSingle();
        }

        return particleData;
    }
}

/// <summary>
/// Particle data for client usage
/// </summary>
public class ParticleDataS
{
    /// <summary>Path to particle</summary>
    public string Path { get; set; }

    /// <summary>View model scale</summary>
    public float VMScale { get; set; } = 1f;

    /// <summary>World model scale</summary>
    public float WMScale { get; set; } = 1f;
}
