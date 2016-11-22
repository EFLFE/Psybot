using System;
using System.Collections.Generic;
using System.IO;

namespace Psybot.Data
{
    internal sealed class IniReader
    {
        private readonly string iniFile;

        /// <summary> ini data: [section][key][value] </summary>
        public readonly Dictionary<string, Dictionary<string, List<string>>> IniData;

        public IniReader(string iniFile)
        {
            this.iniFile = iniFile;
            IniData = new Dictionary<string, Dictionary<string, List<string>>>();
        }

        public void LoadFile()
        {
            IniData.Clear();
            string[] txt = File.ReadAllLines(iniFile);
            string currentSections = null;

            for (int i = 0; i < txt.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(txt[i]) || txt[i].StartsWith(";"))
                    continue;

                if (txt[i].StartsWith("[") && txt[i].EndsWith("]"))
                {
                    // [section]

                    currentSections = txt[i].Replace("[", string.Empty).Replace("]", string.Empty);
                    IniData.Add(currentSections, new Dictionary<string, List<string>>());
                }
                else if (txt[i].Contains("="))
                {
                    // key = value

                    if (currentSections == null)
                        throw new NullReferenceException("Missing section.");

                    string[] spl = txt[i].Split(new[]{'='}, 2);
                    spl[0] = spl[0].Trim();
                    spl[1] = spl[1].Trim();

                    if (IniData[currentSections] == null)
                    {
                        IniData[currentSections] = new Dictionary<string, List<string>>();
                        IniData[currentSections].Add(spl[0], new List<string>());
                    }
                    else if (!IniData[currentSections].ContainsKey(spl[0]))
                    {
                        IniData[currentSections].Add(spl[0], new List<string>());
                    }

                    IniData[currentSections][spl[0]].Add(spl[1]);
                }
            }

        }

    }
}
