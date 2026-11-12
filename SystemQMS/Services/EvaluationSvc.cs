using System.Text.Json;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.EvaluationsDto;

namespace SystemQMS.Services
{
    public class EvaluationSvc : IEvaluationSvc
    {
        private const int PageSize = 10; // RTG-01: cố định 10 dòng/trang
        private const int MaxOpinionLength = 100; // giới hạn thật của cột orther trên model
        private static readonly TimeSpan LocalOffset = TimeSpan.FromHours(7); // giờ của đơn vị (UTC+7)

        private readonly MyDbContext _context;
        private readonly ILogger<EvaluationSvc> _logger;

        public EvaluationSvc(MyDbContext context, ILogger<EvaluationSvc> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Query dùng chung cho danh sách (RTG-01/03) và xuất Excel (RTG-04) để hai nơi không lệch nhau.
        // from/to: lọc theo NGÀY địa phương của đơn vị, bao trọn ngày "to"
        // (quy ngày địa phương về mốc UTC rồi mới so — không so TimeOfDay, bài học từ ScheduleSvc).
        private IQueryable<Evaluation> BuildQuery(
            int unitId, string? keyword, List<int>? counterIds,
            DateTimeOffset? from, DateTimeOffset? to)
        {
            var query = _context.evaluations
                .Include(e => e.Counter)
                .Include(e => e.Service)
                .Where(e => e.unitId == unitId);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(e => e.sttId.ToLower().Contains(kw)
                    || (e.Counter != null && e.Counter.counterName != null && e.Counter.counterName.ToLower().Contains(kw))
                    || (e.orther != null && e.orther.ToLower().Contains(kw)));
            }

            // Chưa lọc = hiện tất cả quầy (SRS: không có giá trị "Tất cả")
            if (counterIds != null && counterIds.Count > 0)
                query = query.Where(e => counterIds.Contains(e.counterId));

            if (from.HasValue)
            {
                var fromLocal = from.Value.ToOffset(LocalOffset);
                var startUtc = new DateTimeOffset(fromLocal.Date, LocalOffset).ToUniversalTime();
                query = query.Where(e => e.evaluationTime >= startUtc);
            }
            if (to.HasValue)
            {
                var toLocal = to.Value.ToOffset(LocalOffset);
                var endUtc = new DateTimeOffset(toLocal.Date.AddDays(1), LocalOffset).ToUniversalTime();
                query = query.Where(e => e.evaluationTime < endUtc);
            }

            return query;
        }
        
        private static EvaluationListItemDto ToListItem(Evaluation e) => new()
        {
            id = e.id,
            sttId = e.sttId,
            counterId = e.counterId,
            counterName = e.Counter?.counterName,
            serviceName = e.Service?.serviceName,
            evaluationTime = e.evaluationTime,
            content = EvaluationMarks.BuildContent(e),
            opinion = e.orther
        };

        private static EvaluationDetailDto ToDetail(Evaluation e) => new()
        {
            id = e.id,
            sttId = e.sttId,
            unitId = e.unitId,
            counterId = e.counterId,
            counterName = e.Counter?.counterName,
            serviceId = e.serviceId,
            serviceName = e.Service?.serviceName,
            templateId = e.templateId,
            timeMark = e.timeMark,
            attitudeMark = e.attitudeMark,
            materialMark = e.materialMark,
            satisfedMark = e.satisfedMark,
            content = EvaluationMarks.BuildContent(e),
            opinion = e.orther,
            guestPhone = e.guestPhone,
            guestName = e.guestName,
            evaluationTime = e.evaluationTime
        };
        //  RTG-01/02/03: danh sách, chi tiết, tìm kiếm + lọc 

        public async Task<EvaluationListResponse> GetEvaluationsAsync(
            int unitId, int page, string? keyword, List<int>? counterIds,
            DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            var query = BuildQuery(unitId, keyword, counterIds, from, to);

            var totalCount = await query.CountAsync(ct);
            var rows = await query
                .OrderByDescending(e => e.evaluationTime)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync(ct);

            return new EvaluationListResponse
            {
                page = page,
                pageSize = PageSize,
                totalCount = totalCount,
                items = rows.Select(ToListItem).ToList()
            };
        }

        public async Task<EvaluationDetailDto> GetEvaluationAsync(int id, CancellationToken ct = default)
        {
            var evaluation = await _context.evaluations
                .Include(e => e.Counter)
                .Include(e => e.Service)
                .FirstOrDefaultAsync(e => e.id == id, ct);
            return evaluation == null
                ? throw new KeyNotFoundException("Đánh giá không tồn tại")
                : ToDetail(evaluation);
        }

        

        //  RTG-04: xuất Excel 

        public async Task<byte[]> ExportEvaluationsAsync(
            int unitId, string? keyword, List<int>? counterIds,
            DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default)
        {
            var rows = await BuildQuery(unitId, keyword, counterIds, from, to)
                .OrderByDescending(e => e.evaluationTime)
                .ToListAsync(ct);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Danh gia");
            var headers = new[]
            {
                "STT đánh giá", "Quầy dịch vụ", "Dịch vụ", "Thời gian đánh giá",
                "Đánh giá Thời gian xử lý", "Đánh giá Thái độ nhân viên",
                "Đánh giá Vật chất trang thiết bị", "Đánh giá Mức độ hài lòng của bạn",
                "Ý kiến khác"
            };
            for (var c = 0; c < headers.Length; c++)
                ws.Cell(1, c + 1).Value = headers[c];
            ws.Row(1).Style.Font.Bold = true;

            var row = 2;
            foreach (var e in rows)
            {
                ws.Cell(row, 1).Value = e.sttId;
                ws.Cell(row, 2).Value = e.Counter?.counterName;
                ws.Cell(row, 3).Value = e.Service?.serviceName;
                ws.Cell(row, 4).Value = e.evaluationTime.ToOffset(LocalOffset).DateTime;
                ws.Cell(row, 4).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                // Nội dung tách 4 cột riêng (cập nhật SRS 29/09/2026); 0 = "Chưa đánh giá" như CMS
                ws.Cell(row, 5).Value = EvaluationMarks.Label(e.timeMark);
                ws.Cell(row, 6).Value = EvaluationMarks.Label(e.attitudeMark);
                ws.Cell(row, 7).Value = EvaluationMarks.Label(e.materialMark);
                ws.Cell(row, 8).Value = EvaluationMarks.Label(e.satisfedMark);
                ws.Cell(row, 9).Value = e.orther;
                row++;
            }
            ws.Range(1, 5, Math.Max(1, row - 1), 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        //  RTG-06/07: tra cứu + gửi 

        public async Task<EvaluationLookupDto> LookupAsync(string sttId, int unitId, CancellationToken ct = default)
        {
            var active = await GetActiveTemplateAsync(unitId, ct);
            // Phải lọc theo unitId: cùng một mã STT ở đơn vị khác không được lộ
            // quầy/dịch vụ/người xử lý sang đây (lỗi chéo đơn vị B3)
            var sticker = await _context.stickers
                .Include(s => s.Counter)
                .Include(s => s.Service)
                .Include(s => s.Users)
                .FirstOrDefaultAsync(s => s.sttId == sttId && s.unitId == unitId, ct);

            if (sticker == null)
            {
                // RTG-07: STT chưa tồn tại vẫn được đánh giá — không 404,
                // trả trống để frontend gửi kèm quầy/dịch vụ theo ngữ cảnh Kiosk.
                return new EvaluationLookupDto
                {
                    sttId = sttId,
                    found = false,
                    templateId = active?.templateId,
                    contentQuestion = active?.contentQuestion
                };
            }

            return new EvaluationLookupDto
            {
                sttId = sticker.sttId,
                found = true,
                counterId = sticker.counterId,
                counterName = sticker.Counter?.counterName,
                serviceId = sticker.serviceId,
                serviceName = sticker.Service?.serviceName,
                handlerName = sticker.Users?.username,
                templateId = active?.templateId,
                contentQuestion = active?.contentQuestion
            };
        }

        public async Task<EvaluationDetailDto> SubmitAsync(EvaluationSubmitDto dto, CancellationToken ct = default)
        {
            ValidateSubmit(dto);

            var template = await ResolveTemplateAsync(dto, ct);
            if (template != null)
                ValidateRequiredStars(template.contentQuestion, dto);

            var sticker = await _context.stickers
                .FirstOrDefaultAsync(s => s.sttId == dto.sttId, ct);
            // Request khai unitId mà phiếu thuộc đơn vị khác -> coi như không
            // tồn tại trong đơn vị đó (cùng quy tắc với Lookup, lỗi B3).
            // unitId = 0 (client không gửi) thì bỏ qua kiểm tra này.
            if (sticker != null && dto.unitId > 0 && sticker.unitId != dto.unitId)
                sticker = null;

            int unitId, serviceId, counterId;
            if (sticker != null)
            {
                // Đường A — STT đã tồn tại: lấy từ chính phiếu, không tin request.
                // Cùng STT gửi lại = thêm bản ghi mới (đánh giá lại được, không unique).
                unitId = sticker.unitId;
                serviceId = sticker.serviceId;
                counterId = sticker.counterId;
            }
            else
            {
                // Đường B — STT chưa tồn tại: Evaluation đang có FK thật tới Sticker nên
                // chưa thể lưu. Trả 400 rõ ràng thay vì để vỡ FK thành 500; chỉ bật đường này
                // khi team duyệt bỏ FK Evaluation -> Sticker (xem hướng dẫn RTG quyết định số 5).
                throw new ArgumentException(
                    "Số thứ tự chưa tồn tại trong hệ thống — đánh giá cho STT chưa có đang chờ team duyệt bỏ khóa ngoại, chưa gửi được");
            }

            var evaluation = new Evaluation
            {
                unitId = unitId,
                sttId = dto.sttId,
                serviceId = serviceId,
                counterId = counterId,
                // Evaluation.templateId là cột bắt buộc trong CSDL: không có mẫu thì để chuỗi rỗng
                templateId = template?.templateId ?? "",
                timeMark = dto.timeMark,
                attitudeMark = dto.attitudeMark,
                materialMark = dto.materialMark,
                satisfedMark = dto.satisfedMark,
                orther = dto.opinion ?? "",
                guestPhone = dto.guestPhone ?? "",
                guestName = dto.guestName ?? "",
                // RTG-07: thời gian đánh giá luôn là thời điểm gửi (kể cả STT in ngày khác)
                evaluationTime = DateTimeOffset.UtcNow
            };
            _context.evaluations.Add(evaluation);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Nhận đánh giá: SttId={SttId}, CounterId={CounterId}", dto.sttId, counterId);

            return await GetEvaluationAsync(evaluation.id, ct);
        }

        //  RTG-08: màn hình thiết bị đánh giá theo quầy 

        public async Task<RatingScreenDto> GetDeviceScreenAsync(
            int? counterId, string? deviceId, CancellationToken ct = default)
        {
            Counter? counter;
            if (!string.IsNullOrWhiteSpace(deviceId))
            {
                var device = await _context.devices
                    .Include(d => d.Counters)
                    .FirstOrDefaultAsync(d => d.deviceId == deviceId, ct);
                if (device == null)
                    throw new KeyNotFoundException("Thiết bị không tồn tại");
                var assignedCounters = device.Counters.ToList();
                if (assignedCounters.Count == 0)
                    throw new ArgumentException("Thiết bị chưa được gán cho quầy nào");
                int chosenCounterId;
                if (assignedCounters.Count == 1)
                {
                    chosenCounterId = assignedCounters[0].id;
                }
                else
                {
                    // Thiết bị gán NHIỀU quầy: không chọn bừa quầy đầu tiên (lỗi E2).
                    // Ưu tiên quầy đang có phiếu Processing; không phân định được
                    // duy nhất một quầy thì yêu cầu client truyền counterId.
                    var assignedIds = assignedCounters.Select(c => c.id).ToList();
                    var servingIds = await _context.stickers
                        .Where(s => assignedIds.Contains(s.counterId) && s.status == StickerStatus.Processing)
                        .Select(s => s.counterId)
                        .Distinct()
                        .ToListAsync(ct);
                    if (servingIds.Count != 1)
                        throw new ArgumentException("Thiết bị được gán nhiều quầy — hãy truyền counterId để chỉ định quầy");
                    chosenCounterId = servingIds[0];
                }
                counter = await LoadCounterAsync(chosenCounterId, ct);
            }
            else if (counterId.HasValue)
            {
                counter = await LoadCounterAsync(counterId.Value, ct);
            }
            else
            {
                throw new ArgumentException("Phải truyền counterId hoặc deviceId");
            }
            if (counter == null)
                throw new KeyNotFoundException("Quầy không tồn tại");

            // Phiếu mục tiêu: đang Processing trước; không có thì Completed gần nhất trong ngày
            var processing = await _context.stickers
                .Include(s => s.Users)
                .Where(s => s.counterId == counter.id && s.status == StickerStatus.Processing)
                .OrderBy(s => s.issuanceTime)
                .FirstOrDefaultAsync(ct);
            var current = processing;
            if (current == null)
            {
                var startOfDayUtc = new DateTimeOffset(
                    DateTimeOffset.UtcNow.ToOffset(LocalOffset).Date, LocalOffset).ToUniversalTime();
                current = await _context.stickers
                    .Include(s => s.Users)
                    .Where(s => s.counterId == counter.id && s.status == StickerStatus.Completed
                                && s.useTime >= startOfDayUtc)
                    .OrderByDescending(s => s.useTime)
                    .FirstOrDefaultAsync(ct);
            }

            // Người xử lý: người của phiếu mục tiêu; chưa có phiếu thì user đang được gán tại quầy
            var handler = current?.Users ?? await _context.users
                .FirstOrDefaultAsync(u => u.counterId == counter.id && u.isActive, ct);

            var template = await GetActiveTemplateAsync(counter.unitId, ct);

            return new RatingScreenDto
            {
                counterId = counter.id,
                counterName = counter.counterName,
                counterStatus = counter.status,
                currentSttId = current?.sttId,
                currentTicketStatus = current?.status,
                canRate = current != null,
                handlerUserId = handler?.userId,
                handlerName = handler?.username,
                handlerAvatarBase64 = handler?.avatar is { Length: > 0 }
                    ? Convert.ToBase64String(handler.avatar) : null,
                services = counter.Services
                    .Select(s => new RatingScreenServiceDto { serviceId = s.id, serviceName = s.serviceName })
                    .ToList(),
                templateId = template?.templateId,
                contentQuestion = template?.contentQuestion
            };
        }

        private async Task<Counter?> LoadCounterAsync(int id, CancellationToken ct)
        {
            return await _context.counters
                .Include(c => c.Services)
                .FirstOrDefaultAsync(c => c.id == id, ct);
        }

        private static void ValidateSubmit(EvaluationSubmitDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.sttId))
                throw new ArgumentException("Số thứ tự không được để trống");
            foreach (var (mark, name) in new[]
            {
                (dto.timeMark, "Thời gian xử lý"), (dto.attitudeMark, "Thái độ nhân viên"),
                (dto.materialMark, "Vật chất trang thiết bị"), (dto.satisfedMark, "Mức độ hài lòng")
            })
            {
                if (mark < EvaluationMarks.Unrated || mark > EvaluationMarks.Good)
                    throw new ArgumentException($"Điểm {name} phải từ 0 (chưa đánh giá) đến 3 (Tốt)");
            }
            if (!string.IsNullOrWhiteSpace(dto.guestPhone)
                && (dto.guestPhone.Length > 10 || !dto.guestPhone.All(char.IsDigit)))
                throw new ArgumentException("Số điện thoại chỉ gồm chữ số, tối đa 10 ký tự");
            if (dto.guestName != null && dto.guestName.Length > 50)
                throw new ArgumentException("Họ tên tối đa 50 ký tự");
            if (dto.opinion != null && dto.opinion.Length > MaxOpinionLength)
                throw new ArgumentException($"Ý kiến khác tối đa {MaxOpinionLength} ký tự");
        }

        private async Task<EvaluationTemplate?> ResolveTemplateAsync(EvaluationSubmitDto dto, CancellationToken ct)
        {
            if (!string.IsNullOrWhiteSpace(dto.templateId))
            {
                var template = await _context.evaluationTemplates
                    .FirstOrDefaultAsync(t => t.unitId == dto.unitId && t.templateId == dto.templateId, ct);
                return template ?? throw new ArgumentException("Mẫu đánh giá không tồn tại trong đơn vị");
            }
            return await GetActiveTemplateAsync(dto.unitId, ct);
        }

        private async Task<EvaluationTemplate?> GetActiveTemplateAsync(int unitId, CancellationToken ct)
        {
            var config = await _context.unitConfigs.AsNoTracking()
                .FirstOrDefaultAsync(c => c.UnitId == unitId, ct);
            if (config?.ActiveEvaluationTemplateId == null) return null;
            // Lọc kèm unitId: phòng khi ActiveEvaluationTemplateId trỏ sang mẫu
            // của đơn vị khác (dữ liệu cũ / lỗi đổi unitId trước đây — lỗi H5)
            return await _context.evaluationTemplates.AsNoTracking()
                .FirstOrDefaultAsync(t => t.id == config.ActiveEvaluationTemplateId.Value
                                          && t.unitId == unitId, ct);
        }

        // Câu sao bắt buộc (r=1) mà điểm tương ứng = 0 -> 400.
        // Câu x/c bắt buộc không kiểm tra ở backend vì đáp án của chúng không có cột lưu
       
        private static void ValidateRequiredStars(string contentQuestion, EvaluationSubmitDto dto)
        {
            try
            {
                using var doc = JsonDocument.Parse(contentQuestion);
                // Guard ValueKind như ValidateQuestions: mẫu lỗi trong DB (root
                // không phải Object, qs không phải mảng, phần tử không phải Object)
                // thì bỏ qua kiểm tra, không để InvalidOperationException thành 500
                if (doc.RootElement.ValueKind != JsonValueKind.Object) return;
                if (!doc.RootElement.TryGetProperty("qs", out var qs) || qs.ValueKind != JsonValueKind.Array) return;
                var marks = new[] { dto.timeMark, dto.attitudeMark, dto.materialMark, dto.satisfedMark };
                var starIndex = 0;
                foreach (var q in qs.EnumerateArray())
                {
                    if (q.ValueKind != JsonValueKind.Object) continue;
                    if (q.TryGetProperty("t", out var type) && type.ValueKind == JsonValueKind.String && type.GetString() == "s")
                    {
                        if (starIndex < marks.Length
                            && q.TryGetProperty("r", out var required)
                            && required.ValueKind == JsonValueKind.Number && required.TryGetInt32(out var rReq) && rReq == 1
                            && marks[starIndex] == EvaluationMarks.Unrated)
                            throw new ArgumentException("Chưa trả lời câu hỏi bắt buộc trong mẫu đánh giá");
                        starIndex++;
                    }
                }
            }
            catch (JsonException)
            {
                // Mẫu lưu trong DB không parse được thì bỏ qua kiểm tra bắt buộc, không chặn khách gửi
            }
        }

       
    }
}
