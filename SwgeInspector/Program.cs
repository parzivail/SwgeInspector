using System.IO.Ports;
using Mtk33x9Gps;
using NavXMxp;

namespace SwgeInspector;

public class Program
{
	public static void Main(string[] args)
	{
		// NormalizeSerialDataRate(args[0]);
		//
		// using var socket = new SerialPort(args[0], 115200);
		// socket.Open();
		//
		// socket.Write("$PMTK314,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0*28\r\n");
		//
		// var gps = new Gps(socket.BaseStream);
		// gps.FixData += (gps1, data) =>
		// {
		// 	var latM = data.LatHemisphere == Hemisphere.South ? -1 : 1;
		// 	var lonM = data.LonHemisphere == Hemisphere.West ? -1 : 1;
		// 	Console.WriteLine($"{latM * (data.LatDeg + data.LatMin / 60)},{lonM * (data.LonDeg + data.LonMin / 60)}");
		// };
		//
		// gps.Start();
		
		using var socket = new SerialPort("COM6", 57600);
		socket.Open();
		
		var navx = new NavX(socket.BaseStream);
		navx.Start();
		
		var mres = new ManualResetEventSlim();
		mres.Wait();
	}

	private static void NormalizeSerialDataRate(string port)
	{
		using var socket = new SerialPort(port, 115200);
		socket.Open();

		var buffer = new byte[512];
		socket.Read(buffer, 0, buffer.Length);
		if (buffer.Any(b => b > 127))
		{
			// Device default baud rate is 9600 bps
			socket.BaudRate = 9600;
			
			socket.DiscardInBuffer();
			
			// Set device baud rate to 115200 bps
			socket.Write("$PMTK251,115200*1F\r\n");
			socket.BaseStream.Flush();
			
			socket.BaudRate = 115200;
			socket.DiscardInBuffer();
			
			Console.WriteLine("Update rate to 115200");
		}
		
		socket.Close();
	}
}