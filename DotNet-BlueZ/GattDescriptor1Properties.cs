using Tmds.DBus;

namespace HashtagChris.DotNetBlueZ;

[Dictionary]
public class GattDescriptor1Properties
{
    private string _UUID = default(string);

    public string UUID
    {
        get { return _UUID; }

        set { _UUID = (value); }
    }

    private ObjectPath _Characteristic = default(ObjectPath);

    public ObjectPath Characteristic
    {
        get { return _Characteristic; }

        set { _Characteristic = (value); }
    }

    private byte[] _Value = default(byte[]);

    public byte[] Value
    {
        get { return _Value; }

        set { _Value = (value); }
    }
}