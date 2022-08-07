namespace HashtagChris.DotNetBlueZ;

public delegate Task GattCharacteristicEventHandlerAsync(GattCharacteristic sender,
    GattCharacteristicValueEventArgs eventArgs);