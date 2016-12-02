namespace Psybot
{
    public static class EXConverter
    {
        /// <summary>
        ///     Get fields from Discord.Channel to PsybotModule.MessageEventData.Channel
        /// </summary>
        public static PsybotModule.MessageEventData.Channel ChannelForModule(this Discord.Channel c)
        {
            return new PsybotModule.MessageEventData.Channel(
                c.Id, c.IsPrivate, c.Mention, c.Name, c.Position, c.Topic);
        }

        /// <summary>
        ///     Get fields from Discord.Message to PsybotModule.MessageEventData.Message
        /// </summary>
        public static PsybotModule.MessageEventData.Message MessageForModule(this Discord.Message m)
        {
            return new PsybotModule.MessageEventData.Message(
                m.Channel.ChannelForModule(), m.Id, m.IsAuthor, m.IsTTS, m.RawText, m.Server.ServerForModule(), m.Text, m.Timestamp, m.User.UserForModule());
        }

        /// <summary>
        ///     Get fields from Discord.Server to PsybotModule.MessageEventData.Server
        /// </summary>
        public static PsybotModule.MessageEventData.Server ServerForModule(this Discord.Server s)
        {
            return new PsybotModule.MessageEventData.Server(
                s.ChannelCount, s.IconId, s.IconUrl, s.Id, s.IsOwner, s.Name, s.SplashId, s.SplashUrl);
        }

        /// <summary>
        ///     Get fields from Discord.User to PsybotModule.MessageEventData.User
        /// </summary>
        public static PsybotModule.MessageEventData.User UserForModule(this Discord.User u)
        {
            return new PsybotModule.MessageEventData.User(
                u.AvatarId, u.AvatarUrl, u.Id, u.IsBot, u.IsSelfDeafened, u.IsSelfMuted, u.IsServerDeafened, u.IsServerMuted, u.IsServerSuppressed, u.JoinedAt, u.Mention, u.Name, u.Nickname, u.NicknameMention);
        }
    }
}
