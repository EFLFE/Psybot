using System;
using System.Threading.Tasks;
using PsybotModule;

namespace TestPlugin
{
    /// <summary> Demo module for PsyBot. </summary>
    public sealed class Hello : IPsybotModule
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

        public async Task Excecute(PsybotModuleArgs e)
        {
            await Task.Run(() =>
            {
                core.SendMessage(e.Channel.Id, "Hello, " + e.User.Mention);
            });
        }

        public void OnEnable()
        {
            Console.WriteLine("'Hello' enabled.");
        }

        public void OnDisable()
        {
            Console.WriteLine("'Hello' disabled.");
        }
    }
}
