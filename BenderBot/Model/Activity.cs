using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xphp.Model
{
    public class Activity
    {
        public int ActivityId { get; set; }

        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public int PlayerId { get; set; }
        public virtual Player Player { get; set; }

        public int ServerId  { get; set; }
        public Server Server  { get; set; }
    }
}
