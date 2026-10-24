using SystemQMS.Models;
using SystemQMS.ViewModels;

namespace SystemQMS.Services.Interface
{
    public interface IDevSvc
    {
        public Device createDevice(CreateDeviceDto device);
        public Device updateDevice(string deviceId, UpdateDeviceDto device);
        public Device updateDeviceStatus(string deviceId);
        public Device GetDevices(string deviceId);

        public List<Device> GetAllDevices();
        public Task<string> GenerateLoginCode(string deviceId);
        public Task<object> LoginCode(DeviceLoginDto dev);

        public string DeleteDevice(string deviceId);
        public string UpdateVolumeDevices(UpdateVolumnDeviceDto updateVolumnDeviceDto);

        public List<FilterDeviceDto> FilterDevices(FilterDeviceDto filterDeviceDto);
    }
}
