using Microsoft.EntityFrameworkCore;
using SystemQMS.Models;
using SystemQMS.Services.Interface;

namespace SystemQMS.Services
{
    public class NumberFomularSvc : INumberFomularSvc
    {
        private MyDbContext _context;
        public NumberFomularSvc (MyDbContext context)
        {
            _context = context;
        }

        private bool IsOverlap(NumberFomular numberFomular)
        {
            return _context.numberFomulars.Any(x =>
            x.serviceId != numberFomular.serviceId &&
            x.sequenceMode == "Independent" &&
            numberFomular.startNumber <= x.endNumber &&
            x.startNumber <= numberFomular.endNumber);
        }
        private string? ValidateNumberFomular(NumberFomular numberFomular)
        {
            //kiem tra che do cap
            if (numberFomular.sequenceMode != "Shared" &&
                numberFomular.sequenceMode != "Independent")
                return "Chế độ cấp số không hợp lệ.";
            //che do chung
            if (numberFomular.sequenceMode == "Shared" && numberFomular.isDailyLimitEnabled)
            {
                if (numberFomular.endNumber < 0 || numberFomular.endNumber > 99999)

                    return "Số kết thúc phải từ 1 đến 99.999";
            }
            // che do doc lap
            
            if (numberFomular.sequenceMode == "Independent")
            {
                if (numberFomular.startNumber < 0 || numberFomular.endNumber < 0)
                    return "Số bắt đầu và số kết thúc không hợp lệ .";
                if (numberFomular.startNumber > numberFomular.endNumber)
                    return "Số băt đầu phải nhỏ hơn hoặc bằng số kết thúc.";
            }

            
            
            // kiem tra tien to
            if(string.IsNullOrEmpty(numberFomular.firstName))
            {

                if (numberFomular.firstName.Length > 3)
                    return "Không quá 3 ký tự";
                if (numberFomular.firstName.Contains(' '))
                    return "Không chứa khoảng trắng";

               
            }
            // kiem tra hau to
            if (string.IsNullOrEmpty(numberFomular.lastName))
            {

                if (numberFomular.lastName.Length > 3)
                    return "Không quá 3 ký tự";
                if (numberFomular.lastName.Contains(' '))
                    return "Không chứa khoảng trắng";


            }
            // chon 1 trong 2
            if(!string.IsNullOrEmpty(numberFomular.firstName) &&
                !string.IsNullOrEmpty(numberFomular.lastName))
            {
                return "Chỉ được chọn tiền tố hoặc hậu tố";
            }
            //kiem tra so
            if (numberFomular.startNumber < 0 || numberFomular.endNumber < 0)
                return "Số bắt đầu và kết thúc không được âm";
            if (numberFomular.startNumber > numberFomular.endNumber)
                return "Số kết thúc phải lớn hơn hoặc bằng số bắt đầu";
            return null;



        }
        public string AddNew(NumberFomular numberFomular)
        {
            var error = ValidateNumberFomular(numberFomular);
            if (error != null)
                return "Error";
            //khong trung 
            //var isOverlap = _context.numberFomulars.Any(x =>
            //x.startNumber <= numberFomular.endNumber &&
            //x.endNumber >= numberFomular.startNumber);
            //if (isOverlap)
            //    return "Error";
            if (numberFomular.sequenceMode == "Independent" && IsOverlap(numberFomular))
                return "Dãy số này trùng với dãy số của dịch vụ khác.";
            _context.numberFomulars.Add(numberFomular);
            _context.SaveChanges();
            return "Added";

           
        }

        public List<NumberFomular> GetNumberFomular()
        {
            return _context.numberFomulars.ToList();
            
        }

        public string Update(NumberFomular numberFomular)
        {
            var exits = _context.numberFomulars.FirstOrDefault(x => x.id == numberFomular.id);
            if (exits == null)
                return "Không tìm thấy quy tắc cấp số";
            var error = ValidateNumberFomular(numberFomular);
            if (error != null)
                return "Error";
            //var isOverlap = _context.numberFomulars.Any(x =>
            //x.id != numberFomular.id &&
            //x.startNumber <= numberFomular.endNumber &&
            //x.endNumber >= numberFomular.startNumber);
            //if(isOverlap )
            //{
            //    return "Trùng lặp";
            //}


            if (numberFomular.sequenceMode == "Independent" && IsOverlap(numberFomular))
            {
                return "Dãy số này trùng với dãy số của dịch vụ khác.";
            }




            // kiem tra so da cap cho dich vu
            var stickers = _context.stickers
                .Where(x => x.serviceId == exits.serviceId)
                .ToList();

            int maxIssuedNumber = 0;
            foreach(var sticker in stickers)
            {
                string digits = new string(sticker.sttId.Where(char.IsDigit).ToArray());
                if(int.TryParse(digits,out int number))
                {
                    if (number > maxIssuedNumber)
                        maxIssuedNumber = number;
                }
            }

            if (maxIssuedNumber > 0 && numberFomular.startNumber > maxIssuedNumber)
                return "Không thể sửa số đã in";

            if (maxIssuedNumber > 0 && numberFomular.endNumber <= maxIssuedNumber)
                return "Dãy số thấp hơn số hiện tại, vui lòng nhập lớn hơn số hiện tại";
            




         
            exits.numberType = numberFomular.numberType;
            exits.firstName = numberFomular.firstName;
            exits.lastName = numberFomular.lastName;
            exits.startNumber = numberFomular.startNumber;
            exits.endNumber = numberFomular.endNumber;
            exits.limitDay = numberFomular.limitDay;
             _context.SaveChanges();
            return "Suscess";
        }


        // Xem truoc
        public string  Preview(int serviceId)
        {
            //kiem tra dich vu 

            var service = _context.services.FirstOrDefault(x => x.id == serviceId);
            if (service == null)
                return null;
            //kiem tra nts
            var rule = _context.numberFomulars.FirstOrDefault(x => x.serviceId == serviceId);
            if(rule == null)
            {
                return null;
            }
            //kiem tra day so
            if (rule.startNumber < 0 || rule.endNumber < rule.startNumber)
                return null;
            string number = rule.startNumber.ToString("D5");
            return $"{rule.firstName}{number}{rule.lastName}";

        }

        public async Task<bool> ConvertToShared(List<int> serviceIds)
        {
            if (serviceIds == null || serviceIds.Count == 0)
                return false;
            serviceIds = serviceIds.Distinct().ToList();
            var formulas = await _context.numberFomulars
                .Where(x => serviceIds.Contains(x.serviceId))
                .ToListAsync();
            // tim quy tac dich vu
            var foundIds = formulas
                .Select(x => x.serviceId)

                .Distinct()
                .ToList();
            if (serviceIds.Except(foundIds).Any())
                return false;
            // chuyen 
            if (formulas.Any(x => x.sequenceMode != "Independent"))
                return false;
            foreach (var formula in formulas)
            {
                formula.sequenceMode = "Shared";
               
            }



           



            await _context.SaveChangesAsync();
            return true;

        }

        public string ReSet(int fomulaId)
        {
            var fomula = _context.numberFomulars.FirstOrDefault(x => x.id == fomulaId);
            if (fomula == null)
                return "Không tìm thấy quy tắc số";

            var services = _context.services.Where(x => x.id == fomula.serviceId).ToList();

            var today = DateTimeOffset.Now.Date;
            var stickers = _context.stickers
                .Where(x => x.serviceId == fomula.serviceId 
                && x.issuanceTime >= today 
                && x.issuanceTime < today.AddDays(1))
                .ToList();
            foreach(var sticker in stickers)
            {
                sticker.status = "Đã hủy";
            }
            _context.SaveChanges();
            return "Reset thành công";
        }
    }
}
