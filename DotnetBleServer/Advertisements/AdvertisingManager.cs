using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetBleServer.Core;
using Tmds.DBus;

namespace DotnetBleServer.Advertisements
{
    public class AdvertisingManager
    {
        private readonly ServerContext _Context;

        public AdvertisingManager(ServerContext context)
        {
            _Context = context;
        }

        public async Task RegisterAdvertisement(Advertisement advertisement, string device)
        {
            await _Context.Connection.RegisterObjectAsync(advertisement);
            Console.WriteLine($"advertisement object {advertisement.ObjectPath} created");

            await GetAdvertisingManager(device).RegisterAdvertisementAsync(((IDBusObject) advertisement).ObjectPath,
                new Dictionary<string, object>());

            Console.WriteLine($"advertisement {advertisement.ObjectPath} registered in BlueZ advertising manager");
        }

        private ILEAdvertisingManager1 GetAdvertisingManager(string device)
        {
            return _Context.Connection.CreateProxy<ILEAdvertisingManager1>("org.bluez", device);
        }

        public async Task CreateAdvertisement(AdvertisementProperties advertisementProperties, string device)
        {
            var advertisement = new Advertisement("/org/bluez/example/advertisement0", advertisementProperties);
            await new AdvertisingManager(_Context).RegisterAdvertisement(advertisement, device);
        }
    }
}