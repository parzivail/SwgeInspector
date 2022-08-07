using Tmds.DBus;

namespace HashtagChris.DotNetBlueZ;

[DBusInterface("org.bluez.GattService1")]
public interface IGattService1 : IDBusObject
{
    Task<T> GetAsync<T>(string prop);
    Task<GattService1Properties> GetAllAsync();
    Task SetAsync(string prop, object val);
    Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
}