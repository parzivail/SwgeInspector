namespace NavXMxp;

[Flags]
public enum SensorStatus
{
    Moving = 0x01,
    YawStable = 0x02,
    MagDisturbance = 0x04,
    AltitudeValid = 0x08,
    SealevelPressSet = 0x10,
    FusedHeadingValid = 0x20
}