namespace HashtagChris.DotNetBlueZ;

public static class GattDescriptor1Extensions
{
    public static Task<string> GetUUIDAsync(this IGattDescriptor1 o) => o.GetAsync<string>("UUID");

    public static Task<IGattCharacteristic1> GetCharacteristicAsync(this IGattDescriptor1 o) =>
        o.GetAsync<IGattCharacteristic1>("Characteristic");

    public static Task<byte[]> GetValueAsync(this IGattDescriptor1 o) => o.GetAsync<byte[]>("Value");
}