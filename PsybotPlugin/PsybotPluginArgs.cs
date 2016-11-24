using PsybotPlugin.MessageEventData;

namespace PsybotPlugin
{
    /// <summary> Аргументы отправителя сообщения и других данных. </summary>
    public sealed class PsybotPluginArgs
    {
        /*
        /// <summary> Сообщение без аргумента. </summary>
        public string Message;

        /// <summary> Полное сообщение. </summary>
        public string RawMessage;

        /// <summary> Ник пользователя, который вызвал команду. </summary>
        public string UserName;

        /// <summary> Индификатор канала, с которого выба вызвана команда. </summary>
        public ulong ChannelID;

        /// <summary> Ссылка на отправителя (прим: "&lt;@123456789&gt;"). </summary>
        public string UserMention;
        */

        public User User;
        public Server Server;
        public Message Message;
        public Channel Channel;

    }
}
