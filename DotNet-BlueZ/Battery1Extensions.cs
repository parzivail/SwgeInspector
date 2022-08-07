namespace HashtagChris.DotNetBlueZ;

public static class Battery1Extensions
{
    public static Task<byte> GetPercentageAsync(this IBattery1 o) => o.GetAsync<byte>("Percentage");
}