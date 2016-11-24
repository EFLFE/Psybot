using System;

namespace PsybotPlugin.MessageEventData
{
    public struct User
    {
        public string AvatarId;
        public string AvatarUrl;
        //public IEnumerable<Channel> Channels;
        //public DiscordClient Client;
        //public Game? CurrentGame;
        //public ushort Discriminator;
        public ulong Id;
        public bool IsBot;
        public bool IsSelfDeafened;
        public bool IsSelfMuted;
        public bool IsServerDeafened;
        public bool IsServerMuted;
        public bool IsServerSuppressed;
        public DateTime JoinedAt;
        //public DateTime? LastActivityAt;
        //public DateTime? LastOnlineAt;
        public string Mention;
        public string Name;
        public string Nickname;
        public string NicknameMention;
        //public Channel PrivateChannel;
        //public IEnumerable<Role> Roles;
        //public Server Server;
        //public ServerPermissions ServerPermissions;
        //public UserStatus Status;
        //public Channel VoiceChannel;

        public User(string avatarId, string avatarUrl, ulong id, bool isBot, bool isSelfDeafened, bool isSelfMuted, bool isServerDeafened,
                    bool isServerMuted, bool isServerSuppressed, DateTime joinedAt, string mention, string name, string nickname, string nicknameMention)
        {
            AvatarId = avatarId;
            AvatarUrl = avatarUrl;
            Id = id;
            IsBot = isBot;
            IsSelfDeafened = isSelfDeafened;
            IsSelfMuted = isSelfMuted;
            IsServerDeafened = isServerDeafened;
            IsServerMuted = isServerMuted;
            IsServerSuppressed = isServerSuppressed;
            JoinedAt = joinedAt;
            Mention = mention;
            Name = name;
            Nickname = nickname;
            NicknameMention = nicknameMention;
        }
    }
}
