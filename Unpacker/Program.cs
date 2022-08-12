using Mtk33x9Gps;

namespace Unpacker;

public class Program
{
    public static void Main(string[] args)
    {
        using var file = File.OpenRead(args[0]);
        var br = new BinaryReader(file);

        while (file.Position < file.Length)
        {
            var ticks = br.ReadInt64();

            var utcHour = br.ReadInt32();
            var utcMin = br.ReadInt32();
            var utcSec = br.ReadDouble();

            var hasFix = br.ReadBoolean();

            if (hasFix)
            {
                var latDeg = br.ReadInt32();
                var latMin = br.ReadDouble();
                var latHemi = (Hemisphere)br.ReadByte();

                var lonDeg = br.ReadInt32();
                var lonMin = br.ReadDouble();
                var lonHemi = (Hemisphere)br.ReadByte();

                var quality = (GpsQuality)br.ReadByte();
                var numSatellites = br.ReadInt32();
                var hdop = br.ReadDouble();

                var altGeoid = br.ReadDouble();
                var altUnit = br.ReadChar();

                var geoidalSep = br.ReadDouble();
                var geoidalSepUnit = br.ReadChar();
            }

            Console.WriteLine(
                $"DEVICE[{new DateTime(ticks, DateTimeKind.Utc):T}] ~ GPS[{utcHour:00}:{utcMin:00}:{utcSec:00.000000}] [{(hasFix ? "" : "no ")}fix]");
        }
    }
}