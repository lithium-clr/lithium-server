using System.Collections.Generic;
using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 4,
    FixedBlockSize = 30,
    VariableFieldCount = 24,
    VariableBlockStart = 126,
    MaxSize = 1677721600
)]
public sealed class Weather : INetworkSerializable
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("tagIndexes")]
    public int[]? TagIndexes { get; set; }

    [JsonPropertyName("stars")]
    public string? Stars { get; set; }

    [JsonPropertyName("moons")]
    public Dictionary<int, string>? Moons { get; set; }

    [JsonPropertyName("clouds")]
    public Cloud[]? Clouds { get; set; }

    [JsonPropertyName("sunlightDampingMultiplier")]
    public Dictionary<float, float>? SunlightDampingMultiplier { get; set; }

    [JsonPropertyName("sunlightColors")]
    public Dictionary<float, Color>? SunlightColors { get; set; }

    [JsonPropertyName("skyTopColors")]
    public Dictionary<float, ColorAlpha>? SkyTopColors { get; set; }

    [JsonPropertyName("skyBottomColors")]
    public Dictionary<float, ColorAlpha>? SkyBottomColors { get; set; }

    [JsonPropertyName("skySunsetColors")]
    public Dictionary<float, ColorAlpha>? SkySunsetColors { get; set; }

    [JsonPropertyName("sunColors")]
    public Dictionary<float, Color>? SunColors { get; set; }

    [JsonPropertyName("sunScales")]
    public Dictionary<float, float>? SunScales { get; set; }

    [JsonPropertyName("sunGlowColors")]
    public Dictionary<float, ColorAlpha>? SunGlowColors { get; set; }

    [JsonPropertyName("moonColors")]
    public Dictionary<float, ColorAlpha>? MoonColors { get; set; }

    [JsonPropertyName("moonScales")]
    public Dictionary<float, float>? MoonScales { get; set; }

    [JsonPropertyName("moonGlowColors")]
    public Dictionary<float, ColorAlpha>? MoonGlowColors { get; set; }

    [JsonPropertyName("fogColors")]
    public Dictionary<float, Color>? FogColors { get; set; }

    [JsonPropertyName("fogHeightFalloffs")]
    public Dictionary<float, float>? FogHeightFalloffs { get; set; }

    [JsonPropertyName("fogDensities")]
    public Dictionary<float, float>? FogDensities { get; set; }

    [JsonPropertyName("screenEffect")]
    public string? ScreenEffect { get; set; }

    [JsonPropertyName("screenEffectColors")]
    public Dictionary<float, ColorAlpha>? ScreenEffectColors { get; set; }

    [JsonPropertyName("colorFilters")]
    public Dictionary<float, Color>? ColorFilters { get; set; }

    [JsonPropertyName("waterTints")]
    public Dictionary<float, Color>? WaterTints { get; set; }

    [JsonPropertyName("particle")]
    public WeatherParticle? Particle { get; set; }

    [JsonPropertyName("fog")]
    public NearFar? Fog { get; set; }

    [JsonPropertyName("fogOptions")]
    public FogOptions? FogOptions { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var nullBits = new byte[4];

        if (Fog is not null)                       nullBits[0] |= 1;
        if (FogOptions is not null)                nullBits[0] |= 2;
        if (Id is not null)                        nullBits[0] |= 4;
        if (TagIndexes is not null)                nullBits[0] |= 8;
        if (Stars is not null)                     nullBits[0] |= 16;
        if (Moons is not null)                     nullBits[0] |= 32;
        if (Clouds is not null)                    nullBits[0] |= 64;
        if (SunlightDampingMultiplier is not null) nullBits[0] |= 128;

        if (SunlightColors is not null)            nullBits[1] |= 1;
        if (SkyTopColors is not null)              nullBits[1] |= 2;
        if (SkyBottomColors is not null)           nullBits[1] |= 4;
        if (SkySunsetColors is not null)           nullBits[1] |= 8;
        if (SunColors is not null)                 nullBits[1] |= 16;
        if (SunScales is not null)                 nullBits[1] |= 32;
        if (SunGlowColors is not null)             nullBits[1] |= 64;
        if (MoonColors is not null)                nullBits[1] |= 128;

        if (MoonScales is not null)                nullBits[2] |= 1;
        if (MoonGlowColors is not null)            nullBits[2] |= 2;
        if (FogColors is not null)                 nullBits[2] |= 4;
        if (FogHeightFalloffs is not null)         nullBits[2] |= 8;
        if (FogDensities is not null)              nullBits[2] |= 16;
        if (ScreenEffect is not null)              nullBits[2] |= 32;
        if (ScreenEffectColors is not null)        nullBits[2] |= 64;
        if (ColorFilters is not null)              nullBits[2] |= 128;

        if (WaterTints is not null)                nullBits[3] |= 1;
        if (Particle is not null)                  nullBits[3] |= 2;

        foreach (var b in nullBits) writer.WriteUInt8(b);

        // Fixed Block
        if (Fog is not null) Fog.Serialize(writer);
        else writer.WriteZero(8);

        if (FogOptions is not null) FogOptions.Serialize(writer);
        else writer.WriteZero(18);

        // Reserve offsets
        var idOffsetSlot = writer.ReserveOffset();
        var tagIndexesOffsetSlot = writer.ReserveOffset();
        var starsOffsetSlot = writer.ReserveOffset();
        var moonsOffsetSlot = writer.ReserveOffset();
        var cloudsOffsetSlot = writer.ReserveOffset();
        var sunlightDampingMultiplierOffsetSlot = writer.ReserveOffset();
        var sunlightColorsOffsetSlot = writer.ReserveOffset();
        var skyTopColorsOffsetSlot = writer.ReserveOffset();
        var skyBottomColorsOffsetSlot = writer.ReserveOffset();
        var skySunsetColorsOffsetSlot = writer.ReserveOffset();
        var sunColorsOffsetSlot = writer.ReserveOffset();
        var sunScalesOffsetSlot = writer.ReserveOffset();
        var sunGlowColorsOffsetSlot = writer.ReserveOffset();
        var moonColorsOffsetSlot = writer.ReserveOffset();
        var moonScalesOffsetSlot = writer.ReserveOffset();
        var moonGlowColorsOffsetSlot = writer.ReserveOffset();
        var fogColorsOffsetSlot = writer.ReserveOffset();
        var fogHeightFalloffsOffsetSlot = writer.ReserveOffset();
        var fogDensitiesOffsetSlot = writer.ReserveOffset();
        var screenEffectOffsetSlot = writer.ReserveOffset();
        var screenEffectColorsOffsetSlot = writer.ReserveOffset();
        var colorFiltersOffsetSlot = writer.ReserveOffset();
        var waterTintsOffsetSlot = writer.ReserveOffset();
        var particleOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        // Variable Block
        if (Id is not null)
        {
            writer.WriteOffsetAt(idOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Id, 4096000);
        }
        else writer.WriteOffsetAt(idOffsetSlot, -1);

        if (TagIndexes is not null)
        {
            writer.WriteOffsetAt(tagIndexesOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(TagIndexes.Length);
            foreach (var item in TagIndexes) writer.WriteInt32(item);
        }
        else writer.WriteOffsetAt(tagIndexesOffsetSlot, -1);

        if (Stars is not null)
        {
            writer.WriteOffsetAt(starsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Stars, 4096000);
        }
        else writer.WriteOffsetAt(starsOffsetSlot, -1);

        if (Moons is not null)
        {
            writer.WriteOffsetAt(moonsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Moons.Count);
            foreach (var (key, value) in Moons)
            {
                writer.WriteInt32(key);
                writer.WriteVarUtf8String(value, 4096000);
            }
        }
        else writer.WriteOffsetAt(moonsOffsetSlot, -1);

        if (Clouds is not null)
        {
            writer.WriteOffsetAt(cloudsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Clouds.Length);
            foreach (var cloud in Clouds) cloud.Serialize(writer);
        }
        else writer.WriteOffsetAt(cloudsOffsetSlot, -1);

        if (SunlightDampingMultiplier is not null)
        {
            writer.WriteOffsetAt(sunlightDampingMultiplierOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(SunlightDampingMultiplier.Count);
            foreach (var (key, value) in SunlightDampingMultiplier)
            {
                writer.WriteFloat32(key);
                writer.WriteFloat32(value);
            }
        }
        else writer.WriteOffsetAt(sunlightDampingMultiplierOffsetSlot, -1);

        if (SunlightColors is not null)
        {
            writer.WriteOffsetAt(sunlightColorsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(SunlightColors.Count);
            foreach (var (key, value) in SunlightColors)
            {
                writer.WriteFloat32(key);
                value.Serialize(writer);
            }
        }
        else writer.WriteOffsetAt(sunlightColorsOffsetSlot, -1);

        if (SkyTopColors is not null)
        {
            writer.WriteOffsetAt(skyTopColorsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(SkyTopColors.Count);
            foreach (var (key, value) in SkyTopColors)
            {
                writer.WriteFloat32(key);
                value.Serialize(writer);
            }
        }
        else writer.WriteOffsetAt(skyTopColorsOffsetSlot, -1);

        if (SkyBottomColors is not null)
        {
            writer.WriteOffsetAt(skyBottomColorsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(SkyBottomColors.Count);
            foreach (var (key, value) in SkyBottomColors)
            {
                writer.WriteFloat32(key);
                value.Serialize(writer);
            }
        }
        else writer.WriteOffsetAt(skyBottomColorsOffsetSlot, -1);

        if (SkySunsetColors is not null)
        {
            writer.WriteOffsetAt(skySunsetColorsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(SkySunsetColors.Count);
            foreach (var (key, value) in SkySunsetColors)
            {
                writer.WriteFloat32(key);
                value.Serialize(writer);
            }
        }
        else writer.WriteOffsetAt(skySunsetColorsOffsetSlot, -1);

        if (SunColors is not null)
        {
            writer.WriteOffsetAt(sunColorsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(SunColors.Count);
            foreach (var (key, value) in SunColors)
            {
                writer.WriteFloat32(key);
                value.Serialize(writer);
            }
        }
        else writer.WriteOffsetAt(sunColorsOffsetSlot, -1);

        if (SunScales is not null)
        {
            writer.WriteOffsetAt(sunScalesOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(SunScales.Count);
            foreach (var (key, value) in SunScales)
            {
                writer.WriteFloat32(key);
                writer.WriteFloat32(value);
            }
        }
        else writer.WriteOffsetAt(sunScalesOffsetSlot, -1);

        if (SunGlowColors is not null)
        {
            writer.WriteOffsetAt(sunGlowColorsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(SunGlowColors.Count);
            foreach (var (key, value) in SunGlowColors)
            {
                writer.WriteFloat32(key);
                value.Serialize(writer);
            }
        }
        else writer.WriteOffsetAt(sunGlowColorsOffsetSlot, -1);

        if (MoonColors is not null)
        {
            writer.WriteOffsetAt(moonColorsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(MoonColors.Count);
            foreach (var (key, value) in MoonColors)
            {
                writer.WriteFloat32(key);
                value.Serialize(writer);
            }
        }
        else writer.WriteOffsetAt(moonColorsOffsetSlot, -1);

        if (MoonScales is not null)
        {
            writer.WriteOffsetAt(moonScalesOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(MoonScales.Count);
            foreach (var (key, value) in MoonScales)
            {
                writer.WriteFloat32(key);
                writer.WriteFloat32(value);
            }
        }
        else writer.WriteOffsetAt(moonScalesOffsetSlot, -1);

        if (MoonGlowColors is not null)
        {
            writer.WriteOffsetAt(moonGlowColorsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(MoonGlowColors.Count);
            foreach (var (key, value) in MoonGlowColors)
            {
                writer.WriteFloat32(key);
                value.Serialize(writer);
            }
        }
        else writer.WriteOffsetAt(moonGlowColorsOffsetSlot, -1);

        if (FogColors is not null)
        {
            writer.WriteOffsetAt(fogColorsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(FogColors.Count);
            foreach (var (key, value) in FogColors)
            {
                writer.WriteFloat32(key);
                value.Serialize(writer);
            }
        }
        else writer.WriteOffsetAt(fogColorsOffsetSlot, -1);

        if (FogHeightFalloffs is not null)
        {
            writer.WriteOffsetAt(fogHeightFalloffsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(FogHeightFalloffs.Count);
            foreach (var (key, value) in FogHeightFalloffs)
            {
                writer.WriteFloat32(key);
                writer.WriteFloat32(value);
            }
        }
        else writer.WriteOffsetAt(fogHeightFalloffsOffsetSlot, -1);

        if (FogDensities is not null)
        {
            writer.WriteOffsetAt(fogDensitiesOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(FogDensities.Count);
            foreach (var (key, value) in FogDensities)
            {
                writer.WriteFloat32(key);
                writer.WriteFloat32(value);
            }
        }
        else writer.WriteOffsetAt(fogDensitiesOffsetSlot, -1);

        if (ScreenEffect is not null)
        {
            writer.WriteOffsetAt(screenEffectOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(ScreenEffect, 4096000);
        }
        else writer.WriteOffsetAt(screenEffectOffsetSlot, -1);

        if (ScreenEffectColors is not null)
        {
            writer.WriteOffsetAt(screenEffectColorsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(ScreenEffectColors.Count);
            foreach (var (key, value) in ScreenEffectColors)
            {
                writer.WriteFloat32(key);
                value.Serialize(writer);
            }
        }
        else writer.WriteOffsetAt(screenEffectColorsOffsetSlot, -1);

        if (ColorFilters is not null)
        {
            writer.WriteOffsetAt(colorFiltersOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(ColorFilters.Count);
            foreach (var (key, value) in ColorFilters)
            {
                writer.WriteFloat32(key);
                value.Serialize(writer);
            }
        }
        else writer.WriteOffsetAt(colorFiltersOffsetSlot, -1);

        if (WaterTints is not null)
        {
            writer.WriteOffsetAt(waterTintsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(WaterTints.Count);
            foreach (var (key, value) in WaterTints)
            {
                writer.WriteFloat32(key);
                value.Serialize(writer);
            }
        }
        else writer.WriteOffsetAt(waterTintsOffsetSlot, -1);

        if (Particle is not null)
        {
            writer.WriteOffsetAt(particleOffsetSlot, writer.Position - varBlockStart);
            Particle.Serialize(writer);
        }
        else writer.WriteOffsetAt(particleOffsetSlot, -1);
    }

    public void Deserialize(PacketReader reader)
    {
        var nullBits = new byte[4];
        for (var i = 0; i < 4; i++) nullBits[i] = reader.ReadUInt8();

        if ((nullBits[0] & 1) != 0) Fog = reader.ReadObject<NearFar>();
        else reader.SeekTo(reader.GetPosition() + 8);

        if ((nullBits[0] & 2) != 0) FogOptions = reader.ReadObject<FogOptions>();
        else reader.SeekTo(reader.GetPosition() + 18);

        var offsets = reader.ReadOffsets(24);

        if ((nullBits[0] & 4)   != 0) Id = reader.ReadVarUtf8StringAt(offsets[0]);
        if ((nullBits[0] & 8)   != 0) TagIndexes = reader.ReadArrayAt(offsets[1], r => r.ReadInt32());
        if ((nullBits[0] & 16)  != 0) Stars = reader.ReadVarUtf8StringAt(offsets[2]);
        if ((nullBits[0] & 32)  != 0) Moons = reader.ReadDictionaryAt(offsets[3], r => r.ReadInt32(), r => r.ReadUtf8String());
        if ((nullBits[0] & 64)  != 0) Clouds = reader.ReadArrayAt(offsets[4], r =>
        {
            var c = new Cloud();
            c.Deserialize(r);
            return c;
        });
        if ((nullBits[0] & 128) != 0) SunlightDampingMultiplier = reader.ReadDictionaryAt(offsets[5], r => r.ReadFloat32(), r => r.ReadFloat32());

        if ((nullBits[1] & 1)   != 0) SunlightColors = reader.ReadDictionaryAt(offsets[6], r => r.ReadFloat32(), r =>
        {
            var c = new Color();
            c.Deserialize(r);
            return c;
        });
        if ((nullBits[1] & 2)   != 0) SkyTopColors = reader.ReadDictionaryAt(offsets[7], r => r.ReadFloat32(), r =>
        {
            var c = new ColorAlpha();
            c.Deserialize(r);
            return c;
        });
        if ((nullBits[1] & 4)   != 0) SkyBottomColors = reader.ReadDictionaryAt(offsets[8], r => r.ReadFloat32(), r =>
        {
            var c = new ColorAlpha();
            c.Deserialize(r);
            return c;
        });
        if ((nullBits[1] & 8)   != 0) SkySunsetColors = reader.ReadDictionaryAt(offsets[9], r => r.ReadFloat32(), r =>
        {
            var c = new ColorAlpha();
            c.Deserialize(r);
            return c;
        });
        if ((nullBits[1] & 16)  != 0) SunColors = reader.ReadDictionaryAt(offsets[10], r => r.ReadFloat32(), r =>
        {
            var c = new Color();
            c.Deserialize(r);
            return c;
        });
        if ((nullBits[1] & 32)  != 0) SunScales = reader.ReadDictionaryAt(offsets[11], r => r.ReadFloat32(), r => r.ReadFloat32());
        if ((nullBits[1] & 64)  != 0) SunGlowColors = reader.ReadDictionaryAt(offsets[12], r => r.ReadFloat32(), r =>
        {
            var c = new ColorAlpha();
            c.Deserialize(r);
            return c;
        });
        if ((nullBits[1] & 128) != 0) MoonColors = reader.ReadDictionaryAt(offsets[13], r => r.ReadFloat32(), r =>
        {
            var c = new ColorAlpha();
            c.Deserialize(r);
            return c;
        });

        if ((nullBits[2] & 1)   != 0) MoonScales = reader.ReadDictionaryAt(offsets[14], r => r.ReadFloat32(), r => r.ReadFloat32());
        if ((nullBits[2] & 2)   != 0) MoonGlowColors = reader.ReadDictionaryAt(offsets[15], r => r.ReadFloat32(), r =>
        {
            var c = new ColorAlpha();
            c.Deserialize(r);
            return c;
        });
        if ((nullBits[2] & 4)   != 0) FogColors = reader.ReadDictionaryAt(offsets[16], r => r.ReadFloat32(), r =>
        {
            var c = new Color();
            c.Deserialize(r);
            return c;
        });
        if ((nullBits[2] & 8)   != 0) FogHeightFalloffs = reader.ReadDictionaryAt(offsets[17], r => r.ReadFloat32(), r => r.ReadFloat32());
        if ((nullBits[2] & 16)  != 0) FogDensities = reader.ReadDictionaryAt(offsets[18], r => r.ReadFloat32(), r => r.ReadFloat32());
        if ((nullBits[2] & 32)  != 0) ScreenEffect = reader.ReadVarUtf8StringAt(offsets[19]);
        if ((nullBits[2] & 64)  != 0) ScreenEffectColors = reader.ReadDictionaryAt(offsets[20], r => r.ReadFloat32(), r =>
        {
            var c = new ColorAlpha();
            c.Deserialize(r);
            return c;
        });
        if ((nullBits[2] & 128) != 0) ColorFilters = reader.ReadDictionaryAt(offsets[21], r => r.ReadFloat32(), r =>
        {
            var c = new Color();
            c.Deserialize(r);
            return c;
        });

        if ((nullBits[3] & 1)   != 0) WaterTints = reader.ReadDictionaryAt(offsets[22], r => r.ReadFloat32(), r =>
        {
            var c = new Color();
            c.Deserialize(r);
            return c;
        });
        if ((nullBits[3] & 2)   != 0) Particle = reader.ReadObjectAt<WeatherParticle>(offsets[23]);
    }
}
