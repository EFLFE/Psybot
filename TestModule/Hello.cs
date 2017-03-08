using System;
using System.Threading.Tasks;
using PsybotModule;

namespace TestModule
{
	/// <summary> Demo module for PsyBot. </summary>
	public sealed class Hello : IPsybotModule
	{
		public string[] RunCommandsName { get; set; }

		public ParameterTypeEnum ParameterType { get; set; }

		public StringComparison CommandComparison { get; set; }

		public bool CheckAllMessage { get; set; }

		private IPsybotCore core;

		public void Load(IPsybotCore _core)
		{
			core = _core;
			RunCommandsName = new[] { "-hi" };
			ParameterType = ParameterTypeEnum.Unparsed;
			CommandComparison = StringComparison.OrdinalIgnoreCase;
		}

		public void Unload()
		{
		}

		public Task Excecute(Message e)
		{
			return Task.Run(() =>
		    {
			    core.SendMessage(e.ChannelId, "Hello, " + e.UserMention);
		    });
		}

		public void OnEnable()
		{
		}

		public void OnDisable()
		{
		}
	}
}
