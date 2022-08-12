using System.Diagnostics;
using System.IO.Ports;

namespace Mtk33x9Gps;

public class GpsLogger : IDisposable
{
    public string Device { get; }
    public string OutputFile { get; }

    public float Latitude { get; private set; }
    public float Longitude { get; private set; }

    private readonly SerialPort _socket;
    private readonly FileStream _outStream;

    public GpsLogger(string device, string outputFile)
    {
        Device = device;
        OutputFile = outputFile;
        _socket = new SerialPort(Device, 115200);
        _outStream = new FileStream(OutputFile, FileMode.Create, FileAccess.Write, FileShare.Read,
            4096, FileOptions.WriteThrough);
    }

    public void Run()
    {
        NormalizeSerialDataRate();

        _socket.Open();

        var bw = new BinaryWriter(_outStream);

        // Enable all output
        _socket.Write("$PMTK314,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0*28\r\n");

        var gps = new Gps(_socket.BaseStream);

        gps.NoFix += (gps1, utcHours, utcMinutes, utcSeconds) =>
        {
            bw.Write(DateTime.UtcNow.Ticks);
            bw.Write(utcHours);
            bw.Write(utcMinutes);
            bw.Write(utcSeconds);
            bw.Write(false);
        };

        gps.FixData += (gps1, data) =>
        {
            var latM = data.LatHemisphere == Hemisphere.South ? -1 : 1;
            var lonM = data.LonHemisphere == Hemisphere.West ? -1 : 1;

            Latitude = (float)(latM * (data.LatDeg + data.LatMin / 60));
            Longitude = (float)(lonM * (data.LonDeg + data.LonMin / 60));

            bw.Write(DateTime.UtcNow.Ticks);
            bw.Write(data.UtcHours);
            bw.Write(data.UtcMin);
            bw.Write(data.UtcSec);
            bw.Write(true);

            bw.Write(data.LatDeg);
            bw.Write(data.LatMin);
            bw.Write((byte)data.LatHemisphere);

            bw.Write(data.LonDeg);
            bw.Write(data.LonMin);
            bw.Write((byte)data.LonHemisphere);

            bw.Write((byte)data.Quality);
            bw.Write(data.NumSatellites);
            bw.Write(data.Hdop);

            bw.Write(data.AltGeoid);
            bw.Write(data.AltUnit);

            bw.Write(data.GeoidalSep);
            bw.Write(data.GeoidalSepUnit);
        };

        gps.Start();
    }

    private void NormalizeSerialDataRate()
    {
        _socket.Open();

        // Device default baud rate is 9600 bps
        _socket.BaudRate = 9600;

        _socket.DiscardInBuffer();

        // Set device baud rate to 115200 bps
        _socket.Write("$PMTK251,115200*1F\r\n");
        _socket.BaseStream.Flush();

        _socket.BaudRate = 115200;
        _socket.DiscardInBuffer();

        _socket.Close();
    }

    public void Dispose()
    {
        _outStream.Dispose();
        _socket.Dispose();
    }
}