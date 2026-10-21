using System.ComponentModel.DataAnnotations;

namespace SystemQMS.Models
{
    public class Unit
    {
        [Key]
        public int unitId { get; set; }
        [MaxLength(100)]
        public string unitName { get; set; }
        [MaxLength(100)]
        public string showName { get; set; }
        [MaxLength(10)]
        public string phoneHelp { get; set; }
        [MaxLength(100)]
        public string emailHelp { get; set; }
        [MaxLength(255)]
        public string address { get; set; }
        public byte[]? logo { get; set; }
        public bool isBan { get; set; }

        public ICollection<Users> users = new List<Users>();
        public ICollection<Service> services = new List<Service>();

        public ICollection<Guest> guests = new List<Guest>();

        public ICollection<Diary> diaries = new List<Diary>();
        public ICollection<Counter> counters = new List<Counter>();
        public ICollection<Sticker > stickers = new List<Sticker>();
        public ICollection<Notification> noti = new List<Notification>();
        public ICollection<Evaluation> evaluations = new List<Evaluation>();
        public ICollection<EvaluationTemplate> evaluationTemplates = new List<EvaluationTemplate>();





    }
}
