using SystemQMS.Models;
using SystemQMS.ViewModels.DevicesDto;
using SystemQMS.ViewModels.PageResultDto;
using SystemQMS.ViewModels.ServiceDto;

namespace SystemQMS.Services.Interface
{
    public interface IDevSvc
    {
        public Device createDevice(CreateDeviceDto device, int unitId);
        public Device updateDevice(int deviceId, UpdateDeviceDto device);
        public Device updateDeviceStatus(string deviceId);
        public DetailDevicesDto GetDevices(int deviceId);

        public PageResultDto<GetDevicesDto> GetAllDevices(FilterDeviceDto filterDevice);
        public Task<string> GenerateLoginCode(string deviceId);
        public Task<object> LoginCode(DeviceLoginDto dev);

        public string DeleteDevice(DeleteDeviceDto devices);
        public string UpdateVolumeDevices(UpdateVolumnDeviceDto updateVolumnDeviceDto);
    }
}
