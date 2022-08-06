using System.Diagnostics;

namespace BleSniffer;

public class BlePcapCapture
{
    private readonly string _device;
    private readonly string _outputFile;

    public long Packets { get; private set; }
    public long OutputFileSize { get; private set; }

    public BlePcapCapture(string device, string outputFile)
    {
        _device = device;
        _outputFile = outputFile;
    }

    public void Run()
    {
        using var ble = new BleSnifferDevice(_device);
        ble.Open();

        using var outputFileStream = new FileStream(_outputFile, FileMode.Create, FileAccess.Write, FileShare.Read,
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

            Packets++;
            OutputFileSize = outputFileStream.Position;
        }
    }
}