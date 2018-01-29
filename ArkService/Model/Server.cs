using System.Collections.Generic;

namespace xphps.Model
{
    public class Server
    {
        public int ServerId { get; set; }
        public string Title { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public int BattleMetricsId { get; set; }
        public virtual List<Activity> Activities { get; set; }
    }
}