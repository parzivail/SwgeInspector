using System.IO.Ports;
using System.Net.NetworkInformation;
using BleSniffer;
using Mtk33x9Gps;
using NavXMxp;

namespace SwgeInspector;

public class Program
{
    private const string DeviceGps = "/dev/tty_gps";
    private const string DeviceBleSniffer = "/dev/tty_ble";
    private const string DeviceImu = "/dev/tty_imu";

    private static readonly PhysicalAddress DeviceBluetooth = PhysicalAddress.Parse("00:E0:4C:2A:46:52");
    // private static readonly PhysicalAddress DeviceBluetooth = PhysicalAddress.Parse("DC:A6:32:35:51:86");

    private readonly long _startTicks;
    private readonly BleServer _bleServer;
    private readonly BlePcapCapture _blePcapCapture;

    private Program()
    {
        _startTicks = DateTime.UtcNow.Ticks;
        _blePcapCapture = new BlePcapCapture(DeviceBleSniffer, $"ble_sniffer_{_startTicks}.pcap");
        _bleServer = new BleServer(DeviceBluetooth, WriteGattData);
    }

    private void WriteGattData(BinaryWriter bw)
    {
        bw.Write(0f); // Latitude
        bw.Write(0f); // Longitude
        bw.Write((ushort)0); // Num Active Beacons
        bw.Write((ushort)0); // Num Total Beacons
        bw.Write(_blePcapCapture.TotalCapturedPackets); // Num Total Packets
    }

    private async Task Run(string[] args)
    {
        // Start BLE capture
        Console.WriteLine($"Starting BLE capture ({DeviceBleSniffer} => {_blePcapCapture.OutputFile})...");
        new Thread(_blePcapCapture.Run).Start();

        // Start BLE server
        Console.WriteLine($"Starting BLE server (adapter {DeviceBluetooth})...");
        await _bleServer.Start();

        await Task.Delay(-1);
    }

    public static async Task Main(string[] args)
    {
        // await BleDevice.Run();

        var program = new Program();
        await program.Run(args);

        // NormalizeSerialDataRate(args[0]);
        //
        // using var socket = new SerialPort(args[0], 115200);
        // socket.Open();
        //
        // socket.Write("$PMTK314,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0*28\r\n");
        //
        // var gps = new Gps(socket.BaseStream);
        // gps.FixData += (gps1, data) =>
        // {
        // 	var latM = data.LatHemisphere == Hemisphere.South ? -1 : 1;
        // 	var lonM = data.LonHemisphere == Hemisphere.West ? -1 : 1;
        // 	Console.WriteLine($"{latM * (data.LatDeg + data.LatMin / 60)},{lonM * (data.LonDeg + data.LonMin / 60)}");
        // };
        //
        // gps.Start();

        // using var socket = new SerialPort("COM6", 57600);
        // socket.Open();
        //
        // var navx = new NavX(socket.BaseStream);
        // navx.Start();
    }

    private static void NormalizeSerialDataRate(string port)
    {
        using var socket = new SerialPort(port, 115200);
        socket.Open();

        var buffer = new byte[512];
        socket.Read(buffer, 0, buffer.Length);
        if (buffer.Any(b => b > 127))
        {
            // Device default baud rate is 9600 bps
            socket.BaudRate = 9600;

            socket.DiscardInBuffer();

            // Set device baud rate to 115200 bps
            socket.Write("$PMTK251,115200*1F\r\n");
            socket.BaseStream.Flush();

            socket.BaudRate = 115200;
            socket.DiscardInBuffer();

            Console.WriteLine("Update rate to 115200");
        }

        socket.Close();
    }
}