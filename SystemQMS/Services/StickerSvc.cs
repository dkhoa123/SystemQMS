using ClosedXML.Excel;
using SystemQMS.Models;
using SystemQMS.Services.Interface;

namespace SystemQMS.Services
{
    public class StickerSvc : IStickerSvc
    {
        private MyDbContext _context;
        public StickerSvc (MyDbContext context)
        {
            _context = context;
        }

        public string AddNew(Tickets sticker)
        {
            var service = _context.Services.FirstOrDefault(x => x.Id == sticker.ServiceId);
            if(service == null)
            {
                return "Không tìm thấy dịch vụ";
            }

             var counter = _context.Counters
            .FirstOrDefault(x => x.Id == sticker.CounterId);

            if (counter == null)
            {
                return "Không tìm thấy quầy dịch vụ";
            }
            if (!service.IsActive)
                return "Dịch vụ hiện không hoạt động";
            var fomular = _context.NumberFomulars.FirstOrDefault(x => x.ServiceId == service.Id);
            if(fomular== null)
            {
                return "Dịch vụ chưa có quy tắc số";
            }



            if (fomular.StartNumber > fomular.EndNumber)
                return "Dãy số không hợp lệ.";

            


            // tim so da cap thuoc day hien tai
            var tickerId = _context.Tickets.Where(x => x.ServiceId == service.Id)
                .Select(x => x.SttId)
                .ToList();


            int nextNumber;

            if (fomular.SequenceMode == "Shared")
            {
                // lay tat ca ve da cap toan he thong

                var issuedNumbers = _context.Tickets
                    .Select(x => x.SttId)
                    .ToList()

                    .Select(sttId =>
                    {
                        string digits = new string(sttId.Where(char.IsDigit).ToArray());
                        return int.TryParse(digits, out int number) ? (int?)number : null;
                    }
                    )
                    .Where(number => number.HasValue
                && number.Value >= fomular.StartNumber
                && number.Value <= fomular.EndNumber
                )
                .Select(number => number!.Value)
                .ToList();


                // so lon nhat da cap tren toan he thng


                int maxNumber = issuedNumbers.Any()
                ? issuedNumbers.Max() : fomular.StartNumber - 1;


                // tinh tien 1

                nextNumber = maxNumber + 1;
            }
            else
            {
                // che do doc lap
                // chi lay so da cap thuoc dich vu hien tai
                var issuedNumbers = _context.Tickets
                    .Where(x => x.ServiceId == service.Id)
                    .Select(x => x.SttId)
                    .ToList()
                    .Select(sttId =>
                    {
                        string digits = new string(sttId.Where(char.IsDigit).ToArray());
                        return int.TryParse(digits, out int number)
                        ? (int?)number : null;
                    })
                    .Where(number => number.HasValue)
                    .Select(number => number!.Value)
                    .ToList();
                nextNumber = issuedNumbers.Any()
                    ? issuedNumbers.Max() + 1 : fomular.StartNumber;

            }

            // kiem tra gioi han 
            if (nextNumber > fomular.EndNumber)
                return "Dãy số đã hết";

           

            //tao phieu
            string nextStt = fomular.FirstName + nextNumber;
            string numberText = nextNumber.ToString("D5");

            string sttId;

            if (!string.IsNullOrEmpty(fomular.FirstName))
            {
                sttId = fomular.FirstName + numberText;
            }
            else if (!string.IsNullOrEmpty(fomular.LastName))
            {
                sttId = numberText + fomular.LastName;
            }
            else
            {
                sttId = numberText;
            }



            // kiem tra trung ma phieu

            if (_context.Tickets.Any(x => x.SttId == nextStt))
                return "Số thứ tự đã tồn tại ";

            // tao phieu

            var newSticker = new Tickets
            {
                SttId = sttId,
                UnitId = service.UnitId,
                ServiceId = service.Id,
                CounterId = counter.Id,
                UserId = sticker.UserId,
                IssuanceTime = DateTimeOffset.UtcNow,
                UseTime = DateTimeOffset.UtcNow,
                Status = "Waiting",
                Stage = "Waiting"


            };

            // luu dl

            _context.Tickets.Add(newSticker);
            _context.SaveChanges();

            return "Cấp số thành công: " + newSticker;

            
        }

        public byte[] ExportExcel()
        {
            var data = _context.Tickets.ToList();

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Stickers");

            sheet.Cell(1, 1).Value = "STT";
            sheet.Cell(1, 2).Value = "Trạng thái";

            int row = 2;
            foreach (var item in data)
            {
                sheet.Cell(row, 1).Value = item.SttId;
                sheet.Cell(row, 2).Value = item.Status;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}