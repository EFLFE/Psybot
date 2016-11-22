using System;
using System.Threading.Tasks;
using PsybotPlugin;

namespace TestPlugin
{
    public sealed class Hello : IPsybotPlugin
    {
        /// <summary> Имя комманды для вызова. </summary>
        public string RunCommandName { get; set; }

        public ParameterTypeEnum ParameterType { get; set; }

        private IPsybotCore core;

        public void Load(IPsybotCore _core)
        {
            core = _core;
            RunCommandName = "-hi";
            ParameterType = ParameterTypeEnum.Unparsed;

            Console.WriteLine("'Hello' load.");
        }

        public void Unload()
        {
            Console.WriteLine("'Hello' unload.");
        }

        public async Task Excecute(PsybotPluginArgs e)
        {
            await Task.Run(() =>
            {
                core.SendMessage(e.ChannelID, "Hello, " + e.UserMention + "!");
            });
        }
    }
}
