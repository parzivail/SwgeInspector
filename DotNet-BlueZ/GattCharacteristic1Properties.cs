using Tmds.DBus;

namespace HashtagChris.DotNetBlueZ;

[Dictionary]
public class GattCharacteristic1Properties
{
    private string _UUID = default(string);

    public string UUID
    {
        get { return _UUID; }

        set { _UUID = (value); }
    }

    private ObjectPath _Service = default(ObjectPath);

    public ObjectPath Service
    {
        get { return _Service; }

        set { _Service = (value); }
    }

    private byte[] _Value = default(byte[]);

    public byte[] Value
    {
        get { return _Value; }

        set { _Value = (value); }
    }

    private bool _Notifying = default(bool);

    public bool Notifying
    {
        get { return _Notifying; }

        set { _Notifying = (value); }
    }

    private string[] _Flags = default(string[]);

    public string[] Flags
    {
        get { return _Flags; }

        set { _Flags = (value); }
    }

    private bool _WriteAcquired = default(bool);

    public bool WriteAcquired
    {
        get { return _WriteAcquired; }

        set { _WriteAcquired = (value); }
    }

    private bool _NotifyAcquired = default(bool);

    public bool NotifyAcquired
    {
        get { return _NotifyAcquired; }

        set { _NotifyAcquired = (value); }
    }
}