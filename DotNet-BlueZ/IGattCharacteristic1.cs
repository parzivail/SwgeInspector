using Tmds.DBus;

namespace HashtagChris.DotNetBlueZ;

[DBusInterface("org.bluez.GattCharacteristic1")]
public interface IGattCharacteristic1 : IDBusObject
{
    Task<byte[]> ReadValueAsync(IDictionary<string, object> Options);
    Task WriteValueAsync(byte[] Value, IDictionary<string, object> Options);
    Task<(CloseSafeHandle fd, ushort mtu)> AcquireWriteAsync(IDictionary<string, object> Options);
    Task<(CloseSafeHandle fd, ushort mtu)> AcquireNotifyAsync(IDictionary<string, object> Options);
    Task StartNotifyAsync();
    Task StopNotifyAsync();
    Task<T> GetAsync<T>(string prop);
    Task<GattCharacteristic1Properties> GetAllAsync();
    Task SetAsync(string prop, object val);
    Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
}