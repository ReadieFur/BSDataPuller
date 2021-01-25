# DataPuller
Gathers data about the current map you are playing to then be sent out over a websocket for other software to use, e.g. A web overlay like [BSDP-Overlay](https://github.com/kOFReadie/BSDP-Overlay). This mod works with multi PC setups!

## Installation:
To install this mod, download the [latest version](https://github.com/kOFReadie/BSDataPuller/releases/latest) and place the `DataPuller.dll` into your mods folder. Make sure to also have any of the dependencies listed below installed too.
### Dependencies, these can all be found on the [Mod Assistant](https://github.com/Assistant/ModAssistant) app:
In order for this mod to function properly you must have installed the following mods:
- [BSIPA ^4.1.4](https://github.com/bsmg/BeatSaber-IPA-Reloaded)
- [BeatSaverSharp ^2.0.1](https://github.com/lolPants/BeatSaverSharp)
- WebsocketSharp ^1.0.4
- [SongCore ^3.0.3](https://github.com/Kylemc1413/SongCore)
- [SongDataCore ^1.3.6](https://github.com/halsafar/BeatSaberSongDataCore/)
- [SiraUtil ^2.4.0](https://github.com/Auros/SiraUtil)

## Overlays:
There are few overlays that I know of at the moment that work with this mod but here are some:
| Overlay | Creator |
| --- | --- |
| [BSDP-Overlay](https://github.com/kOFReadie/BSDP-Overlay) | kOF.Readie |
| [Freakylay](https://github.com/UnskilledFreak/Freakylay) | UnskilledFreak |

## Planned changes:
- Other players status in multiplayer
- Campaign mode

## Output data:
This mod outputs quite a bit of data to be used by other mods and overlays. Here is some of the data that the mod exposes:
- Map info:
    - Hash
    - Song name
    - Song sub name
    - Song author
    - Mapper
    - BSR key
    - Cover image
    - Length
    - Time elapsed
- Difficulty info:
    - Map type
    - Difficulty
    - PP
    - Star
    - BPM
    - NJS
    - Modifiers
    - Pratice mode
- Level info:
    - Paused
    - Failed
    - Finished
    - Quit
- Score info:
    - Score
    - Score with modifiers
    - Previous record
    - Full combo
    - Combo
    - Misses
    - Accuracy
    - Block hit score
    - Health

And more!

## Dev docs (WIP):
I've not got much here yet but to make a start I will provide some sample data that is sent out over the websocket, I may have forgotten some of the details here but this should be enough to get going for now.  
The data is sent out as a JSON over an unsecure websocket at `ws://0.0.0.0:2946/BSDataPuller/<DATACLASS>`.  
I am working on getting it to be sent out over a secure websocket but unfortunatly I dont think it would be possible.  
If you want to access this data with another mod, add DataPuller as a refrence and subscribe to the data classes `Update` events, they will pass the data as a JSON however you can read the values straight from the class if you do `<class>.<data>`.
There are currently two data classes, they are:  
**MapData**:  
This is sent out every time a level is started, failed or paused.
```json
{
    "GameVersion": "1.13.2",
    "PluginVersion": "2.0.0.0",
    "InLevel": true,
    "LevelPaused": false,
    "LevelFinished": false,
    "LevelFailed": false,
    "LevelQuit": false,
    "Hash": "648B6FE961C398DE638FA1E614878F1194ADF92E",
    "SongName": "Tera I/O",
    "SongSubName": "[200 Step]",
    "SongAuthor": "Camellia",
    "Mapper": "cerret",
    "BSRKey": "11a27",
    "coverImage": "https://beatsaver.com/cdn/11a27/648b6fe961c398de638fa1e614878f1194adf92e.jpg",
    "Length": 336,
    "TimeScale": 0,
    "MapType": "Standard",
    "Difficulty": "ExpertPlus",
    "CustomDifficultyLabel": "Normal",
    "BPM": 200,
    "NJS": 23,
    "Modifiers":
    {
        "instaFail": false,
        "batteryEnergy": false,
        "disappearingArrows": false,
        "ghostNotes": false,
        "fasterSong": false,
        "noFail": false,
        "noObstacles": false,
        "noBombs": false,
        "slowerSong": false,
        "noArrows": false
    },
    "ModifiersMultiplier": 1,
    "PracticeMode": false,
    "PracticeModeModifiers":
    {
        "songSpeedMul": 1
    },
    "PP": 0,
    "Star": 0,
    "IsMultiplayer": false,
    "PreviousRecord": 2714014,
    "PreviousBSR": null
}
```
**LiveData**:  
This data is sent out every time the score is updated, health is lost or the time progresses.
```json
{
    "Score": 574728,
    "ScoreWithMultipliers": 574728,
    "MaxScore": 612835,
    "MaxScoreWithMultipliers": 612835,
    "Rank": "SS",
    "FullCombo": false,
    "Combo": 352,
    "Misses": 2,
    "Accuracy": 94.20143961906433,
    "BlockHitScore":
    [
        70,
        30,
        14
    ],
    "PlayerHealth": 100,
    "TimeElapsed": 77
}
```