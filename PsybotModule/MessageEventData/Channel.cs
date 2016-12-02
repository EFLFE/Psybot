using System;
using System.Collections.Generic;

namespace PsybotModule.MessageEventData
{
    public struct Channel
    {
        //public DiscordClient Client;
        public ulong Id;
        public bool IsPrivate;
        public string Mention;
        //public IEnumerable<Message> Messages;
        public string Name;
        //public IEnumerable<PermissionOverwrite> PermissionOverwrites;
        public int Position;
        //public User Recipient;
        //public Server Server;
        public string Topic;
        //public ChannelType Type;
        //public User[] Users;

        public Channel(ulong id, bool isPrivate, string mention, string name, int position, string topics)
        {
            Id = id;
            IsPrivate = isPrivate;
            Mention = mention;
            Name = name;
            Position = position;
            Topic = topics;
        }
    }
}
