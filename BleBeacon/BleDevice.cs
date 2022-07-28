using System.Collections.Concurrent;
using HashtagChris.DotNetBlueZ;
using HashtagChris.DotNetBlueZ.Extensions;
using Microsoft.IO;
using Tmds.DBus;

namespace BleBeacon;

public class BleDevice
{
    private static readonly RecyclableMemoryStreamManager MemoryStreamManager = new();
    
    public static ushort ReverseBytes(ushort value)
    {
         return (ushort)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
    }

    public static async Task Run()
    {
        Console.WriteLine("Getting adapter");
        var adapter = await BlueZManager.GetAdapterAsync("hci1");

        var timeout = TimeSpan.FromSeconds(3);
        // adapter.DeviceFound += async (sender, args) => await InterrogateDevice(args.Device, timeout);

        var address = "EA:3D:77:C8:12:75";

        Console.WriteLine($"Looking for device {address}");

        await adapter.StartDiscoveryAsync();
        // Device? device;
        // do
        // {
        //     device = await adapter.GetDeviceAsync(address);
        //     Thread.Sleep(500);
        // } while (device == null);
        //
        // await adapter.StopDiscoveryAsync();
        //
        // Console.WriteLine("Found device");

        while (true)
        {
            var devices = await adapter.GetDevicesAsync();
            foreach (var device in devices)
            {
                IDictionary<ushort, object> data;
                try
                {
                    data = await device.GetManufacturerDataAsync();
                }
                catch (DBusException)
                {
                    continue;
                }
                
                if (!data.TryGetValue(0x004C, out var manufData) || !(manufData is byte[] array))
                    continue;

                using var ms = MemoryStreamManager.GetStream(array);
                using var br = new BinaryReader(ms);

                var ibeaconSubtype = br.ReadByte();
                if (ibeaconSubtype != 2)
                    continue;
                
                var ibeaconSubtypeLength = br.ReadByte();

                var uuid = Convert.ToHexString(br.ReadBytes(16));
                var major = ReverseBytes(br.ReadUInt16());
                var minor = ReverseBytes(br.ReadUInt16());
                var txPower = br.ReadSByte();
                    
                Console.WriteLine($"{uuid} {major} {minor} {txPower}");
            }

            Console.WriteLine();
            Thread.Sleep(1000);
        }
    }
}