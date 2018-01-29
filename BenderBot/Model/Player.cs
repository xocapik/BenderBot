using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xphp.Model
{
    public class Player
    {
        public int PlayerId { get; set; }
        public string Name { get; set; }
        public int status { get; set; }
        public string Descripcion { get; set; }
        public int BattlemetricsId { get; set; }
        public virtual List<Activity> Activities { get; set; }
    }
}
