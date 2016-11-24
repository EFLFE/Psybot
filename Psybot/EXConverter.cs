namespace Psybot
{
    public static class EXConverter
    {
        /// <summary>
        ///     Get fields from Discord.Channel to PsybotPlugin.MessageEventData.Channel
        /// </summary>
        public static PsybotPlugin.MessageEventData.Channel ChannelForPlugin(this Discord.Channel c)
        {
            return new PsybotPlugin.MessageEventData.Channel(
                c.Id, c.IsPrivate, c.Mention, c.Name, c.Position, c.Topic);
        }

        /// <summary>
        ///     Get fields from Discord.Message to PsybotPlugin.MessageEventData.Message
        /// </summary>
        public static PsybotPlugin.MessageEventData.Message MessageForPlugin(this Discord.Message m)
        {
            return new PsybotPlugin.MessageEventData.Message(
                m.Channel.ChannelForPlugin(), m.Id, m.IsAuthor, m.IsTTS, m.RawText, m.Server.ServerForPlugin(), m.Text, m.Timestamp, m.User.UserForPlugin());
        }

        /// <summary>
        ///     Get fields from Discord.Server to PsybotPlugin.MessageEventData.Server
        /// </summary>
        public static PsybotPlugin.MessageEventData.Server ServerForPlugin(this Discord.Server s)
        {
            return new PsybotPlugin.MessageEventData.Server(
                s.ChannelCount, s.IconId, s.IconUrl, s.Id, s.IsOwner, s.Name, s.SplashId, s.SplashUrl);
        }

        /// <summary>
        ///     Get fields from Discord.User to PsybotPlugin.MessageEventData.User
        /// </summary>
        public static PsybotPlugin.MessageEventData.User UserForPlugin(this Discord.User u)
        {
            return new PsybotPlugin.MessageEventData.User(
                u.AvatarId, u.AvatarUrl, u.Id, u.IsBot, u.IsSelfDeafened, u.IsSelfMuted, u.IsServerDeafened, u.IsServerMuted, u.IsServerSuppressed, u.JoinedAt, u.Mention, u.Name, u.Nickname, u.NicknameMention);
        }
    }
}
