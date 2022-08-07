using Tmds.DBus;

namespace HashtagChris.DotNetBlueZ;

public static class MediaControl1Extensions
{
    public static Task<bool> GetConnectedAsync(this IMediaControl1 o) => o.GetAsync<bool>("Connected");
    public static Task<ObjectPath> GetPlayerAsync(this IMediaControl1 o) => o.GetAsync<ObjectPath>("Player");
}