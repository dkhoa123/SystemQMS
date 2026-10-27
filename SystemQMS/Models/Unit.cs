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

        public ICollection<Users> Users { get; set; } = new List<Users>();
        public ICollection<Service> Services { get; set; } = new List<Service>();

        public ICollection<Guest> Guests { get; set; } = new List<Guest>();

        public ICollection<Diary> Diaries { get; set; } = new List<Diary>();
        public ICollection<Counter> Counters { get; set; } = new List<Counter>();
        public ICollection<Sticker > Stickers { get; set; } = new List<Sticker>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
        public ICollection<EvaluationTemplate> EvaluationTemplates { get; set; } = new List<EvaluationTemplate>();
        public virtual UnitConfig UnitConfig { get; set; }




    }
}
