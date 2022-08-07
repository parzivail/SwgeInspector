using Tmds.DBus;

namespace HashtagChris.DotNetBlueZ;

[Dictionary]
public class LEAdvertisingManager1Properties
{
    private byte _ActiveInstances = default(byte);

    public byte ActiveInstances
    {
        get { return _ActiveInstances; }

        set { _ActiveInstances = (value); }
    }

    private byte _SupportedInstances = default(byte);

    public byte SupportedInstances
    {
        get { return _SupportedInstances; }

        set { _SupportedInstances = (value); }
    }

    private string[] _SupportedIncludes = default(string[]);

    public string[] SupportedIncludes
    {
        get { return _SupportedIncludes; }

        set { _SupportedIncludes = (value); }
    }
}