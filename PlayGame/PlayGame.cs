using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using PsybotModule;

namespace PlayGame
{
	public sealed class PlayGame : IPsybotModule
	{
		private static HtmlWeb htmlWeb;
		private static Random rnd;

		public string[] RunCommandsName { get; set; }

		public ParameterTypeEnum ParameterType { get; set; }

		public StringComparison CommandComparison { get; set; }

		public bool CheckAllMessage { get; set; }

		private static IPsybotCore core;
		private static bool enabled;
		private static int delayTime;

		public void Load(IPsybotCore _core)
		{
			core = _core;
			ParameterType = ParameterTypeEnum.Unparsed;
			CommandComparison = StringComparison.OrdinalIgnoreCase;
			rnd = new Random();
			ThreadPool.QueueUserWorkItem(new WaitCallback(pool), null);
		}

		private static void pool(object _)
		{
			while (true)
			{
				Thread.Sleep(999);
				if (enabled)
				{
					if (delayTime == 0)
						setGame();
					else
						delayTime--;
				}
			}
		}

		public void Unload()
		{
			enabled = false;
		}

		public Task Excecute(Message e)
		{
			return Task.Run(() =>
			{
			});
		}

		public void OnEnable()
		{
			enabled = true;
		}

		public void OnDisable()
		{
			enabled = false;
			core.UpdateStatus(null);
		}

		private static void setGame()
		{
			// play: set new random game
			if (htmlWeb == null)
			{
				htmlWeb = new HtmlWeb() { AutoDetectEncoding = false, OverrideEncoding = Encoding.UTF8 };
			}

			core.SendLog("Search game", ConsoleColor.White);

			HtmlDocument htmlDocument = htmlWeb.Load("http://store.steampowered.com/games/");
			HtmlNode node1 = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='NewReleasesRows']");

			if (node1 == null)
			{
				core.SendLog("Games not found.", ConsoleColor.Red);
				return;
			}

			var gameList = new List<string[]>();

			// search
			var cnodesUrl  = node1.Descendants("a");
			var cnodesName = node1.Descendants("div")
							.Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "tab_item_name");

			int cnodesUrlCount = cnodesUrl.Count();
			int cnodesNameCount = cnodesName.Count();

			// check
			if (cnodesUrlCount == 0)
			{
				core.SendLog("Games url not found.", ConsoleColor.Red);
				return;
			}
			if (cnodesNameCount == 0)
			{
				core.SendLog("Games name not found.", ConsoleColor.Red);
				return;
			}
			if (cnodesUrlCount != cnodesNameCount)
			{
				core.SendLog("Nodes disparity.", ConsoleColor.Red);
				return;
			}

			// load
			foreach (var item in cnodesName)
			{
				gameList.Add(new string[2] { item.InnerText, null });
			}
			int i = 0;
			foreach (var item in cnodesUrl)
			{
				gameList[i++][1] = item.Attributes["href"].Value;
			}

			// apply
			if (gameList.Count == 0)
			{
				core.SendLog("Games not found.", ConsoleColor.Red);
			}
			else
			{
				int rndNum = rnd.Next(gameList.Count);

				core.SendLog("Set game: " + gameList[rndNum][0], ConsoleColor.White);
				core.UpdateStatus(gameList[rndNum][0], -1, gameList[rndNum][1]);
				delayTime = 60 * 60;
			}
		}

	}
}
