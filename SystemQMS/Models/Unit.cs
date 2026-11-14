using System.ComponentModel.DataAnnotations;

namespace SystemQMS.Models
{
    public class Unit
    {
        [Key]
        public int UnitId { get; set; }
        [MaxLength(100)]
        public string UnitName { get; set; }
        [MaxLength(100)]
        public string ShowName { get; set; }
        [MaxLength(10)]
        public string PhoneHelp { get; set; }
        [MaxLength(100)]
        public string EmailHelp { get; set; }
        [MaxLength(255)]
        public string Address { get; set; }
        public byte[]? Logo { get; set; }
        public bool IsBan { get; set; }

        public ICollection<Users> Users { get; set; } = new List<Users>();
        public ICollection<Service> Services { get; set; } = new List<Service>();

        public ICollection<Guest> Guests { get; set; } = new List<Guest>();

        public ICollection<Diary> Diaries { get; set; } = new List<Diary>();
        public ICollection<Counter> Counters { get; set; } = new List<Counter>();
        public ICollection<Ticket > Tickets { get; set; } = new List<Ticket>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
        public ICollection<EvaluationTemplate> EvaluationTemplates { get; set; } = new List<EvaluationTemplate>();

        public ICollection<Device> Devices { get; set; } = new List<Device>();
        public virtual UnitConfig UnitConfig { get; set; }




    }
}
