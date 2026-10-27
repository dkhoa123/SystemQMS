using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.DevicesDto;

namespace SystemQMS.Services
{
    public class DevSvc : IDevSvc
    {
        private MyDbContext _context;
        private IConfiguration _config;
        public DevSvc(MyDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        private string HashValue(string value)
        {
            var secretKey = _config["DeviceAuth:Secret"]
                ?? throw new Exception("Secret key not found in configuration.");

            using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secretKey));
            return Convert.ToHexString(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(value)));
        }

        

        public Device createDevice(CreateDeviceDto device)
        {
            try
            {
                Device dev = new Device
                {
                    deviceId = device.deviceId,
                    deviceName = device.deviceName,
                    deviceType = device.deviceType,
                    statusActive = "Không khả dụng",
                    statusConnect = "Không kết nối",
                    ipAddress = "0.0.0.0",
                    volume = 10,
                    show = true
                };
                var service = _context.services.Where(s => device.serviceIds.Contains(s.serviceId)).ToList();
                // Chỉ thêm những dịch vụ chưa được gán
                var existingIds = dev.Services
                    .Select(s => s.serviceId)
                    .ToHashSet();

                var newServices = service
                    .Where(s => !existingIds.Contains(s.serviceId))
                    .ToList();

                foreach (var svc in newServices)
                {
                    dev.Services.Add(svc);
                }
                _context.devices.Add(dev);

                _context.SaveChanges();

                return dev;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating device: " + ex.Message);
            }
        }

        public Device updateDevice(string deviceId, UpdateDeviceDto device)
        {
            var dev = _context.devices.FirstOrDefault(d => d.deviceId == deviceId);
            if(dev == null)
            {
                throw new Exception("Device not found");
            }
            try
            {
                dev.deviceName = device.deviceName;
                dev.deviceType = device.deviceType;
                dev.statusActive = "Khả dụng";
                dev.statusConnect = device.statusConnect;
                dev.ipAddress = device.ipAddress;
                dev.show = true;

                // Xóa các dịch vụ được chọn khỏi thiết bị
                var servicesToRemove = dev.Services
                    .Where(s => device.serviceIdsRemove.Contains(s.serviceId))
                    .ToList();

                foreach (var service in servicesToRemove)
                {
                    dev.Services.Remove(service);
                }
                _context.SaveChanges();
                return dev;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating device: " + ex.Message);
            }
        }

        public Device updateDeviceStatus(string deviceId)
        {
           var dev = _context.devices.FirstOrDefault(d => d.deviceId == deviceId);
            if(dev == null)
            {
                throw new Exception("Device not found");
            }
            try
            {
                dev.statusActive = "Không khả dụng";
                dev.show = false;
                _context.SaveChanges();
                return dev;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating device status: " + ex.Message);
            }

        }

        public Device GetDevices(string deviceId)
        {
            var dev = _context.devices.FirstOrDefault(d => d.deviceId == deviceId);
            if (dev == null)
            {
                throw new Exception("Device not found");
            }
            Device device = new Device();
            device.deviceId = dev.deviceId;
            device.deviceName = dev.deviceName;
            device.deviceType = dev.deviceType;
            device.statusActive = dev.statusActive;
            device.statusConnect = dev.statusConnect;
            device.ipAddress = dev.ipAddress;
            device.show = dev.show;

            device.sessionTokenHash = dev.sessionTokenHash;
            device.sessionExpiresAt = dev.sessionExpiresAt;

            return device;
        }

        public List<Device> GetAllDevices()
        {
            var devices = _context.devices.ToList();
            return devices;
        }

        public async Task<string> GenerateLoginCode(string deviceId)
        {
            var device = await _context.devices.FirstOrDefaultAsync(d => d.deviceId == deviceId);
            if (device == null)
            {
                throw new Exception("Device not found");
            }
            if (device.statusActive != "Khả dụng")
            {
                throw new Exception("Device is not active");
            }

            // Generate a random 6-digit code
            var code = RandomNumberGenerator
                .GetInt32(0, 1_000_000)
                .ToString("D6");
            // Hash the code and set expiration time
            device.loginCodeHash = HashValue($"{deviceId}:{code}");
            device.loginCodeExpiresAt = DateTime.UtcNow.AddMinutes(5); // Code expires in 5 minutes
            await _context.SaveChangesAsync();
            return code;
        }

        public async Task<object> LoginCode(DeviceLoginDto dev)
        {
            var device = _context.devices.FirstOrDefault(d => d.deviceId == dev.deviceId);
            if (device == null)
            {
                throw new Exception("Device not found");
            }
            if (device.statusActive != "Khả dụng")
            {
                throw new Exception("Device is not active");
            }
            if(string.IsNullOrEmpty(device.loginCodeHash) ||
                device.loginCodeExpiresAt == null ||
                device.loginCodeExpiresAt < DateTime.UtcNow)
            {
                throw new Exception("Invalid or expired login code");
            }
            
            var submittedHash = HashValue($"{dev.deviceId}:{dev.code}");

            if(!CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(device.loginCodeHash),
                Convert.FromHexString(submittedHash)))
            {
                throw new Exception("Invalid login code");
            }

            device.loginCodeHash = null;
            device.loginCodeExpiresAt = null;

            var token = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(32));

            device.sessionTokenHash = HashValue(token);
            device.sessionExpiresAt = DateTime.UtcNow.AddHours(24); // Session expires in 1 day

            await _context.SaveChangesAsync();
            return new
            {
                deviceId = device.deviceId,
                deviceName = device.deviceName,
                sessionToken = token,
                sessionExpiresAt = device.sessionExpiresAt
            };
        }

        public string DeleteDevice(string deviceId)
        {
            var device = _context.devices.FirstOrDefault(d => d.deviceId == deviceId);
            if (device == null)
            {
                throw new Exception("Device not found");
            }
            _context.devices.Remove(device);
            _context.SaveChanges();
            return "Device deleted successfully";
        }

        public string UpdateVolumeDevices(UpdateVolumnDeviceDto updateVolumnDeviceDto)
        {
            foreach(var deviceId in updateVolumnDeviceDto.deviceIds)
            {
                var device = _context.devices.FirstOrDefault(d => d.deviceId == deviceId);
                if (device == null)
                {
                    throw new Exception($"Device with ID {deviceId} not found");
                }
                device.volume = updateVolumnDeviceDto.volume;
            }
            _context.SaveChanges();
            return "Volume updated successfully";
        }

        public List<FilterDeviceDto> FilterDevices(FilterDeviceDto filterDeviceDto)
        {
            var query = _context.devices
                .Include(d => d.Services)
                .Include(d => d.Counters)
                .AsQueryable();
            if (!string.IsNullOrEmpty(filterDeviceDto.statusActive))
            {
                query = query.Where(d => d.statusActive == filterDeviceDto.statusActive);
            }
            if (!string.IsNullOrEmpty(filterDeviceDto.statusConnect))
            {
                query = query.Where(d => d.statusConnect == filterDeviceDto.statusConnect);
            }
            if (!string.IsNullOrEmpty(filterDeviceDto.deviceType))
            {
                query = query.Where(d => d.deviceType == filterDeviceDto.deviceType);
            }
            if (!string.IsNullOrEmpty(filterDeviceDto.deviceId))
            {
                query = query.Where(d => d.deviceId == filterDeviceDto.deviceId);
            }
            if (!string.IsNullOrEmpty(filterDeviceDto.deviceName))
            {
                query = query.Where(d => d.deviceName.Contains(filterDeviceDto.deviceName));
            }
            if (!string.IsNullOrEmpty(filterDeviceDto.ipAddress))
            {
                query = query.Where(d => d.ipAddress == filterDeviceDto.ipAddress);
            }
            if(!string.IsNullOrEmpty(filterDeviceDto.serviceId))
            {
                query = query.Where(d => d.Services.Any(s => s.serviceId == filterDeviceDto.serviceId));
            }
            if(!string.IsNullOrEmpty(filterDeviceDto.couterId))
            {
                query = query.Where(d => d.Counters.Any(c => c.counterId == filterDeviceDto.couterId));
            }
            var devices = query.ToList();
            return devices.Select(d => new FilterDeviceDto
            {
                statusActive = d.statusActive,
                statusConnect = d.statusConnect,
                deviceType = d.deviceType,
                deviceId = d.deviceId,
                deviceName = d.deviceName,
                ipAddress = d.ipAddress,
                serviceId = d.Services.FirstOrDefault()?.serviceId,
                couterId = d.Counters.FirstOrDefault()?.counterId
            }).ToList();
        }
    }
}
