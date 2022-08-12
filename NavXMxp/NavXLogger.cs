using System.IO.Ports;

namespace NavXMxp;

public class NavXLogger : IDisposable
{
    public string Device { get; }
    public string OutputFile { get; }

    private readonly SerialPort _socket;
    private readonly FileStream _outStream;

    public NavXLogger(string device, string outputFile)
    {
        Device = device;
        OutputFile = outputFile;
        _socket = new SerialPort(Device, 57600);
        _outStream = new FileStream(OutputFile, FileMode.Create, FileAccess.Write, FileShare.Read,
            4096, FileOptions.WriteThrough);
    }

    public void Run()
    {
        _socket.Open();

        var bw = new BinaryWriter(_outStream);

        var navx = new NavX(_socket.BaseStream);

        navx.AhrsUpdate += (sender, data) =>
        {
            bw.Write(DateTime.UtcNow.Ticks);
            bw.Write(data.Yaw);
            bw.Write(data.Pitch);
            bw.Write(data.Roll);
            bw.Write(data.CompassHeading);
            bw.Write(data.Altitude);
            bw.Write(data.FusedHeading);
            bw.Write(data.LinearAccelX);
            bw.Write(data.LinearAccelY);
            bw.Write(data.LinearAccelZ);
            bw.Write(data.VelX);
            bw.Write(data.VelY);
            bw.Write(data.VelZ);
            bw.Write(data.DispX);
            bw.Write(data.DispY);
            bw.Write(data.DispZ);
            bw.Write(data.MpuTemp);
            bw.Write(data.QuatW);
            bw.Write(data.QuatX);
            bw.Write(data.QuatY);
            bw.Write(data.QuatZ);
            bw.Write((byte)data.OpStatus);
            bw.Write((byte)data.SensorStatus);
            bw.Write((byte)data.CalStatus);
            bw.Write((byte)data.SelftestStatus);
        };

        navx.Start();
    }

    public void Dispose()
    {
        _outStream.Dispose();
        _socket.Dispose();
    }
}