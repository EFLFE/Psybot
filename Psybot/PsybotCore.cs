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

        #region IPsybotCore

        public bool Connected { get; private set; }

        #endregion

        private PluginManager pluginManager;
        private DiscordClient client;
        private bool stop;

        // console commands:
        private const string CMD_CONNECT    = "connect";
        private const string CMD_DISCONNECT = "disconnect";
        private const string CMD_SEND       = "send";
        private const string CMD_EXIT       = "exit";
        private const string CMD_CLEAR      = "clear";
        private const string CMD_EXPAND     = "expand";
        // todo: command list

        public PsybotCore()
        {
            client = new DiscordClient(x =>
            {
                x.AppName = "Psybot";
                x.LogLevel = LogSeverity.Debug;
                x.LogHandler = Log;
                //x.AppUrl = "???";
            });

            pluginManager = new PluginManager(this as IPsybotCore);
        }

        public void Run()
        {
            Term.OnDraw += Term_OnDraw;
            client.MessageReceived += Client_MessageReceived;
            pluginManager.EnterGUI(); // TEST
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
            pluginManager.ExcecutePlugins(new PsybotPluginArgs()
            {
                ChannelID = e.Channel.Id,
                UserName = e.User.Name,
                UserMention = e.User.Mention,
                Message = e.Message.RawText
            });

            //if (e.User.Id == ID_EFLFE)
            //    e.Channel.SendMessage(e.User.Mention + " send some text?");
        }

        private void Term_OnDraw()
        {
            Term.ClearLine(0);
            string tx = string.Empty;

            if (Connected)
            {
                tx += "Status: Connected";
            }
            else
            {
                tx += "Status: Disconected";
            }

            tx += " | MEM: " + (GC.GetTotalMemory(false) / 100000L) + " mb";

            Term.Draw(tx, 0, 0);
        }

        // console commands
        private void addCommands()
        {
            Term.AddCommand(CMD_CONNECT, async (s) =>
            {
                Term.Log(CMD_CONNECT, ConsoleColor.White);
                if (!File.Exists(TOKEN_FILE))
                {
                    Term.Log("Token file '" + TOKEN_FILE + "' not found.", ConsoleColor.Red);
                    return;
                }

                try
                {
                    var tocken = File.ReadAllText(TOKEN_FILE);
                    await client.Connect(tocken, TokenType.Bot);
                    Connected = true;
                }
                catch (Exception ex)
                {
                    Term.Log("Error: " + ex.Message, ConsoleColor.Red);
                }

            }, "Connect to server.");
            // ====================================================================== //
            Term.AddCommand(CMD_DISCONNECT, async (s) =>
            {
                Term.Log(CMD_DISCONNECT, ConsoleColor.White);
                if (client == null)
                {
                    Term.Log("DiscordClient is null.", ConsoleColor.Yellow);
                    return;
                }
                Connected = false;
                await client.Disconnect();
            }, "Disonnect from server.");
            // ====================================================================== //
            //Term.AddCommand(CMD_SEND, async (s) =>
            //{
            //    Term.Log(CMD_SEND, ConsoleColor.White);
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
                Term.Log(CMD_EXIT, ConsoleColor.White);
                stop = true;
            }, "Close this program.");
            // ====================================================================== //
            Term.AddCommand(CMD_CLEAR, (s) =>
            {
                Term.Log(CMD_CLEAR, ConsoleColor.White);
                Console.Clear();
                Term.Log("Clear console.");
            }, "Clear console.");
            // ====================================================================== //
            Term.AddCommand(CMD_EXPAND, (s) =>
            {
                Term.Log(CMD_EXPAND, ConsoleColor.White);
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
        }

        private void Log(object sender, LogMessageEventArgs e)
        {
            Term.Log($"[{e.Severity}] {e.Source}: {e.Message}", ConsoleColor.Gray);
        }

        #region IPsybotCore

        public async void SendMessage(ulong channelID, string message)
        {
            if (!Connected)
                return;
            await client.GetChannel(channelID)?.SendMessage(message);
        }

        #endregion

    }
}
