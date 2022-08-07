using Tmds.DBus;

namespace HashtagChris.DotNetBlueZ;

[Dictionary]
public class MediaControl1Properties
{
    private bool _Connected = default(bool);

    public bool Connected
    {
        get { return _Connected; }

        set { _Connected = (value); }
    }

    private ObjectPath _Player = default(ObjectPath);

    public ObjectPath Player
    {
        get { return _Player; }

        set { _Player = (value); }
    }
}