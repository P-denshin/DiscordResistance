using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resistance {
    class MessageReceiver {
        String receivedMessage = null;

        public String ReceiveMessage(Player player) {
            receivedMessage = null;
            player.OnGetMessage += receive;

            while (receivedMessage == null)
                ;

            player.OnGetMessage -= receive;

            var res = new String(receivedMessage);

            return res;
        }

        private void receive(SocketMessage message) {
            receivedMessage = message.Content;
        }

    }
}
