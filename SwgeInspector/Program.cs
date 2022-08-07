using System.IO.Ports;
using System.Net.NetworkInformation;
using BleSniffer;
using Mtk33x9Gps;
using NavXMxp;

namespace SwgeInspector;

public class Program : IDisposable
{
    private const string DeviceGps = "/dev/tty_gps";
    private const string DeviceBleSniffer = "/dev/tty_ble";
    private const string DeviceImu = "/dev/tty_imu";

    private static readonly PhysicalAddress DeviceBluetooth = PhysicalAddress.Parse("00:E0:4C:2A:46:52");
    // private static readonly PhysicalAddress DeviceBluetooth = PhysicalAddress.Parse("DC:A6:32:35:51:86");

    private readonly long _startTicks;
    private readonly BleServer _bleServer;
    private readonly BlePcapCapture _blePcapCapture;
    private readonly GpsLogger _gpsLogger;
    private readonly NavXLogger _navxLogger;

    private Program()
    {
        _startTicks = DateTime.UtcNow.Ticks;

        // _blePcapCapture = new BlePcapCapture(DeviceBleSniffer, $"ble_sniffer_{_startTicks}.pcap");
        // _gpsLogger = new GpsLogger(DeviceGps, $"gps_{_startTicks}.bin");
        // _navxLogger = new NavXLogger(DeviceImu, $"imu_{_startTicks}.bin");
        _blePcapCapture = new BlePcapCapture(DeviceBleSniffer, $"/dev/null");
        _gpsLogger = new GpsLogger(DeviceGps, $"/dev/null");
        _navxLogger = new NavXLogger(DeviceImu, $"/dev/null");

        _bleServer = new BleServer(DeviceBluetooth, WriteGattData);
    }

    private void WriteGattData(BinaryWriter bw)
    {
        bw.Write(_gpsLogger.Latitude); // Latitude
        bw.Write(_gpsLogger.Longitude); // Longitude
        bw.Write((ushort)0); // Num Active Beacons
        bw.Write((ushort)0); // Num Total Beacons
        bw.Write(_blePcapCapture.TotalCapturedPackets); // Num Total Packets
    }

    private async Task Run(string[] args)
    {
        // Start BLE capture
        Console.WriteLine($"Starting BLE capture ({DeviceBleSniffer} => {_blePcapCapture.OutputFile})...");
        new Thread(_blePcapCapture.Run)
        {
            Name = "BLE Capture Thread"
        }.Start();

        // Start GPS logger
        Console.WriteLine($"Starting GPS logger ({DeviceGps} => {_gpsLogger.OutputFile})...");
        _gpsLogger.Run();

        // Start IMU logger
        Console.WriteLine($"Starting NavX logger ({DeviceImu} => {_navxLogger.OutputFile})...");
        _navxLogger.Run();

        // Start BLE server
        Console.WriteLine($"Starting BLE server (adapter {DeviceBluetooth})...");
        await _bleServer.Start();

        await Task.Delay(-1);
    }

    public static async Task Main(string[] args)
    {
        var program = new Program();
        await program.Run(args);
    }

    public void Dispose()
    {
        _bleServer.Dispose();
        _gpsLogger.Dispose();
        _navxLogger.Dispose();
    }
}