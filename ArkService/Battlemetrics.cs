using HtmlAgilityPack;
using Newtonsoft.Json;
using QueryMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xphps.Model;

namespace xphps
{
    public class BattleMetrics
    {

        public void FillServerListJsonParse()
        {
            List<Model.Server> serverlist = new List<Model.Server>();
            int i = 1;
            HtmlWeb web = new HtmlWeb();

            do
            {
                HtmlDocument doc = web.Load("https://www.battlemetrics.com/servers/search?feature%3A8=True%2C+official&game=ark&page="+i);

                dynamic djson = doc.GetElementbyId("storeBootstrap");

                var js = JsonConvert.DeserializeObject(djson.InnerText);
                var servers = js.ServerList.servers.ark.servers.results;
           
                foreach (var s in servers)
                {
                    int port = (int)s.port.Value;
                    int id = (int)s.id.Value;
                    
                    if (!(s.name.Value.ToLower().Contains("primitive")
                                                 || s.name.Value.ToLower().Contains("hardcore")
                                                 || s.name.Value.ToLower().Contains("procedural")
                                                 || s.name.Value.ToLower().Contains("extinction")
                                                 || s.name.Value.ToLower().Contains("pve")))
                    {
                        serverlist.Add(new Model.Server
                        {
                            Title = ParseServerName(s.name.Value),
                            Address = s.ip.Value,
                            Port = FixPort(port),
                            BattleMetricsId = id,
                            Location = s.country.Value
                        });
                    }
                }
                i++;
            } while (i < 102);

            using (DataContext contexto = new DataContext())
            {
                var dbservers = contexto.Servers.ToList();

                foreach (var s in serverlist)
                {
                    if (dbservers.Where(bds => bds.Address == s.Address && bds.Port == s.Port).Count() == 0)
                        contexto.Set<Model.Server>().Add(s);
                }
                contexto.SaveChanges();
            }
           

        }

        public void FillServerListHtmlParse()
        {
            HtmlWeb web = new HtmlWeb();
            int i = 1;
            List<Model.Server> serverlist = new List<Model.Server>();
            do
            {
                HtmlDocument doc = web.Load("https://www.battlemetrics.com/servers/search?feature%3A8=True%2C+official&game=ark&page=" + i);

                doc.Save(@"C:\temp\p.html");
                var query = (from table in doc.DocumentNode.SelectNodes("//table//tbody").Cast<HtmlNode>()
                             from row in table.SelectNodes("tr").Cast<HtmlNode>()
                             select new
                             {
                                 Server = row.Descendants("td").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("server-name")).FirstOrDefault().InnerText,
                                 Address = row.Descendants("td").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("server-address")).FirstOrDefault().InnerText,
                             }).ToList();


                var parsed = (from q in query 
                              select new Model.Server
                              {
                                  Title = ParseServerName(q.Server),
                                  Address = q.Address.Split(':')[0],
                                  Port = FixPort(int.Parse(q.Address.Split(':')[1]))
                              }).ToList();


                var rejectList = parsed.Where(p => p.Title.ToLower().Contains("sotf") 
                                                || p.Title.ToLower().Contains("primitive") 
                                                || p.Title.ToLower().Contains("hardcore") 
                                                || p.Title.ToLower().Contains("procedural") 
                                                || p.Title.ToLower().Contains("extinction") 
                                                || p.Title.ToLower().Contains("pve")).ToList();

                var filteredList = parsed.Except(rejectList).ToList();


                serverlist.AddRange(filteredList);
                i++;
            } while (i < 102);

            using (DataContext contexto = new DataContext())
            {
                var dbservers = contexto.Servers.ToList();

                foreach (var s in serverlist)
                {
                    if (dbservers.Where(bds => bds.Address == s.Address && bds.Port == s.Port).Count() == 0)
                        contexto.Set<Model.Server>().Add(s);
                }
                contexto.SaveChanges();
            }
        }

        public void FillUsers()
        {

            using (DataContext contexto = new DataContext())
            {
                var dbservers = contexto.Servers.ToList();

                foreach (var s in dbservers)
                {
                    FillUsers(s.BattleMetricsId);
                }
            }
        }

        public void FillUsers(int id)
        {
            List<Model.Player> players = new List<Model.Player>();

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load("https://www.battlemetrics.com/servers/ark/" + id);

            dynamic djson = doc.GetElementbyId("storeBootstrap");

            var dynamicJson = JsonConvert.DeserializeObject(djson.InnerText);


            foreach (var s in dynamicJson.Server.servers)
            {
                foreach (var p in s.First.players)
                {
                    players.Add(new Model.Player {
                        Name = p.identifiers[0].identifier,
                        BattlemetricsId = p.identifiers[0].player_id
                    });
                    
                }
            }

            using (DataContext contexto = new DataContext())
            {
                var dbplayers = contexto.Players.ToList();

                foreach (var player in players)
                {
                    var dbplayer = dbplayers.Where(bds => bds.Name.Equals(player.Name, StringComparison.InvariantCulture)).FirstOrDefault();

                    if (dbplayer == null)
                    {
                        contexto.Set<Model.Player>().Add(player);
                    }
                    else if (dbplayer.BattlemetricsId == 0)
                    {
                        dbplayer.BattlemetricsId = player.BattlemetricsId;
                    }    
                }
                contexto.SaveChanges();
            }


        }

        private int FixPort(int port)
        {
            switch (port)
            {
                case 7777:
                    port = 27015;
                    break;
                case 7779:
                    port = 27017;
                    break;
                case 7781:
                    port = 27019;
                    break;
                case 7783:
                    port = 27021;
                    break;
            }
            return port;
        }

        public static string ParseServerName(string server)
        {
            return server.Substring(0, server.IndexOf(" (v") - 2);
        }
    }
}
