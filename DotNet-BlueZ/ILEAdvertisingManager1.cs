using Tmds.DBus;

namespace HashtagChris.DotNetBlueZ;

[DBusInterface("org.bluez.LEAdvertisingManager1")]
public interface ILEAdvertisingManager1 : IDBusObject
{
    Task RegisterAdvertisementAsync(ObjectPath Advertisement, IDictionary<string, object> Options);
    Task UnregisterAdvertisementAsync(ObjectPath Service);
    Task<T> GetAsync<T>(string prop);
    Task<LEAdvertisingManager1Properties> GetAllAsync();
    Task SetAsync(string prop, object val);
    Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
}