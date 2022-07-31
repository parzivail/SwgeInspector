﻿using System.IO.Ports;
using Microsoft.IO;

namespace BleSniffer;

public class BleSnifferDevice : IDisposable
{
    private const int SlipStart = 0xAB;
    private const int SlipEnd = 0xBC;
    private const int SlipEsc = 0xCD;
    private const int SlipEscStart = SlipStart + 1;
    private const int SlipEscEnd = SlipEnd + 1;
    private const int SlipEscEsc = SlipEsc + 1;
    
    private static readonly RecyclableMemoryStreamManager MemoryStreamManager = new();

    private readonly BinaryReader _reader;
    private readonly SerialPort _port;

    public BleSnifferDevice(string device)
    {
        _port = new SerialPort("COM4", 460800)
        {
            RtsEnable = true
        };
        _reader = new BinaryReader(_port.BaseStream);
    }

    public void Open()
    {
        _port.Open();
    }

    public void Close()
    {
        _port.Close();
    }

    private void AdvanceToPacket()
    {
        byte b;
        do
        {
            b = _reader.ReadByte();
        } while (b != SlipStart);
    }

    private byte[] ReadPacketToEnd()
    {
        using var stream = MemoryStreamManager.GetStream();
        while (true)
        {
            var b = _reader.ReadByte();
            switch (b)
            {
                case SlipEnd:
                    return stream.ToArray();
                case SlipEsc:
                {
                    var escapedByte = _reader.ReadByte();
                    switch (escapedByte)
                    {
                        case SlipEscStart:
                            stream.WriteByte(SlipStart);
                            break;
                        case SlipEscEnd:
                            stream.WriteByte(SlipEnd);
                            break;
                        case SlipEscEsc:
                            stream.WriteByte(SlipEsc);
                            break;
                        default:
                            stream.WriteByte(SlipEnd);
                            break;
                    }
                    break;
                }
                default:
                    stream.WriteByte(b);
                    break;
            }
        }
    }

    public byte[]? ReadPacket()
    {
        try
        {
            AdvanceToPacket();
            return ReadPacketToEnd();
        }
        catch (EndOfStreamException)
        {
            return null;
        }
    }

    public void Dispose()
    {
        _port.Dispose();
    }
}