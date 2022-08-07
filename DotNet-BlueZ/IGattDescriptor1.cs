using Tmds.DBus;

namespace HashtagChris.DotNetBlueZ;

[DBusInterface("org.bluez.GattDescriptor1")]
public interface IGattDescriptor1 : IDBusObject
{
    Task<byte[]> ReadValueAsync(IDictionary<string, object> Options);
    Task WriteValueAsync(byte[] Value, IDictionary<string, object> Options);
    Task<T> GetAsync<T>(string prop);
    Task<GattDescriptor1Properties> GetAllAsync();
    Task SetAsync(string prop, object val);
    Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
}