namespace SystemQMS.Models
{
    // RTG: thang điểm đánh giá — 1 = Tệ, 2 = Bình thường, 3 = Tốt, 0 = chưa đánh giá.
    // Dùng chung cho danh sách/chi tiết (EvaluationSvc), xuất Excel và DSH-05 sau này.
    public class EvaluationMarks
    {
        public const int Unrated = 0;
        public const int Bad = 1;
        public const int Normal = 2;
        public const int Good = 3;

        public static string Label(int mark) => mark switch
        {
            Unrated => "Chưa đánh giá",
            Bad => "Tệ",
            Normal => "Bình thường",
            Good => "Tốt",
            _ => "Không xác định"
        };

        // Chuỗi "nội dung" của RTG-01
        public static string BuildContent(Evaluation e) =>
           $"Thời gian xử lý: {Label(e.timeMark)} / Thái độ nhân viên: {Label(e.attitudeMark)} / " +
           $"Vật chất trang thiết bị: {Label(e.materialMark)} / Mức độ hài lòng của bạn: {Label(e.satisfedMark)}";


    }
}
