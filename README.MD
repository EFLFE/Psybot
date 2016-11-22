![psybot icon](art/psybot_logo.png)v0.1.2 βeta
* * *
## Psybot
Bot for [Discord](https://discordapp.com), writing on C# with using [Discord.Net **v0.9.6**](https://github.com/RogueException/Discord.Net). It aims to cross-platform.

The main feature is a dynamic functional expandability using libraries.

You can write library using **PsybotPlugin** and get a plug-in, which can be loaded intro Psybot program witchout restarting and reassembly. Or you can find exists plug-in (in future).

Instructions in the Wiki.

## Installation

**For Windows:**
Download the latest binary version of the Psybot, extract, and run **Psybot.exe**.

**For Linux:**
You have to download the source code and compile **.sin** project, using **xbuild**.

**Token:**
You must have to create a **token.txt** file in the program folder and put the Token key.

## Installation plugins

By default the plugins in a **Plugins** directory as **.dll** files.

## Dependency

Psybot writing on C# 6 / .NET 4.5 / Visual Studio 2015

NuGet library:
- Discord.Net
- Antlr
- HtmlAgilityPack
- Newtonsoft.Json
- Nito.AsyncEx
- Nito.Deque
- RestSharp
- WebSocket4Net

## TODO

- [ ] Custom terminal commands
- [ ] PsybotPlugin Version on load dll
- [ ] Minimize to tray for Winodws
- [ ] Get decriptrion for command from channel
- [ ] Plugin for administration management
