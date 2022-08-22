using BleSniffer;
using Mtk33x9Gps;

namespace Unpacker;

public class Program
{
	public static void Main(string[] args)
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

		using var f = File.OpenWrite(args[0] + "_ll.bin");
		var bw = new BinaryWriter(f);

		gps.FixData += (gps1, data) =>
		{
			var latM = data.LatHemisphere == Hemisphere.South ? -1 : 1;
			var lonM = data.LonHemisphere == Hemisphere.West ? -1 : 1;
			var latitude = (float)(latM * (data.LatDeg + data.LatMin / 60));
			var longitude = (float)(lonM * (data.LonDeg + data.LonMin / 60));

			bw.Write(latitude);
			bw.Write(longitude);
		};

		while (file.Position < file.Length)
		{
			try
			{
				var ticks = br.ReadInt64();
				var sentence = br.ReadString();
				gps.ConsumeSentence(sentence);
			}
			catch
			{
				break;
			}
		}
	}
}