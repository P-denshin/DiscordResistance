using Discord;
using Discord.WebSocket;
using System;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Resistance {
    public delegate void MessageDelegate(SocketMessage socketMessage);

    class DiscordManager {
        private readonly DiscordSocketClient client;
        private List<DiscordUser> players = new List<DiscordUser>();

        /// <summary>
        /// メッセージを受け取った際に発火する
        /// </summary>
        public static event MessageDelegate OnReceiveMessage;

        static void Main(string[] args) {
            OnReceiveMessage += (e) => { };
            new DiscordManager().MainAsync().GetAwaiter().GetResult();
        }

        public DiscordManager() {
            client = new DiscordSocketClient();
            client.Log += LogAsync;
            client.Ready += onReady;
            client.MessageReceived += onMessage;
        }

        public async Task MainAsync() {
            await client.LoginAsync(TokenType.Bot, getToken());
            await client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }

        /// <summary>
        /// DiscordのTokenを取得する
        /// </summary>
        /// <returns>Token</returns>
        private string getToken() {
            ResourceManager resource = Properties.Resources.ResourceManager;
            var tokenPath = resource.GetString("TokenPath");

            StreamReader sr = new StreamReader(tokenPath);
            var token = sr.ReadLine();
            sr.Close();

            return token;
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

            if (!GameManager.Instance.IsGaming) {
                if (message.Content.Equals("!dr join")) {
                    DiscordUser player = new DiscordUser(message.Author);
                    players.Add(player);
                    await message.Channel.SendMessageAsync(message.Author.Username + "が参加しました。");
                    return;
                } else if (message.Content.Equals("!dr start")) {
                    await message.Channel.SendMessageAsync("ゲームを開始します！");
                    GameManager.Instance.GameStart(players, message.Channel);
                    return;
                }
            }

            if(message.Content.Equals("!dr end")) {
                players.Clear();
                GameManager.Instance.GameExit();
                return;
            }

            OnReceiveMessage(message);
        }
    }
}
