using Discord;
using Discord.WebSocket;

using NLog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscrodBot
{
    public static class Logic
    {
        static Logger nlog;
        public static void init()
        {
            nlog = LogManager.GetCurrentClassLogger();
        }
        public static bool containsEmoji(string content,IEnumerable<string> tags)
        {
            var reg = Regex.Match(content, "^(\s*(<:[^:]+:[0-9]+>|\p{Cs})\s*)+$");
            return match.Success;
        }


        public static IEnumerable<string> emotesTags(IReadOnlyCollection<ITag> tags)
        {
            return tags.Select(s => s.ToString().Split(" ")[0]).Distinct();
        }

        public static async Task<IMessage> LastMessage(ISocketMessageChannel channel, SocketUser user)
        {
            IEnumerable<IMessage> hismessages = await channel.GetMessagesAsync(20).FlattenAsync();
            var lastmessage = hismessages
                .Where(u => u.Author.Id == user.Id)
                .OrderByDescending(m => m.CreatedAt)
                .Skip(1)
                .FirstOrDefault(m => 
                    !containsEmoji(m.Content, emotesTags(m.Tags))
                    );
            if (lastmessage == null) return null;

            return lastmessage;
        }

        public static bool emojiFreeInterval(DateTime? a, DateTime? b, TimeSpan allowedinterval)
        {
            if (a - b < allowedinterval) return true;
            return false;
        }

        public static async Task<bool> DeleteStupidEmojis(SocketMessage message)
        {
            if (containsEmoji(message.Content,emotesTags(message.Tags)))
            {
                nlog.Info($"{message.Author} : {message.Content} ");
                IMessage lastmessage = await LastMessage(message.Channel, message.Author);
                DateTime? currentMessage = message.CreatedAt.UtcDateTime;
                DateTime? previousMessage = lastmessage.CreatedAt.UtcDateTime;
                if (previousMessage == null)
                {
                    nlog.Info($"{message.Author} no previous messages. Deleting");

                    await message.DeleteAsync();
                    return true;
                }
                if (!emojiFreeInterval(currentMessage, previousMessage, TimeSpan.FromSeconds(5)))
                {
                    nlog.Info($"{message.Author} posted with no message.");
                    await message.DeleteAsync();
                    await CreateReactions(lastmessage);
                   
                    return true;
                }
            }
            return false;
        }

        static List<Emoji> emojis = new List<Emoji>()
        {
               new Emoji("🇮"),
               new Emoji("◻️"),
               new Emoji("🅰️"),
               new Emoji("🇲"),
               new Emoji("⬜"),
               new Emoji("🇬"),
               new Emoji("🇦"),
               new Emoji("🇾")
        };

        public static async Task CreateReactions(IMessage message)
        {

            foreach (var item in emojis)
            {
              await  message.AddReactionAsync(item);
            }
        }
        
    }
}
