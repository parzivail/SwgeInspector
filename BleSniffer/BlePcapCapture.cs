using System.Diagnostics;

namespace BleSniffer;

public class BlePcapCapture
{
    public string Device { get; }
    public string OutputFile { get; }

    public long TotalCapturedPackets { get; private set; }
    public long OutputFileSize { get; private set; }

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
            if (packet.Length < 23)
                continue;

            pcap.WriteBlePacket(sw.Elapsed, packet);

            TotalCapturedPackets++;
            OutputFileSize = outputFileStream.Position;
        }
    }
}