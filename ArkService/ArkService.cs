
using xphps.Model;
using QueryMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;

namespace xphps
{
    public static class xphps
    {
        const uint appId = 346110;
        const int retries = 1;

        public static void Run(string address, ushort port)
        {
            var activities = new List<Activity>();

            //Since we set throwExceptions to false,no exceptions would be thrown
            using (var server = ServerQuery.GetServerInstance((Game)appId, address, port,false,  4000,  4000))
            {
                //get player information
                var playerInfo = server.GetPlayers(x => Console.WriteLine("Fetching Player Information, Attempt " + x));
                if (playerInfo == null)
                {
                    Console.WriteLine("Error fetching player information.");
                }
                else
                {
                    var players = playerInfo.ToList();

                    using (DataContext contexto = new DataContext())
                    {
                        //obtengo Server, si no existe lo creo
                        var dbserver = contexto.Servers
                            .Where(s => s.Address == address && s.Port == port)
                            .FirstOrDefault();
                        if (dbserver == null)
                        {
                            var serverInfo = server.GetInfo(x => Console.WriteLine("Fetching Server Information, Attempt " + x));

                            dbserver = new Model.Server
                            {
                                Title = parseServerName(serverInfo.Name),
                                Address = address,
                                Port = port
                            };
                            dbserver = contexto.Servers.Add(dbserver);
                            contexto.SaveChanges();
                        }
                        else
                        {
                            var serverId = dbserver.ServerId;

                            //actividades abiertas del servidor
                            activities = contexto.Activities
                                .Include(a => a.Player)
                                .Where(a => a.End == null && a.ServerId == serverId).ToList();

                            foreach (var act in activities)
                            {
                                //si no sigue conectado actualizo
                                var player = players.Where(p => p.Name == act.Player.Name).FirstOrDefault();
                                if (player == null)
                                {
                                    act.End = DateTime.Now;
                                    contexto.SaveChanges();
                                }
                            }

                        }

                        //actividades nuevas
                        foreach (var player in players)
                        {
                            if (player.Name != "")
                            {

                                //si no existe usuario lo añado
                                var dbplayers = contexto.Players.Where(u => u.Name.Equals(player.Name, StringComparison.InvariantCulture)).ToList();
                                Model.Player dbplayer = null;
                                foreach (var p in dbplayers)
                                {
                                    if (player.Name.Equals(p.Name, StringComparison.InvariantCulture))
                                    {
                                        dbplayer = p;
                                        break;
                                    }
                                }

                                if (dbplayer == null)
                                {
                                    dbplayer = new Model.Player
                                    {
                                        Name = player.Name
                                    };
                                    dbplayer = contexto.Players.Add(dbplayer);
                                    contexto.SaveChanges();

                                }

                                if (activities.Where(a => a.Player.PlayerId == dbplayer.PlayerId).Count() == 0)
                                {
                                    var activity = new Activity
                                    {
                                        Start = DateTime.Now,
                                        PlayerId = dbplayer.PlayerId,
                                        ServerId = dbserver.ServerId,
                                        End = null
                                    };
                                    contexto.Activities.Add(activity);
                                    contexto.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }

        }

        public static string parseServerName(string server)
        {
            return server.Substring(0, server.IndexOf(" - "));
        }

        
    }
}
