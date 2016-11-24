using System;

namespace PsybotPlugin
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
    }
}
