using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace BleSniffer;

public class BlePcapCapture
{
    public string Device { get; }
    public string OutputFile { get; }

    public long TotalCapturedPackets { get; private set; }
    public long OutputFileSize { get; private set; }

    public readonly ConcurrentDictionary<PhysicalAddress, DateTime> DeviceHeartbeat = new();

    public BlePcapCapture(string device, string outputFile)
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
        var pcap = new PcapFile(outputFileStream);

        pcap.WriteHeader();

        var sw = new Stopwatch();
        sw.Start();

        while (ble.ReadPacket() is { } packet)
        {
            if (packet.Length < 24 + 6)
                continue;

            pcap.WriteBlePacket(sw.Elapsed, packet);

            var macAddr = new PhysicalAddress(new[]
            {
                packet[28],
                packet[27],
                packet[26],
                packet[25],
                packet[24],
                packet[23],
            });
            DeviceHeartbeat[macAddr] = DateTime.UtcNow;

            TotalCapturedPackets++;
            OutputFileSize = outputFileStream.Position;
        }
    }
}