using System.Net.NetworkInformation;
using System.Text;
using DotnetBleServer.Advertisements;
using DotnetBleServer.Core;
using DotnetBleServer.Gatt;
using DotnetBleServer.Gatt.Description;
using HashtagChris.DotNetBlueZ;
using Microsoft.IO;

namespace SwgeInspector;

public class BleServer : IDisposable
{
    private readonly PhysicalAddress _adapter;
    private readonly Action<BinaryWriter> _dataWriter;
    private readonly RecyclableMemoryStreamManager _memoryStreamManager = new();
    private readonly ValueBackedCharacteristicSource _characteristic = new();

    private ServerContext? _serverContext;
    private Timer? _timer;

    public BleServer(PhysicalAddress adapter, Action<BinaryWriter> dataWriter)
    {
        _adapter = adapter;
        _dataWriter = dataWriter;
    }

    public async Task RegisterGattApplication(ServerContext serverContext)
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

    public async Task Start()
    {
        if (_serverContext == null)
        {
            var adapter = await GetAdapter(_adapter);
            if (adapter == null)
                throw new InvalidOperationException("Adapter not found");

            _serverContext = new ServerContext(adapter.ObjectPath);
        }

        await _serverContext.Connect();

        await new AdvertisingManager(_serverContext).CreateAdvertisement(
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

        await RegisterGattApplication(_serverContext);

        if (_timer != null)
            await _timer.DisposeAsync();

        _timer = new Timer(async state =>
        {
            using var stream = _memoryStreamManager.GetStream();
            var bw = new BinaryWriter(stream);
            _dataWriter.Invoke(bw);

            await _characteristic.WriteValueAsync(stream.ToArray(), false);
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    private async Task<Adapter?> GetAdapter(PhysicalAddress address)
    {
        foreach (var adapter in await BlueZManager.GetAdaptersAsync())
        {
            if (address.Equals(PhysicalAddress.Parse(await adapter.GetAddressAsync())))
                return adapter;
        }

        return null;
    }

    public void Dispose()
    {
        _serverContext?.Dispose();
    }
}