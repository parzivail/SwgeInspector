using System.Collections.Concurrent;
using System.Text;
using DotnetBleServer.Advertisements;
using DotnetBleServer.Core;
using DotnetBleServer.Gatt;
using DotnetBleServer.Gatt.Description;
using HashtagChris.DotNetBlueZ;
using HashtagChris.DotNetBlueZ.Extensions;
using Microsoft.IO;
using Tmds.DBus;

namespace BleBeacon;

public class SampleAdvertisement
{
    public static async Task RegisterSampleAdvertisement(ServerContext serverContext)
    {
        var advertisementProperties = new AdvertisementProperties
        {
            Type = "peripheral",
            LocalName = "Stardust",
            ManufacturerData = new Dictionary<ushort, object>()
            {
                [0x004C] = Convert.FromHexString("0215DEADBEEFDEADBEEFDEADBEEFDEADBEEFA2B8270FB3")
            }
        };

        await new AdvertisingManager(serverContext).CreateAdvertisement(advertisementProperties, "/org/bluez/hci1");
    }
}

internal class SampleGattApplication
{
    public static async Task RegisterGattApplication(ServerContext serverContext)
    {
        var gattServiceDescription = new GattServiceDescription
        {
            UUID = "12345678-1234-5678-1234-56789abcdef0",
            Primary = true
        };

        var gattCharacteristicDescription = new GattCharacteristicDescription
        {
            CharacteristicSource = new ExampleCharacteristicSource(),
            UUID = "12345678-1234-5678-1234-56789abcdef1",
            Flags = CharacteristicFlags.Read | CharacteristicFlags.Write | CharacteristicFlags.WritableAuxiliaries
        };
        var gattDescriptorDescription = new GattDescriptorDescription
        {
            Value = new[] { (byte)'t' },
            UUID = "12345678-1234-5678-1234-56789abcdef2",
            Flags = new[] { "read", "write" }
        };
        var gab = new GattApplicationBuilder();
        gab
            .AddService(gattServiceDescription)
            .WithCharacteristic(gattCharacteristicDescription, Array.Empty<GattDescriptorDescription>());

        await new GattApplicationManager(serverContext).RegisterGattApplication(gab.BuildServiceDescriptions(),
            "/org/bluez/hci1");
    }

    internal class ExampleCharacteristicSource : ICharacteristicSource
    {
        public Task WriteValueAsync(byte[] value)
        {
            Console.WriteLine("Writing value");
            return Task.Run(() => Console.WriteLine(Encoding.ASCII.GetChars(value)));
        }

        public Task<byte[]> ReadValueAsync()
        {
            Console.WriteLine("Reading value");
            return Task.FromResult(BitConverter.GetBytes(DateTime.Now.Ticks));
        }
    }
}

public class BleDevice
{
    public static async Task Run()
    {
        using var serverContext = new ServerContext();

        await serverContext.Connect();
        await SampleAdvertisement.RegisterSampleAdvertisement(serverContext);
        await SampleGattApplication.RegisterGattApplication(serverContext);

        await Task.Delay(-1);
    }
}