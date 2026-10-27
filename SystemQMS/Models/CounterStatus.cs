using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemQMS.Models
{
    public static class CounterStatus
    {
        public const string Idle = "Idle";
        public const string Serving = "Serving";
        public const string Closed = "Closed";
        public const string PausedAccepting = "PausedAccepting";
    }
}
