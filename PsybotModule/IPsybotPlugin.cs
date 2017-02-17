using System;
using System.Threading.Tasks;

namespace PsybotModule
{
    /// <summary>
    ///     Base interface for interact with module.
    /// </summary>
    public interface IPsybotModule
    {
        /// <summary>
        ///     Command name for run Excecute method.
        /// </summary>
        string RunCommandName { get; set; }

        /// <summary>
        ///     Command name comparison.
        /// </summary>
        StringComparison CommandComparison { get; set; }

        /// <summary>
        ///     Parse type for command (todo).
        /// </summary>
        ParameterTypeEnum ParameterType { get; set; }

        /// <summary>
        ///     On enable modele.
        /// </summary>
        void OnEnable();

        /// <summary>
        ///     On disable module.
        /// </summary>
        void OnDisable();

        /// <summary>
        ///     On load module.
        /// </summary>
        /// <param name="_core"> Owner API. </param>
        void Load(IPsybotCore _core);

        /// <summary>
        ///     On unload module.
        /// </summary>
        void Unload();

        /// <summary>
        ///     Excecute when enter command name from chat.
        /// </summary>
        /// <param name="e"> Args. </param>
        Task Excecute(Message e);
    }
}
