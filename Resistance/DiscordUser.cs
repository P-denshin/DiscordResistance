using System;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Text;

namespace Resistance {
    public delegate void UserMessageDelegate(SocketMessage message);

    public class DiscordUser {
        /// <summary>
        /// SocketUserを取得します。
        /// </summary>
        public SocketUser SocketUser {
            get; private set;
        }

        /// <summary>
        /// ユーザの名前を取得する。
        /// </summary>
        public String Name {
            get { return SocketUser.Username; }
        }
        
        /// <summary>
        /// このユーザーがメッセージを送信したとき発火します。
        /// </summary>
        public event UserMessageDelegate OnGetMessage;

        public DiscordUser(SocketUser socketUser) {
            OnGetMessage += (e) => { };
            DiscordManager.OnReceiveMessage += onRead;
            this.SocketUser = socketUser;
        }

        private void onRead(SocketMessage message) {
            if(message.Author.Id == SocketUser.Id) {
                OnGetMessage(message);
            }
        }
    }
}
