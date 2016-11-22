using System;
using System.IO;
using PsybotPlugin;

namespace Psybot.Plugins
{
    internal sealed class PluginData
    {
        /// <summary> Состояние. </summary>
        public enum StatusEnum
        {
            /// <summary> Отключён. Готов для включения. </summary>
            Disable,

            /// <summary> Включён. </summary>
            Enable,

            /// <summary> Ошибка при работе. </summary>
            Crash,

            /// <summary> Выгружен. </summary>
            Unloaded
        }

        public enum PluginAssemblyTypeEnum
        {
            NotLoaded,
            Source,
            // dll
            Library
        }

        public PluginAssemblyTypeEnum PluginAssemblyType = PluginAssemblyTypeEnum.NotLoaded;

        public StatusEnum Status = StatusEnum.Disable;

        // имя файла без рашрирения
        public string FileName { get; private set; }

        // полное имя файла
        public string FullFileName;

        // полный путь к файлу
        public string FullPath;

        //public string Namespace;
        //public string MainClass;

        /// <summary> Если плагин при работе выдал ошибку, то она будет записана в этом поле. </summary>
        public Exception CrashException;

        public IPsybotPlugin Plugin;

        public PluginData(string fullPath, IPsybotPlugin plugin)
        {
            Plugin = plugin;
            FullPath = fullPath;
            FileName = Path.GetFileNameWithoutExtension(fullPath);
            FullFileName = Path.GetFileName(fullPath);

            if (fullPath.EndsWith(".dll"))
            {
                PluginAssemblyType = PluginAssemblyTypeEnum.Library;
                //Status = StatusEnum.Disable;
            }
        }

    }
}
