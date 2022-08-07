namespace HashtagChris.DotNetBlueZ;

public static class Device1Extensions
{
    public static Task<string> GetAddressAsync(this IDevice1 o) => o.GetAsync<string>("Address");
    public static Task<string> GetAddressTypeAsync(this IDevice1 o) => o.GetAsync<string>("AddressType");
    public static Task<string> GetNameAsync(this IDevice1 o) => o.GetAsync<string>("Name");
    public static Task<string> GetAliasAsync(this IDevice1 o) => o.GetAsync<string>("Alias");
    public static Task<uint> GetClassAsync(this IDevice1 o) => o.GetAsync<uint>("Class");
    public static Task<ushort> GetAppearanceAsync(this IDevice1 o) => o.GetAsync<ushort>("Appearance");
    public static Task<string> GetIconAsync(this IDevice1 o) => o.GetAsync<string>("Icon");
    public static Task<bool> GetPairedAsync(this IDevice1 o) => o.GetAsync<bool>("Paired");
    public static Task<bool> GetTrustedAsync(this IDevice1 o) => o.GetAsync<bool>("Trusted");
    public static Task<bool> GetBlockedAsync(this IDevice1 o) => o.GetAsync<bool>("Blocked");
    public static Task<bool> GetLegacyPairingAsync(this IDevice1 o) => o.GetAsync<bool>("LegacyPairing");
    public static Task<short> GetRSSIAsync(this IDevice1 o) => o.GetAsync<short>("RSSI");
    public static Task<bool> GetConnectedAsync(this IDevice1 o) => o.GetAsync<bool>("Connected");
    public static Task<string[]> GetUUIDsAsync(this IDevice1 o) => o.GetAsync<string[]>("UUIDs");
    public static Task<string> GetModaliasAsync(this IDevice1 o) => o.GetAsync<string>("Modalias");
    public static Task<IAdapter1> GetAdapterAsync(this IDevice1 o) => o.GetAsync<IAdapter1>("Adapter");

    public static Task<IDictionary<ushort, object>> GetManufacturerDataAsync(this IDevice1 o) =>
        o.GetAsync<IDictionary<ushort, object>>("ManufacturerData");

    public static Task<IDictionary<string, object>> GetServiceDataAsync(this IDevice1 o) =>
        o.GetAsync<IDictionary<string, object>>("ServiceData");

    public static Task<short> GetTxPowerAsync(this IDevice1 o) => o.GetAsync<short>("TxPower");
    public static Task<bool> GetServicesResolvedAsync(this IDevice1 o) => o.GetAsync<bool>("ServicesResolved");
    public static Task SetAliasAsync(this IDevice1 o, string val) => o.SetAsync("Alias", val);
    public static Task SetTrustedAsync(this IDevice1 o, bool val) => o.SetAsync("Trusted", val);
    public static Task SetBlockedAsync(this IDevice1 o, bool val) => o.SetAsync("Blocked", val);
}