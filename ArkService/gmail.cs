using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S22.Imap;
using System.Net.Mail;
using Discord;

namespace xphps
{
    public class gmail
    {
        public async void test()
        {
            try
            {
                //todo: change credential
                using (ImapClient Client = new ImapClient("imap.gmail.com", 993,
                    "[ENTER gmail account]", "[ENTER gmail pass]", AuthMethod.Login, true))
                {
                    Console.WriteLine("We are connected!");

                    IEnumerable<uint> uids = Client.Search(SearchCondition.Unseen().And(SearchCondition.From("community@survivetheark.com")));


                    if (uids.Count() > 0)
                    {
                        IEnumerable<MailMessage> messages = Client.GetMessages(uids);

                        StringBuilder message = new StringBuilder();
                        message.AppendLine("```Markdown");
                        message.AppendLine("#Notisias freshcas");
                        message.AppendLine("```");

                        foreach (var mess in messages)
                        {
                            message.Append(mess.Body.Substring(mess.Body.IndexOf("=====") + 5, mess.Body.IndexOf("=====", 50) - mess.Body.IndexOf("=====") - 5).Replace("\r", "").Replace("\n", "").Replace("&#039", "'") + "\r");
                        }

                        var client = new DiscordClient();
                        //todo: change token
                        await client.Connect("[PON AKI TU TOKEN]", TokenType.Bot);
                        await SendMessage(client.GetChannel(289034769159946240), message.ToString());
                    }
                }
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
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

