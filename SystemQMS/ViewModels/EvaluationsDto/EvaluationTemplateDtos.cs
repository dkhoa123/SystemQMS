namespace SystemQMS.ViewModels.EvaluationsDto
{
    // Mẫu đánh giá (RTG-05) — contentQuestion là JSON gọn:
    // {"qs":[{"q":"...","t":"s|c|x","o":[...],"r":0|1}]}
    // s = sao, c = trắc nghiệm, x = nhập tự do; r = bắt buộc.
    // 4 câu sao đầu tiên ánh xạ lần lượt timeMark/attitudeMark/materialMark/satisfedMark.
    public class EvaluationTemplateDto
    {
        public int id { get; set; }
        public int unitId { get; set; }
        public string templateId { get; set; } = "";
        public string templateName { get; set; } = "";
        public string contentQuestion { get; set; } = "";
        public bool isActive { get; set; }
    }

    public class EvaluationTemplateUpsertDto
    {
        public int unitId { get; set; }
        public string templateId { get; set; } = "";
        public string templateName { get; set; } = "";
        public string contentQuestion { get; set; } = "";
    }
}
