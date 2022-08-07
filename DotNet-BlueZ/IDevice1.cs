using Tmds.DBus;

namespace HashtagChris.DotNetBlueZ;

[DBusInterface("org.bluez.Device1")]
public interface IDevice1 : IDBusObject
{
    Task DisconnectAsync();
    Task ConnectAsync();
    Task ConnectProfileAsync(string UUID);
    Task DisconnectProfileAsync(string UUID);
    Task PairAsync();
    Task CancelPairingAsync();
    Task<T> GetAsync<T>(string prop);
    Task<Device1Properties> GetAllAsync();
    Task SetAsync(string prop, object val);
    Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
}