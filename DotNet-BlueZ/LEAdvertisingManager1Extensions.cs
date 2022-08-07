namespace HashtagChris.DotNetBlueZ;

public static class LEAdvertisingManager1Extensions
{
    public static Task<byte> GetActiveInstancesAsync(this ILEAdvertisingManager1 o) =>
        o.GetAsync<byte>("ActiveInstances");

    public static Task<byte> GetSupportedInstancesAsync(this ILEAdvertisingManager1 o) =>
        o.GetAsync<byte>("SupportedInstances");

    public static Task<string[]> GetSupportedIncludesAsync(this ILEAdvertisingManager1 o) =>
        o.GetAsync<string[]>("SupportedIncludes");
}