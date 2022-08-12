using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace BleSniffer;

public class BleDirectCapture : IBleCapture
{
    public string Device { get; }
    public string OutputFile { get; }

    public long TotalCapturedPackets { get; private set; }
    public long OutputFileSize { get; private set; }

    private readonly ConcurrentDictionary<PhysicalAddress, DateTime> _deviceHeartbeat = new();
    public ConcurrentDictionary<PhysicalAddress, DateTime> DeviceHeartbeat => _deviceHeartbeat;

    public BleDirectCapture(string device, string outputFile)
    {
        Device = device;
        OutputFile = outputFile;
    }

    public void Run()
    {
        using var ble = new BleSnifferDevice(Device);
        ble.Open();

        using var outputFileStream = new FileStream(OutputFile, FileMode.Create, FileAccess.Write, FileShare.Read,
            4096, FileOptions.WriteThrough);
        var bw = new BinaryWriter(outputFileStream);

        while (ble.ReadPacket() is { } packet)
        {
            var utcNow = DateTime.UtcNow;

            bw.Write(utcNow.Ticks);
            bw.Write(packet.Length);
            bw.Write(packet);

            OutputFileSize = outputFileStream.Position;

            if (packet.Length < 24 + 6)
                continue;

            var macAddr = new PhysicalAddress(new[]
            {
                packet[28],
                packet[27],
                packet[26],
                packet[25],
                packet[24],
                packet[23],
            });
            _deviceHeartbeat[macAddr] = utcNow;

            TotalCapturedPackets++;
        }
    }
}