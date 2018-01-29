using xphp.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xphp
{
    public static class db
    {
        internal static void BlackList(string playerName)
        {
            using (DataContext contexto = new DataContext())
            {
                var player = getPlayer(playerName);

                if (player != null)
                {
                    player.status = 2;
                    contexto.Entry(player).State = System.Data.Entity.EntityState.Modified;
                    contexto.SaveChanges();
                }
            }
        }
        internal static void WhiteList(string playerName)
        {
            using (DataContext contexto = new DataContext())
            {
                var player = getPlayer(playerName);

                if (player != null)
                {
                    player.status = 1;

                    contexto.Entry(player).State = System.Data.Entity.EntityState.Modified;
                    contexto.SaveChanges();
                }
            }
        }

        internal static Player getPlayer(string name)
        {
            Player player = null;

            using (DataContext contexto = new DataContext())
            {
                var players = contexto.Players.Where(u => u.Name.Equals(name, StringComparison.InvariantCulture)).ToList();
                
                foreach (var p in players)
                {
                    if (name.Equals(p.Name, StringComparison.InvariantCulture))
                    {
                        player = p;
                        break;
                    }
                }
            }
            return player;
        }

        internal static void AddDescription(string playerName, string description)
        {
            using (DataContext contexto = new DataContext())
            {
                var player = getPlayer(playerName);

                player.Descripcion= description;

                contexto.Entry(player).State = System.Data.Entity.EntityState.Modified;
                contexto.SaveChanges();
            }
        }

        internal static string Historic(string server, string fecha)
        {
            StringBuilder message = new StringBuilder();


            DateTime dt = DateTime.ParseExact(fecha, "dd/MM/yy HH:mm", CultureInfo.InvariantCulture);

            using (DataContext contexto = new DataContext())
            {
                var servers = contexto.Servers
                    .Where(s => s.Title.Contains(server)).ToList();

                if (servers.Count == 0)
                {
                    message.AppendLine("No tengo ningun servidor con ese nombre");
                }
                else if (servers.Count > 1)
                {
                    message.AppendLine("Hay más de un servidor con ese nombre, especifica más!");
                    foreach (var s in servers)
                    {
                        message.AppendLine(s.Title + " " +s.Address + ":" + s.Port);
                    }
                }
                else
                {
                    message.AppendLine("```Markdown");
                    message.AppendLine("#" + servers[0].Title + " Usuarios Online a las " + dt);
                    message.AppendLine("```");

                    var serverid = servers[0].ServerId;
                    var query = contexto.Activities
                        .Include("Player")
                        .Where(a => a.ServerId == serverid && a.Start < dt && ((a.End ?? DateTime.MaxValue) > dt));
                        
                    var activities = query.ToList();

                    message.AppendLine("```diff");
                    message.AppendLine("              Nombre |       Inicio        |        Fin        | Descripción");
                    message.AppendLine("----------------------------------------------------------------------------");

                    foreach (var activity in activities)
                    {
                        message.AppendLine(Query.checkWhiteList(activity.Player.status) + 
                            Query.parseCharacterName(activity.Player.Name) + " | " + 
                            activity.Start + " | " +  
                            activity.End.ToString().PadLeft(17) + " | " + 
                            activity.Player.Descripcion);
                    }
                    message.AppendLine("```");



                }
                return message.ToString();
            }

        }
        internal static List<Server> getServers(string name)
        {
            using (DataContext contexto = new DataContext())
            {
                return contexto.Servers.Where(u => u.Title.Contains(name)).ToList();
            }
        }
    }
}
