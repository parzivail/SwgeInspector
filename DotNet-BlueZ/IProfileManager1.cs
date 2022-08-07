using Tmds.DBus;

namespace HashtagChris.DotNetBlueZ;

[DBusInterface("org.bluez.ProfileManager1")]
public interface IProfileManager1 : IDBusObject
{
    Task RegisterProfileAsync(ObjectPath Profile, string UUID, IDictionary<string, object> Options);
    Task UnregisterProfileAsync(ObjectPath Profile);
}