using System;
using System.IO;
using System.Threading;
using Discord;
using Psybot.Plugins;
using Psybot.UI;
using PsybotPlugin;

namespace Psybot
{
    internal sealed class PsybotCore : IPsybotCore
    {
        private const string TOKEN_FILE = "token.txt";

        #region IPsybotCore VARS

        /// <summary>
        ///     Get connected status.
        /// </summary>
        public bool Connected => client == null ? false : client.State == ConnectionState.Connected;

        #endregion

        private PluginManager pluginManager;
        private DiscordClient client;
        private bool stop;

        // console commands:
        private const string CMD_CONNECT    = "connect";
        private const string CMD_DISCONNECT = "disconnect";
        //private const string CMD_SEND       = "send";
        private const string CMD_EXIT       = "exit";
        private const string CMD_CLEAR      = "clear";
        private const string CMD_EXPAND     = "expand";
        private const string CMD_PLUGINS    = "plugins";
        private const string CMD_COMMANDS   = "commands";

        public PsybotCore()
        {
            client = new DiscordClient(x =>
            {
                x.AppName = "Psybot";
#if DEBUG
                x.LogLevel = LogSeverity.Debug;
#else
                x.LogLevel = LogSeverity.Info;
#endif
                x.LogHandler = Log;
                //x.AppUrl = "???";
            });

            pluginManager = new PluginManager(this as IPsybotCore);
        }

        public void Run()
        {
            Term.OnDraw += Term_OnDraw;
            client.MessageReceived += Client_MessageReceived;

            addCommands();
            Term.Start();

            while (!stop)
            {
                Thread.Sleep(33);
            }
        }

        // любое сообщение на сервере
        private void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            pluginManager.ExcecutePlugins(new PsybotPluginArgs
            {
                Channel = e.Channel.ChannelForPlugin(),
                Message = e.Message.MessageForPlugin(),
                Server = e.Server.ServerForPlugin(),
                User = e.User.UserForPlugin()
            });
        }

        private void Term_OnDraw()
        {
            Term.ClearLine(0);

            Term.Draw("State:", 0, 0, ConsoleColor.Gray);

            switch (client.State)
            {
            case ConnectionState.Disconnected:
                Term.Draw("Disconnected", 7, 0, ConsoleColor.Red);
                break;

            case ConnectionState.Connecting:
                Term.Draw("Connecting", 7, 0, ConsoleColor.Yellow);
                break;

            case ConnectionState.Connected:
                Term.Draw("Connected", 7, 0, ConsoleColor.Green);
                break;

            case ConnectionState.Disconnecting:
                Term.Draw("Disconnecting", 7, 0, ConsoleColor.Yellow);
                break;

            default:
                Term.Draw(client.State.ToString(), 7, 0, ConsoleColor.Gray);
                break;
            }

            Term.Draw(
                $" | MEM: {(GC.GetTotalMemory(false) / (1024L * 1024L))} mb | Plug-in: {pluginManager.GetEnabledPlugins}/{pluginManager.GetInstalledPlugins}",
                20, 0, ConsoleColor.Gray);
        }

        // console commands
        private void addCommands()
        {
            Term.AddCommand(CMD_CONNECT, async (s) =>
            {
                if (!File.Exists(TOKEN_FILE))
                {
                    Term.Log("Token file '" + TOKEN_FILE + "' not found.", ConsoleColor.Red);
                    return;
                }

                try
                {
                    var tocken = File.ReadAllText(TOKEN_FILE);
                    await client.Connect(tocken, TokenType.Bot);
                }
                catch (Exception ex)
                {
                    Term.Log("Error: " + ex.Message, ConsoleColor.Red);
                }

            }, "Connect to server.");
            // ====================================================================== //
            Term.AddCommand(CMD_DISCONNECT, async (s) =>
            {
                if (client == null)
                {
                    Term.Log("DiscordClient is null.", ConsoleColor.Yellow);
                    return;
                }
                await client.Disconnect();
            }, "Disonnect from server.");
            // ====================================================================== //
            //Term.AddCommand(CMD_SEND, async (s) =>
            //{
            //    if (client == null)
            //    {
            //        Term.Log("DiscordClient is null.", ConsoleColor.Yellow);
            //        return;
            //    }
            //    await client.GetChannel(ID_BOT_CHANNEL)?.SendMessage(s);
            //}, "Send text to server channel.");
            // ====================================================================== //
            Term.AddCommand(CMD_EXIT, (s) =>
            {
                if (Connected)
                {
                    client.Disconnect();
                }
                stop = true;
            }, "Close this program.");
            // ====================================================================== //
            Term.AddCommand(CMD_CLEAR, (s) =>
            {
                Console.Clear();
            }, "Clear console.");
            // ====================================================================== //
            Term.AddCommand(CMD_EXPAND, (s) =>
            {
                try
                {
                    Console.BufferWidth = 120;
                    Console.WindowWidth = 120;
                    Console.BufferHeight = 80;
                    Console.WindowHeight = 40;
                }
                catch (Exception ex)
                {
                    Term.Log(CMD_EXPAND + " error: " + ex.Message);
                }
                Console.Clear();
                Term.ReDrawLog();
            }, "Expadn console window.");
            // ====================================================================== //
            Term.AddCommand(CMD_PLUGINS, (s) =>
            {
                pluginManager.EnterGUI();
            }, "Enter plugins manager.");
            // ====================================================================== //
            Term.AddCommand(CMD_COMMANDS, (s) =>
            {
                Term.ShowAllCommands();
            }, "Show all commands.");
        }

        // Discord.Net client log
        private void Log(object sender, LogMessageEventArgs e)
        {
            var clr = ConsoleColor.Gray;
            switch (e.Severity)
            {
            case LogSeverity.Error:
                clr = ConsoleColor.Red;
                break;

            case LogSeverity.Warning:
                clr = ConsoleColor.Yellow;
                break;

            case LogSeverity.Info:
                clr = ConsoleColor.White;
                break;

            case LogSeverity.Verbose:
                //clr = ConsoleColor.Gray;
                break;

            case LogSeverity.Debug:
                clr = ConsoleColor.Gray;
                break;
            }

            Term.Log($"{e.Source}: {e.Message}", clr);
        }

        #region IPsybotCore METHODS

        /// <summary> Send a message to the server. </summary>
        /// <param name="channelID"> Channel ID. </param>
        /// <param name="message"> Message. </param>
        public async void SendMessage(ulong channelID, string message)
        {
            if (!Connected)
                return;
            await client.GetChannel(channelID)?.SendMessage(message);
        }

        /// <summary>
        ///     Write a text to server log.
        /// </summary>
        /// <param name="mess"> Text. </param>
        /// <param name="color"> Text color. </param>
        public void SendLog(string mess, ConsoleColor color)
        {
            // todo: auto detect owner plugin name
            Term.Log("$ " + mess, color);
        }

        #endregion

    }
}
