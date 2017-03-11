using System;

namespace PsybotModule
{
	/// <summary>
	///		Message data from channel.
	/// </summary>
	public struct Message
	{
		/// <summary> Message ID. </summary>
		public ulong MessageId;

		/// <summary> Channel ID. </summary>
		public ulong ChannelId;

		/// <summary> User ID. </summary>
		public ulong UserId;

		/// <summary> Channel name. </summary>
		public string ChannelName;

		/// <summary> Message created time. </summary>
		public DateTimeOffset CreatedAt;

		/// <summary> Inner message. </summary>
		public string Content;

		/// <summary> Username. </summary>
		public string UserName;

		/// <summary> User mention. </summary>
		public string UserMention;

		/// <summary> Is Bot. </summary>
		public bool IsBot;

		/// <summary> Avatar URL. </summary>
		public string AvatarUrl;

		/// <summary> Is pinned. </summary>
		public bool IsPinned;

		/// <summary> Is TTS. </summary>
		public bool IsTTS;

		/// <summary> Is commands message. </summary>
		public bool IsCommandsMessage;

		/// <summary> Commands message (if IsCommandsMessage == true). </summary>
		public string CommandsMessage;

		public Message(ulong messageId, ulong channelId, string channelName, DateTimeOffset createdAt, string content, string userName,
			string userMention, ulong userId, bool isBot, string avatarUrl, bool isPinned, bool isTTS, bool isCommandsMessage, string commandsMessage)
		{
			UserId = userId;
			ChannelId = channelId;
			MessageId = messageId;
			ChannelName = channelName;
			CreatedAt = createdAt;
			Content = content;
			UserName = userName;
			UserMention = userMention;
			IsBot = isBot;
			AvatarUrl = avatarUrl;
			IsPinned = isPinned;
			IsTTS = isTTS;
			IsCommandsMessage = isCommandsMessage;
			CommandsMessage = commandsMessage;
		}
	}
}
