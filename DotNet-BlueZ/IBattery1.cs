using Tmds.DBus;

namespace HashtagChris.DotNetBlueZ;

[DBusInterface("org.bluez.Battery1")]
public interface IBattery1 : IDBusObject
{
    Task<T> GetAsync<T>(string prop);
    Task<Battery1Properties> GetAllAsync();
    Task SetAsync(string prop, object val);
    Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
}