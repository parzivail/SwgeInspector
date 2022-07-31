namespace BleSniffer;

public class PcapFile
{
    public Stream BaseStream { get; }
    private readonly BinaryWriter _writer;

    public PcapFile(Stream stream)
    {
        BaseStream = stream;
        _writer = new BinaryWriter(stream);
    }

    public void WriteHeader()
    {
        _writer.Write((uint)0xa1b2c3d4); // PCAP magic number
        _writer.Write((ushort)2); // PCAP major version
        _writer.Write((ushort)4); // PCAP minor version
        _writer.Write((uint)0); // Reserved
        _writer.Write((uint)0); // Reserved
        _writer.Write((uint)0x0000ffff); // Max length of capture frame
        _writer.Write((uint)272); // Nordic BLE link type
    }

    public void WriteBlePacket(TimeSpan timestamp, byte[] packet)
    {
        var seconds = (int)timestamp.TotalSeconds;
        var offsetMicroseconds = (int)((timestamp.TotalMilliseconds % 1000) * 1000);
        _writer.Write(seconds);
        _writer.Write(offsetMicroseconds);
        _writer.Write(packet.Length);
        _writer.Write(packet.Length);
        
        _writer.Write((byte)0x04);
            
        packet[1]--;
        _writer.Write(packet[..22]);
        _writer.Write(packet[23..]);
    }
}