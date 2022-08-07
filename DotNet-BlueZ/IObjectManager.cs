using Tmds.DBus;

namespace HashtagChris.DotNetBlueZ;

[DBusInterface("org.freedesktop.DBus.ObjectManager")]
public interface IObjectManager : IDBusObject
{
    Task<IDictionary<ObjectPath, IDictionary<string, IDictionary<string, object>>>> GetManagedObjectsAsync();

    Task<IDisposable> WatchInterfacesAddedAsync(
        Action<(ObjectPath @object, IDictionary<string, IDictionary<string, object>> interfaces)> handler,
        Action<Exception> onError = null);

    Task<IDisposable> WatchInterfacesRemovedAsync(Action<(ObjectPath @object, string[] interfaces)> handler,
        Action<Exception> onError = null);
}