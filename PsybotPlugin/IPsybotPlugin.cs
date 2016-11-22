using System;
using System.Threading.Tasks;

namespace PsybotPlugin
{
    /// <summary> Базовый интерфейс для взаимодействия с плагином. </summary>
    public interface IPsybotPlugin
    {
        /// <summary> Имя комманды для вызова. </summary>
        string RunCommandName { get; set; }

        StringComparison CommandComparison { get; set; }

        ParameterTypeEnum ParameterType { get; set; }

        void Load(IPsybotCore _core);

        void Unload();

        Task Excecute(PsybotPluginArgs e);
    }
}
