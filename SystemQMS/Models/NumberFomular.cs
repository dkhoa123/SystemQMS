
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class NumberFomular
    {
        public int id { get; set; }

     
        public int serviceId { get; set; }
        public int? sharedNextNumber { get; set; }



        [MaxLength(100)]
        public string numberType { get; set; } = "Prefix";


        [MaxLength(20)]
        public string firstName { get; set; } = "";

  
        [MaxLength(20)]
        public string lastName { get; set; } = "";

        
        public int startNumber { get; set; }

        public int endNumber { get; set; }

    
        public bool isDailyLimitEnabled { get; set; } = false;

        public int? dailyEndNumber { get; set; }

       
        public DateTimeOffset limitDay { get; set; }

        // Shared hoặc Independent
        [MaxLength(20)]
        public string sequenceMode { get; set; } = "Independent";


        [MaxLength(20)]
        public string formatSource { get; set; } = "ServiceCode";


        [ForeignKey("serviceId")]
        public virtual Service? Service { get; set; }
    }
}
