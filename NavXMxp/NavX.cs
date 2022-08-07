namespace NavXMxp;

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

    public record AhrsUpdateEvent(
        double Yaw, double Pitch, double Roll,
        double CompassHeading, double Altitude, double FusedHeading,
        double LinearAccelX, double LinearAccelY, double LinearAccelZ,
        double VelX, double VelY, double VelZ,
        double DispX, double DispY, double DispZ,
        double MpuTemp,
        float QuatW, float QuatX, float QuatY, float QuatZ,
        OpStatus OpStatus, SensorStatus SensorStatus, CalibrationStatus CalStatus, SelfTestStatus SelftestStatus
    );

    public event EventHandler<AhrsUpdateEvent>? AhrsUpdate;

    public NavX(Stream stream)
    {
        _stream = stream;
        _reader = new BinaryReader(stream);

        _thread = new Thread(BackgroundThreadLoop)
        {
            Name = "NavX Serial Thread"
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
        if (header[0] != BinaryPacketIndicatorChar || header[1] != AhrsUpdateMessageLength - 2 ||
            header[2] != MsgidAhrsposUpdate)
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

        AhrsUpdate?.Invoke(this, new AhrsUpdateEvent(yaw, pitch, roll,
            compassHeading, altitude, fusedHeading,
            linearAccelX, linearAccelY, linearAccelZ,
            velX, velY, velZ,
            dispX, dispY, dispZ,
            mpuTemp,
            quatW, quatX, quatY, quatZ,
            opStatus, sensorStatus, calStatus, selftestStatus
        ));
    }

    public void DecodeAhrsPacket()
    {
        var header = _reader.ReadBytes(3);
        if (header[0] != BinaryPacketIndicatorChar || header[1] != AhrsUpdateMessageLength - 2 ||
            header[2] != MsgidAhrsUpdate)
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