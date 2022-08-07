using Tmds.DBus;

namespace HashtagChris.DotNetBlueZ;

[DBusInterface("org.bluez.GattManager1")]
public interface IGattManager1 : IDBusObject
{
    Task RegisterApplicationAsync(ObjectPath Application, IDictionary<string, object> Options);
    Task UnregisterApplicationAsync(ObjectPath Application);
}