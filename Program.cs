using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;

namespace TutorialBot
{
    class Program
    {
        static void Main(string[] args)
            => new Program().RunBotAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

        // Configuration
        private readonly List<ulong> AutoDeleteChannelIds = new List<ulong> { 1466907225049010289, 1466901460532072468 };
        private readonly List<ulong> SafeChannelIds = new List<ulong> { 1466537673505378548 };

        private const ulong NumberGuessChannelId = 1468014231856353340;
        private const ulong GameStarterRoleId = 1466924908217892892;
        private const ulong WinnerRoleId = 1466804645518246042;

        private bool _gameActive = false;
        private int _currentRandomNumber;
        private readonly Random _rng = new Random();
        private bool _channelLocked = false;

        public async Task RunBotAsync()
        {
            // For Discord.Net v2.x, we initialize without the GatewayIntents object
            _client = new DiscordSocketClient();
            _commands = new CommandService();

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            _client.Log += LogAsync;

            await RegisterCommandsAsync();

            // DO NOT share this token. Reset it in the Dev Portal if it was leaked.
            // The 'User' target ensures it finds the variable you set in PowerShell
            string token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN", EnvironmentVariableTarget.User);

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log);
            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleMessageAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleMessageAsync(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message)) return;
            if (message.Author.IsBot) return;

            var channel = message.Channel as SocketTextChannel;
            int argPos = 0;

            // 1. COMMANDS FIRST (!vip, etc)
            if (message.HasStringPrefix("!", ref argPos))
            {
                var context = new SocketCommandContext(_client, message);
                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (result.IsSuccess) return;
            }

            // 2. NUMBER GUESSING GAME
            if (message.Channel.Id == NumberGuessChannelId)
            {
                var authorUser = message.Author as SocketGuildUser;

                if (_channelLocked)
                {
                    await message.DeleteAsync();
                    return;
                }

                if (!_gameActive && authorUser.Roles.Any(r => r.Id == GameStarterRoleId))
                {
                    _currentRandomNumber = _rng.Next(1, 201);
                    _gameActive = true;
                    _channelLocked = false;
                    await message.Channel.SendMessageAsync("ricxvis gamocnobis tamashi daiwyo! gamoicani ricxvi 1_dan 200_mde.");
                    return;
                }

                if (!_gameActive)
                {
                    await message.DeleteAsync();
                    return;
                }

                if (message.Content.Any(char.IsLetter))
                {
                    await message.DeleteAsync();
                    return;
                }

                if (int.TryParse(message.Content, out int guess))
                {
                    if (guess == _currentRandomNumber)
                    {
                        var role = channel.Guild.GetRole(WinnerRoleId);
                        if (role != null) await authorUser.AddRoleAsync(role);

                        await message.Channel.SendMessageAsync($"{authorUser.Mention} gamoicno swori ricxvi da moigo VIP!");

                        _gameActive = false;
                        _channelLocked = true;

                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(60000);
                            _channelLocked = false;
                            await message.Channel.SendMessageAsync("chati mzataa axali tamashis dasawyebad!");
                        });
                        return;
                    }
                }
                return;
            }

            // 3. SAFE CHANNELS (Ignored by Auto-Delete)
            if (SafeChannelIds.Contains(message.Channel.Id)) return;

            // 4. AUTO-DELETE (If not safe channel and no attachment)
            if (AutoDeleteChannelIds.Contains(message.Channel.Id))
            {
                if (message.Attachments.Count == 0 && message.Embeds.Count == 0)
                {
                    await message.DeleteAsync();
                    string safeMentions = string.Join(", ", SafeChannelIds.Select(id => $"<#{id}>"));
                    var warning = await message.Channel.SendMessageAsync($"aq nudebi chayaret mesijebistvis gamoiyenet {safeMentions}");

                    _ = Task.Run(async () => {
                        await Task.Delay(15000);
                        await warning.DeleteAsync();
                    });
                }
            }
        }
    }
}