# DataPuller
Sends out game data such as PP, BSR, health e.t.c. over a websocket to be displayed on an overlay such as [BSDP-Overlay](https://github.com/kOFReadie/BSDP-Overlay) or to be used by further mods.

## Installation:
To install this mod all you have to do is download the [latest version](https://github.com/kOFReadie/BSDataPuller/releases/latest) and place BOTH of the folders inside the archive into your BeatSaber ROOT directory, replace any files it asks you to.
### Dependencies, these can all be found on the [Mod Assistant](https://github.com/Assistant/ModAssistant) app:
In order for this mod to function properly you must have installed the following mods:
- [BSIPA ^4.1.3](https://github.com/bsmg/BeatSaber-IPA-Reloaded)
- [BSUtils ^1.6.5](https://github.com/Kylemc1413/Beat-Saber-Utils)
- [BeatSaverSharp ^1.6.0](https://github.com/lolPants/BeatSaverSharp)
- WebsocketSharp (Included in the mod download)
- [SongCore ^3.0.1](https://github.com/Kylemc1413/SongCore)
- [SongDataCore ^1.3.5](https://github.com/halsafar/BeatSaberSongDataCore/)

## Overlay:
There are few overlays that I know of at the moment but here are some:
| Overlay | Creator |
| --- | --- |
| [BSDP](https://github.com/kOFReadie/BSDP-Overlay) | kOF.Readie |
| [Freakylay](https://github.com/UnskilledFreak/Freakylay) | UnskilledFreak |

## Upcoming changes:
I plan to slightly improve the peformance of this again as well as adding new features like:
- Other players status in multiplayer
- Support for replay mode

## Output data:
- Map:
    - Hash
    - Song name
    - Song sub name
    - Song author
    - Mapper
    - BSR key
    - Cover image
    - Length
    - Time elapsed
- Difficulty:
    - Map type
    - Difficulty
    - PP
    - Star
    - BPM
    - NJS
    - Modifiers
    - Pratice mode
- Level status:
    - Paused
    - Failed
    - Finished
    - Quit
- Score status
    - Previous record
    - Full combo
    - Combo
    - Misses
    - Accuracy
    - Block hits
    - Health

## Dev docs:
Coming soon!
