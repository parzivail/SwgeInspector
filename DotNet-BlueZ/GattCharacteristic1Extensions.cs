namespace HashtagChris.DotNetBlueZ;

public static class GattCharacteristic1Extensions
{
    public static Task<string> GetUUIDAsync(this IGattCharacteristic1 o) => o.GetAsync<string>("UUID");

    public static Task<IGattService1> GetServiceAsync(this IGattCharacteristic1 o) =>
        o.GetAsync<IGattService1>("Service");

    public static Task<byte[]> GetValueAsync(this IGattCharacteristic1 o) => o.GetAsync<byte[]>("Value");
    public static Task<bool> GetNotifyingAsync(this IGattCharacteristic1 o) => o.GetAsync<bool>("Notifying");
    public static Task<string[]> GetFlagsAsync(this IGattCharacteristic1 o) => o.GetAsync<string[]>("Flags");

    public static Task<bool> GetWriteAcquiredAsync(this IGattCharacteristic1 o) =>
        o.GetAsync<bool>("WriteAcquired");

    public static Task<bool> GetNotifyAcquiredAsync(this IGattCharacteristic1 o) =>
        o.GetAsync<bool>("NotifyAcquired");
}