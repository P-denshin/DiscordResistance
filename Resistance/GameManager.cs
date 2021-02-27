using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace Resistance {
    /// <summary>
    /// Resistanceのゲームを統括するクラス
    /// </summary>
    public class GameManager {
        /// <summary>
        /// 参加プレイヤーリスト
        /// </summary>
        List<Player> players;

        /// <summary>
        /// ゲームボード
        /// </summary>
        RestUserMessage board;

        /// <summary>
        /// 各ラウンドの結果
        /// </summary>
        RoundState[] roundStates = new RoundState[5];

        /// <summary>
        /// 公開チャンネル
        /// </summary>
        ISocketMessageChannel channel;

        /// <summary>
        /// 現在のラウンド
        /// </summary>
        int round;

        /// <summary>
        /// 現在のリーダー
        /// </summary>
        int leaderIndex;

        /// <summary>
        /// 現在ゲーム中かどうかを取得する。
        /// </summary>
        public bool IsGaming { get; private set; }

        /// <summary>
        /// ゲームを開始する。
        /// </summary>
        /// <param name="discordUsers">参加ユーザのリスト</param>
        public async void GameStart(List<DiscordUser> discordUsers, ISocketMessageChannel channel) {
#if DEBUG
            await channel.SendMessageAsync("デバッグのため1人でもスタート可とします。");
#else
            if (discordUsers.Count < 5 || discordUsers.Count > 10) {
                await channel.SendMessageAsync("人数は5人以上10人以下でプレイ可能です。");
                return;
            }
#endif

            await channel.SendMessageAsync("ゲームを開始します！");

            this.channel = channel;
            await initGame(discordUsers);

            main();
        }

        /// <summary>
        /// ゲームを強制的に終了する。
        /// </summary>
        public void GameExit() {
            IsGaming = false;
            players.Clear();
        }

        private async void main() {
            while(round < 5) {
                var leader = players[leaderIndex];
                await this.board.ModifyAsync(e => e.Embed = buildBoard("現在、" + leader.Name + "がメンバーを選んでいます。"));
                var members = await selectMember(leader);
                await this.board.ModifyAsync(e => e.Embed = buildBoard("現在、メンバーがこれで良いか決定する投票中です。"));
            }
        }

        /// <summary>
        /// リーダーがメンバーを決める。
        /// </summary>
        private async Task<List<Player>> selectMember(Player leader) {
            String playerList = "";
            for(var i = 0; i < players.Count; i++) {
                playerList += (i + 1) + ":" + players[i].Name + "\n";
            }

            var leaderDM = await leader.SocketUser.GetOrCreateDMChannelAsync();
            var missionNum = new MissionNumber(players.Count, round);
            while (true) {
                await leaderDM.SendMessageAsync("プレイヤーを" + missionNum.NumberRequiredForFailer + "人選択して下さい。\nプレイヤーはスペース区切りで番号を指定してください。\n\nプレイヤーリスト\n" + playerList);

                string playersListStr = MessageReceiver.ReceiveMessage(leader);

                string[] numbers = playersListStr.Split(" ");
                
                if(numbers.Length != missionNum.NumberOfMembers) {
                    await leaderDM.SendMessageAsync(missionNum.NumberOfMembers + "人指定してください。");
                    continue;
                }

                bool isFormatError = false;

                List<Player> result = new List<Player>();
                foreach(var numStr in numbers) {
                    int num;
                    if(!int.TryParse(numStr, out num)) {
                        isFormatError = true;
                        break;
                    }

                    if(num < 1 || num > players.Count) {
                        isFormatError = true;
                        break;
                    }

                    result.Add(players[num - 1]);
                }

                if(isFormatError) {
                    await leaderDM.SendMessageAsync("1から" + players.Count + "までの数字で選択してください。");
                    continue;
                }

                string message = "本当につぎの人たちでいいですか？ y/n\n";
                foreach(var mem in result) {
                    message += "・" + mem.Name + "\n";
                }
                await leaderDM.SendMessageAsync(message);

                var yorn = MessageReceiver.ReceiveMessage(leader);

                if (!yorn.Equals("y")) {
                    continue;
                }

                return result;
            }
        }

        /// <summary>
        /// ゲーム開始時の初期化を行う。
        /// </summary>
        private async Task initGame(List<DiscordUser> discordUsers) {
            IsGaming = true;
            round = 0;
            leaderIndex = 0;

            assignRoles(discordUsers);
            noticeRole();

            for(var i = 0; i < roundStates.Length; i++) {
                roundStates[i] = RoundState.NotYet;
            }

            EmbedBuilder eb = new EmbedBuilder();
            eb = eb.WithTitle("ボード");
            this.board = await channel.SendMessageAsync(embed: eb.Build());
        }

#if DEBUG
        private void assignRoles(List<DiscordUser> discordUsers) {
            players = new List<Player>();

            // デバッグの場合一人とする
            for (var i = 0; i < discordUsers.Count; i++) {
                DiscordUser discordUser = discordUsers[i];
                if (i < 1) {
                    players.Add(new Player(discordUser.SocketUser, Role.Spy));
                } else {
                    players.Add(new Player(discordUser.SocketUser, Role.Resistance));
                }
            }
            players = players.OrderBy(a => Guid.NewGuid()).ToList();
        }
#else
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
#endif

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
        private Embed buildBoard(string message) {
            var eb = new EmbedBuilder();
            eb = eb.WithTitle("ボード");

            // ユーザリストと現状の役割
            EmbedFieldBuilder[] efb = new EmbedFieldBuilder[players.Count];
            int count = 0;
            foreach (var player in players) {
                efb[count] = new EmbedFieldBuilder();
                efb[count] = efb[count].WithIsInline(true);
                efb[count].Name = (count + 1) + " : " + player.Name;
                if (count == leaderIndex) {
                    efb[count].Value = "【リーダー】";
                } else {
                    efb[count].Value = "";
                }

                count += 1;
            }
            eb = eb.WithFields(efb);

            var description = "";
            for(var i = 0; i < roundStates.Length; i++) {
                switch (roundStates[i]) {
                    case RoundState.NotYet:
                        description += "○";
                        break;
                    case RoundState.ResistanceWin:
                        description += "🔵";
                        break;
                    case RoundState.SpyWin:
                        description += "🔴";
                        break;
                }
            }

            eb = eb.WithDescription(message + "\n" + description);

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
