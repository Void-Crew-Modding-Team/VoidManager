[![](https://img.shields.io/badge/-Void_Crew_Modding_Team-111111?style=just-the-label&logo=github&labelColor=24292f)](https://github.com/Void-Crew-Modding-Team)
[![](https://img.shields.io/github/v/release/Void-Crew-Modding-Team/VoidManager?include_prereleases&style=flat&label=Release%20Version&labelColor=24292f&color=111111)](https://github.com/Void-Crew-Modding-Team/VoidManager/releases/)
![](https://img.shields.io/badge/Game%20Version-0.25.2-111111?style=flat&labelColor=24292f&color=111111)
[![](https://img.shields.io/github/license/Void-Crew-Modding-Team/VoidManager?style=flat&label=License&labelColor=24292f&color=111111)](https://github.com/Void-Crew-Modding-Team/VoidManager/blob/master/LICENSE)
[![](https://img.shields.io/discord/1180651062550593536.svg?&logo=discord&logoColor=ffffff&style=flat&label=Discord&labelColor=24292f&color=111111)](https://discord.gg/g2u5wpbMGu "Void Crew Modding Discord")

# Void Manager

`BepInEx` Plugin Manager for `Void Crew`

Developed by Mest and Dragon, based on [Pulsar Mod Loader](https://github.com/PULSAR-Modders/pulsar-mod-loader)

Notes on Multiplayer mod checks: By default, any given mod must be installed by the host for clients to join. The host can configure this setting with VoidManager vie the F5 menu (ModManager > Mod Settings > VoidManager > Trust MPType.Unspecified mods). Additionally, Mods configured for VoidManager can change this setting.

---------------------

### 💡 : Function : **Several features to assist in mod handling and management.**
- Handling for mods requiring installation by all users
- Restrictions for unspecified mods
- Client side chat commands
- Public chat commands
- Mod to mod messages (including between different clients)
- Marked Void Manager rooms
- Mod settings UI

### 🎮 : Client Usage :

- `F5` ingame will bring up a menu which lists all installed `Void Manager` plugins and their mod settings.
- `/` is the prefix for client commands. `/help` lists all commands available.
- `!` is the prefix for public commands. `!help` lists all public commands available.

### 👥 : Multiplayer Functionality : 

**Complex**  -  VoidManager handles mods connectiveity to prevent mods not configured for VoidManager from joining vanilla games. If all mods are configured for VoidManager as Client mods, clients will be allowed to join vanilla games. This behaviour is to prevent mods which break vanilla clients from doing so.

- ✅ Client
- ⬜ Host
- ⬜ All

---------------------

## 🔧 : Install Instructions : **Install following the normal BepInEx procedure.**

Ensure that you have [BepInEx 5](https://thunderstore.io/c/void-crew/p/BepInEx/BepInExPack/) installed, stable version 5 **MONO** build to be precise.

#### ✔️ : Mod install : **Unzip the contents into the BepInEx plugin directory**

Drag and drop `VoidManager.dll` into `Void Crew\BepInEx\plugins`

---------------------

### 🤔 : Development Guide : **Documentation to create Void Manager mods is** [on the wiki](https://github.com/Void-Crew-Modding-Team/VoidManager/wiki)

---------------------

### Future Plans:

- Sent Message History
- mod whitelist/blacklisting
- Clients ability to view a room's mod list before joining.
