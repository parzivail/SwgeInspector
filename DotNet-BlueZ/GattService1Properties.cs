using Tmds.DBus;

namespace HashtagChris.DotNetBlueZ;

[Dictionary]
public class GattService1Properties
{
    private string _UUID = default(string);

    public string UUID
    {
        get { return _UUID; }

        set { _UUID = (value); }
    }

    private ObjectPath _Device = default(ObjectPath);

    public ObjectPath Device
    {
        get { return _Device; }

        set { _Device = (value); }
    }

    private bool _Primary = default(bool);

    public bool Primary
    {
        get { return _Primary; }

        set { _Primary = (value); }
    }

    private ObjectPath[] _Includes = default(ObjectPath[]);

    public ObjectPath[] Includes
    {
        get { return _Includes; }

        set { _Includes = (value); }
    }
}