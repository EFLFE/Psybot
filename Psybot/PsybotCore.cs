using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Discord;
using Psybot.Modules;
using Psybot.UI;
using PsybotModule;

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

        private ModuleManager moduleManager;
        private DiscordClient client;
        private bool stop;
        private string tempCodeName;
        private Discord.User userAdmin;
        private System.Timers.Timer codeTimer;
        private int waitCodeTime;
        private string tempFileAdminID;

        // console commands:
        public const string CMD_CONNECT    = "connect";
        public const string CMD_DISCONNECT = "disconnect";
        //public const string CMD_SEND       = "send";
        public const string CMD_EXIT       = "exit";
        public const string CMD_CLEAR      = "clear";
        public const string CMD_EXPAND     = "expand";
        public const string CMD_MODULE     = "modules";
        public const string CMD_COMMANDS   = "commands";
        // admin commands
        public const string CMD_ADMIN                 = "psy";
        public const string CMD_ADMIN_ARG1_LOGIN      = "login";

        public const string CMD_ADMIN_ARG1_MOD        = "mod";
        public const string CMD_ADMIN_ARG2_MODINFO    = "info";
        public const string CMD_ADMIN_ARG2_MODINSTALL = "install";
        public const string CMD_ADMIN_ARG2_MODSEARCH  = "search";
        public const string CMD_ADMIN_ARG2_MODENABLE  = "enable";
        public const string CMD_ADMIN_ARG2_MODDISABLE = "disable";
        public const string CMD_ADMIN_ARG2_MODREMOVE  = "remove";

        public const string CMD_ADMIN_ARG1_DISCONNECT = "disconnect";

        // temp login id
        private const string ADMIN_FILE = "psy-admin.txt";

        public PsybotCore()
        {
            client = new DiscordClient(x =>
            {
                x.AppName = "Psybot";
#if DEBUG
                x.LogLevel = LogSeverity.Verbose;
#else
                x.LogLevel = LogSeverity.Info;
#endif
                x.LogHandler = Log;
                //x.AppUrl = "???";
            });

            moduleManager = new ModuleManager(this as IPsybotCore);
            codeTimer = new System.Timers.Timer(1000.0);
            codeTimer.AutoReset = true;
            codeTimer.Elapsed += onEverySeconds;
            codeTimer.Enabled = true;

            if (File.Exists(ADMIN_FILE))
            {
                tempFileAdminID = File.ReadAllText(ADMIN_FILE);
            }
        }

        private void onEverySeconds(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (waitCodeTime > 0)
            {
                waitCodeTime--;
                if (waitCodeTime == 0)
                {
                    Term.Log("Code time is out.", ConsoleColor.White);
                }
            }
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
            if (moduleManager.IsCommandContains(e.Message.RawText))
            {
                if (e.Message.RawText.StartsWith(CMD_ADMIN, StringComparison.Ordinal))
                {
                    try
                    {
                        excecutePsyCommand(e);
                    }
                    catch (Exception ex)
                    {
                        Term.Log("Psy command error: " + ex.Message, ConsoleColor.Red);
                    }
                }
                else
                {
                    var args = new PsybotModuleArgs
                    {
                        Channel = e.Channel.ChannelForModule(),
                        Message = e.Message.MessageForModule(),
                        Server = e.Server.ServerForModule(),
                        User = e.User.UserForModule()
                    };
                    moduleManager.ExcecuteModules(args);
                }
            }
        }

        private async void excecutePsyCommand(MessageEventArgs e)
        {
            var commands = e.Message.RawText.Split(' ');
            if (commands.Length > 1 && !string.IsNullOrWhiteSpace(commands[1]))
            {
                #region ADMIN LOGIN
                // confirm key
                if (waitCodeTime > 0)
                {
                    if (tempCodeName.Equals(commands[1], StringComparison.Ordinal))
                    {
                        userAdmin = e.User;
                        waitCodeTime = 0;
                        tempCodeName = null;

                        // save ID. load on 'psy login'
                        File.WriteAllText(ADMIN_FILE, userAdmin.Id.ToString());

                        Term.Log($"Admin ID apply: {userAdmin.Name} ({userAdmin.Id})", ConsoleColor.White);
                        await e.Channel.SendMessage(e.User.Mention + " :white_check_mark:");
                    }
                    return;
                }

                if (userAdmin == null)
                {
                    // must login
                    if (commands[1] != CMD_ADMIN_ARG1_LOGIN)
                        return;

                    if (userAdmin == null && waitCodeTime == 0)
                    {
                        if (tempFileAdminID != null)
                        {
                            // load from save temp file
                            if (tempFileAdminID.Equals(e.User.Id.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                userAdmin = e.User;
                                Term.Log($"Admin ID apply: {userAdmin.Name} ({userAdmin.Id})", ConsoleColor.White);
                                await e.Channel.SendMessage(e.User.Mention + " :white_check_mark:");
                            }
                        }
                        else
                        {
                            // set code
                            tempCodeName = new Random().Next(1000, 10000).ToString();
                            waitCodeTime = 60;
                            Term.Log($"! Login confirm key - send '{CMD_ADMIN} {tempCodeName}' for 60 seconds", ConsoleColor.Magenta);
                        }
                    }
                    return;
                }
                #endregion

                switch (commands[1])
                {
                case CMD_ADMIN_ARG1_MOD:

                    if (commands.Length > 2 && !string.IsNullOrWhiteSpace(commands[2]))
                    {
                        switch (commands[2])
                        {
                        case CMD_ADMIN_ARG2_MODINFO:

                            await e.Channel.SendMessage(moduleManager.GetModulesInfo());
                            break;
                        // =================================================================== //
                        case CMD_ADMIN_ARG2_MODINSTALL:

                            int containsType = 0;
                            if (commands.Length > 4)
                            {
                                // 2 - [skip] all, 3 - [reload] all
                                if (commands[4].Equals("skip", StringComparison.OrdinalIgnoreCase))
                                    containsType = 2;
                                else if (commands[4].Equals("reload", StringComparison.OrdinalIgnoreCase))
                                    containsType = 3;
                            }
                            if (commands.Length > 3)
                            {
                                var ava = moduleManager.GetAvaiableModules(false);
                                int n;
                                string _log = string.Empty;

                                if (int.TryParse(commands[3], out n))
                                {
                                    // is single number
                                    _log = moduleManager.LoadModuleLibrarys(
                                        new[] { $"{ModuleManager.DEFAULT_MODULE_PATH}\\{ava[n]}.dll" }, // todo: more that one
                                        containsType,
                                        true);
                                }
                                else if (commands[3].Contains(","))
                                {
                                    // numbers
                                    string[] numbers = commands[3].Split(',');
                                    var avaList = new List<string>(numbers.Length);

                                    for (n = 0; n < numbers.Length; n++)
                                    {
                                        int num;
                                        if (int.TryParse(numbers[n], out num))
                                        {
                                            avaList.Add(ava[num - 1]);
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessage("Error: Bad bumbers format. Sample: 1,2,4");
                                            return;
                                        }
                                    }

                                    _log = moduleManager.LoadModuleLibrarys(
                                        avaList.ToArray(),
                                        containsType,
                                        true);
                                }
                                else
                                {
                                    // string name
                                    _log = moduleManager.LoadModuleLibrarys(
                                        new[] { $"{ModuleManager.DEFAULT_MODULE_PATH}\\{commands[3]}.dll" },
                                        containsType,
                                        true);
                                }

                                await e.Channel.SendMessage(_log);
                            }
                            else
                            {
                                await e.Channel.SendMessage("Missing module name arg.");
                            }

                            break;
                        // =================================================================== //
                        case CMD_ADMIN_ARG2_MODSEARCH:

                            var sb = new StringBuilder("Avaiable modules:\n");

                            var mods = moduleManager.GetAvaiableModules(true);
                            if (mods.Length == 0)
                            {
                                sb.Append("(not found)");
                            }
                            else
                            {
                                for (int i = 0; i < mods.Length; i++)
                                {
                                    sb.AppendLine((i + 1) + ": " + mods[i]);
                                }
                            }

                            await e.Channel.SendMessage(sb.ToString());
                            break;
                        // =================================================================== //
                        case CMD_ADMIN_ARG2_MODENABLE:

                            if (commands.Length > 3)
                            {
                                if (commands[3] == "all")
                                {
                                    var ins = moduleManager.GetInstalledModulesName;
                                    for (int i = 0; i < ins.Length; i++)
                                    {
                                        moduleManager.EnableModuleByName(ins[i]);
                                    }
                                    goto case CMD_ADMIN_ARG2_MODINFO;
                                }
                                await e.Channel.SendMessage(
                                    moduleManager.EnableModuleByName(commands[3]) ? "Module was enabled." : "Module not found.");
                            }
                            else
                            {
                                await e.Channel.SendMessage("Missing module name arg.");
                            }

                            break;
                        // =================================================================== //
                        case CMD_ADMIN_ARG2_MODDISABLE:

                            if (commands.Length > 3)
                            {
                                if (commands[3] == "all")
                                {
                                    var ins = moduleManager.GetInstalledModulesName;
                                    for (int i = 0; i < ins.Length; i++)
                                    {
                                        moduleManager.DisableModuleByName(ins[i]);
                                    }
                                    goto case CMD_ADMIN_ARG2_MODINFO;
                                }
                                await e.Channel.SendMessage(
                                    moduleManager.DisableModuleByName(commands[3]) ? "Module was disabled." : "Module not found.");
                            }
                            else
                            {
                                await e.Channel.SendMessage("Missing module name arg.");
                            }

                            break;
                        // =================================================================== //
                        case CMD_ADMIN_ARG2_MODREMOVE:

                            if (commands.Length > 3)
                            {
                                await e.Channel.SendMessage(
                                    moduleManager.UnloadModule(commands[3], true) ? "Module was removed." : "Module not found.");
                            }
                            else
                            {
                                await e.Channel.SendMessage("Missing module name arg.");
                            }

                            break;
                        }
                    }
                    else
                    {
                        var sb = new StringBuilder("**psy mod** commands:\n");
                        sb.AppendLine(CMD_ADMIN_ARG2_MODINFO + " - module information");
                        sb.AppendLine(CMD_ADMIN_ARG2_MODINSTALL + " [module_name/1,2,4,..] [skip/reload]* - install module");
                        sb.AppendLine(CMD_ADMIN_ARG2_MODSEARCH + " - show modules library");
                        sb.AppendLine(CMD_ADMIN_ARG2_MODENABLE + " [module_name/all] - enable module");
                        sb.AppendLine(CMD_ADMIN_ARG2_MODDISABLE + " [module_name/all] - disable module");
                        sb.AppendLine(CMD_ADMIN_ARG2_MODREMOVE + " [module_name] - remove module");
                        await e.Channel.SendMessage(sb.ToString());
                    }

                    break;

                case CMD_ADMIN_ARG1_DISCONNECT:

                    Term.Log("Disconnect from channel (from chat).", ConsoleColor.White);
                    await client.Disconnect();

                    break;
                }
            }
            else
            {
                var sb = new StringBuilder("**psy** commands:\n");
                sb.AppendLine(CMD_ADMIN_ARG1_MOD + " - modules");
                sb.AppendLine(CMD_ADMIN_ARG1_DISCONNECT + " - disconnect bot from server");
                await e.Channel.SendMessage(sb.ToString());
            }
        }

        private void Term_OnDraw()
        {
            Term.ClearLine(0);

            // Y:0
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
                $" | MEM: {(GC.GetTotalMemory(false) / (1024L * 1024L))} mb | Modules: {moduleManager.GetEnabledModules}/{moduleManager.GetInstalledModulesCount}",
                20, 0, ConsoleColor.Gray);

            // Y:1
            if (userAdmin == null)
            {
                Term.Draw("Psy admin: none", 0, 1, ConsoleColor.Gray);
            }
            else
            {
                Term.Draw("Psy admin: " + userAdmin.Name, 0, 1, ConsoleColor.Gray);
            }
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
            Term.AddCommand(CMD_MODULE, (s) =>
            {
                moduleManager.EnterGUI();
            }, "Enter modules manager.");
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

        public async void SendImage(ulong channelID, string filePath)
        {
            if (!Connected || !File.Exists(filePath))
                return;
            await client.GetChannel(channelID)?.SendFile(filePath);
        }

        /// <summary>
        ///     Write a text to server log.
        /// </summary>
        /// <param name="mess"> Text. </param>
        /// <param name="color"> Text color. </param>
        public void SendLog(string mess, ConsoleColor color)
        {
            // todo: auto detect owner module name
            Term.Log("$ " + mess, color);
        }

        #endregion

    }
}
