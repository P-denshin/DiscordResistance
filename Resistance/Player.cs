using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resistance {
    class Player : DiscordUser {
        /// <summary>
        /// プレイヤーの役割
        /// </summary>
        public Role Role {   get; private set; }

        public Player(SocketUser socketUser, Role role) : base(socketUser) {
            this.Role = role;
        }
    }
}
