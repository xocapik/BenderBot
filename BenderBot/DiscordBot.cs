using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xphp
{

    public class DiscordBot
    {
        DiscordClient client;
        CommandService commands;
        

        public DiscordBot()
        {
            client = new DiscordClient(i =>
            {
                i.LogLevel = LogSeverity.Info;
                i.LogHandler = Log;
            });

            client.UsingCommands(i =>
            {
                i.PrefixChar = '!';
                i.AllowMentionPrefix = true;
                i.HelpMode = HelpMode.Private;
            });

            commands = client.GetService<CommandService>();
            
            commands.CreateCommand("Chiste")
                .Alias("habla", "di")
                .Description("Haz que bender hable")
                .Do(async (e) =>
            {
               await Chiste(e);
            });

            commands.CreateCommand("Jugadores")
                .Alias("players", "p")
                .Description(@"Jugadores online en un servidor
                             Ejemplo: !p 113")
                .AddCheck((cmd, user, channel) => channel.Name == "bot" || channel.IsPrivate)
                .Parameter("servidor",ParameterType.Required).Do(async (e) =>
            {
                await PlayerInfo(e);
            });

            commands.CreateCommand("Informacion")
                .Alias("info", "i")
                .Description(@"Información del servidor
                             Ejemplo: !i 113")
                .AddCheck((cmd, user, channel) => channel.Name == "bot" || channel.IsPrivate)
                .Parameter("servidor", ParameterType.Required).Do(async (e) =>
            {
                await ServerInfo(e);
            });

            commands.CreateCommand("Reglas")
                .Alias("rules", "r")
                .Description(@"Reglas del servidor (útil para saber si se pueden descargar jugadores a la nube o items)
                             Ejemplo: !r 113")
                .AddCheck((cmd, user, channel) => channel.Name == "bot" || channel.IsPrivate)
                .Parameter("servidor", ParameterType.Required).Do(async (e) =>
            {
                await ServerRules(e);
            });

            commands.CreateCommand("Whitelist")
                .Alias("listablanca", "lb")
                .Description(@"Añadir jugador a lista blanca (aparecerá en verde en la lista de jugadores de un servidor)
                              Ejemplo: !lb pako")
                .AddCheck((cmd, user, channel) => channel.Name == "bot" || channel.IsPrivate)
                .Parameter("jugador", ParameterType.Unparsed).Do(async (e) =>
                {
                    await WhiteList(e);
                });

            commands.CreateCommand("BlackList")
                .Alias("listanegra", "bl")
                .Description(@"Añadir jugador a lista negra (aparecerá en rojo en la lista de jugadores de un servidor)
                               Ejemplo: !bl pako")
                .AddCheck((cmd, user, channel) => channel.Name == "bot" || channel.IsPrivate)
                .Parameter("jugador", ParameterType.Unparsed).Do(async (e) =>
                {
                    await BlackList(e);
                });

            commands.CreateCommand("Descripcion")
                .Alias("description", "d")
                .Description(@"Añadir descripción de jugador (aparecerá en la lista de jugadores de un servidor, útil para saber quien coño es el de los numeritos)
                               Ejemplos: !d pako:::puto amo] 
                                         !d Akt:::Muy pesado, matarle si lo veis.")
                .AddCheck((cmd, user, channel) => channel.Name == "bot" || channel.IsPrivate )
                .Parameter("jugador:::descripcion", ParameterType.Unparsed).Do(async (e) =>
                {
                    await Description(e);
                });

            commands.CreateCommand("Historico")
                .Alias("history", "h")
                .Description(@"Muestra los jugadores que habia online a determinada fecha. formato fecha dd-MM-yyyy HH:mm
                               Ejemplos: !h 113 07-03-17 09:00")
                .AddCheck((cmd, user, channel) => channel.Name == "bot" || channel.IsPrivate || user.Name == "pako")
                .Parameter("server", ParameterType.Required)
                .Parameter("fecha", ParameterType.Unparsed).Do(async (e) =>
                {
                    await Historic(e);
                });

            commands.CreateCommand("Rastrear")
                .Alias("track", "t")
                .Description(@"Muestra links para rastrear a un jugador
                               Ejemplos: !r Akt")
                .AddCheck((cmd, user, channel) => channel.Name == "bot" || channel.IsPrivate)
                .Parameter("jugador", ParameterType.Unparsed).Do(async (e) =>
                {
                    await Track(e);
                });

            commands.CreateCommand("ScanPlayers")
                .Description(@"Escanear juegos battlemetics")
                .AddCheck((cmd, user, channel) => channel.IsPrivate || user.Name == "pako")
                .Do(async (e) =>
                {
                    await ScanPlayers(e);
                });

            commands.CreateCommand("ScanServers")
                .Description(@"Escanear servidores battlemetics")
                .AddCheck((cmd, user, channel) => channel.IsPrivate || user.Name == "pako")
                .Do(async (e) =>
                {
                    await ScanServers(e);
                });

            client.ExecuteAndWait(async () =>
            {
                //todo: change token
                await client.Connect("[PON AKI TU TOKEN]", TokenType.Bot);
            }); 
        }

        private async Task ScanPlayers(CommandEventArgs e)
        {
            var bt = new BattleMetrics();
            bt.FillUsers();
            await e.Channel.SendMessage("He acabado de escanear players... uf!");
        }

        private async Task ScanServers(CommandEventArgs e)
        {
            var bt = new BattleMetrics();
            bt.FillServerListJsonParse();
            await e.Channel.SendMessage("He acabado de escanear servers... uf!");
        }

        private async Task Track(CommandEventArgs e)
        {
            StringBuilder message = new StringBuilder();

            var player = e.Args[0];

            var dbplayer = db.getPlayer(player);

            if (dbplayer ==null || dbplayer.BattlemetricsId == 0)
            {
                message.AppendLine("Battlemetrics");
                message.AppendLine("no encontrado");
            }
            else
            {
                message.AppendLine("Battlemetrics");
                message.AppendLine("https://www.battlemetrics.com/players/" + dbplayer.BattlemetricsId);
            }
            
            message.AppendLine("Ark Traker");
            message.AppendLine("http://www.arktracker.com/players/view/" + player);
            message.AppendLine("Steam");
            message.AppendLine("https://steamcommunity.com/search/users/#filter=users&text=" + player);
            message.AppendLine("Teamspeak");
            message.AppendLine("https://www.tsviewer.com/index.php?page=search&action=ausgabe_user&nickname=" + player);
            
            
            

            await e.Channel.SendMessage(message.ToString());
        }

        private async Task Historic(CommandEventArgs e)
        {
            var server = e.Args[0];
            var fecha = e.Args[1];

            await SendMessage(e.Channel, db.Historic(server, fecha));
        }

        private async Task Description(CommandEventArgs e)
        {
            var args = e.Args[0].Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
            var playerName = args[0];
            var description = args[1];
            
            db.AddDescription(playerName, description);
        }

        private async Task BlackList(CommandEventArgs e)
        {
            var playerName = e.Args[0];
            db.BlackList(playerName);
        }

        private async Task WhiteList(CommandEventArgs e)
        {
            var playerName = e.Args[0];
            db.WhiteList(playerName);
        }

        private async Task PlayerInfo(CommandEventArgs e)
        {
            var serverName = e.Args[0];
            await SendMessage(e.Channel, Query.ServerInfo(serverName, false, false, true));
        }

        private async Task ServerInfo(CommandEventArgs e)
        {
            var serverName = e.Args[0];
            await SendMessage(e.Channel, Query.ServerInfo(serverName, true, false, false));
        }

        private async Task ServerRules(CommandEventArgs e)
        {
            var serverName = e.Args[0];
            await SendMessage(e.Channel, Query.ServerInfo(serverName, false, true, false));
        }

        private void Log(object sender , LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private async Task Chiste(CommandEventArgs e)
        {
            #region chiste
            var chistes = new string[] {
        " ¿Una robopilingui de trecientos pavos o trescientas robopilinguis de a dolar?",
        " Tengo que dejar de atropellar gente.No soy lo suficientemente famoso como para librarme.",
        " ¿Que clase de fiesta es esta?, ¡No hay alcohol y sólo se ve una furcia!",
        " ¡Qué horrible pesadilla! ¡Unos y ceros por todas partes!Hasta me pareció ver un 2",
        " Funcionan igual que los demás organismos vivos, disparándose el ADN unos a otros para fabricar crías. ¡Es insultante!",

        " Bender: “Díselo a mi brillante culo metálico. " + Environment.NewLine +
        "Fry: “A mi no me parece tan brillante… " + Environment.NewLine +
        "Bender: “Más que el tuyo cacho carne.”",

        " Bender: “Tras hojear su carta de vinos aguardentosos, he seleccionado Delicia del Vagabundo del 71, Chateau La Juerga del 57 y Sobignon Melopea del 66.”" + Environment.NewLine +
        " Camarero: “Exquisita elección señor.”" + Environment.NewLine +
        " Bender: “Y… mezclemelos todos en una jarra grande.",

        " Fry: “¿Tú a que partido votas, Bender ?”" + Environment.NewLine +
        " Bender: “No.Yo no puedo votar.”" + Environment.NewLine +
        " Fry: “¿Por ser robot ?”" + Environment.NewLine +
        " Bender: “No.Criminal convicto.”",

        " ¿Han probado alguna vez a apagar la tele, sentarse con sus hijos y darles una paliza?",

        " Todos le amaron cuando era Bender el que Ofende, ahora todos le odiarán por ser… Indefinido Bender",

        " Dile a Don Bot que dejo el crimen organizado, a partir de ahora me dedicaré sólo al crimen normal",

        " Siempre quise saber si puedo fastidiar a la gente más de lo que la fastidio",

        " Llámame anticuado, pero me gusta que un abandono sea tan imprevisto como cruel",

        " Ahh, ¿no hay sitio para Bender, eh? Vale, me construiré mi propio módulo lunar, con casinos, y furcias.Es más, paso de la nave lunar… y de los casinos. ¡Al cuerno todo!",

        "El cuerpo es para las furcias y los tios gordos, yo solo necesito pasta alrededor de mi cabeza",

        " Te juro que es cierto, tengo a tu Dios como mi testigo",

        " ¡No quiero un amigo! ¡Quiero una operación de cambio de sexo y la quiero ahora!",

        " Fry, de todos los amigos que he tenido, tú… el único",

        " Se hizo popular cuando prometió que no mataría a todo el que se encontrara a su paso",

        " La abogada soltera, lucha por su cliente, lleva minifaldas provocativas y además es autosuficiente. ¿A que no lo hago mal?",

        " Y a pesar de que el ordenador estaba apagado y desenchufado, una imagen permanecía en la pantalla… era… ¡¡el logotipo de Windows!!",

        " No quiero morir, todavía hay muchas cosas que no tengo",

        " Alguien: ¿No te importa vivir con un humano? Bender: No, siempre quise tener una mascota",

        " ¿Activar antivirus? ¡Me estoy bajando porno!No ",

        " Chantaje es una palabra muy fea, yo prefiero… extorsión, la X le da mucha clase ",

        " La llevaré con orgullo y la empeñaré en cuanto pueda ",

        " Comparad vuestras vidas con la mia… y luego podeis suicidaros"
    };
            #endregion

            Random random = new Random();
            await e.Channel.SendMessage(chistes.GetValue(random.Next(chistes.Length)).ToString());

        }

        private async Task SendMessage(Channel ch, string msg)
        {
            StringBuilder sb = new StringBuilder();

            var messages = StringExtension.Split(msg, 1980).ToList();

            if (messages.Count > 1)
            {
                await ch.SendMessage(messages[0] + "```");

                for (int i = 1; i < messages.Count - 1; i++)
                {
                    sb = new StringBuilder();
                    sb.AppendLine("```diff");
                    sb.AppendLine(messages[i]);
                    sb.AppendLine("```");
                    await ch.SendMessage(sb.ToString());
                }
                sb = new StringBuilder();
                sb.AppendLine("```diff");
                sb.AppendLine(messages[messages.Count - 1]);
                await ch.SendMessage(sb.ToString());
            }
            else
            {
                await ch.SendMessage(messages[0]);
            }
        }

    }
}
