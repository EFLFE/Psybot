using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Psybot.Data;
using Psybot.UI;
using PsybotModule;
using System.Text;

namespace Psybot.Modules
{
    internal sealed class ModuleManager
    {
        public const string DEFAULT_MODULE_PATH = "Modules";

        public const string INI_CONFIG_FILE_NAME     = "config.ini";
        public const string INI_CONFIG_SECTION_NAME  = "search.modules";
        public const string INI_CONFIG_SECTION_PATH  = "path";
        // TODO private const string INI_CONFIG_SECTION_PATHS = "paths";
        /* (for ModuleCompiler)
        private const string INI_MODULE_CONFIG_FILE_NAME  = "info.ini";
        private const string INI_MODULE_SECTION_NAME      = "module";
        private const string INI_MODULE_SECTION_NAMESPACE = "namespace";
        private const string INI_MODULE_SECTION_CLASS     = "class";
        private const string INI_MODULE_SECTION_TITLE     = "title";
        private const string INI_MODULE_SECTION_DESC      = "desc";
        */

        private IPsybotCore core;
        //private ModuleCompiler compiler;
        private IniReader iniReader;
        private List<ModuleData> modulesListData;
        private Dictionary<string, int> modulesDictData; // поиск индекса плагина в листе по имени
        private int enabledModules;

        // RunCommandName array from all modulef for fast parse
        private List<string> runCommandsList;

        public int GetInstalledModulesCount => modulesListData.Count;

        public int GetEnabledModules => enabledModules;

        public string[] GetInstalledModulesName => modulesDictData.Keys.ToArray();

        public ModuleManager(IPsybotCore ipsycore)
        {
            core = ipsycore;
            iniReader = new IniReader(INI_CONFIG_FILE_NAME);
            modulesListData = new List<ModuleData>();
            modulesDictData = new Dictionary<string, int>();
            runCommandsList = new List<string>() { PsybotCore.CMD_ADMIN };

            if (!Directory.Exists(DEFAULT_MODULE_PATH))
            {
                Directory.CreateDirectory(DEFAULT_MODULE_PATH);
            }
        }

        /// <summary>
        ///     Load module.
        /// </summary>
        /// <param name="moduleName"> Module name. </param>
        /// <returns> Module was need? </returns>
        public bool LoadModule(string moduleName)
        {
            if (modulesDictData.ContainsKey(moduleName))
            {
                var index = modulesDictData[moduleName];
                modulesListData[index].Module.Load(core);
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Unload module.
        /// </summary>
        /// <param name="moduleName"> Module name. </param>
        /// <param name="remove"> Remove module. </param>
        /// <returns> Module was need? </returns>
        public bool UnloadModule(string moduleName, bool remove)
        {
            if (modulesDictData.ContainsKey(moduleName))
            {
                var index = modulesDictData[moduleName];

                if (modulesListData[index].Status == ModuleData.StatusEnum.Enable)
                {
                    enabledModules--;
                }

                // unload
                if (modulesListData[index].Status == ModuleData.StatusEnum.Enable)
                    modulesListData[index].Module.OnDisable();
                modulesListData[index].Module.Unload();
                modulesListData[index].Status = ModuleData.StatusEnum.Unloaded;

                if (remove)
                {
                    runCommandsList.Remove(modulesListData[index].Module.RunCommandName);
                    modulesListData.RemoveAt(index);
                    // recheck index
                    modulesDictData.Clear();
                    for (int i = 0; i < modulesListData.Count; i++)
                    {
                        modulesDictData.Add(modulesListData[i].FileName, i);
                    }
                }
                return true;
            }
            return false;
        }

        // ooooooptimizaaaatioooon!!! (no, crutch)
        public bool IsCommandContains(string messageRawText)
        {
            if (!string.IsNullOrWhiteSpace(messageRawText))
            {
                for (int i = runCommandsList.Count - 1; i >= 0; i--)
                {
                    if (messageRawText.StartsWith(runCommandsList[i], StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // on user send message
        public async void ExcecuteModules(PsybotModuleArgs args)
        {
            // for modules
            for (int i = 0; i < modulesListData.Count; i++)
            {
                // check status
                if (modulesListData[i].Status == ModuleData.StatusEnum.Enable)
                {
                    // check command
                    string cmd = modulesListData[i].Module.RunCommandName;

                    if (args.Message.RawText.StartsWith(cmd, modulesListData[i].Module.CommandComparison))
                    {
                        try
                        {
                            args.Message.Text = args.Message.RawText.Remove(0, modulesListData[i].Module.RunCommandName.Length).Trim(); // rm command

                            await modulesListData[i].Module.Excecute(args);
                        }
                        catch (Exception ex)
                        {
                            enabledModules--;
                            modulesListData[i].Status = ModuleData.StatusEnum.Crash;
                            modulesListData[i].CrashException = ex;
                            Term.Log("Module crash: " + modulesListData[i].FileName, ConsoleColor.Red);
                        }
                    }
                }
            }
        }

        /// <summary> Перейти в окно управления плагинами. Данный метод сам останавливает и воспроизводит логирование. </summary>
        public void EnterGUI()
        {
            Term.Pause = true;
            Term.Flush();

            myModulesControl();

            Console.Clear();
            Term.Pause = false;
            Term.ReDrawLog();
        }

        public string GetModulesInfo()
        {
            var sb = new StringBuilder("Psybot module info:\n");
            if (modulesDictData.Count == 0)
            {
                sb.Append("No module found.");
            }
            else
            {
                for (int i = 0; i < modulesListData.Count; i++)
                {
                    sb.AppendLine($"{i + 1}: {modulesListData[i].FileName} ({modulesListData[i].Status})");
                }
            }
            return sb.ToString();
        }

        private void myModulesControl()
        {
            Console.Clear();
            int selected = 0;

            while (true)
            {
                Term.Draw("= My Modules", 0, 1, ConsoleColor.White);

                if (modulesListData.Count != 0)
                {
                    Term.Draw("[Arrow] - Navigation | [E] Enable | [D] Disable | [R] Remove", 0, Console.WindowHeight - 2);
                }
                Term.Draw("[I] Module info      | [B] Back   | [F] Find new modules", 0, Console.WindowHeight - 1);

                if (modulesListData.Count == 0)
                {
                    Term.Draw("(empty)", 0, 3, ConsoleColor.Gray);
                }
                else
                {
                    for (int i = 0; i < modulesListData.Count; i++)
                    {
                        drawModuleInfo(i + 1, i + 3, null, modulesListData[i].FileName, modulesListData[i].Status, i == selected);
                    }
                }

                Term.Flush();
                var k = Term.ReadKey();

                switch (k.Key)
                {
                case ConsoleKey.B: // back
                    return;

                case ConsoleKey.UpArrow:
                case ConsoleKey.W:
                case ConsoleKey.NumPad8:

                    if (modulesListData.Count == 0) break;

                    if (selected > 0)
                        selected--;
                    break;

                case ConsoleKey.DownArrow:
                case ConsoleKey.S:
                case ConsoleKey.NumPad2:

                    if (modulesListData.Count == 0) break;

                    if (selected < modulesListData.Count - 1)
                        selected++;
                    break;

                case ConsoleKey.E: // enable

                    if (modulesListData.Count == 0) break;

                    enableModule(selected);
                    break;

                case ConsoleKey.D: // disable

                    if (modulesListData.Count == 0) break;

                    disableModule(selected);
                    break;

                case ConsoleKey.R: // remove

                    if (modulesListData.Count == 0) break;
                    var st = modulesListData[selected].Status;

                    if (UnloadModule(modulesListData[selected].FileName, true))
                    {
                        if (selected == modulesListData.Count)
                            selected--;

                        Console.Clear();
                    }
                    break;

                case ConsoleKey.I: // info

                    if (modulesListData.Count == 0) break;

                    showModuleInfo(selected);

                    break;

                case ConsoleKey.F: // find
                    searchModulesLibrary();
                    // reset
                    selected = 0;
                    break;
                }
            }
        }

        public bool EnableModuleByName(string moduleName)
        {
            if (modulesDictData.ContainsKey(moduleName))
            {
                var index = modulesDictData[moduleName];
                enableModule(index);
                return true;
            }
            return false;
        }

        public bool DisableModuleByName(string moduleName)
        {
            if (modulesDictData.ContainsKey(moduleName))
            {
                var index = modulesDictData[moduleName];
                disableModule(index);
                return true;
            }
            return false;
        }

        private void enableModule(int index)
        {
            if (modulesListData[index].Status != ModuleData.StatusEnum.Enable)
            {
                if (modulesListData[index].Status == ModuleData.StatusEnum.Disable)
                {
                    enabledModules++;
                }
                try
                {
                    modulesListData[index].Module.OnEnable();
                    modulesListData[index].Status = ModuleData.StatusEnum.Enable;
                }
                catch (Exception ex)
                {
                    Term.Log(modulesListData[index].FileName + " error: " + ex.Message);
                    modulesListData[index].Status = ModuleData.StatusEnum.Crash;
                }
            }
        }

        private void disableModule(int index)
        {
            if (modulesListData[index].Status != ModuleData.StatusEnum.Disable)
            {
                modulesListData[index].Status = ModuleData.StatusEnum.Disable;
                try
                {
                    modulesListData[index].Module.OnDisable();
                }
                finally
                {
                    enabledModules--;
                }
            }
        }

        private void showModuleInfo(int listIndex)
        {
            var module = modulesListData[listIndex];

            Console.Clear();
            Term.FastDraw("== Module info: " + module.FileName, 0, 2, ConsoleColor.White);

            Term.FastDraw("Full name: " + module.FullFileName, 0, 3, ConsoleColor.Gray);
            Term.FastDraw("\nFull path: " + module.FullPath);
            Term.FastDraw("\nAssembly type: " + module.ModuleAssemblyType.ToString());

            // todo: Term.FastDraw("File version: ");
            // todo description

            if (module.Status == ModuleData.StatusEnum.Crash && module.CrashException != null)
            {
                Term.FastDraw("\n\nCrash information:\n", ConsoleColor.Red);
                Term.FastDraw(module.CrashException.ToString(), ConsoleColor.Gray);
            }

            Term.ReadKey(true);
            Console.Clear();
        }

        public string[] GetAvaiableModules(bool nameOnly)
        {
            string[] mods = Directory.GetFiles(DEFAULT_MODULE_PATH, "*.dll");
            if (nameOnly)
            {
                for (int i = 0; i < mods.Length; i++)
                {
                    mods[i] = Path.GetFileNameWithoutExtension(mods[i]);
                }
            }
            return mods;
        }

        private void searchModulesLibrary()
        {
            // get library in default folder
            string[] libs = Directory.GetFiles(DEFAULT_MODULE_PATH, "*.dll");

            if (libs.Length == 0)
            {
                Console.Clear();
                Term.Draw("== Browse Modules >> Modules not found... ", 0, 1, ConsoleColor.White);
                Term.Flush();
                Term.ReadKey();
                return;
            }

            Console.Clear();
            Term.Draw("== Browse Modules", 0, 1, ConsoleColor.White);
            Term.Draw("[Arrow] - Navigation | [Space] Mark | [L] Load selected | [B] Back", 0, Console.WindowHeight - 1);

            int selected = 0;
            bool[] flags = new bool[libs.Length];
            string[] names = new string[libs.Length];

            // get full info
            for (int i = 0; i < libs.Length; i++)
            {
                var ver = System.Diagnostics.FileVersionInfo.GetVersionInfo(libs[i]);
                var fileName = string.IsNullOrWhiteSpace(ver.ProductName) ? Path.GetFileName(libs[i]) : ver.ProductName;
                var fileVersion = string.IsNullOrWhiteSpace(ver.FileVersion) ? "?" : ver.FileVersion;

                names[i] = $"{fileName}  v{fileVersion}";
                libs[i] = Path.GetFullPath(libs[i]);
            }

            while (true)
            {
                // menu dll's
                for (int i = 0; i < libs.Length; i++)
                {
                    drawModuleInfo(i + 1, 3 + i, flags[i], names[i], null, selected == i);
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
                    LoadModuleLibrarys(selectedLibs, 0, false);

                    return;
                }
            }
        }

        /// <summary>
        ///     Load modules.
        /// </summary>
        /// <param name="path"> Modules full path (root from <see cref="DEFAULT_MODULE_PATH"/>). </param>
        /// <param name="containsType"> Action flag for contains modules: 2 - [skip] all, 3 - [reload] all. </param>
        /// <param name="returnLog"> If true, this method return string log (for excecute from channel). </param>
        /// <returns> Log (if returnLog == true). </returns>
        public string LoadModuleLibrarys(string[] path, int containsType, bool returnLog)
        {
            StringBuilder sb = null;
            if (returnLog)
            {
                sb = new StringBuilder();
            }
            else
            {
                Console.Clear();
                Console.SetCursorPosition(0, 0);
            }
            Term.FastDraw("=== Loading " + path.Length + " modules\n", ConsoleColor.White);
            sb?.AppendLine("Loading " + path.Length + " modules");

            // 0 - skip, 1 - reload, 2 - skip all, 3 - reload all
            int loaded = 0, skips = 0, errors = 0;

            for (int i = 0; i < path.Length; i++)
            {
                var modFileName = Path.GetFileNameWithoutExtension(path[i]);

                Term.FastDraw("\n> " + path[i] + "\n", ConsoleColor.White);
                sb?.AppendLine("> Installing '**" + modFileName + "**'");

                if (!File.Exists(path[i]))
                {
                    Term.FastDraw("Error: File not found.\n", ConsoleColor.Red);
                    sb?.AppendLine(":warning: Error: Module not found.");
                    continue;
                }

                try
                {
                    // check for reload module
                    if (modulesDictData.ContainsKey(modFileName))
                    {
                        if (returnLog && containsType < 2)
                        {
                            // we can not run Term.ReadKey
                            sb.AppendLine(":warning: This module already exists.\nUse args:\n  [skip] - skip all\n  [reload] - reload all");
                            goto RETURN;
                        }
                        else
                        {
                            Term.FastDraw("This module already exists.\n", ConsoleColor.Yellow);
                            if (containsType < 2)
                            {
                                Term.FastDraw("[S] Skip (default) | [R] Reload | [G] Skip all | [U] Reload all ", ConsoleColor.Gray);

                                switch (Term.ReadKey(true).Key) // !!!
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
                        }

                        switch (containsType)
                        {
                        case 0:
                        case 2:
                            // skip
                            skips++;
                            Term.FastDraw("Skip\n", ConsoleColor.Gray);
                            continue;

                        case 1:
                        case 3:
                            // reload
                            Term.FastDraw("Reload\n", ConsoleColor.Gray);
                            UnloadModule(modFileName, true);
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
                            Type ipp = types[j].GetInterface(nameof(IPsybotModule), false);

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
                        errors++;
                        var errorText = "Fail: Interface 'IPsybotModule' not found.\n";
                        sb?.Append(":warning: " + errorText);
                        Term.FastDraw(errorText, ConsoleColor.Red);
                        continue;
                    }

                    Term.FastDraw("Create instance\n", ConsoleColor.Gray);
                    IPsybotModule moduel = Activator.CreateInstance(types[typeIndex]) as IPsybotModule;

                    // loading
                    Term.FastDraw("Loading\n", ConsoleColor.Gray);
                    moduel.Load(core);

                    // check
                    if (string.IsNullOrWhiteSpace(moduel.RunCommandName))
                    {
                        errors++;
                        var errorText = "Error: 'RunCommandName' is not set.\n";
                        sb?.Append(":warning: " + errorText);
                        Term.FastDraw(errorText, ConsoleColor.Red);
                        continue;
                    }
                    if (moduel.RunCommandName.Length > 18)
                    {
                        errors++;
                        var errorText = "Error: 'RunCommandName' text size should not be more than 18.\n";
                        sb?.Append(":warning: " + errorText);
                        Term.FastDraw(errorText, ConsoleColor.Red);
                        continue;
                    }
                    if (moduel.RunCommandName.StartsWith(PsybotCore.CMD_ADMIN, StringComparison.Ordinal))
                    {
                        errors++;
                        var errorText = "Error: 'RunCommandName' can not start with '" + PsybotCore.CMD_ADMIN + "'.\n";
                        sb?.Append(":warning: " + errorText);
                        Term.FastDraw(errorText, ConsoleColor.Red);
                        continue;
                    }

                    // add to dict
                    modulesListData.Add(new ModuleData(path[i], moduel));
                    modulesDictData.Add(modFileName, modulesListData.Count - 1);
                    runCommandsList.Add(modulesListData[modulesListData.Count - 1].Module.RunCommandName);

                    // success
                    loaded++;

                    sb?.AppendLine(":white_check_mark: Success");
                    Term.FastDraw("Success\n", ConsoleColor.Green);
                }
                catch (Exception ex)
                {
                    errors++;
                    var exm = ex.Message;

                    if (!returnLog && exm.Length > Console.WindowWidth - 12)
                    {
                        exm = exm.Remove(Console.WindowWidth - 12) + "...";
                    }

                    var exText = "Exception: " + ex.Message + " \n";
                    sb?.Append(":warning: " + exText);
                    Term.FastDraw(exText, ConsoleColor.Red);
                }
            }

            var summary = $"\nSummary:\n  Loaded: {loaded}\n  Errors: {errors}\n  Skips:  {skips}";
            Term.FastDraw(summary, ConsoleColor.Gray);
            sb?.AppendLine(summary);

            if (!returnLog)
            {
                Term.FastDraw("\nPress any key ", ConsoleColor.White);
                Term.ReadKey(true); // !!!
                Console.Clear();
            }

        RETURN:
            if (returnLog)
            {
                return sb.ToString();
            }
            return null;
        }

        private void drawModuleInfo(int num, int y, bool? flag, string title, ModuleData.StatusEnum? status = null, bool selected = false)
        {
            // > {num} │ [{(flag ? "x" : " ")}] │ {title} │ {status}
            var clr = selected ? ConsoleColor.White : ConsoleColor.Gray;

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
                    Term.FastDraw($"> {num} │ [{(flag.Value ? "x" : " ")}] │ ", 0, y, ConsoleColor.Gray);
                else
                    Term.FastDraw($"> {num} │ ", 0, y, ConsoleColor.Gray);
            }

            if (title.Length > 48)
                title = title.Remove(48);

            Term.FastDraw(title, clr);

            if (status.HasValue)
            {
                Term.FastDraw(" │ ", clr);

                switch (status.Value)
                {
                case ModuleData.StatusEnum.Disable:
                    Term.FastDraw(status.ToString() + "   ", ConsoleColor.Gray);
                    break;

                case ModuleData.StatusEnum.Enable:
                    Term.FastDraw(status.ToString() + "   ", ConsoleColor.Green);
                    break;

                case ModuleData.StatusEnum.Crash:
                    Term.FastDraw(status.ToString() + "   ", ConsoleColor.Red);
                    break;

                case ModuleData.StatusEnum.Unloaded:
                    Term.FastDraw(status.ToString() + "   ", ConsoleColor.Gray);
                    break;
                }
            }
        }

    }
}
