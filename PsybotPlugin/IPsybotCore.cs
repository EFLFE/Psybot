using System;

namespace PsybotPlugin
{
    /// <summary> Базовый интерфейс, который является загрузчиком плагина. </summary>
    public interface IPsybotCore
    {
        /// <summary> Подключён ли клиент к серверу. </summary>
        bool Connected { get; }

        /// <summary> Отправить сообщение на сервер. </summary>
        /// <param name="channelID"> Индификатор канала. </param>
        /// <param name="message"> Сообщение. </param>
        void SendMessage(ulong channelID, string message);
    }
}
