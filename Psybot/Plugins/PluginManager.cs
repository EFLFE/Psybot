using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Psybot.Data;
using Psybot.UI;
using PsybotPlugin;

namespace Psybot.Plugins
{
    internal sealed class PluginManager
    {
        private const string DEFAULT_PLUGIN_PATH = "Plugins";

        private const string INI_CONFIG_FILE_NAME     = "config.ini";
        private const string INI_CONFIG_SECTION_NAME  = "search.plugins";
        private const string INI_CONFIG_SECTION_PATH  = "path";
        // TODO private const string INI_CONFIG_SECTION_PATHS = "paths";
        /* (for PluginCompiler)
        private const string INI_PLUGIN_CONFIG_FILE_NAME  = "info.ini";
        private const string INI_PLUGIN_SECTION_NAME      = "plugin";
        private const string INI_PLUGIN_SECTION_NAMESPACE = "namespace";
        private const string INI_PLUGIN_SECTION_CLASS     = "class";
        private const string INI_PLUGIN_SECTION_TITLE     = "title";
        private const string INI_PLUGIN_SECTION_DESC      = "desc";
        */
        private IPsybotCore core;
        //private PluginCompiler compiler;
        private IniReader iniReader;
        private List<PluginData> pluginsListData;
        private Dictionary<string, int> pluginsDictData; // поиск индекса плагина в листе по имени

        public PluginManager(IPsybotCore ipsycore)
        {
            core = ipsycore;
            iniReader = new IniReader(INI_CONFIG_FILE_NAME);
            pluginsListData = new List<PluginData>();
            pluginsDictData = new Dictionary<string, int>();

            if (!Directory.Exists(DEFAULT_PLUGIN_PATH))
            {
                Directory.CreateDirectory(DEFAULT_PLUGIN_PATH);
            }
        }

        public bool LoadPlugin(string pluginName)
        {
            if (pluginsDictData.ContainsKey(pluginName))
            {
                var index = pluginsDictData[pluginName];
                pluginsListData[index].Plugin?.Load(core);
                return true;
            }
            return false;
        }

        public bool UnloadPlugin(string pluginName, bool remove)
        {
            if (pluginsDictData.ContainsKey(pluginName))
            {
                var index = pluginsDictData[pluginName];
                pluginsListData[index].Plugin?.Unload();
                if (remove)
                {
                    pluginsListData.RemoveAt(index);
                    pluginsDictData.Remove(pluginName);
                }
                return true;
            }
            return false;
        }

        public async void ExcecutePlugins(PsybotPluginArgs args)
        {
            for (int i = 0; i < pluginsListData.Count; i++)
            {
                if (pluginsListData[i].Status == PluginData.StatusEnum.Enable)
                {
                    if (args.Message.StartsWith(pluginsListData[i].Plugin.RunCommandName, StringComparison.Ordinal))
                    {
                        try
                        {
                            await pluginsListData[i].Plugin.Excecute(args);
                        }
                        catch (Exception ex)
                        {
                            pluginsListData[i].Status = PluginData.StatusEnum.Crash;
                            Term.Log(ex.Message, ConsoleColor.Red);
                            Term.Log("Plugin crash: " + pluginsListData[i].FileName, ConsoleColor.Red);
                        }
                    }
                }
            }
        }

        /// <summary> Перейти в окно управления плагинами. Данный метод сам останавливает и воспроизводит логирование. </summary>
        public void EnterGUI()
        {
            Term.Stop();
            Term.Flush();

            myPluginsControl();

            Console.Clear();
            Term.Start();
            Term.ReDrawLog();
        }

        private void myPluginsControl()
        {
            Console.Clear();
            int selected = 0;

            while (true)
            {
                Term.Draw("= My Plugins", 0, 1, ConsoleColor.White);

                if (pluginsListData.Count != 0)
                {
                    Term.Draw("[Arrow] - Navigation | [E] Enable | [D] Disable | [R] Remove", 0, Console.WindowHeight - 2);
                }
                Term.Draw("[B] Back | [F] Find new plugins", 0, Console.WindowHeight - 1);

                if (pluginsListData.Count == 0)
                {
                    Term.Draw("(empty)", 0, 3, ConsoleColor.DarkGray);
                }
                else
                {
                    for (int i = 0; i < pluginsListData.Count; i++)
                    {
                        drawPlugInfo(i + 1, i + 3, null, pluginsListData[i].FileName, pluginsListData[i].Status.ToString(), i == selected);
                    }
                }

                Term.Flush();
                var k = Term.ReadKey();

                switch (k.Key)
                {
                case ConsoleKey.B:
                    return;

                case ConsoleKey.UpArrow:
                case ConsoleKey.W:
                case ConsoleKey.NumPad8:

                    if (pluginsListData.Count == 0) break;

                    if (selected > 0)
                        selected--;
                    break;

                case ConsoleKey.DownArrow:
                case ConsoleKey.S:
                case ConsoleKey.NumPad2:

                    if (pluginsListData.Count == 0) break;

                    if (selected < pluginsListData.Count - 1)
                        selected++;
                    break;

                case ConsoleKey.E:

                    pluginsListData[selected].Status = PluginData.StatusEnum.Enable;
                    break;

                case ConsoleKey.D:

                    if (pluginsListData.Count == 0) break;

                    pluginsListData[selected].Status = PluginData.StatusEnum.Disable;
                    break;

                case ConsoleKey.R:

                    if (pluginsListData.Count == 0) break;

                    if (UnloadPlugin(pluginsListData[selected].FileName, true) && selected == pluginsListData.Count)
                        selected--;
                    break;

                case ConsoleKey.F:
                    searchPluginsLibrary();
                    // reset
                    selected = 0;
                    break;
                }
            }
        }

        private void searchPluginsLibrary()
        {
            string[] libs = Directory.GetFiles(DEFAULT_PLUGIN_PATH, "*.dll");

            if (libs.Length == 0)
            {
                Console.Clear();
                Term.Draw("== Browse Plugins >> Plugins not found... ", 0, 1, ConsoleColor.White);
                Term.Flush();
                Term.ReadKey();
                return;
            }

            Console.Clear();
            Term.Draw("== Browse Plugins", 0, 1, ConsoleColor.White);
            Term.Draw("[Arrow] - Navigation | [Space] Mark | [L] Load selected | [B] Back", 0, Console.WindowHeight - 1);

            int selected = 0;
            bool[] flags = new bool[libs.Length];
            string[] names = new string[libs.Length];

            for (int i = 0; i < libs.Length; i++)
            {
                names[i] = Path.GetFileNameWithoutExtension(libs[i]);
                libs[i] = Path.GetFullPath(libs[i]);
            }

            while (true)
            {
                // menu dll's
                for (int i = 0; i < libs.Length; i++)
                {
                    drawPlugInfo(i + 1, 3 + i, flags[i], names[i], null, selected == i);
                }

                Term.Flush();
                var k = Term.ReadKey();

                switch (k.Key)
                {
                case ConsoleKey.UpArrow:
                case ConsoleKey.W:
                case ConsoleKey.NumPad8:

                    if (selected > 0)
                        selected--;
                    break;

                case ConsoleKey.DownArrow:
                case ConsoleKey.S:
                case ConsoleKey.NumPad2:

                    if (selected < libs.Length - 1)
                        selected++;
                    break;

                case ConsoleKey.Enter:
                case ConsoleKey.Spacebar:
                case ConsoleKey.NumPad5:

                    flags[selected] = !flags[selected];
                    break;

                case ConsoleKey.B:
                case ConsoleKey.Escape:

                    Console.Clear();
                    return;

                case ConsoleKey.L:

                    // load
                    if (flags.All(f => f == false))
                        break;

                    string[] selectedLibs = libs.Where((p, x) => flags[x] == true).ToArray();
                    loadLibrarys(selectedLibs);

                    return;
                }
            }
        }

        private void loadLibrarys(string[] path)
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Term.FastDraw("=== Loading " + path.Length + " plugins\n", ConsoleColor.White);

            // 0 - skip, 1 - reload, 2 - skip all, 3 - reload all
            int containsType = 0;

            for (int i = 0; i < path.Length; i++)
            {
                try
                {
                    Term.FastDraw("\n> " + path[i] + "\n", ConsoleColor.White);

                    // check for reload plugin
                    if (pluginsDictData.ContainsKey(Path.GetFileNameWithoutExtension(path[i])))
                    {
                        Term.FastDraw("This plugin are contains.\n", ConsoleColor.Yellow);
                        if (containsType < 2)
                        {
                            Term.FastDraw("[S] Skip (default) | [R] Reload | [G] Skip all | [U] Reload all ", ConsoleColor.Gray);

                            switch (Term.ReadKey(true).Key)
                            {
                            case ConsoleKey.S:
                                containsType = 0;
                                break;

                            case ConsoleKey.R:
                                containsType = 1;
                                break;

                            case ConsoleKey.G:
                                containsType = 2;
                                break;

                            case ConsoleKey.U:
                                containsType = 3;
                                break;
                            }
                            Term.FastDraw("\n");
                        }

                        switch (containsType)
                        {
                        case 0:
                        case 2:
                            // skip
                            Term.FastDraw("Skip\n", ConsoleColor.Gray);
                            continue;

                        case 1:
                        case 3:
                            // reload
                            Term.FastDraw("Reload\n", ConsoleColor.Gray);
                            UnloadPlugin(Path.GetFileNameWithoutExtension(path[i]), true);
                            break;
                        }
                    }

                    //Assembly asm = Assembly.LoadFile(dllPath); // блокирует файл до закрытия приложения
                    Term.FastDraw("Loading assembly\n", ConsoleColor.Gray);
                    Assembly asm = Assembly.Load(File.ReadAllBytes(path[i]));

                    Term.FastDraw("Search\n", ConsoleColor.Gray);
                    Type[] types = asm.ExportedTypes.ToArray(); // (i'm hate the foreach)
                    int typeIndex = -1;

                    // find class winh interface
                    for (int j = 0; j < types.Length; j++)
                    {
                        if (types[j].IsClass)
                        {
                            Type ipp = types[j].GetInterface(nameof(IPsybotPlugin), false);

                            if (ipp != null)
                            {
                                typeIndex = j;
                                break;
                            }
                        }
                    }
                    // wad found?
                    if (typeIndex == -1)
                    {
                        Term.FastDraw("Fail: Interface 'IPsybotPlugin' not found.\n", ConsoleColor.Red);
                        continue;
                    }

                    Term.FastDraw("Create instance\n", ConsoleColor.Gray);
                    IPsybotPlugin plug = Activator.CreateInstance(types[typeIndex]) as IPsybotPlugin;

                    // loading
                    Term.FastDraw("Loading\n", ConsoleColor.Gray);
                    plug.Load(core);

                    // check
                    if (string.IsNullOrWhiteSpace(plug.RunCommandName))
                    {
                        Term.FastDraw("Error: 'RunCommandName' is not set.\n", ConsoleColor.Red);
                        continue;
                    }
                    if (plug.RunCommandName.Length > 18)
                    {
                        Term.FastDraw("Error: 'RunCommandName' text size should not be more than 18.\n", ConsoleColor.Red);
                        continue;
                    }

                    // add to dict
                    pluginsListData.Add(new PluginData(path[i], plug));
                    pluginsDictData.Add(Path.GetFileNameWithoutExtension(path[i]), pluginsListData.Count - 1);

                    // success
                    Term.FastDraw("Success\n", ConsoleColor.Green);
                }
                catch (Exception ex)
                {
                    var exm = ex.Message;
                    if (exm.Length > Console.WindowWidth - 12)
                        exm = exm.Remove(Console.WindowWidth - 12) + "...";

                    Term.FastDraw("Exception: " + ex.Message + " \n", ConsoleColor.Red);
                }
            }

            Term.FastDraw("\nReady. Press any key ", ConsoleColor.Blue);
            Term.ReadKey(true);
            Console.Clear();
        }

        private void drawPlugInfo(int num, int y, bool? flag, string title, string status = null, bool selected = false)
        {
            // > {num} │ [{(flag ? "x" : " ")}] │ {title} │ {status}
            var clr = selected ? ConsoleColor.White : ConsoleColor.DarkGray;

            if (selected)
            {
                Term.FastDraw("> " + num, 0, y, ConsoleColor.Black, ConsoleColor.White);

                if (flag.HasValue)
                    Term.FastDraw($" │ [{(flag.Value ? "x" : " ")}] │ ", ConsoleColor.White, ConsoleColor.Black);
                else
                    Term.FastDraw($" │ ", ConsoleColor.White, ConsoleColor.Black);
            }
            else
            {
                if (flag.HasValue)
                    Term.FastDraw($"> {num} │ [{(flag.Value ? "x" : " ")}] │ ", 0, y, ConsoleColor.DarkGray);
                else
                    Term.FastDraw($"> {num} │ ", 0, y, ConsoleColor.DarkGray);
            }

            if (title.Length > 48)
                title = title.Remove(48);

            Term.FastDraw(title, clr);

            if (status != null)
                Term.FastDraw(" │ " + status + "   ", clr);
        }

    }
}
