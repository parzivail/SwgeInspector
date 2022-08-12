using System.Collections.Concurrent;
using System.Net.NetworkInformation;

namespace BleSniffer;

public interface IBleCapture
{
    string Device { get; }
    string OutputFile { get; }
    long TotalCapturedPackets { get; }
    long OutputFileSize { get; }

    ConcurrentDictionary<PhysicalAddress, DateTime> DeviceHeartbeat { get; }

    void Run();
}