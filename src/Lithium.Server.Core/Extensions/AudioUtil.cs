namespace Lithium.Server.Core;

public static class AudioUtil
{
    public const float MinDecibelVolume = -100.0F;
    public const float MaxDecibelVolume = 10.0F;
    public const float MinSemitonePitch = -12.0F;
    public const float MaxSemitonePitch = 12.0F;

    public static float DecibelsToLinearGain(float decibels)
    {
        return decibels <= -100.0F ? 0.0F : (float)Math.Pow(10.0, decibels / 20.0F);
    }

    public static float LinearGainToDecibels(float linearGain)
    {
        return linearGain <= 0.0F ? -100.0F : (float)(Math.Log(linearGain) / Math.Log(10.0) * 20.0);
    }

    public static float SemitonesToLinearPitch(float semitones)
    {
        return (float)(1.0 / Math.Pow(2.0, -semitones / 12.0F));
    }

    public static float LinearPitchToSemitones(float linearPitch)
    {
        return (float)(Math.Log(linearPitch) / Math.Log(2.0) * 12.0);
    }
}