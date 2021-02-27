using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resistance {
    static class MessageReceiver {
        static String receivedMessage = null;

        public static String ReceiveMessage(Player player) {
            receivedMessage = null;
            player.OnGetMessage += receive;

            while (receivedMessage == null)
                ;

            player.OnGetMessage -= receive;

            var res = new String(receivedMessage);

            return res;
        }

        private static void receive(SocketMessage message) {
            receivedMessage = message.Content;
        }

    }
}
