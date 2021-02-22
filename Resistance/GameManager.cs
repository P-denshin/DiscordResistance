using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace Resistance {
    /// <summary>
    /// Resistanceのゲームを統括するクラス
    /// </summary>
    public class GameManager {
        List<DiscordUser> discordUsers;

        /// <summary>
        /// ゲームボード
        /// </summary>
        RestUserMessage board;

        /// <summary>
        /// 現在ゲーム中かどうかを取得する。
        /// </summary>
        public bool IsGaming { get; private set; }

        /// <summary>
        /// ゲームを開始する。
        /// </summary>
        /// <param name="discordUsers">参加ユーザのリスト</param>
        public async void GameStart(List<DiscordUser> discordUsers, ISocketMessageChannel channel) {
            IsGaming = true;
            this.discordUsers = discordUsers;

            var boardEmbed = buildBoard();

            this.board = await channel.SendMessageAsync(embed: boardEmbed);
        }

        /// <summary>
        /// ゲームを強制的に終了する。
        /// </summary>
        public void GameExit() {
            IsGaming = false;
            discordUsers.Clear();
        }

        /// <summary>
        /// 現在の情報でEmbedを作成する。
        /// </summary>
        /// <returns></returns>
        private Embed buildBoard() {
            var eb = new EmbedBuilder();
            eb = eb.WithTitle("ボード");

            // ユーザリスト
            EmbedFieldBuilder[] efb = new EmbedFieldBuilder[discordUsers.Count];
            int count = 0;
            foreach (var user in discordUsers) {
                efb[count] = new EmbedFieldBuilder();
                efb[count] = efb[count].WithIsInline(true);
                efb[count].Name = user.Name;
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
