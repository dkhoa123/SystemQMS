using SystemQMS.ViewModels.EvaluationsDto;

namespace SystemQMS.Services.Interface
{
    public interface IEvaluationTemplateSvc
    {
        Task<List<EvaluationTemplateDto>> GetTemplatesAsync(int unitId, string? keyword, CancellationToken ct = default);
        Task<EvaluationTemplateDto> GetTemplateAsync(int id, CancellationToken ct = default);
        Task<EvaluationTemplateDto> CreateTemplateAsync(EvaluationTemplateUpsertDto dto, CancellationToken ct = default);
        Task<EvaluationTemplateDto> UpdateTemplateAsync(int id, EvaluationTemplateUpsertDto dto, CancellationToken ct = default);
        Task DeleteTemplateAsync(int id, CancellationToken ct = default);
        Task<EvaluationTemplateDto> CloneTemplateAsync(int id, CancellationToken ct = default);

        // Cài đặt đánh giá: đặt mẫu đang áp dụng cho đơn vị qua UnitConfig.ActiveEvaluationTemplateId
        Task ActivateTemplateAsync(int id, CancellationToken ct = default);
    }
}
