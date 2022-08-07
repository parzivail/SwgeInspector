namespace NavXMxp;

[Flags]
public enum CalibrationStatus
{
    ImuCalInprogress = 0x00,
    ImuCalAccumulate = 0x01,
    ImuCalComplete = 0x02,
    MagCalComplete = 0x04,
    BaroCalComplete = 0x08
}