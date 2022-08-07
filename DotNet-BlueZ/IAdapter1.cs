using Tmds.DBus;

namespace HashtagChris.DotNetBlueZ;

[DBusInterface("org.bluez.Adapter1")]
public interface IAdapter1 : IDBusObject
{
    Task StartDiscoveryAsync();
    Task SetDiscoveryFilterAsync(IDictionary<string, object> Properties);
    Task StopDiscoveryAsync();
    Task RemoveDeviceAsync(ObjectPath Device);
    Task<string[]> GetDiscoveryFiltersAsync();
    Task<T> GetAsync<T>(string prop);
    Task<Adapter1Properties> GetAllAsync();
    Task SetAsync(string prop, object val);
    Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
}