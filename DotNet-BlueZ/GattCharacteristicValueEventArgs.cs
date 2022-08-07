namespace HashtagChris.DotNetBlueZ;

public class GattCharacteristicValueEventArgs : EventArgs
{
    public GattCharacteristicValueEventArgs(byte[] value)
    {
        Value = value;
    }

    public byte[] Value { get; }
}