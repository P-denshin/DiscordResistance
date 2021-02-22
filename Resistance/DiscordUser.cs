using System;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Text;

namespace Resistance {
    public delegate void UserMessageDelegate(SocketMessage message);

    public class DiscordUser {
        private SocketUser socketUser;

        /// <summary>
        /// ユーザの名前を取得する。
        /// </summary>
        public String Name {
            get { return socketUser.Username; }
        }
        
        /// <summary>
        /// このユーザーがメッセージを送信したとき発火します。
        /// </summary>
        public event UserMessageDelegate OnGetMessage;

        public DiscordUser(SocketUser socketUser) {
            OnGetMessage += (e) => { };
            DiscordManager.OnReceiveMessage += onRead;
            this.socketUser = socketUser;
        }

        private void onRead(SocketMessage message) {
            if(message.Author.Id == socketUser.Id) {
                OnGetMessage(message);
            }
        }
    }
}
