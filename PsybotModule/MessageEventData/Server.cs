using System;
using System.Collections.Generic;

namespace PsybotModule.MessageEventData
{
    public struct Server
    {
        //public Channel AFKChannel;
        //public int AFKTimeout;
        //public IEnumerable<Channel> AllChannels;
        public int ChannelCount;
        //public DiscordClient Client;
        //public User CurrentUser;
        //public IEnumerable<Emoji> CustomEmojis;
        //public Channel DefaultChannel;
        //public Role EveryoneRole;
        //public IEnumerable<string> Features;
        public string IconId;
        public string IconUrl;
        public ulong Id;
        public bool IsOwner;
        //public DateTime JoinedAt;
        public string Name;
        //public User Owner;
        //public Region Region;
        //public int RoleCount;
        //public IEnumerable<Role> Roles;
        public string SplashId;
        public string SplashUrl;
        //public Channel[] TextChannels;
        //public int UserCount;
        //public User[] Users;
        //public IEnumerable<Channel> VoiceChannels;

        public Server(int channelCount, string iconId, string iconUrl, ulong id, bool isOwner, string name, string splashId, string splashUrl)
        {
            ChannelCount = channelCount;
            IconId = iconId;
            IconUrl = iconUrl;
            Id = id;
            IsOwner = isOwner;
            Name = name;
            SplashId = splashId;
            SplashUrl = splashUrl;
        }
    }
}
