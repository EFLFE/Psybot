using System;
using System.Threading.Tasks;
using PsybotPlugin;

namespace TestPlugin
{
    public sealed class Hello : IPsybotPlugin
    {
        public string RunCommandName { get; set; }

        public ParameterTypeEnum ParameterType { get; set; }

        public StringComparison CommandComparison { get; set; }

        private IPsybotCore core;

        public void Load(IPsybotCore _core)
        {
            core = _core;
            RunCommandName = "-hi";
            ParameterType = ParameterTypeEnum.Unparsed;
            CommandComparison = StringComparison.OrdinalIgnoreCase;

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
                core.SendMessage(e.Channel.Id, "Hello, " + e.User.Mention);
            });
        }
    }
}
