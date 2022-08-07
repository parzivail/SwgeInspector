using Tmds.DBus;

namespace HashtagChris.DotNetBlueZ;

public static class GattService1Extensions
{
    public static Task<string> GetUUIDAsync(this IGattService1 o) => o.GetAsync<string>("UUID");
    public static Task<IDevice1> GetDeviceAsync(this IGattService1 o) => o.GetAsync<IDevice1>("Device");
    public static Task<bool> GetPrimaryAsync(this IGattService1 o) => o.GetAsync<bool>("Primary");
    public static Task<ObjectPath[]> GetIncludesAsync(this IGattService1 o) => o.GetAsync<ObjectPath[]>("Includes");
}