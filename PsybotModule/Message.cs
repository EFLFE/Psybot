using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsybotModule
{
	public struct Message
	{
		/// <summary> Message ID. </summary>
		public ulong MessageId;

		public ulong ChannelId;

		/// <summary> Channel name. </summary>
		public string ChannelName;

		/// <summary> Message created time. </summary>
		public DateTimeOffset CreatedAt;

		/// <summary> Inner message. </summary>
		public string Content;

		/// <summary> Username. </summary>
		string UserName;

		/// <summary> User mention. </summary>
		public string UserMention;

		/// <summary> Is Bot. </summary>
		public bool IsBot;

		/// <summary> Avatar ID. </summary>
		public string AvatarId;

		/// <summary> Avatar URL. </summary>
		public string AvatarUrl;

		/// <summary> Is pinned. </summary>
		public bool IsPinned;

		/// <summary> Is TTS. </summary>
		public bool IsTTS;

		public Message(ulong messageId, ulong channelId, string channelName, DateTimeOffset createdAt, string content, string userName, string userMention, bool isBot, string avatarId, string avatarUrl, bool isPinned, bool isTTS)
		{
			ChannelId = channelId;
			MessageId = messageId;
			ChannelName = channelName;
			CreatedAt = createdAt;
			Content = content;
			UserName = userName;
			UserMention = userMention;
			IsBot = isBot;
			AvatarId = avatarId;
			AvatarUrl = avatarUrl;
			IsPinned = isPinned;
			IsTTS = isTTS;
		}
	}
}
