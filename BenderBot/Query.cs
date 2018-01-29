using QueryMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xphp
{
    public static class Query
    {
        

        public static string ServerInfo(string ServerName , bool showServer, bool showRules, bool showPlayers ) {

            try
            {
                string address = "";
                uint appId = 346110;
                ushort port = 27015;
                int retries = 5;
                StringBuilder message = new StringBuilder();
                int leak = 0;

                //switch (ServerName)
                //{
                //    case "113":
                //        address = "31.214.160.104";
                //        port = 27019;
                //        break;
                //    case "662":
                //        address = "85.190.155.67";
                //        port = 27015;
                //        break;
                //    case "414":
                //        address = "31.214.160.214";
                //        port = 27021;
                //        break;
                //}
                var servers = db.getServers(ServerName);

                if (servers.Count == 0)
                {
                    message.AppendLine("No tengo ningun servidor con ese nombre");
                }
                else if (servers.Count > 1)
                {
                    message.AppendLine("Hay más de un servidor con ese nombre, especifica más!");
                    foreach (var s in servers)
                    {
                        message.AppendLine(s.Title + " " + s.Address + ":" + s.Port);
                    }
                }
                else
                {
                    address = servers[0].Address;
                    port = (ushort)servers[0].Port;

                    //Since we set throwExceptions to false,no exceptions would be thrown
                    using (var server = ServerQuery.GetServerInstance((QueryMaster.Game)appId, address, port,false,   4000, 4000))
                    {
                        //Get Server Information
                        var serverInfo = server.GetInfo(x => Console.WriteLine("Fetching Server Information, Attempt " + x));
                        if (serverInfo == null)
                            Console.WriteLine("Error accediendo al servidor.");

                        message.AppendLine("```Markdown");
                        message.AppendLine("#" + serverInfo.Name + "  " + serverInfo.Players + "/" + serverInfo.MaxPlayers);
                        message.AppendLine("```");
                        if (showServer)
                        {
                            message.AppendLine("```JSON");
                            message.AppendLine(serverInfo.ToString());
                            message.AppendLine("```");
                        }

                        //Get Server rules. 
                        if (showRules)
                        {
                            var serverRules = server.GetRules(x => Console.WriteLine("Fetching Server Rules, Attempt " + x));
                            if (serverRules == null)
                            {
                                Console.WriteLine("Error obteniendo reglas.");
                            }
                            else
                            {
                                message.AppendLine("```JSON");
                                message.AppendLine(serverRules.ToString());
                                message.AppendLine("```");
                            }
                        }


                        if (showPlayers)
                        {
                            //get player information
                            var playerInfo = server.GetPlayers(x => Console.WriteLine("Fetching Player Information, Attempt " + x));
                            if (playerInfo == null)
                            {
                                message.Append("Error obteniendo usuarios.");
                            }
                            else
                            {
                                message.AppendLine("```diff");
                                message.AppendLine("              Nombre |  Online  | Descripción");
                                message.AppendLine("------------------------------------------");
                                foreach (var player in playerInfo)
                                {
                                    if (player.Name != "")
                                    {
                                        var dbPlayer = db.getPlayer(player.Name);
                                        if (dbPlayer != null)
                                        {
                                            message.AppendLine(checkWhiteList(dbPlayer.status) + parseCharacterName(player.Name) + " | " + player.Time.ToString("hh\\:mm\\:ss") + " | " + dbPlayer.Descripcion);
                                        }
                                        else
                                        {
                                            message.AppendLine(" " + parseCharacterName(player.Name) + " | " + player.Time.ToString("hh\\:mm\\:ss") + " | ");
                                        }
                                    }
                                    else
                                    {
                                        leak++;
                                    }

                                }

                                message.AppendLine("Usuarios en memoria pero offline: " + leak);
                                message.AppendLine("```");
                            }
                        }
                    }
                }
                return message.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error " + e.Message);
                return string.Empty;
            }
        }
        


        public static string parseCharacterName(string name)
        {
            var array = name.ToCharArray();

            name = name.Replace((char)65039, '?');
            name = name.Replace((char)10240, '?');
            name = name.Replace((char)4442, '?');

            return name.PadLeft(19);
        }

        public static string checkWhiteList(int status)
        {
            string s = " ";

            switch (status)
            {
                case 0:
                    break;
                case 1:
                    s = "+";
                    break;
                case 2:
                    s = "-";
                    break;
            }
            return s;
        }
    }
}
