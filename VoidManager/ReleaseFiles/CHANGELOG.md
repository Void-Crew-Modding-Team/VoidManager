## 1.1.8
- Scream at us if this is still here.

## 1.1.7
- Mod settings now sort alphabetically
- Mod settings now display mod name
- Potentially mod breaking obsolete method removal
- Added Reset button to color picker Developer API
- API Additions

## 1.1.6
- Developer API Additions

## 1.1.4
- Fixed networking issue caused by removed game fields

## 1.1.3
- Added room modlist display to matchmaking terminal
- Fixed issues with Matchmaking terminal
- Additions to GUITools API
- Fixed issues with commands runtimes
- Changes/fixes to Notifications API

## 1.1.2
- GUI API additions
- Now hiding chainloader ManagerObject (gets deleted automatically, useful for some BepInEx mods)
- Added !ListMods public command for vanilla clients to check host mod list.
- Now unlocks mouse while using text chat.
- Added sent chat input history (Yay, one tick off the future plans.)
- Added tab auto-complete and auto-complete API for developers.

## 1.1.1
- Fixed bug with Crafting API

## 1.1.0
- Fixed bug with unspecified mods not detected as installed by host
- Changed default value for trust unspecified mods to true
- Fixed bug where only MPType.all mods were displayed when Unspecified mods should have also been displayed in join fail message
- Fixed kick message from host not displaying
- GUI highight color improved
- GUI now highlights selected mod in mod list and various other locations.
- Documentation XML now included with dll for developers
- Crafting API updated for Endless, various changes
- various minor changes.

## 1.0.9
- Fixed Modded Room tag not applying on room first open
- Adjusted Terminal join fail popup showing old message
- Incompatable mods/missing mods checks now show mods in text chat (Might get extensive in some cases.)

## 1.0.8
- Fixed for Game version 0.26.0 (Update 4)
- '[Modded]' now added to modded session names, with '[Mods Required]' added to applicable sessions
- changes to Recipe and Crafting APIs
- Added 'Host' MPType. Same as client, just has a different name
- Mods now sorted alphabetically in F5 menu (Linux wasn't previously sorted by name)
- Added voidmanager config shortcut to Voidmanager about page

## 1.0.7
- F5 menu now displays mod lists for other players
- Added API for modifying crafting recipes
- Added API for modifying unlock requirements

## 1.0.6
- F5 menu now displays Non-VoidManager mods
- F5 menu now displays overall MPType level
- F5 menu changes mod name color in mod list based on MPType
- F5 menu now provides details on MPTypes
- Mods can now have their MPType manually configured via VoidManager's config file.

## 1.0.5
- Added more detail to client self-disconnection message

## 1.0.4
- Developer API Changes

## 1.0.3
- Fixed clients not recieving popup when failing to join room due to client mod checks
- Fixed a developer API not being accessible

## 1.0.2
- Fixed bug related to GUI loading

## 1.0.1
- Updated GUI URLs

## 1.0.0
- Initial release