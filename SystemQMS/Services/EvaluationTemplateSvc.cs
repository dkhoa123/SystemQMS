using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.EvaluationsDto;

namespace SystemQMS.Services
{
    public class EvaluationTemplateSvc : IEvaluationTemplateSvc
    {
        // Giới hạn THẬT của model trên main (SRS cho 256 nhưng cột chỉ chứa được
        // 100/100/255 ký tự — validate theo SRS sẽ vỡ lúc lưu DB)
        private const int MaxTemplateIdLength = 100;
        private const int MaxTemplateNameLength = 100;
        private const int MaxContentLength = 255;
        private const int MaxQuestions = 5;

        private readonly MyDbContext _context;
        private readonly ILogger<EvaluationTemplateSvc> _logger;

        public EvaluationTemplateSvc(MyDbContext context, ILogger<EvaluationTemplateSvc> logger)
        {
            _context = context;
            _logger = logger;
        }
        private static EvaluationTemplateDto ToDto(EvaluationTemplate t, int? activeId) => new()
        {
            id = t.id,
            unitId = t.unitId,
            templateId = t.templateId,
            templateName = t.templateName,
            contentQuestion = t.contentQuestion,
            isActive = activeId == t.id
        };

        public async Task<List<EvaluationTemplateDto>> GetTemplatesAsync(
            int unitId, string? keyword, CancellationToken ct = default)
        {
            var query = _context.evaluationTemplates.Where(t => t.unitId == unitId);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(t => t.templateId.ToLower().Contains(kw)
                                      || t.templateName.ToLower().Contains(kw));
            }

            var templates = await query.OrderBy(t => t.id).ToListAsync(ct);
            var activeId = await ActiveTemplateIdAsync(unitId, ct);
            return templates.Select(t => ToDto(t, activeId)).ToList();
        }
       

        public async Task<EvaluationTemplateDto> GetTemplateAsync(int id, CancellationToken ct = default)
        {
            var template = await GetOrThrowAsync(id, ct);
            var activeId = await ActiveTemplateIdAsync(template.unitId, ct);
            return ToDto(template, activeId);
        }

        public async Task<EvaluationTemplateDto> CreateTemplateAsync(
            EvaluationTemplateUpsertDto dto, CancellationToken ct = default)
        {
            await ValidateAsync(dto, null, ct);
            var template = new EvaluationTemplate
            {
                unitId = dto.unitId,
                templateId = dto.templateId.Trim(),
                templateName = dto.templateName.Trim(),
                contentQuestion = dto.contentQuestion
            };
            _context.evaluationTemplates.Add(template);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Tạo mẫu đánh giá: TemplateId={TemplateId}, UnitId={UnitId}",
                template.templateId, template.unitId);
            return ToDto(template, await ActiveTemplateIdAsync(dto.unitId, ct));
        }

        public async Task<EvaluationTemplateDto> UpdateTemplateAsync(
            int id, EvaluationTemplateUpsertDto dto, CancellationToken ct = default)
        {
            var template = await GetOrThrowAsync(id, ct);
            // Không cho đổi đơn vị của mẫu: UnitConfig.ActiveEvaluationTemplateId của
            // đơn vị cũ sẽ trỏ sang mẫu thuộc đơn vị khác (lỗi chéo đơn vị H5).
            // Muốn dùng mẫu ở đơn vị khác thì tạo/clone mẫu mới ở đơn vị đó.
            if (dto.unitId != template.unitId)
                throw new ArgumentException("Không được đổi đơn vị của mẫu — hãy tạo mẫu mới ở đơn vị cần dùng");
            await ValidateAsync(dto, id, ct);
            template.unitId = dto.unitId;
            template.templateId = dto.templateId.Trim();
            template.templateName = dto.templateName.Trim();
            template.contentQuestion = dto.contentQuestion;
            await _context.SaveChangesAsync(ct);
            return ToDto(template, await ActiveTemplateIdAsync(dto.unitId, ct));
        }

        public async Task DeleteTemplateAsync(int id, CancellationToken ct = default)
        {
            var template = await GetOrThrowAsync(id, ct);
            var activeId = await ActiveTemplateIdAsync(template.unitId, ct);
            if (activeId == id)
                throw new ArgumentException("Mẫu đang được áp dụng cho đơn vị, hãy chọn mẫu khác trước khi xóa");
            _context.evaluationTemplates.Remove(template);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<EvaluationTemplateDto> CloneTemplateAsync(int id, CancellationToken ct = default)
        {
            var source = await GetOrThrowAsync(id, ct);
            var newCode = source.templateId + "-COPY";
            var suffix = 2;
            while (await _context.evaluationTemplates
                       .AnyAsync(t => t.unitId == source.unitId && t.templateId == newCode, ct))
            {
                newCode = $"{source.templateId}-COPY{suffix++}";
            }
            if (newCode.Length > MaxTemplateIdLength)
                throw new ArgumentException("Mã mẫu gốc quá dài, không nhân bản tự động được — hãy tạo mẫu mới và đặt mã ngắn hơn");

            var clone = new EvaluationTemplate
            {
                unitId = source.unitId,
                templateId = newCode,
                templateName = source.templateName + " (bản sao)",
                contentQuestion = source.contentQuestion
            };
            if (clone.templateName.Length > MaxTemplateNameLength)
                clone.templateName = clone.templateName[..MaxTemplateNameLength];
            _context.evaluationTemplates.Add(clone);
            await _context.SaveChangesAsync(ct);
            return ToDto(clone, await ActiveTemplateIdAsync(source.unitId, ct));
        }

        public async Task ActivateTemplateAsync(int id, CancellationToken ct = default)
        {
            var template = await GetOrThrowAsync(id, ct);
            var config = await _context.unitConfigs
                .FirstOrDefaultAsync(c => c.UnitId == template.unitId, ct);
            if (config == null)
            {
                // Theo đúng pattern ConfigSvc của CFG: chưa có cấu hình thì tạo mới
                config = new UnitConfig { UnitId = template.unitId };
                _context.unitConfigs.Add(config);
            }
            config.ActiveEvaluationTemplateId = template.id;
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Áp dụng mẫu đánh giá: TemplateId={TemplateId}, UnitId={UnitId}",
                template.templateId, template.unitId);
        }

        private async Task<EvaluationTemplate> GetOrThrowAsync(int id, CancellationToken ct)
        {
            var template = await _context.evaluationTemplates.FirstOrDefaultAsync(t => t.id == id, ct);
            return template ?? throw new KeyNotFoundException("Mẫu đánh giá không tồn tại");
        }

        private async Task<int?> ActiveTemplateIdAsync(int unitId, CancellationToken ct)
        {
            var config = await _context.unitConfigs.AsNoTracking()
                .FirstOrDefaultAsync(c => c.UnitId == unitId, ct);
            return config?.ActiveEvaluationTemplateId;
        }

        private async Task ValidateAsync(EvaluationTemplateUpsertDto dto, int? currentId, CancellationToken ct)
        {
            // Kiểm tra đơn vị tồn tại trước: không kiểm tra thì unitId lạ sẽ vỡ
            // khóa ngoại FK_evaluationTemplates_units_unitId thành 500 (lỗi G9)
            if (!await _context.units.AnyAsync(u => u.unitId == dto.unitId, ct))
                throw new ArgumentException("Đơn vị không tồn tại");
            if (string.IsNullOrWhiteSpace(dto.templateId))
                throw new ArgumentException("Mã mẫu không được để trống");
            if (dto.templateId.Trim().Length > MaxTemplateIdLength)
                throw new ArgumentException($"Mã mẫu tối đa {MaxTemplateIdLength} ký tự");
            if (string.IsNullOrWhiteSpace(dto.templateName))
                throw new ArgumentException("Tên mẫu không được để trống");
            if (dto.templateName.Trim().Length > MaxTemplateNameLength)
                throw new ArgumentException($"Tên mẫu tối đa {MaxTemplateNameLength} ký tự");
            if (string.IsNullOrWhiteSpace(dto.contentQuestion))
                throw new ArgumentException("Nội dung câu hỏi không được để trống");
            if (dto.contentQuestion.Length > MaxContentLength)
                throw new ArgumentException($"Nội dung câu hỏi tối đa {MaxContentLength} ký tự (giới hạn cột trong CSDL)");

            var duplicate = await _context.evaluationTemplates.AnyAsync(t =>
                t.unitId == dto.unitId && t.templateId == dto.templateId.Trim()
                && (currentId == null || t.id != currentId.Value), ct);
            if (duplicate)
                throw new ArgumentException("Mã mẫu đã tồn tại trong đơn vị");

            ValidateQuestions(dto.contentQuestion);
        }

        // Định dạng JSON gọn: {"qs":[{"q","t":"s|c|x","o":[...],"r":0|1}]}
        // "Sắp xếp lên/xuống" và cờ "Bắt buộc" của RTG-05 nằm ngay trong mảng qs này
        // (đổi thứ tự phần tử / đổi r rồi PUT lại) — EvaluationTemplate không có cột thứ tự riêng.
        private static void ValidateQuestions(string contentQuestion)
        {
            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(contentQuestion);
            }
            catch (JsonException)
            {
                throw new ArgumentException("Nội dung câu hỏi phải là JSON hợp lệ");
            }

            using (doc)
            {
                // TryGetProperty chỉ gọi được trên Object — root là số/chuỗi/mảng
                // sẽ ném InvalidOperationException thành 500 (lỗi G6)
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                    throw new ArgumentException("Nội dung câu hỏi phải là một đối tượng JSON có mảng \"qs\"");
                if (!doc.RootElement.TryGetProperty("qs", out var qs) || qs.ValueKind != JsonValueKind.Array)
                    throw new ArgumentException("Nội dung câu hỏi phải có mảng \"qs\" chứa các câu hỏi");
                if (qs.GetArrayLength() < 1 || qs.GetArrayLength() > MaxQuestions)
                    throw new ArgumentException($"Mẫu phải có từ 1 đến {MaxQuestions} câu hỏi");

                foreach (var q in qs.EnumerateArray())
                {
                    // Phần tử qs không phải Object (số, mảng con...) cũng làm
                    // TryGetProperty ném InvalidOperationException (lỗi G7/G8)
                    if (q.ValueKind != JsonValueKind.Object)
                        throw new ArgumentException("Mỗi câu hỏi phải là một đối tượng JSON");
                    if (!q.TryGetProperty("q", out var text) || text.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(text.GetString()))
                        throw new ArgumentException("Mỗi câu hỏi phải có nội dung \"q\"");
                    if (!q.TryGetProperty("t", out var typeEl))
                        throw new ArgumentException("Mỗi câu hỏi phải có loại \"t\" (s, c hoặc x)");
                    var type = typeEl.ValueKind == JsonValueKind.String ? typeEl.GetString() : null;
                    if (type != "s" && type != "c" && type != "x")
                        throw new ArgumentException("Loại câu hỏi chỉ nhận s (sao), c (trắc nghiệm) hoặc x (nhập tự do)");
                    if (type == "c" && (!q.TryGetProperty("o", out var options)
                                        || options.ValueKind != JsonValueKind.Array
                                        || options.GetArrayLength() < 2))
                        throw new ArgumentException("Câu trắc nghiệm phải có ít nhất 2 lựa chọn trong \"o\"");
                    // "r" có mặt thì BẮT BUỘC là số 0/1 — dạng chuỗi "1" cũng phải
                    // trả 400, không được lọt qua chỉ vì khác kiểu (lỗi G10)
                    if (q.TryGetProperty("r", out var required)
                        && (required.ValueKind != JsonValueKind.Number
                            || !required.TryGetInt32(out var rVal) || (rVal != 0 && rVal != 1)))
                        throw new ArgumentException("Cờ bắt buộc \"r\" chỉ nhận 0 hoặc 1");
                }
            }
        }

    }
}
