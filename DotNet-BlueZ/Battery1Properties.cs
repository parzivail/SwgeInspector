using Tmds.DBus;

namespace HashtagChris.DotNetBlueZ;

[Dictionary]
public class Battery1Properties
{
    private byte _Percentage = default(byte);

    public byte Percentage
    {
        get { return _Percentage; }

        set { _Percentage = (value); }
    }
}