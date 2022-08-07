namespace HashtagChris.DotNetBlueZ;

public class DeviceFoundEventArgs : BlueZEventArgs
{
    public DeviceFoundEventArgs(Device device, bool isStateChange = true)
        : base(isStateChange)
    {
        Device = device;
    }

    public Device Device { get; }
}