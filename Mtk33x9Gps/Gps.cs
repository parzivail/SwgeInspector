using StringReader.NET;

namespace Mtk33x9Gps;

public class Gps
{
    private delegate void SentenceConsumer(Gps gps, StringDataReader sr);

    private static readonly Dictionary<string, Talker> Talkers = new()
    {
        ["GP"] = Talker.Gps,
        ["GL"] = Talker.Glonass,
        ["GA"] = Talker.Galileo,
        ["GN"] = Talker.Gnss
    };

    private static readonly Dictionary<char, Hemisphere> Hemispheres = new()
    {
        ['N'] = Hemisphere.North,
        ['S'] = Hemisphere.South,
        ['E'] = Hemisphere.East,
        ['W'] = Hemisphere.West
    };

    private static readonly Dictionary<Talker, SentenceConsumer> SentenceConsumers = new()
    {
        [Talker.Gps] = ConsumeGpsSentence,
        [Talker.Glonass] = ConsumeGlonassSentence,
        [Talker.Galileo] = ConsumeGalileoSentence,
        [Talker.Gnss] = ConsumeGnssSentence,
        [Talker.Device] = ConsumeDeviceSentence,
    };

    private readonly Stream _stream;
    private readonly StreamReader _reader;

    private readonly Thread _thread;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public delegate void FixDataDelegate(Gps gps, GpsFixData data);

    public delegate void NoFixDelegate(Gps gps, int utcHours, int utcMinutes, double utcSeconds);

    public event FixDataDelegate? FixData;
    public event NoFixDelegate? NoFix;

    public Gps(Stream stream)
    {
        _stream = stream;
        _reader = new StreamReader(stream);

        _thread = new Thread(BackgroundThreadLoop)
        {
            Name = "GPS Serial Thread"
        };
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public void Start()
    {
        _thread.Start(_cancellationTokenSource);
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
    }

    private static void ConsumeGpsSentence(Gps gps, StringDataReader sr)
    {
        var command = sr.TakeString(5);

        switch (command)
        {
            case "GPGSA":
                break;
            case "GPGSV":
                break;
            default:
                throw new InvalidDataException($"Unknown NMEA sentence: G{command}");
        }
    }

    private static void ConsumeGlonassSentence(Gps gps, StringDataReader sr)
    {
        var command = sr.TakeString(5);

        switch (command)
        {
            case "GLGSA":
                break;
            case "GLGSV":
                break;
            default:
                throw new InvalidDataException($"Unknown NMEA sentence: G{command}");
        }
    }

    private static void ConsumeGalileoSentence(Gps gps, StringDataReader sr)
    {
        throw new NotImplementedException();
    }

    private static void ConsumeGnssSentence(Gps gps, StringDataReader sr)
    {
        var command = sr.TakeString(5);
        sr.ReadChar(',');

        switch (command)
        {
            case "GNGGA":
            {
                // $GNGGA,190350.000,3013.0594,N,08137.3034,W,2,07,1.39,19.4,M,-31.6,M,,*74

                var utcHours = int.Parse(sr.TakeString(2));
                var utcMinutes = int.Parse(sr.TakeString(2));
                var utcSec = double.Parse(sr.TakeString(6));
                sr.ReadChar(',');
                if (sr.PeekChar() == ',')
                {
                    // No fix
                    gps.NoFix?.Invoke(gps, utcHours, utcMinutes, utcSec);
                    break;
                }

                var latDeg = int.Parse(sr.TakeString(2));
                var latMin = double.Parse(sr.TakeString(7));
                sr.ReadChar(',');
                var latHemisphere = sr.TakeChar();
                sr.ReadChar(',');
                var lonDeg = int.Parse(sr.TakeString(3));
                var lonMin = double.Parse(sr.TakeString(7));
                sr.ReadChar(',');
                var lonHemisphere = sr.TakeChar();
                sr.ReadChar(',');
                var quality = (GpsQuality)int.Parse(sr.TakeString(1));
                sr.ReadChar(',');
                var numSatellites = int.Parse(sr.TakeString(2));
                sr.ReadChar(',');
                var hdop = double.Parse(sr.TakeUntil('.') + sr.TakeString(2));
                sr.ReadChar(',');
                var altGeoid = double.Parse(sr.TakeUntil('.') + sr.TakeString(1));
                sr.ReadChar(',');
                var altUnit = sr.TakeChar();
                sr.ReadChar(',');
                var geoidalSep = double.Parse(sr.TakeUntil('.') + sr.TakeString(1));
                sr.ReadChar(',');
                var geoidalSepUnit = sr.TakeChar();

                gps.FixData?.Invoke(gps, new GpsFixData(
                    utcHours, utcMinutes, utcSec,
                    latDeg, latMin, Hemispheres[latHemisphere],
                    lonDeg, lonMin, Hemispheres[lonHemisphere],
                    quality, numSatellites, hdop,
                    altGeoid, altUnit,
                    geoidalSep, geoidalSepUnit
                ));

                break;
            }
            case "GNGLL":
                break;
            case "GNRMC":
                break;
            case "GNVTG":
                break;
            default:
                throw new InvalidDataException($"Unknown NMEA sentence: G{command}");
        }
    }

    private static void ConsumeDeviceSentence(Gps gps, StringDataReader sr)
    {
    }

    private void ConsumeSentence(string sentence)
    {
        var sr = new StringDataReader(sentence);

        sr.ReadChar('$');
        var talkerId = sr.PeekString(2);

        var talker = Talkers.ContainsKey(talkerId) ? Talkers[talkerId] : Talker.Device;

        if (!SentenceConsumers.ContainsKey(talker))
            throw new InvalidDataException($"No sentence consumer for talker {talker}");

        SentenceConsumers[talker].Invoke(this, sr);
    }

    private void BackgroundThreadLoop(object? param)
    {
        if (param is not CancellationTokenSource tokenSource)
            throw new ArgumentException("Param must be CancellationTokenSource", nameof(param));

        while (!tokenSource.IsCancellationRequested)
        {
            var line = _reader.ReadLine();
            if (line == null)
                break;
            ConsumeSentence(line);
        }
    }
}