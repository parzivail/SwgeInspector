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

        var gattCharacteristicDescription = new GattCharacteristicDescription
        {
            CharacteristicSource = new ExampleCharacteristicSource(),
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

    internal class ExampleCharacteristicSource : ICharacteristicSource
    {
        const bool NotificationAvoiding = false;

        public override Task WriteValueAsync(byte[] value, bool response)
        {
            // The client sent us some data!

            Console.WriteLine(Encoding.ASCII.GetChars(value));

            if (NotificationAvoiding)
            {
                // We get our properties, so that we can directly write the value
                var props = Properties.GetAllAsync().Result;

                Console.WriteLine("Writing value");
                // Assign the value directly, since we don't want to notify the client of what it sent us - it should probably know that!
                props.Value = value;
                return Task.CompletedTask;
            }

            Console.WriteLine("Writing value");
            // Let's write it to our characteristic's value! (This will trigger a notification, if they're turned on)
            return Properties.SetAsync("Value", value);
        }

        public override Task<byte[]> ReadValueAsync()
        {
            // The client asked for our value
            Console.WriteLine("Reading value");

            // Get our value, in the form of a byte array task
            var props = Properties.GetAsync<byte[]>("Value");

            // Return our task, mission accomplished!
            return props;
        }

        public override Task StartNotifyAsync()
        {
            // Our client has requested notifications!
            Console.WriteLine("Starting to Notify");

            // Get our properties class
            var props = Properties.GetAllAsync().Result;
            // Set notifying to true, so we'll start sending replies on Properties.SetAsync("Value", [value])
            props.Notifying = true;
            // Return something, since we have to.
            return Task.CompletedTask;
        }

        public override Task StopNotifyAsync()
        {
            // Our client has asked us to stop talking to it :(
            Console.WriteLine("Stopping notifications..");

            // Get our properties class again
            var props = Properties.GetAllAsync().Result;
            // Set notifying to false. Now Properties.SetAsync("Value", [value]) won't send notifications anymore.
            props.Notifying = false;
            // Return a completed task so we can leave.
            return Task.CompletedTask;
        }

        public override Task ConfirmAsync()
        {
            // The client told us that it got the packet. That's very polite!
            // Indicate characteristics will have their notifications confirmed by the client, which will trigger this function.
            return Task.CompletedTask;
        }
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