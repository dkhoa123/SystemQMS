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
            return _context.NumberFomulars.Any(x =>
            x.ServiceId != numberFomular.ServiceId &&
            x.SequenceMode == "Independent" &&
            numberFomular.StartNumber <= x.EndNumber &&
            x.StartNumber <= numberFomular.EndNumber);
        }
        private string? ValidateNumberFomular(NumberFomular numberFomular)
        {
            //kiem tra che do cap
            if (numberFomular.SequenceMode != "Shared" &&
                numberFomular.SequenceMode != "Independent")
                return "Chế độ cấp số không hợp lệ.";
            //che do chung
            if (numberFomular.SequenceMode == "Shared" && numberFomular.IsDailyLimitEnabled)
            {
                if (numberFomular.EndNumber < 0 || numberFomular.EndNumber > 99999)

                    return "Số kết thúc phải từ 1 đến 99.999";
            }
            // che do doc lap
            
            if (numberFomular.SequenceMode == "Independent")
            {
                if (numberFomular.StartNumber < 0 || numberFomular.EndNumber < 0)
                    return "Số bắt đầu và số kết thúc không hợp lệ .";
                if (numberFomular.StartNumber > numberFomular.EndNumber)
                    return "Số băt đầu phải nhỏ hơn hoặc bằng số kết thúc.";
            }

            
            
            // kiem tra tien to
            if(string.IsNullOrEmpty(numberFomular.FirstName))
            {

                if (numberFomular.FirstName.Length > 3)
                    return "Không quá 3 ký tự";
                if (numberFomular.FirstName.Contains(' '))
                    return "Không chứa khoảng trắng";

               
            }
            // kiem tra hau to
            if (string.IsNullOrEmpty(numberFomular.LastName))
            {

                if (numberFomular.LastName.Length > 3)
                    return "Không quá 3 ký tự";
                if (numberFomular.LastName.Contains(' '))
                    return "Không chứa khoảng trắng";


            }
            // chon 1 trong 2
            if(!string.IsNullOrEmpty(numberFomular.FirstName) &&
                !string.IsNullOrEmpty(numberFomular.LastName))
            {
                return "Chỉ được chọn tiền tố hoặc hậu tố";
            }
            //kiem tra so
            if (numberFomular.StartNumber < 0 || numberFomular.EndNumber < 0)
                return "Số bắt đầu và kết thúc không được âm";
            if (numberFomular.StartNumber > numberFomular.EndNumber)
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
            if (numberFomular.SequenceMode == "Independent" && IsOverlap(numberFomular))
                return "Dãy số này trùng với dãy số của dịch vụ khác.";
            _context.NumberFomulars.Add(numberFomular);
            _context.SaveChanges();
            return "Added";

           
        }

        public List<NumberFomular> GetNumberFomular()
        {
            return _context.NumberFomulars.ToList();
            
        }

        public string Update(NumberFomular numberFomular)
        {
            var exits = _context.NumberFomulars.FirstOrDefault(x => x.Id == numberFomular.Id);
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


            if (numberFomular.SequenceMode == "Independent" && IsOverlap(numberFomular))
            {
                return "Dãy số này trùng với dãy số của dịch vụ khác.";
            }




            // kiem tra so da cap cho dich vu
            var stickers = _context.Tickets
                .Where(x => x.ServiceId == exits.ServiceId)
                .ToList();

            int maxIssuedNumber = 0;
            foreach(var sticker in stickers)
            {
                string digits = new string(sticker.SttId.Where(char.IsDigit).ToArray());
                if(int.TryParse(digits,out int number))
                {
                    if (number > maxIssuedNumber)
                        maxIssuedNumber = number;
                }
            }

            if (maxIssuedNumber > 0 && numberFomular.StartNumber > maxIssuedNumber)
                return "Không thể sửa số đã in";


            if (maxIssuedNumber > 0 && numberFomular.EndNumber <= maxIssuedNumber)
                return "Dãy số thấp hơn số hiện tại, vui lòng nhập lớn hơn số hiện tại";
            




         
            exits.NumberType = numberFomular.NumberType;
            exits.FirstName = numberFomular.FirstName;
            exits.LastName = numberFomular.LastName;
            exits.StartNumber = numberFomular.StartNumber;
            exits.EndNumber = numberFomular.EndNumber;
            exits.LimitDay = numberFomular.LimitDay;
             _context.SaveChanges();
            return "Suscess";
        }


        // Xem truoc
        public string  Preview(int serviceId)
        {
            //kiem tra dich vu 

            var service = _context.Services.FirstOrDefault(x => x.Id == serviceId);
            if (service == null)
                return null;
            //kiem tra nts
            var rule = _context.NumberFomulars.FirstOrDefault(x => x.ServiceId == serviceId);
            if(rule == null)
            {
                return null;
            }
            //kiem tra day so
            if (rule.StartNumber < 0 || rule.EndNumber < rule.StartNumber)
                return null;
            string number = rule.StartNumber.ToString("D5");
            return $"{rule.FirstName}{number}{rule.LastName}";

        }

        public async Task<bool> ConvertToShared(List<int> serviceIds)
        {
            if (serviceIds == null || serviceIds.Count == 0)
                return false;
            serviceIds = serviceIds.Distinct().ToList();
            var formulas = await _context.NumberFomulars
                .Where(x => serviceIds.Contains(x.ServiceId))
                .ToListAsync();
            // tim quy tac dich vu
            var foundIds = formulas
                .Select(x => x.ServiceId)

                .Distinct()
                .ToList();
            if (serviceIds.Except(foundIds).Any())
                return false;
            // chuyen 
            if (formulas.Any(x => x.SequenceMode != "Independent"))
                return false;
            foreach (var formula in formulas)
            {
                formula.SequenceMode = "Shared";
               
            }



           



            await _context.SaveChangesAsync();
            return true;

        }

        public string ReSet(int fomulaId)
        {
            var fomula = _context.NumberFomulars.FirstOrDefault(x => x.Id == fomulaId);
            if (fomula == null)
                return "Không tìm thấy quy tắc số";

            var services = _context.Services.Where(x => x.Id == fomula.ServiceId).ToList();

            var today = DateTimeOffset.Now.Date;
            var stickers = _context.Tickets
                .Where(x => x.ServiceId == fomula.ServiceId 
                && x.IssuanceTime >= today 
                && x.IssuanceTime < today.AddDays(1))
                .ToList();
            foreach(var sticker in stickers)
            {
                sticker.Status = "Đã hủy";
            }
            _context.SaveChanges();
            return "Reset thành công";
        }
    }
}
