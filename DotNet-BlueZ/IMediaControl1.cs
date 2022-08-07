using Tmds.DBus;

namespace HashtagChris.DotNetBlueZ;

[DBusInterface("org.bluez.MediaControl1")]
public interface IMediaControl1 : IDBusObject
{
    Task PlayAsync();
    Task PauseAsync();
    Task StopAsync();
    Task NextAsync();
    Task PreviousAsync();
    Task VolumeUpAsync();
    Task VolumeDownAsync();
    Task FastForwardAsync();
    Task RewindAsync();
    Task<T> GetAsync<T>(string prop);
    Task<MediaControl1Properties> GetAllAsync();
    Task SetAsync(string prop, object val);
    Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
}