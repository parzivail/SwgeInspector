using System.Net.NetworkInformation;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using BleSniffer;
using Microsoft.IO;
using Mtk33x9Gps;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Unpacker;

public record TimestampedObject<T>(DateTime Timestamp, T Data) : IComparable<TimestampedObject<T>>
{
    public int CompareTo(TimestampedObject<T>? y)
    {
        return Timestamp.CompareTo(y.Timestamp);
    }
}

public record GpsAndTimestampedObject<T>(DateTime Timestamp, GpsPosition Position, T Data) : TimestampedObject<T>(Timestamp, Data);

public record GpsPosition(double Latitude, double Longitude, int SatelliteCount, TimeOnly UtcTime);

public class GpsInterpolator
{
    public TimestampedObject<GpsPosition>[] Data { get; }

    public GpsInterpolator(TimestampedObject<GpsPosition>[] data)
    {
        Data = data;
    }

    public GpsPosition GetPosition(DateTime time)
    {
        if (time < Data[0].Timestamp)
            return Data[0].Data;

        if (time > Data[^1].Timestamp)
            return Data[^1].Data;

        var index = Array.BinarySearch(Data,
            new TimestampedObject<GpsPosition>(time, new GpsPosition(0, 0, 0, TimeOnly.MinValue)));
        if (index >= 0)
            return Data[index].Data;

        index = ~index;
        var far = Data[index];
        var near = Data[index - 1];

        var t = (time - near.Timestamp) / (far.Timestamp - near.Timestamp);
        var dt = far.Timestamp - near.Timestamp;
        if (dt.TotalSeconds > 8)
            t = Math.Round(t);

        return new GpsPosition(
            Lerp(t, near.Data.Latitude, far.Data.Latitude),
            Lerp(t, near.Data.Longitude, far.Data.Longitude),
            Math.Min(near.Data.SatelliteCount, far.Data.SatelliteCount),
            Lerp(t, near.Data.UtcTime, far.Data.UtcTime)
        );
    }

    private static double Lerp(double t, double a, double b)
    {
        return a * (1 - t) + b * t;
    }

    private static TimeOnly Lerp(double t, TimeOnly a, TimeOnly b)
    {
        return a.Add((b - a) * t);
    }
}

public record BeaconDataEntry(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("installationsUnlocked")]
    IReadOnlyList<string> InstallationsUnlocked,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("playExperienceNameDLR")]
    string PlayExperienceNameDLR,
    [property: JsonPropertyName("playExperienceNameWDW")]
    string PlayExperienceNameWDW,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("waypointId")]
    int WaypointId
);

public record MapLocationDataEntry(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("x")] double X,
    [property: JsonPropertyName("y")] double Y
);

public record MapObjectDataEntry(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("installationId")]
    string InstallationId,
    [property: JsonPropertyName("locationId")]
    string LocationId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("priority")]
    string Priority,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("mapSizeProportion")]
    double? MapSizeProportion,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("beaconId")]
    string BeaconId,
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("jobIds")] IReadOnlyList<string> JobIds,
    [property: JsonPropertyName("locationKey")]
    string LocationKey,
    [property: JsonPropertyName("installationIds")]
    IReadOnlyList<string> InstallationIds
);

public enum AppDataOpcode : byte
{
    Connect = 0x01,
    Data = 0x02,
    Disconnect = 0x03
}

public record InteropPacket(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("timestamp")]
    DateTime Timestamp,
    [property: JsonPropertyName("payload")]
    JsonObject Payload
);

public record PdpRequest(
    [property: JsonPropertyName("requestType")]
    string? RequestType,
    [property: JsonPropertyName("payload")]
    object? Payload
);

public record PdpBeaconPayload(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("experienceName")]
    string? ExperienceName,
    [property: JsonPropertyName("rssi")] int Rssi
);

public record MapExtent(double Left, double Top, double Right, double Bottom);

public class Program
{
    private static readonly MapExtent OsmSwge = new(-81.56299, 28.35546, -81.55967, 28.35309);
    private static readonly MapExtent OsmDroidbath = new(-81.56247, 28.35465, -81.56209, 28.35435);
    private static readonly MapExtent OsmMarket = new(-81.56169, 28.35452, -81.56105, 28.35421);
    private static readonly MapExtent Wdw = new(-81.56279, 28.35570, -81.55983, 28.35334);

    private static double Remap(double t, double a, double b, double x, double y)
    {
        return (t - a) / (b - a) * (y - x) + x;
    }

    private static (double X, double Y) WdwMapPointToOsmMapPoint(double x, double y, MapExtent target)
    {
        var wdwLon = Remap(x, 0, 1, Wdw.Left, Wdw.Right);
        var wdwLat = Remap(y, 0, 1, Wdw.Top, Wdw.Bottom);
        return LatLonToOsmMapPoint(wdwLon, wdwLat, target);
    }

    private static (double X, double Y) LatLonToOsmMapPoint(double wdwLon, double wdwLat, MapExtent target)
    {
        return (
            Remap(wdwLon, target.Left, target.Right, 0, 1),
            Remap(wdwLat, target.Top, target.Bottom, 0, 1)
        );
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("Loading GPS data");
        // Load GPS data
        using var gpsStream = File.OpenRead("/home/cnewman/Documents/gps_637956226262456597.bin");
        var gps = new BinaryReader(gpsStream);
        var gpsPings = new List<TimestampedObject<GpsPosition>>();

        try
        {
            while (true)
            {
                var timestamp = new DateTime(gps.ReadInt64());
                var sentence = gps.ReadString();
                var parsedSentence = ParseSentence(sentence);
                if (parsedSentence != null)
                    gpsPings.Add(new TimestampedObject<GpsPosition>(timestamp, parsedSentence));
            }
        }
        catch (EndOfStreamException)
        {
            // ignored
        }

        Console.WriteLine("Synchronizing system timestamps");
        // Synchronize GPS and system time
        var trueDate = new DateOnly(2022, 8, 22);
        var trueTime = gpsPings[0].Data.UtcTime;
        var trueTimestamp = trueDate.ToDateTime(trueTime, DateTimeKind.Utc);
        var localTimestamp = gpsPings[0].Timestamp;
        var timeOffset = trueTimestamp - localTimestamp;

        var gpsInterpolator =
            new GpsInterpolator(gpsPings.Select(o => o with { Timestamp = o.Timestamp + timeOffset }).ToArray());

        Console.WriteLine("Loading BLE data");
        // Load BLE data
        using var bleStream =
            File.OpenRead("/home/cnewman/Documents/ble_sniffer_637956226262456597.bin");
        var ble = new BinaryReader(bleStream);
        var rawPackets = new List<TimestampedObject<byte[]>>();

        try
        {
            while (true)
            {
                var timestamp = new DateTime(ble.ReadInt64()) + timeOffset;
                var packetLength = ble.ReadInt32();
                var data = ble.ReadBytes(packetLength);

                rawPackets.Add(new TimestampedObject<byte[]>(timestamp, data));
            }
        }
        catch (EndOfStreamException)
        {
            // ignored
        }

        Console.WriteLine("Creating PCAP packets");
        // Create PCAP-compatible packets
        var packets = new List<GpsAndTimestampedObject<byte[]>>();

        var manager = new RecyclableMemoryStreamManager();
        foreach (var (time, data) in rawPackets)
        {
            if (data.Length <= 35)
                continue;

            var gpsLocation = gpsInterpolator.GetPosition(time);

            var stream = manager.GetStream();
            var bw = new BinaryWriter(stream);

            bw.Write((byte)0x04);
            data[1]--;
            bw.Write(data[..22]);
            bw.Write(data[23..]);

            var packet = stream.ToArray();

            const int flagCrcOk = 0b1;

            var flags = packet[8];
            if ((flags & flagCrcOk) == 0)
                continue;

            packets.Add(new GpsAndTimestampedObject<byte[]>(time, gpsLocation, packet));
        }

        Console.WriteLine("Loading interop data");
        // Load interop packet data
        using var fs = File.OpenRead("/home/cnewman/Documents/pdp_interop_packets.bin");
        var br = new BinaryReader(fs);

        var interopPacketData = new List<GpsAndTimestampedObject<string>>();

        try
        {
            while (true)
            {
                var timestamp = new DateTime(br.ReadInt64(), DateTimeKind.Utc);
                var opcode = (AppDataOpcode)br.ReadByte();
                if (opcode == AppDataOpcode.Data)
                {
                    var packet = JsonSerializer.Deserialize<InteropPacket>(br.ReadString());
                    var data = packet?.Payload.Deserialize<PdpRequest>();
                    if (data?.Payload is not JsonElement { ValueKind: JsonValueKind.Object } payload)
                        continue;

                    if (data is not { RequestType: "SHOW_CONTROL_EFFECT_IN_RANGE" or "SHOW_CONTROL_EFFECT_READY" or "BEACON_GAME_ADVANCE_IN_RANGE" }) continue;

                    var beaconPayload = payload.Deserialize<PdpBeaconPayload>();
                    if (beaconPayload.Rssi > -75)
                        interopPacketData.Add(new GpsAndTimestampedObject<string>(timestamp, gpsInterpolator.GetPosition(timestamp),
                            beaconPayload.Name ?? beaconPayload.ExperienceName));
                }
            }
        }
        catch (EndOfStreamException)
        {
            // ignored
        }

        Console.WriteLine("Loading Datapad data");
        // Load Datapad beacon definitions
        var beaconData = JsonSerializer.Deserialize<Dictionary<string, BeaconDataEntry>>(
            File.ReadAllText("/home/cnewman/Documents/starWarsGalaxysEdgeGame_91_beacon-data.json")
        ).Select(pair => pair.Value).ToDictionary(entry => entry.Id, entry => entry);
        var mapLocationData = JsonSerializer.Deserialize<Dictionary<string, MapLocationDataEntry>>(
            File.ReadAllText("/home/cnewman/Documents/starWarsGalaxysEdgeGame_91_map-location-data-wdw.json")
        ).Select(pair => pair.Value).ToDictionary(entry => entry.Id, entry => entry);
        var mapObjectData = JsonSerializer.Deserialize<Dictionary<string, MapObjectDataEntry>>(
            File.ReadAllText("/home/cnewman/Documents/starWarsGalaxysEdgeGame_91_map-object-data-wdw.json")
        ).Select(pair => pair.Value).ToDictionary(entry => entry.Id, entry => entry);

        Console.WriteLine("Loading images and fonts");
        // Load images
        // var wdwMap = Image.Load<Rgba32>("/home/cnewman/Documents/map-wdw.png");
        var osmMap = Image.Load<Rgba32>("/home/cnewman/Documents/osm_map_no_text.png");
        var targetMap = OsmSwge;

        // Setup fonts
        var collection = new FontCollection();
        var family = collection.Add("/home/cnewman/.fonts/iosevka-p-term/iosevka-p-term-regular.ttf");
        var font = family.CreateFont(12);

        const int targetBeacon = 0xA5;

        Console.WriteLine("Mapping beacons and waypoints");
        var waypointToLocationMap = new Dictionary<int, (double X, double Y)>();
        var nameToLocationMap = new Dictionary<string, (double X, double Y)>();
        foreach (var (id, entry) in mapObjectData.Where(pair => pair.Value.Type == "BeaconMarker"))
        {
            var pos = mapLocationData[entry.LocationId];
            var data = beaconData[entry.BeaconId];
            waypointToLocationMap[data.WaypointId] = nameToLocationMap[data.PlayExperienceNameWDW] = new ValueTuple<double, double>(pos.X, pos.Y);
        }

        var waypointMacMap = new Dictionary<int, HashSet<string>>();

        osmMap.Mutate(context =>
        {
            Console.WriteLine("Plotting BLE packets");
            foreach (var (time, pos, packet) in packets)
            {
                if (pos.SatelliteCount < 10)
                    continue;

                var rssi = -packet[10];
                var macAddr = new PhysicalAddress(new[]
                {
                    packet[28],
                    packet[27],
                    packet[26],
                    packet[25],
                    packet[24],
                    packet[23],
                });

                var adManufacturer = (packet[35] << 8) | packet[34];
                if (adManufacturer != 0x183) // Walt Disney
                    continue;

                var advertType = (packet[36] << 8) | packet[37];
                if (advertType != 0x1004) // Location Beacon
                    continue;

                var beaconId = packet[40];

                // if (beaconId != targetBeacon)
                //     continue;

                if (!waypointMacMap.ContainsKey(beaconId))
                    waypointMacMap[beaconId] = new HashSet<string>();
                waypointMacMap[beaconId].Add(macAddr.ToString());

                var (x, y) = LatLonToOsmMapPoint(pos.Longitude, pos.Latitude, targetMap);
                if (x > 1 || y > 1 || x < 0 || y < 0)
                    continue;
                var pt = new PointF((float)(x * osmMap.Width), (float)(y * osmMap.Height));

                if (waypointToLocationMap.TryGetValue(beaconId, out var entry))
                {
                    var (sx, sy) = WdwMapPointToOsmMapPoint(entry.X, entry.Y, targetMap);
                    var spt = new PointF((float)(sx * osmMap.Width), (float)(sy * osmMap.Height));

                    context.DrawLines(Color.Black.WithAlpha(0.1f), 1, pt, spt);
                    context.Fill(Color.Blue, new EllipsePolygon(pt, 3));
                }
                else
                    context.Fill(Color.Cyan, new EllipsePolygon(pt, 3));

                // context.DrawText($"{beaconId} 0x{beaconId:X}", font, Color.Black, pt);
            }

            Console.WriteLine(
                $"Beacon MAC map ({waypointMacMap.Count} waypoints, {waypointMacMap.Values.SelectMany(set => set).Distinct().Count()} physical addresses)");
            Console.WriteLine("| Beacon | Physical Address |");
            Console.WriteLine("| --- | --- |");

            foreach (var (waypoint, macs) in waypointMacMap)
            {
                var beacon = beaconData.Values.FirstOrDefault(entry => entry.WaypointId == waypoint);
                if (beacon == null)
                    Console.WriteLine($"\t| <orphan 0x{waypoint:X2}> | {string.Join(", ", macs)} |");
                else
                    Console.WriteLine($"\t| {beacon.Name} | {string.Join(", ", macs)} |");
            }

            Console.WriteLine("Plotting interop packets");
            foreach (var (time, pos, beaconName) in interopPacketData.Take(0))
            {
                if (pos.SatelliteCount < 10)
                    continue;

                var (x, y) = LatLonToOsmMapPoint(pos.Longitude, pos.Latitude, targetMap);
                if (x > 1 || y > 1 || x < 0 || y < 0)
                    continue;
                var pt = new PointF((float)(x * osmMap.Width), (float)(y * osmMap.Height));

                if (nameToLocationMap.TryGetValue(beaconName, out var beaconLocation))
                {
                    var (sx, sy) = WdwMapPointToOsmMapPoint(beaconLocation.X, beaconLocation.Y, targetMap);
                    var spt = new PointF((float)(sx * osmMap.Width), (float)(sy * osmMap.Height));

                    context.DrawLines(Color.Black.WithAlpha(0.1f), 1, pt, spt);
                    context.Fill(Color.Orange, new EllipsePolygon(pt, 3));
                }
                else
                    context.Fill(Color.Yellow, new EllipsePolygon(pt, 3));

                // context.DrawText($"{beaconId} 0x{beaconId:X}", font, Color.Black, pt);
            }

            Console.WriteLine("Plotting beacons");
            // Draw Datapad beacons
            foreach (var (id, entry) in mapObjectData.Where(pair => pair.Value.Type == "BeaconMarker"))
            {
                var pos = mapLocationData[entry.LocationId];
                var data = beaconData[entry.BeaconId];

                var (x, y) = WdwMapPointToOsmMapPoint(pos.X, pos.Y, targetMap);
                var pt = new PointF((float)(x * osmMap.Width), (float)(y * osmMap.Height));

                var bWaypoint = (byte)data.WaypointId;
                context.Fill(Color.Red, new EllipsePolygon(pt, 3));
                context.DrawText($"{data.Name}", font, Color.Black, pt);
            }
        });

        Console.WriteLine("Exporting image");
        osmMap.Save("/home/cnewman/Documents/osm_map_beacons.png");
    }

    private static GpsPosition? ParseSentence(string nmeaSentence)
    {
        // $GNGGA,190350.000,3013.0594,N,08137.3034,W,2,07,1.39,19.4,M,-31.6,M,,*74
        if (!nmeaSentence.StartsWith("$GNGGA,"))
            return null;

        var sentence = nmeaSentence.Split(',');
        var talker = sentence[0];
        var utcTime = sentence[1];
        var latitude = sentence[2];
        var latitudeHemisphere = sentence[3];
        var longitude = sentence[4];
        var longitudeHemisphere = sentence[5];
        var quality = (GpsQuality)int.Parse(sentence[6]);
        var numSatellites = int.Parse(sentence[7]);

        if (quality == GpsQuality.NoFix)
            return null;

        var utcHours = int.Parse(utcTime[..2]);
        var utcMinutes = int.Parse(utcTime[2..4]);
        var utcSeconds = double.Parse(utcTime[4..]);

        var latitudeDeg = int.Parse(latitude[..2]);
        var latitudeMinutes = double.Parse(latitude[2..]);

        var longitudeDeg = int.Parse(longitude[..3]);
        var longitudeMinutes = double.Parse(longitude[3..]);

        var latM = Gps.Hemispheres[latitudeHemisphere[0]] == Hemisphere.South ? -1 : 1;
        var lonM = Gps.Hemispheres[longitudeHemisphere[0]] == Hemisphere.West ? -1 : 1;

        var degLatitude = latM * (latitudeDeg + latitudeMinutes / 60);
        var degLongitude = lonM * (longitudeDeg + longitudeMinutes / 60);

        return new GpsPosition(degLatitude, degLongitude, numSatellites,
            new TimeOnly(utcHours, utcMinutes, (int)utcSeconds, (int)(utcSeconds * 1000) % 1000));
    }

    public static void MainBLE(string[] args)
    {
        using var file = File.OpenRead(args[0]);
        var br = new BinaryReader(file);

        using var f = File.OpenWrite(args[0] + ".pcap");
        var pcap = new PcapFile(f);
        pcap.WriteHeader();

        var firstTimestamp = DateTime.MinValue;

        while (file.Position < file.Length)
        {
            try
            {
                var ticks = br.ReadInt64();
                var dt = new DateTime(ticks);

                if (firstTimestamp == DateTime.MinValue)
                    firstTimestamp = dt;

                var blePacketlength = br.ReadInt32();
                var blePacket = br.ReadBytes(blePacketlength);

                if (blePacketlength > 22)
                    pcap.WriteBlePacket(dt - firstTimestamp, blePacket);
            }
            catch
            {
                break;
            }
        }
    }

    public static void MainGps(string[] args)
    {
        using var file = File.OpenRead(args[0]);
        var br = new BinaryReader(file);

        var gps = new Gps(null);

        using var f = File.OpenWrite(args[0] + "_nmea.txt");
        // var bw = new BinaryWriter(f);

        // gps.FixData += (gps1, data) =>
        // {
        // 	var latM = data.LatHemisphere == Hemisphere.South ? -1 : 1;
        // 	var lonM = data.LonHemisphere == Hemisphere.West ? -1 : 1;
        // 	var latitude = (latM * (data.LatDeg + data.LatMin / 60));
        // 	var longitude = (lonM * (data.LonDeg + data.LonMin / 60));
        //
        // 	bw.Write(latitude);
        // 	bw.Write(longitude);
        // };

        var sw = new StreamWriter(f);

        while (file.Position < file.Length)
        {
            try
            {
                var ticks = br.ReadInt64();
                var sentence = br.ReadString();
                // gps.ConsumeSentence(sentence);
                sw.WriteLine(sentence);
            }
            catch
            {
                break;
            }
        }
    }
}