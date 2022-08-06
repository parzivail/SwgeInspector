using System.IO.Ports;
using BleBeacon;
using BleSniffer;
using Mtk33x9Gps;
using NavXMxp;

namespace SwgeInspector;

public class Program
{
    private const string DEVICE_GPS = "/dev/tty_gps";
    private const string DEVICE_BLE_SNIFFER = "/dev/tty_ble";
    private const string DEVICE_IMU = "/dev/tty_imu";

    public static async Task Main(string[] args)
    {
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

        // await BleDevice.Run();

        var nowTicks = DateTime.UtcNow.Ticks;

        // Start BLE capture
        var pcapFile = $"ble_sniffer_{nowTicks}.pcap";
        Console.WriteLine($"Starting BLE capture ({pcapFile})...");

        var bleSniffer = new BlePcapCapture(DEVICE_BLE_SNIFFER, pcapFile);
        new Thread(bleSniffer.Run).Start();

        var lastPacketCount = 0L;
        var timer = new Timer(async state =>
        {
            var packetCount = bleSniffer.Packets;
            Console.WriteLine(
                $"{packetCount} (+{packetCount - lastPacketCount}) [{bleSniffer.OutputFileSize:N0} bytes]");

            lastPacketCount = packetCount;
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

        await Task.Delay(-1);
    }

    private static void StartBleCapture()
    {
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