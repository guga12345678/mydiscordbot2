using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace TutorialBot.Modules
{
    public class Direct : ModuleBase<SocketCommandContext>
    {
        [Command("vip")]
        [Alias("ვიპ")] // This handles both !vip and !ვიპ in one place
        [Summary("Sends a direct message to the user about VIP status")]
        public async Task DmMeAsync()
        {
            try
            {
                // Send DM to the user who triggered the command
                // Note: I removed the unused 'otherUserId' variable to fix your compiler warnings
                await Context.User.SendMessageAsync(
                    "👋 გამარჯობა! როლი @VIP არის სამუდამო ღირს 20 ლარი შესაძენად მიწერეთ სერვერის მფლობელს @shenennenene."
                );

                // Confirm in the public channel so the user knows to check their DMs
                await ReplyAsync($"{Context.User.Mention}, 📩 პმ შეამოწმე!");
            }
            catch
            {
                // This block runs if the user has "Allow direct messages from server members" turned off
                await ReplyAsync($"{Context.User.Mention}, ❌ ვერ გწერ. გთხოვ გახსენი შეტყობინებები სერვერის წევრებისგან (User Settings -> Privacy & Safety).");
            }
        }
    }
}