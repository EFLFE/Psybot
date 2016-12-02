using System;

namespace PsybotModule.MessageEventData
{
    public struct Message
    {
        //public Attachment[] Attachments;
        public Channel Channel;
        //public DiscordClient Client;
        //public DateTime? EditedTimestamp;
        //public Embed[] Embeds;
        public ulong Id;
        public bool IsAuthor;
        public bool IsTTS;
        //public IEnumerable<Channel> MentionedChannels;
        //public IEnumerable<Role> MentionedRoles;
        //public IEnumerable<User> MentionedUsers;
        public string RawText;
        public Server Server;
        //public MessageState State;
        public string Text;
        public DateTime Timestamp;
        public User User;

        public Message(Channel channel, ulong id, bool isAuthor, bool isTTS, string rawText, Server server, string text, DateTime timestamp, User user)
        {
            Channel = channel;
            Id = id;
            IsAuthor = isAuthor;
            IsTTS = isTTS;
            RawText = rawText;
            Server = server;
            Text = text;
            Timestamp = timestamp;
            User = user;
        }
    }
}
