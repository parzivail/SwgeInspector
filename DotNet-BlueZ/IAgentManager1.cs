using Tmds.DBus;

namespace HashtagChris.DotNetBlueZ;

[DBusInterface("org.bluez.AgentManager1")]
public interface IAgentManager1 : IDBusObject
{
    Task RegisterAgentAsync(ObjectPath Agent, string Capability);
    Task UnregisterAgentAsync(ObjectPath Agent);
    Task RequestDefaultAgentAsync(ObjectPath Agent);
}