using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Psybot.UI
{
    // TODO: наверсти порядок в этом классе
    public static class Term
    {
        // VAR //

        private const int LOG_LIST_CAPACITY = 128;

        public delegate void DelOnCommandEnter(string comName, string arg);
        public delegate void DelOnDraw();

        public static event DelOnCommandEnter OnCommandEnter = null;
        public static event DelOnDraw OnDraw = null;

        /// <summary> [command name, [action, description]] </summary>
        private static Dictionary<string, Tuple<Action<string>, string>> commands
                 = new Dictionary<string, Tuple<Action<string>, string>>();

        private static Queue<WriteData> writeData = new Queue<WriteData>();

        // [string text], [color, time]
        private static List<Tuple<string, ConsoleColor, string>> logList
                 = new List<Tuple<string, ConsoleColor, string>>(LOG_LIST_CAPACITY);

        private static int logListCount;
        private static bool isRunning;
        private static string inputText = string.Empty;
        private static string searchText = string.Empty;
        private static int skip;
        private static int startLogY = 4;

        private static object writeLock = new object();

        public static bool Pause = false;

        public static bool IsRunning => isRunning;

        // MET //

        // todo: void ExportLogToFile

        /// <summary> Добавить комманду. </summary>
        /// <param name="comName"> Имя комманды. </param>
        /// <param name="action"> Метод, который вызывается при вводе данной комманды. </param>
        public static void AddCommand(string comName, Action<string> action, string description)
        {
            commands.Add(comName, new Tuple<Action<string>, string>(action, description));
        }

        public static void ShowAllCommands()
        {
            lock (writeLock)
            {
                Pause = true;
                Console.Clear();
                Console.SetCursorPosition(0, 0);

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Commands:");
                Console.ForegroundColor = ConsoleColor.Gray;
                foreach (var com in commands)
                {
                    if (com.Value.Item2 != null && com.Value.Item2.Length > 0)
                    {
                        Console.Write($"  {com.Key}");
                        Console.CursorLeft = 20;
                        Console.WriteLine($"- {com.Value.Item2}");
                    }
                    else
                    {
                        Console.WriteLine($"  {com.Key}");
                    }
                }

                Console.ReadKey(true);
                Console.Clear();
                Pause = false;
                ReDrawLog();
            }
        }

        /// <summary> Настройка терминала. </summary>
        /// <param name="title"> Заголовок, если необходимо. </param>
        public static void Init(string title)
        {
            if (title != null && title.Length > 0)
                Console.Title = "MasterServerProgram";

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.TreatControlCAsInput = true;
            Console.Clear();
        }

        /// <summary> Запустить поток управления вводом. </summary>
        public static void Start()
        {
            if (!isRunning)
            {
                Console.Clear();
                Log("Terminal started!", ConsoleColor.Cyan);
                isRunning = true;
                Console.CursorVisible = false;
                ThreadPool.QueueUserWorkItem(termPool, null);
            }
        }

        /// <summary> Остановить поток. </summary>
        public static void Stop()
        {
            if (isRunning)
            {
                Log("Terminal stoped!", ConsoleColor.Cyan);
                isRunning = false;
                Console.CursorVisible = true;
            }
        }

        private static void termPool(object _)
        {
            int sleepTime = 50;

            Draw(":", 0, Console.WindowHeight - 1, ConsoleColor.Cyan);
            Log("Welcome!", ConsoleColor.Magenta);

            try
            {
                while (isRunning)
                {
                    Thread.Sleep(sleepTime);

                    if (sleepTime < 500)
                        sleepTime++;

                    if (Pause)
                        continue;

                    #region INPUT

                    if (Console.KeyAvailable)
                    {
                        sleepTime = 1;
                        var k = Console.ReadKey(true);
                        if (k.Key == ConsoleKey.Tab)
                        {
                            if (commands.Count != 0 && inputText.Length != 0 && !inputText.Contains(' '))
                            {
                                // подбор
                                if (skip == 0)
                                {
                                    searchText = inputText;
                                }
                                using (IEnumerator<string> keyEnum = commands.Keys.GetEnumerator())
                                {
                                    while (keyEnum.MoveNext())
                                    {
                                        for (int i = 0; i < skip; i++)
                                            if (!keyEnum.MoveNext())
                                                keyEnum.Reset();

                                        if (keyEnum.Current == null)
                                            break; // for safe

                                        if (keyEnum.Current.Contains(searchText))
                                        {
                                            // set
                                            inputText = keyEnum.Current;
                                            //Console.CursorLeft = inputText.Length + 1;
                                            skip++;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else if (k.Key == ConsoleKey.Backspace)
                        {
                            inputText = inputText.Length > 1 ? inputText.Remove(inputText.Length - 1) : string.Empty;
                        }
                        else if (k.Key == ConsoleKey.Spacebar)
                        {
                            inputText += " ";
                        }
                        else if (k.Key == ConsoleKey.Escape)
                        {
                            Draw(inputText, 1, Console.WindowHeight - 1, ConsoleColor.Black); // clear
                            inputText = string.Empty;
                        }
                        else
                        {
                            skip = 0;
                            if (k.Key == ConsoleKey.Enter && inputText.Split().Length > 0)
                            {
                                // ENTER
                                string args = null;
                                if (inputText.Contains(' '))
                                {
                                    var spl = inputText.Split(new char[1] { ' ' }, 2);
                                    inputText = spl[0].Trim();
                                    args = spl[1].Trim();
                                }

								if (inputText.Trim().Length > 0)
								{
									OnCommandEnter?.Invoke(inputText, args);

									if (commands.ContainsKey(inputText))
									{
										Log("> " + inputText, ConsoleColor.Gray);
										commands[inputText].Item1.Invoke(args);
									}
									else
									{
										Log($"Unknown command \'{inputText}\'", ConsoleColor.Gray);
									}

									//Draw(inputText, 1, Console.WindowHeight - 1, ConsoleColor.Black); // clear
									ClearLine(Console.WindowHeight - 1);
								}
                                inputText = string.Empty;
                            }
                            else if (inputText.Length < Console.BufferWidth)
                            {
                                //if ((k.Key >= ConsoleKey.D0 && k.Key <= ConsoleKey.Z) ||
                                //    (k.Key >= ConsoleKey.NumPad0 && k.Key <= ConsoleKey.Divide))
                                inputText += char.ToString(k.KeyChar);
                            }
                        }
                        Draw(":", 0, Console.WindowHeight - 1, ConsoleColor.Cyan);
                        Draw(inputText + "  ", 1, Console.WindowHeight - 1, ConsoleColor.White);
                    }

                    #endregion

                    #region DRAW

                    if (writeData.Count != 0)
                    {
                        lock (writeLock)
                        {
                            // clear terminal log border
                            //Console.SetCursorPosition(0, startLogY);
                            //Console.Write(new string(' ', (Console.BufferWidth - 1) * Console.WindowHeight));

                            sleepTime = 1;
                            Flush();
                            // подсветка курсора
                            Console.SetCursorPosition(inputText.Length + 1, Console.WindowHeight - 1);
                            Console.BackgroundColor = ConsoleColor.Gray;
                            Console.Write(' ');
                            Console.BackgroundColor = ConsoleColor.Black;

                            //OnDraw?.Invoke();
                        } // end lock
                    }

                    #endregion

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nTerm pool exception:\n" + ex.ToString());
            }

            Console.Write("\nTerminal pool stoped.");
        }

        /// <summary> Отрисовка текста из буфера. </summary>
        public static void Flush()
        {
            OnDraw?.Invoke();
            while (writeData.Count > 0)
            {
                WriteData wd = writeData.Dequeue();
                if (wd.Text == null)
                    continue;

                // set pos
                if (wd.X.HasValue)
                {
                    Console.CursorLeft = wd.X.Value;
                }
                if (wd.Y.HasValue)
                {
                    Console.CursorTop = wd.Y.Value;
                }

                // set color
                if (wd.ForeColor.HasValue)
                {
                    Console.ForegroundColor = wd.ForeColor.Value;
                }
                if (wd.BackColor.HasValue)
                {
                    Console.BackgroundColor = wd.BackColor.Value;
                }

                // write
                if (wd.Text.Length > Console.BufferWidth)
                {
                    // fix width
                    Console.Write(wd.Text.Substring(0, Console.BufferWidth - 3) + "...");
                }
                else
                {
                    Console.Write(wd.Text);
                }
            }
        }

        /// <summary> Безопасный вывод текста к терминале. </summary>
        /// <param name="text"> Текст. </param>
        /// <param name="x"> Позиция X. </param>
        /// <param name="y"> Позиция Y. </param>
        /// <param name="color"> Цвет. </param>
        /// <param name="backColor"> Цвет фона. </param>
        public static void Draw(string text, int x, int y, ConsoleColor color = ConsoleColor.Gray, ConsoleColor? backColor = null)
        {
            writeData.Enqueue(new WriteData(text, color, backColor, x, y));
        }

        public static void FastDraw(string text)
        {
            if (!Pause) return;
            Console.Write(text);
        }

        public static void FastDraw(string text, int x, int y)
        {
            if (!Pause) return;
            Console.SetCursorPosition(x, y);
            Console.Write(text);
        }

        public static void FastDraw(string text, ConsoleColor fcolor)
        {
            if (!Pause) return;
            Console.ForegroundColor = fcolor;
            Console.Write(text);
        }

        public static void FastDraw(string text, int x, int y, ConsoleColor fcolor)
        {
            if (!Pause) return;
            Console.ForegroundColor = fcolor;
            Console.SetCursorPosition(x, y);
            Console.Write(text);
        }

        public static void FastDraw(string text, ConsoleColor fcolor, ConsoleColor bcolor)
        {
            if (!Pause) return;
            Console.ForegroundColor = fcolor;
            Console.BackgroundColor = bcolor;
            Console.Write(text);
        }

        public static void FastDraw(string text, int x, int y, ConsoleColor fcolor, ConsoleColor bcolor)
        {
            if (!Pause) return;
            Console.ForegroundColor = fcolor;
            Console.BackgroundColor = bcolor;
            Console.SetCursorPosition(x, y);
            Console.Write(text);
        }

        /// <summary> Добавит в лог сообщение. </summary>
        /// <param name="message"> Сообщение. </param>
        public static void Log(string message, ConsoleColor color = ConsoleColor.Gray)
        {
            logList.Insert(0, new Tuple<string, ConsoleColor, string>(message, color, DateTime.Now.ToString("HH:mm:ss")));
            logListCount++;

            if (!Pause)
                ReDrawLog();

            if (logList.Count > LOG_LIST_CAPACITY)
            {
                logList.RemoveAt(LOG_LIST_CAPACITY - 1);
            }
        }

        /// <summary> Вывод последних сообщений лога. </summary>
        public static void ReDrawLog()
        {
            int logLineCount = Console.WindowHeight - 6;
            for (int i = 0; i < logLineCount && i < logList.Count; i++)
            {
                ClearLine(startLogY + i);
                //Draw($"{logListCount - i,4} | {logList[i].Item3} | {logList[i].Item1}", 0, startLogY + i, logList[i].Item2);
                Draw($"{logList[i].Item3} | {logList[i].Item1}", 0, startLogY + i, ConsoleColor.Gray);
                Draw($"{logList[i].Item1}", logList[i].Item3.Length + 3, startLogY + i, logList[i].Item2);
            }
        }

        /// <summary> Очистить линию. </summary>
        /// <param name="y"> Позиция Y. </param>
        public static void ClearLine(int y)
        {
            Draw(new string(' ', Console.BufferWidth), 0, y, ConsoleColor.Gray);
        }

        /// <summary> Ожидать нажатия клавиши, включив мигание курсора. </summary>
        public static ConsoleKeyInfo ReadKey(bool cursorVisible = false)
        {
            if (cursorVisible)
                Console.CursorVisible = true;

            var k = Console.ReadKey(true);

            if (cursorVisible)
                Console.CursorVisible = false;

            return k;
        }

    }
}
