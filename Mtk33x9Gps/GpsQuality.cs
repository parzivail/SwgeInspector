namespace Mtk33x9Gps;

public enum GpsQuality : byte
{
    NoFix = 0,
    ValidFix = 1,
    RtkFixedAmbiguities = 4,
    RtkFloatAmbiguities = 5
}