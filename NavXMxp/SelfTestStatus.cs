namespace NavXMxp;

[Flags]
public enum SelfTestStatus
{
    StatusComplete = 0x80,
    ResultGyroPassed = 0x01,
    ResultAccelPassed = 0x02,
    ResultMagPassed = 0x04,
    ResultBaroPassed = 0x08
}