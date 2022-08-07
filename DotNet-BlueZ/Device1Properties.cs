using Tmds.DBus;

namespace HashtagChris.DotNetBlueZ;

[Dictionary]
public class Device1Properties
{
    private string _Address = default(string);

    public string Address
    {
        get { return _Address; }

        set { _Address = (value); }
    }

    private string _AddressType = default(string);

    public string AddressType
    {
        get { return _AddressType; }

        set { _AddressType = (value); }
    }

    private string _Name = default(string);

    public string Name
    {
        get { return _Name; }

        set { _Name = (value); }
    }

    private string _Alias = default(string);

    public string Alias
    {
        get { return _Alias; }

        set { _Alias = (value); }
    }

    private uint _Class = default(uint);

    public uint Class
    {
        get { return _Class; }

        set { _Class = (value); }
    }

    private ushort _Appearance = default(ushort);

    public ushort Appearance
    {
        get { return _Appearance; }

        set { _Appearance = (value); }
    }

    private string _Icon = default(string);

    public string Icon
    {
        get { return _Icon; }

        set { _Icon = (value); }
    }

    private bool _Paired = default(bool);

    public bool Paired
    {
        get { return _Paired; }

        set { _Paired = (value); }
    }

    private bool _Trusted = default(bool);

    public bool Trusted
    {
        get { return _Trusted; }

        set { _Trusted = (value); }
    }

    private bool _Blocked = default(bool);

    public bool Blocked
    {
        get { return _Blocked; }

        set { _Blocked = (value); }
    }

    private bool _LegacyPairing = default(bool);

    public bool LegacyPairing
    {
        get { return _LegacyPairing; }

        set { _LegacyPairing = (value); }
    }

    private short _RSSI = default(short);

    public short RSSI
    {
        get { return _RSSI; }

        set { _RSSI = (value); }
    }

    private bool _Connected = default(bool);

    public bool Connected
    {
        get { return _Connected; }

        set { _Connected = (value); }
    }

    private string[] _UUIDs = default(string[]);

    public string[] UUIDs
    {
        get { return _UUIDs; }

        set { _UUIDs = (value); }
    }

    private string _Modalias = default(string);

    public string Modalias
    {
        get { return _Modalias; }

        set { _Modalias = (value); }
    }

    private ObjectPath _Adapter = default(ObjectPath);

    public ObjectPath Adapter
    {
        get { return _Adapter; }

        set { _Adapter = (value); }
    }

    private IDictionary<ushort, object> _ManufacturerData = default(IDictionary<ushort, object>);

    public IDictionary<ushort, object> ManufacturerData
    {
        get { return _ManufacturerData; }

        set { _ManufacturerData = (value); }
    }

    private IDictionary<string, object> _ServiceData = default(IDictionary<string, object>);

    public IDictionary<string, object> ServiceData
    {
        get { return _ServiceData; }

        set { _ServiceData = (value); }
    }

    private short _TxPower = default(short);

    public short TxPower
    {
        get { return _TxPower; }

        set { _TxPower = (value); }
    }

    private bool _ServicesResolved = default(bool);

    public bool ServicesResolved
    {
        get { return _ServicesResolved; }

        set { _ServicesResolved = (value); }
    }
}