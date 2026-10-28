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

        public string AddNew(Sticker sticker)
        {
            var service = _context.services.FirstOrDefault(x => x.id == sticker.serviceId);
            if(service == null)
            {
                return "Không tìm thấy dịch vụ";
            }

             var counter = _context.counters
            .FirstOrDefault(x => x.id == sticker.counterId);

            if (counter == null)
            {
                return "Không tìm thấy quầy dịch vụ";
            }
            if (!service.isActive)
                return "Dịch vụ hiện không hoạt động";
            var fomular = _context.numberFomulars.FirstOrDefault(x => x.serviceId == service.id);
            if(fomular== null)
            {
                return "Dịch vụ chưa có quy tắc số";
            }



            if (fomular.startNumber > fomular.endNumber)
                return "Dãy số không hợp lệ.";

            


            // tim so da cap thuoc day hien tai
            var tickerId = _context.stickers.Where(x => x.serviceId == service.id)
                .Select(x => x.sttId)
                .ToList();


            int nextNumber;

            if (fomular.sequenceMode == "Shared")
            {
                // lay tat ca ve da cap toan he thong

                var issuedNumbers = _context.stickers
                    .Select(x => x.sttId)
                    .ToList()

                    .Select(sttId =>
                    {
                        string digits = new string(sttId.Where(char.IsDigit).ToArray());
                        return int.TryParse(digits, out int number) ? (int?)number : null;
                    }
                    )
                    .Where(number => number.HasValue
                && number.Value >= fomular.startNumber
                && number.Value <= fomular.endNumber
                )
                .Select(number => number!.Value)
                .ToList();


                // so lon nhat da cap tren toan he thng


                int maxNumber = issuedNumbers.Any()
                ? issuedNumbers.Max() : fomular.startNumber - 1;


                // tinh tien 1

                nextNumber = maxNumber + 1;
            }
            else
            {
                // che do doc lap
                // chi lay so da cap thuoc dich vu hien tai
                var issuedNumbers = _context.stickers
                    .Where(x => x.serviceId == service.id)
                    .Select(x => x.sttId)
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
                    ? issuedNumbers.Max() + 1 : fomular.startNumber;

            }

            // kiem tra gioi han 
            if (nextNumber > fomular.endNumber)
                return "Dãy số đã hết";

           

            //tao phieu
            string nextStt = fomular.firstName + nextNumber;
            string numberText = nextNumber.ToString("D5");

            string sttId;

            if (!string.IsNullOrEmpty(fomular.firstName))
            {
                sttId = fomular.firstName + numberText;
            }
            else if (!string.IsNullOrEmpty(fomular.lastName))
            {
                sttId = numberText + fomular.lastName;
            }
            else
            {
                sttId = numberText;
            }



            // kiem tra trung ma phieu

            if (_context.stickers.Any(x => x.sttId == nextStt))
                return "Số thứ tự đã tồn tại ";

            // tao phieu

            var newSticker = new Sticker
            {
                sttId = sttId,
                unitId = service.unitId,
                serviceId = service.id,
                counterId = counter.id,
                userId = sticker.userId,
                issuanceTime = DateTimeOffset.UtcNow,
                useTime = DateTimeOffset.UtcNow,
                status = "Waiting",
                stage = "Waiting"


            };

            // luu dl

            _context.stickers.Add(newSticker);
            _context.SaveChanges();

            return "Cấp số thành công: " + newSticker;

            
        }

        public byte[] ExportExcel()
        {
            var data = _context.stickers.ToList();

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Stickers");

            sheet.Cell(1, 1).Value = "STT";
            sheet.Cell(1, 2).Value = "Trạng thái";

            int row = 2;
            foreach (var item in data)
            {
                sheet.Cell(row, 1).Value = item.sttId;
                sheet.Cell(row, 2).Value = item.status;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}