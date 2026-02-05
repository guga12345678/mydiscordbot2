using Discord;
using System;
using System.Threading.Tasks;

namespace TutorialBot.Modules
{
    public class SocketSlashCommand
    {
        public object Data { get; internal set; }

        internal async Task RespondAsync(Embed embed)
        {
            throw new NotImplementedException();
        }

        internal async Task RespondAsync(string v)
        {
            throw new NotImplementedException();
        }
    }
}