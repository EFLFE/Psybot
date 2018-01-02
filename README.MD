![psybot icon](art/psybot_logo.png)
 
## Warning!

Psybot version 0.2 now is archived.

Important about the modules:
You cannot unload an assembly after loading (`Assembly.Load`).
The solution to load assemblies in a separate Domain also does not work,
Because .NET CORE does not support `AppDomain.CreateDomain`.

Out of the situation: You can only dynamically load assemblies.
To unload/Restart the module-you need to restart the program itself.
Alas, otherwise neither.

This issue will be resolved in the third version of the project.

## Release info

Last release version: **0.2.0 βeta** _(24.11.16)_

Last PsybotPlugin version: **0.2.0** _(24.11.16)_

* * *
## Psybot
Bot for [Discord](https://discordapp.com), writing on C# with using [Discord.Net **v0.9.6**](https://github.com/RogueException/Discord.Net). It aims to cross-platform.

The main feature is a dynamic functional expandability using libraries.

You can write library using **PsybotPlugin** and get a module, which can be loaded intro Psybot program witchout restarting and reassembly. Or you can find exists module.

Instructions in the Wiki.

# Warning

This is experimental version. I'm plan to completely rewrite the code for [Discord.Net](https://discordapp.com) **v1.0**.

## Installation

**For Windows:**
Download the latest binary version of the Psybot, extract, and run **Psybot.exe**.

**For Linux:**
~~You have to download the source code and compile **.sin** project, using **xbuild**.~~

Coming soon.

**Token:**
You must have to create a **token.txt** file in the program folder and put the Token key for bot.

## Installation modules

By default the modules in a **Modules** directory as **.dll** files.

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
- [ ] Check PsybotModule Version on load dll
- [ ] Minimize to tray for Winodws
- [ ] Get desriptrion for command from channel
- [x] Module for administration management
