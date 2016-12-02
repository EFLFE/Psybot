using System;
using System.IO;
using PsybotModule;

namespace Psybot.Modules
{
    internal sealed class ModuleData
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

        public enum ModuleAssemblyTypeEnum
        {
            NotLoaded,
            Source,
            // dll
            Library
        }

        public ModuleAssemblyTypeEnum ModuleAssemblyType = ModuleAssemblyTypeEnum.NotLoaded;

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

        public IPsybotModule Module;

        public ModuleData(string fullPath, IPsybotModule module)
        {
            Module = module;
            FullPath = fullPath;
            FileName = Path.GetFileNameWithoutExtension(fullPath);
            FullFileName = Path.GetFileName(fullPath);

            if (fullPath.EndsWith(".dll"))
            {
                ModuleAssemblyType = ModuleAssemblyTypeEnum.Library;
                //Status = StatusEnum.Disable;
            }
        }

    }
}
