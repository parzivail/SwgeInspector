using System.Collections.Concurrent;
using System.Net.NetworkInformation;
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

        await new AdvertisingManager(serverContext)
            .CreateAdvertisement("/org/bluez/example/advertisement0", advertisementProperties);
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

        var c = new ValueBackedCharacteristicSource();
        c.ValueSet += (value, response, didNotify) => { Console.WriteLine(Encoding.UTF8.GetString(value)); };

        var gattCharacteristicDescription = new GattCharacteristicDescription
        {
            CharacteristicSource = c,
            UUID = "12345678-1234-5678-1234-56789abcdef1",
            Flags = CharacteristicFlags.Read | CharacteristicFlags.Write | CharacteristicFlags.WritableAuxiliaries |
                    CharacteristicFlags.Notify
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

        await new GattApplicationManager(serverContext)
            .RegisterGattApplication("/org/bluez/example/gattapp0", gab.BuildServiceDescriptions());
    }
}

public class BleDevice
{
    public static async Task Run()
    {
        var adapter = await GetAdapter(PhysicalAddress.Parse("00:E0:4C:2A:46:52"));
        if (adapter == null)
            throw new InvalidOperationException("Adapter not found");

        using var serverContext = new ServerContext(adapter.ObjectPath);
        await serverContext.Connect();
        await SampleAdvertisement.RegisterSampleAdvertisement(serverContext);
        await SampleGattApplication.RegisterGattApplication(serverContext);

        await Task.Delay(-1);
    }

    private static async Task<Adapter?> GetAdapter(PhysicalAddress address)
    {
        foreach (var adapter in await BlueZManager.GetAdaptersAsync())
        {
            if (address.Equals(PhysicalAddress.Parse(await adapter.GetAddressAsync())))
                return adapter;
        }

        return null;
    }
}