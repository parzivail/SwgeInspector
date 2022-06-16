namespace NavXMxp;

enum OpStatus
{
	Initializing = 0x00,
	SelftestInProgress = 0x01,
	Error = 0x02,
	ImuAutocalInProgress = 0x03,
	Normal = 0x04
}

[Flags]
enum SensorStatus
{
	Moving = 0x01,
	YawStable = 0x02,
	MagDisturbance = 0x04,
	AltitudeValid = 0x08,
	SealevelPressSet = 0x10,
	FusedHeadingValid = 0x20
}

[Flags]
enum CalibrationStatus
{
	ImuCalInprogress = 0x00,
	ImuCalAccumulate = 0x01,
	ImuCalComplete = 0x02,
	MagCalComplete = 0x04,
	BaroCalComplete = 0x08
}

[Flags]
enum SelfTestStatus
{
	StatusComplete = 0x80,
	ResultGyroPassed = 0x01,
	ResultAccelPassed = 0x02,
	ResultMagPassed = 0x04,
	ResultBaroPassed = 0x08
}

public class NavX
{
	private const byte PacketStartChar = (byte)'!';
	private const byte BinaryPacketIndicatorChar = (byte)'#';
	private const byte MsgidAhrsUpdate = (byte)'a';
	private const byte MsgidAhrsposUpdate = (byte)'p';

	private const int AhrsUpdateMessageLength = 66;

	private readonly Stream _stream;
	private readonly BinaryReader _reader;

	private readonly Thread _thread;
	private readonly CancellationTokenSource _cancellationTokenSource;

	public NavX(Stream stream)
	{
		_stream = stream;
		_reader = new BinaryReader(stream);

		_thread = new Thread(BackgroundThreadLoop);
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

	private void BackgroundThreadLoop(object? param)
	{
		if (param is not CancellationTokenSource tokenSource)
			throw new ArgumentException("Param must be CancellationTokenSource", nameof(param));

		while (!tokenSource.IsCancellationRequested)
		{
			byte b;
			do
			{
				b = _reader.ReadByte();
			} while (b != PacketStartChar);

			DecodeAhrsUpdatePacket();
		}
	}

	public void DecodeAhrsUpdatePacket()
	{
		var header = _reader.ReadBytes(3);
		if (header[0] != BinaryPacketIndicatorChar || header[1] != AhrsUpdateMessageLength - 2 || header[2] != MsgidAhrsposUpdate)
			return;

		var yaw = ReadProtocolSignedHundredthsFloat(_reader);
		var pitch = ReadProtocolSignedHundredthsFloat(_reader); /* FIXME */
		var roll = ReadProtocolSignedHundredthsFloat(_reader); /* FIXME */
		var compassHeading = ReadProtocolUnsignedHundredthsFloat(_reader);
		var altitude = ReadProtocol1616Float(_reader);
		var fusedHeading = ReadProtocolUnsignedHundredthsFloat(_reader);
		var linearAccelX = ReadProtocolSignedThousandthsFloat(_reader);
		var linearAccelY = ReadProtocolSignedThousandthsFloat(_reader);
		var linearAccelZ = ReadProtocolSignedThousandthsFloat(_reader);
		var velX = ReadProtocol1616Float(_reader);
		var velY = ReadProtocol1616Float(_reader);
		var velZ = ReadProtocol1616Float(_reader);
		var dispX = ReadProtocol1616Float(_reader);
		var dispY = ReadProtocol1616Float(_reader);
		var dispZ = ReadProtocol1616Float(_reader);
		var mpuTemp = ReadProtocolSignedHundredthsFloat(_reader);
		/* AHRSPosUpdate:  Quaternions are signed int (16-bit resolution); divide by 16384 to yield +/- 2 radians */
		var quatW = _reader.ReadInt16() / 16384f;
		var quatX = _reader.ReadInt16() / 16384f;
		var quatY = _reader.ReadInt16() / 16384f;
		var quatZ = _reader.ReadInt16() / 16384f;
		var opStatus = (OpStatus)_reader.ReadByte();
		var sensorStatus = (SensorStatus)_reader.ReadByte();
		var calStatus = (CalibrationStatus)_reader.ReadByte();
		var selftestStatus = (SelfTestStatus)_reader.ReadByte();

		Console.WriteLine($"{fusedHeading:N3}");
	}

	public void DecodeAhrsPacket()
	{
		var header = _reader.ReadBytes(3);
		if (header[0] != BinaryPacketIndicatorChar || header[1] != AhrsUpdateMessageLength - 2 || header[2] != MsgidAhrsUpdate)
			return;

		var yaw = ReadProtocolSignedHundredthsFloat(_reader);
		var pitch = ReadProtocolSignedHundredthsFloat(_reader); /* FIXME */
		var roll = ReadProtocolSignedHundredthsFloat(_reader); /* FIXME */
		var compassHeading = ReadProtocolUnsignedHundredthsFloat(_reader);
		var altitude = ReadProtocol1616Float(_reader);
		var fusedHeading = ReadProtocolUnsignedHundredthsFloat(_reader);
		var linearAccelX = ReadProtocolSignedThousandthsFloat(_reader);
		var linearAccelY = ReadProtocolSignedThousandthsFloat(_reader);
		var linearAccelZ = ReadProtocolSignedThousandthsFloat(_reader);
		var calMagX = _reader.ReadInt16();
		var calMagY = _reader.ReadInt16();
		var calMagZ = _reader.ReadInt16();
		var magFieldNormRatio = ReadProtocolUnsignedHundredthsFloat(_reader);
		var magFieldNormScalar = ReadProtocol1616Float(_reader);
		var mpuTemp = ReadProtocolSignedHundredthsFloat(_reader);
		var rawMagX = _reader.ReadInt16();
		var rawMagY = _reader.ReadInt16();
		var rawMagZ = _reader.ReadInt16();
		/* AHRSPosUpdate:  Quaternions are signed int (16-bit resolution); divide by 16384 to yield +/- 2 radians */
		var quatW = _reader.ReadInt16() / 16384f;
		var quatX = _reader.ReadInt16() / 16384f;
		var quatY = _reader.ReadInt16() / 16384f;
		var quatZ = _reader.ReadInt16() / 16384f;
		var barometricPressure = ReadProtocol1616Float(_reader);
		var baroTemp = ReadProtocolSignedHundredthsFloat(_reader);
		var opStatus = (OpStatus)_reader.ReadByte();
		var sensorStatus = (SensorStatus)_reader.ReadByte();
		var calStatus = (CalibrationStatus)_reader.ReadByte();
		var selftestStatus = (SelfTestStatus)_reader.ReadByte();
	}

	private static double ReadProtocolSignedHundredthsFloat(BinaryReader reader)
	{
		return reader.ReadInt16() / 100f;
	}

	private static double ReadProtocolSignedThousandthsFloat(BinaryReader reader)
	{
		return reader.ReadInt16() / 1000f;
	}

	private static double ReadProtocolUnsignedHundredthsFloat(BinaryReader reader)
	{
		return reader.ReadUInt16() / 100f;
	}

	private static double ReadProtocol1616Float(BinaryReader reader)
	{
		return reader.ReadUInt32() / 65536f;
	}
}