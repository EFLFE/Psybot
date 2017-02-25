using System;

namespace PsybotModule
{
    /// <summary>
    ///     Base interface for Plug-in's.
    /// </summary>
    public interface IPsybotCore
    {
        /// <summary>
        ///     Get connected status.
        /// </summary>
        bool Connected { get; }

        /// <summary> Send a message to the server. </summary>
        /// <param name="channelID"> Channel ID. </param>
        /// <param name="message"> Message. </param>
        void SendMessage(ulong channelID, string message);

        // <summary>
        //     Get a list of text channels on the server.
        // </summary>
        //MessageEventData.Channel[] GetTextChannels();

        // <summary>
        //     Get a list of users on the server
        // </summary>
        //MessageEventData.User[] GetUsers();

        /// <summary>
        ///     Write a text to server log.
        /// </summary>
        /// <param name="mess"> Text. </param>
        /// <param name="color"> Text color. </param>
        void SendLog(string mess, ConsoleColor color);

		/// <summary>
		///     [Obsolete] Send file.
		/// </summary>
		/// <param name="channelID"> Channel ID. </param>
		/// <param name="filePath"> File path. </param>
		/// <param name="text"> Add text (can be null). </param>
		[Obsolete("Not ready.")]
		void SendImage(ulong channelID, string filePath, string text);

		// TODO: UpdateStatus access
		/// <summary>
		///		Updates current bot status.
		/// </summary>
		/// <param name="game"> Game name. </param>
		/// <param name="idle_since"></param>
		/// <param name="url"> Game url (optional, for 'psy game' command). </param>
		void UpdateStatus(string game = "", int idle_since = -1, string url = null);

	}
}
