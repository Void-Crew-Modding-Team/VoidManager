[![](https://img.shields.io/badge/-Void_Crew_Modding_Team-111111?style=just-the-label&logo=github&labelColor=24292f)](https://github.com/Void-Crew-Modding-Team)
[![](https://img.shields.io/github/v/release/Void-Crew-Modding-Team/VoidManager?include_prereleases&style=flat&label=Release%20Version&labelColor=24292f&color=111111)](https://github.com/Void-Crew-Modding-Team/VoidManager/releases/)
![](https://img.shields.io/badge/Game%20Version-0.27.0-111111?style=flat&labelColor=24292f&color=111111)
[![](https://img.shields.io/github/license/Void-Crew-Modding-Team/VoidManager?style=flat&label=License&labelColor=24292f&color=111111)](https://github.com/Void-Crew-Modding-Team/VoidManager/blob/master/LICENSE)
[![](https://img.shields.io/discord/1180651062550593536.svg?&logo=discord&logoColor=ffffff&style=flat&label=Discord&labelColor=24292f&color=111111)](https://discord.gg/g2u5wpbMGu "Void Crew Modding Discord")

# VoidManager

`BepInEx` Plugin Manager for `Void Crew`

Version 1.1.8  
For Game Version 0.27.0  
Developed by Mest, Dragon, and 18107  
Based on [Pulsar Mod Loader](https://github.com/PULSAR-Modders/pulsar-mod-loader)

---------------------

### 💡 Function - **Several features to assist in mod handling and management.**
- Handling for mods requiring installation by all users
- Restrictions for unspecified mods
- Manual configuration of unspecified mods
- Listing of other players' mod lists
- '[Modded]' added to modded session names, with '[Mods Required]' added to applicable sessions. VoidManager clients view as yellow and red '[M]' respectably.
- Room mods list in Matchmaking terminal
- Mod settings UI
- Hides Chainloader object for developers
- Unlocks mouse while using text chat
- Chat input history
- Command Auto-complete via tab

## ⌨ API
- Mod MPType Specification
- local and public chat commands
- Networked Mod to mod messages
- Detection of mods installed on other clients
- Networking events
- Recipe and unlock modifications
- Mod settings UI
- Notification API
- Harmony Transpiler Patching API
- Various Utilities

### 🎮 Client Usage

- `F5` ingame will bring up a menu which lists all installed `Void Manager` plugins and their mod settings.
- `/` is the prefix for client commands. `/help` lists all commands available.
- `!` is the prefix for public commands. `!help` lists all public commands available.

### 👥 Multiplayer Functionality 

**Complex** - VoidManager handles mods connectiveity to prevent mods not configured for VoidManager from joining vanilla games. If all mods are configured for VoidManager as Client mods, clients will be allowed to join vanilla games. This behaviour is to prevent mods which break vanilla clients from doing so.  
By default, any given mod must be installed by the host for clients to join. The host can configure this setting with VoidManager via the F5 menu (ModManager > Mod Settings > VoidManager > Trust MPType.Unspecified mods). Additionally, Mods configured for VoidManager can change this setting.

---------------------

## 🔧 Install Instructions - **Install following the normal BepInEx procedure.**

Ensure that you have [BepInEx 5](https://thunderstore.io/c/void-crew/p/BepInEx/BepInExPack/) (stable version 5 **MONO**) and [VoidManager](https://thunderstore.io/c/void-crew/p/VoidCrewModdingTeam/VoidManager/) installed.

#### ✔️ Mod installation - **Unzip the contents into the BepInEx plugin directory**

Drag and drop `VoidManager.dll` into `Void Crew\BepInEx\plugins`

---------------------

### 🤔 Development Guide - **Documentation to create Void Manager mods is** [on the wiki](https://github.com/Void-Crew-Modding-Team/VoidManager/wiki)

---------------------

### Future Plans:

- mod whitelist/blacklisting
- Downgrading to [client] session tag based on mod types, and the ability to join vanilla sessions with Non-Game Influencing mods.
