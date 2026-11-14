using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.DevicesDto;
using SystemQMS.ViewModels.PageResultDto;
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

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(value)));
        }

        

        public Device createDevice(CreateDeviceDto device, int unitId)
        {
            try
            {
                if(_context.Devices.Any(d => d.DeviceId == device.deviceCode))
                {
                    throw new Exception("Device code already exists");
                }
                Device dev = new Device
                {
                    UnitId = unitId,
                    DeviceId = device.deviceCode,
                    DeviceName = device.deviceName,
                    DeviceType = device.deviceType,
                    StatusActive = device.availability,
                    StatusConnect = "DISCONNECTED",
                    IpAddress = "0.0.0.0",
                    Volume = 50,
                    Show = false,
                    Description = device.description,
                    TemplateId = device.templateId,
                };
                var service = _context.Services.Where(s => device.serviceIds.Contains(s.ServiceId)).ToList();
                // Chỉ thêm những dịch vụ chưa được gán
                var existingIds = dev.Services
                    .Select(s => s.ServiceId)
                    .ToHashSet();

                var newServices = service
                    .Where(s => !existingIds.Contains(s.ServiceId))
                    .ToList();

                foreach (var svc in newServices)
                {
                    dev.Services.Add(svc);
                }

                _context.Devices.Add(dev);

                _context.SaveChanges();

                return dev;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public Device updateDevice(int deviceId, UpdateDeviceDto device)
        {
            var dev = _context.Devices
                .Include(d => d.Services)
                .FirstOrDefault(d => d.Id == deviceId);

            if (dev == null)
            {
                throw new Exception("Device not found");
            }

            try
            {
                // 1. Cập nhật thông tin thiết bị
                dev.DeviceName = device.deviceName;
                dev.DeviceType = device.deviceType;
                dev.StatusActive = device.availability;
                dev.StatusConnect = "CONNECTED";
                dev.Show = true;

                // 2. Lấy danh sách serviceId FE gửi lên
                var requestedServiceIds = (device.serviceIds ?? new List<string>())
                    .Distinct()
                    .ToList();

                // 3. Xóa những dịch vụ không còn được chọn
                var servicesToRemove = dev.Services
                    .Where(s => !requestedServiceIds.Contains(s.ServiceId))
                    .ToList();

                foreach (var service in servicesToRemove)
                {
                    dev.Services.Remove(service);
                }

                // 4. Lấy những dịch vụ thiết bị đang có
                var currentServiceIds = dev.Services
                    .Select(s => s.ServiceId)
                    .ToHashSet();

                // 5. Tìm những dịch vụ mới được chọn
                var servicesToAdd = _context.Services
                    .Where(s => requestedServiceIds.Contains(s.ServiceId)
                             && !currentServiceIds.Contains(s.ServiceId))
                    .ToList();

                // 6. Thêm dịch vụ mới vào thiết bị
                foreach (var service in servicesToAdd)
                {
                    dev.Services.Add(service);
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
           var dev = _context.Devices.FirstOrDefault(d => d.DeviceId == deviceId);
            if(dev == null)
            {
                throw new Exception("Device not found");
            }
            try
            {
                dev.StatusActive = "Không khả dụng";
                dev.Show = false;
                _context.SaveChanges();
                return dev;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating device status: " + ex.Message);
            }

        }

        public DetailDevicesDto GetDevices(int deviceId)
        {
            var dev = _context.Devices.FirstOrDefault(d => d.Id == deviceId);
            if (dev == null)
            {
                throw new Exception("Device not found");
            }
            DetailDevicesDto device = new DetailDevicesDto();
            device.deviceCode = dev.DeviceId;
            device.deviceName = dev.DeviceName;
            device.deviceType = dev.DeviceType;
            device.connectionStatus = dev.StatusConnect;
            device.ipAddress = dev.IpAddress;
            device.volume = dev.Volume;

            //device.sessionTokenHash = dev.sessionTokenHash;
            //device.sessionExpiresAt = dev.sessionExpiresAt;

            return device;
        }

        public PageResultDto<GetDevicesDto> GetAllDevices(FilterDeviceDto filterDevice)
        {
            var query = _context.Devices.AsQueryable();

            // Lọc keyword
            if (!string.IsNullOrEmpty(filterDevice.keyword))
            {
                query = query.Where(d =>
                    d.DeviceName.Contains(filterDevice.keyword) ||
                    d.DeviceId.Contains(filterDevice.keyword));
            }

            // Lọc trạng thái hoạt động
            if (!string.IsNullOrEmpty(filterDevice.availability))
            {
                query = query.Where(d =>
                    d.StatusActive == filterDevice.availability.ToUpper());
            }

            // Lọc trạng thái kết nối
            if (!string.IsNullOrEmpty(filterDevice.connectionStatus))
            {
                query = query.Where(d =>
                    d.StatusConnect == filterDevice.connectionStatus);
            }

            // Đếm tổng số bản ghi sau khi lọc
            int totalRecords = query.Count();

            int page = Math.Max(1, filterDevice.page); // Đảm bảo trang tối thiểu là 1
            int pageSize = Math.Max(1, filterDevice.pageSize); // Đảm bảo kích thước trang tối thiểu là 1

            // Tính tổng số trang
            int totalPages = (int)Math.Ceiling(
                (double)totalRecords / filterDevice.pageSize
            );

            // Phân trang
            var devices = query
                .OrderBy(d => d.Id)
                .Skip((filterDevice.page - 1) * filterDevice.pageSize)
                .Take(filterDevice.pageSize)
                .ToList();
            var data = devices.Select(d => new GetDevicesDto
            {
                deviceId = d.Id,
                deviceCode = d.DeviceId,
                deviceName = d.DeviceName,
                deviceType = d.DeviceType,
                availability = d.StatusActive,
                connectionStatus = d.StatusConnect,
                ipAddress = d.IpAddress,
                volume = d.Volume,
                showQueue = d.Show,
                kioskTemplateId = d.TemplateId,
                serviceIds = d.Services.Select(s => s.ServiceId).ToList(),
                counterIds = d.Counters.Select(c => c.CounterId).ToList(),
                description = d.Description
            }).ToList();


            return new PageResultDto<GetDevicesDto>
            {
                items = data,
                page = filterDevice.page,
                pageSize = filterDevice.pageSize,
                total = totalPages
            };
        }

        public async Task<string> GenerateLoginCode(string deviceId)
        {
            var device = await _context.Devices.FirstOrDefaultAsync(d => d.DeviceId == deviceId);
            if (device == null)
            {
                throw new Exception("Device not found");
            }
            if (device.StatusActive != "Khả dụng")
            {
                throw new Exception("Device is not active");
            }

            // Generate a random 6-digit code
            var code = RandomNumberGenerator
                .GetInt32(0, 1_000_000)
                .ToString("D6");
            // Hash the code and set expiration time
            device.LoginCodeHash = HashValue($"{deviceId}:{code}");
            device.LoginCodeExpiresAt = DateTime.UtcNow.AddMinutes(5); // Code expires in 5 minutes
            await _context.SaveChangesAsync();
            return code;
        }

        public async Task<object> LoginCode(DeviceLoginDto dev)
        {
            var device = _context.Devices.FirstOrDefault(d => d.DeviceId == dev.deviceId);
            if (device == null)
            {
                throw new Exception("Device not found");
            }
            if (device.StatusActive != "Khả dụng")
            {
                throw new Exception("Device is not active");
            }
            if(string.IsNullOrEmpty(device.LoginCodeHash) ||
                device.LoginCodeExpiresAt == null ||
                device.LoginCodeExpiresAt < DateTime.UtcNow)
            {
                throw new Exception("Invalid or expired login code");
            }
            
            var submittedHash = HashValue($"{dev.deviceId}:{dev.code}");

            if(!CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(device.LoginCodeHash),
                Convert.FromHexString(submittedHash)))
            {
                throw new Exception("Invalid login code");
            }

            device.LoginCodeHash = null;
            device.LoginCodeExpiresAt = null;

            var token = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(32));

            device.SessionTokenHash = HashValue(token);
            device.SessionExpiresAt = DateTime.UtcNow.AddHours(24); // Session expires in 1 day

            await _context.SaveChangesAsync();
            return new
            {
                deviceId = device.DeviceId,
                deviceName = device.DeviceName,
                sessionToken = token,
                sessionExpiresAt = device.SessionExpiresAt
            };
        }

        public string DeleteDevice(DeleteDeviceDto devices)
        {
            foreach(var deviceId in devices.deviceIds)
            {
                var device = _context.Devices.FirstOrDefault(d => d.Id == deviceId);
                if (device == null)
                {
                    throw new Exception("Device not found");
                }
                _context.Devices.Remove(device);
            }
            _context.SaveChanges();
            return "Device deleted successfully";
        }

        public string UpdateVolumeDevices(UpdateVolumnDeviceDto updateVolumnDeviceDto)
        {
            foreach(var deviceId in updateVolumnDeviceDto.deviceIds)
            {
                var device = _context.Devices.FirstOrDefault(d => d.Id == deviceId);
                if (device == null)
                {
                    throw new Exception($"Device with ID {deviceId} not found");
                }
                device.Volume = updateVolumnDeviceDto.volume;
            }
            _context.SaveChanges();
            return "Volume updated successfully";
        }
    }
}
