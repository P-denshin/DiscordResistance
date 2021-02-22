using Discord;
using Discord.WebSocket;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Resistance {
    class Program {
        private readonly DiscordSocketClient client;
        static void Main(string[] args) {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public Program() {
            client = new DiscordSocketClient();
            client.Log += LogAsync;
            client.Ready += onReady;
            client.MessageReceived += onMessage;
        }

        public async Task MainAsync() {
            await client.LoginAsync(TokenType.Bot, "ODEzMzA0MjkyMzI4NjAzNjk4.YDNWpQ.Wiuf6EaZj892IAQBe2WNbV0LCrU");
            await client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }

        private Task LogAsync(LogMessage log) {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private Task onReady() {
            Console.WriteLine($"{client.CurrentUser} is Running!!");
            return Task.CompletedTask;
        }

        private async Task onMessage(SocketMessage message) {
            if (message.Author.Id == client.CurrentUser.Id) {
                return;
            }
            if (message.Content == "こんにちは") {
                await message.Channel.SendMessageAsync("こんにちは、" + message.Author.Username + "さん！");
            }
        }
    }
}
