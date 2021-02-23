using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace Resistance {
    /// <summary>
    /// Resistanceのゲームを統括するクラス
    /// </summary>
    public class GameManager {
        List<Player> players;

        /// <summary>
        /// ゲームボード
        /// </summary>
        RestUserMessage board;

        /// <summary>
        /// 公開チャンネル
        /// </summary>
        ISocketMessageChannel channel;

        /// <summary>
        /// 現在のラウンド
        /// </summary>
        int round;

        /// <summary>
        /// 現在ゲーム中かどうかを取得する。
        /// </summary>
        public bool IsGaming { get; private set; }

        /// <summary>
        /// ゲームを開始する。
        /// </summary>
        /// <param name="discordUsers">参加ユーザのリスト</param>
        public void GameStart(List<DiscordUser> discordUsers, ISocketMessageChannel channel) {
            this.channel = channel;
            initGame(discordUsers);
        }

        /// <summary>
        /// ゲームを強制的に終了する。
        /// </summary>
        public void GameExit() {
            IsGaming = false;
            players.Clear();
        }

        /// <summary>
        /// ゲーム開始時の初期化を行う。
        /// </summary>
        private async void initGame(List<DiscordUser> discordUsers) {
            IsGaming = true;
            round = 0;

            assignRoles(discordUsers);
            noticeRole();

            this.board = await channel.SendMessageAsync(embed: buildBoard());
        }

        /// <summary>
        /// プレイヤーの役割決める
        /// </summary>
        private void assignRoles(List<DiscordUser> discordUsers) {
            int[] number = { 2, 2, 3, 3, 3, 4 };

            players = new List<Player>();
            players = players.OrderBy(a => Guid.NewGuid()).ToList();

            for (var i = 0; i < discordUsers.Count; i++) {
                DiscordUser discordUser = discordUsers[i];
                if (i < number[discordUsers.Count - 5]) {
                    players.Add(new Player(discordUser.SocketUser, Role.Spy));
                } else {
                    players.Add(new Player(discordUser.SocketUser, Role.Resistance));
                }
            }

            players = players.OrderBy(a => Guid.NewGuid()).ToList();
        }

        /// <summary>
        /// 役割を通達する。
        /// </summary>
        private async void noticeRole() {
            String spyList = "";
            foreach(var player in players) {
                if(player.Role == Role.Spy) {
                    spyList += "・" + player.Name + "\n";
                }
            }

            foreach(var player in players) {
                var dm = await player.SocketUser.GetOrCreateDMChannelAsync();
                if (player.Role == Role.Spy) {
                    EmbedBuilder eb = new EmbedBuilder().WithTitle("役割").WithColor(Color.Red).WithDescription("あなたはスパイです！\n\n他のスパイ\n" + spyList);
                    await dm.SendMessageAsync(embed: eb.Build());
                } else if(player.Role == Role.Resistance) {
                    EmbedBuilder eb = new EmbedBuilder().WithTitle("役割").WithColor(Color.Blue).WithDescription("あなたはレジスタンスです！");
                    await dm.SendMessageAsync(embed: eb.Build());
                }
            }

        }

        /// <summary>
        /// 現在の情報でEmbedを作成する。
        /// </summary>
        /// <returns></returns>
        private Embed buildBoard() {
            var eb = new EmbedBuilder();
            eb = eb.WithTitle("ボード");

            // ユーザリスト
            EmbedFieldBuilder[] efb = new EmbedFieldBuilder[players.Count];
            int count = 0;
            foreach (var player in players) {
                efb[count] = new EmbedFieldBuilder();
                efb[count] = efb[count].WithIsInline(true);
                efb[count].Name = player.Name;
                efb[count].Value = "リーダー";
            }
            eb = eb.WithFields(efb);

            return eb.Build();
        }

        private GameManager() {
            IsGaming = false;
        }

        #region Singleton
        static GameManager gameManager;

        /// <summary>
        /// インスタンスを取得する。
        /// </summary>
        public static GameManager Instance {
            get {
                if (gameManager == null) {
                    gameManager = new GameManager();
                }
                return gameManager;
            }
        }
        #endregion
    }
}
