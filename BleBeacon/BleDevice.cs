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

public class BleDevice
{
    private static readonly RecyclableMemoryStreamManager _memoryStreamManager = new();
    private static readonly ValueBackedCharacteristicSource _characteristic = new();

    public static async Task RegisterGattApplication(ServerContext serverContext)
    {
        var gattServiceDescription = new GattServiceDescription
        {
            UUID = "a720b426-e3fd-48ed-9861-a7b6ff000000",
            Primary = true
        };

        var gattCharacteristicDescription = new GattCharacteristicDescription
        {
            CharacteristicSource = _characteristic,
            UUID = "a720b426-e3fd-48ed-9861-a7b6ff000001",
            Flags = CharacteristicFlags.Read | CharacteristicFlags.Write | CharacteristicFlags.WritableAuxiliaries |
                    CharacteristicFlags.Notify
        };
        var gab = new GattApplicationBuilder();
        gab
            .AddService(gattServiceDescription)
            .WithCharacteristic(gattCharacteristicDescription, Array.Empty<GattDescriptorDescription>());

        await new GattApplicationManager(serverContext)
            .RegisterGattApplication("/org/bluez/stardust/gatt", gab.BuildServiceDescriptions());
    }

    public static async Task Run()
    {
        var adapter = await GetAdapter(PhysicalAddress.Parse("00:E0:4C:2A:46:52"));
        if (adapter == null)
            throw new InvalidOperationException("Adapter not found");

        using var serverContext = new ServerContext(adapter.ObjectPath);
        await serverContext.Connect();

        await new AdvertisingManager(serverContext).CreateAdvertisement(
            "/org/bluez/stardust/advert",
            new AdvertisementProperties
            {
                Type = "peripheral",
                LocalName = "Stardust",
                ManufacturerData = new Dictionary<ushort, object>
                {
                    [0x1001] = Encoding.ASCII.GetBytes("STARDUST")
                }
            }
        );

        await RegisterGattApplication(serverContext);

        var random = new Random();
        var timer = new Timer(async state =>
        {
            using var stream = _memoryStreamManager.GetStream();
            var bw = new BinaryWriter(stream);

            bw.Write(random.NextSingle()); // Latitude
            bw.Write(random.NextSingle()); // Longitude
            bw.Write((ushort)random.Next()); // Num Active Beacons
            bw.Write((ushort)random.Next()); // Num Total Beacons
            bw.Write(random.NextInt64()); // Num Total Packets

            await _characteristic.WriteValueAsync(stream.ToArray(), false);
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

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