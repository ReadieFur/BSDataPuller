# DataPuller
Gathers data about the current map you are playing to then be sent out over a websocket for other software to use, e.g. A web overlay like [BSDP-Overlay](../BSDP-Overlay). This mod works with multi PC setups!

## Installation:
To install this mod, download the [latest version](./releases/latest) and place the `DataPuller.dll` into your mods folder. Make sure to also have any of the dependencies listed below installed too.
### Dependencies, these can all be found on the [Mod Assistant](https://github.com/Assistant/ModAssistant) app:
In order for this mod to function properly you must have installed the following mods:
- [BSIPA ^4.2.2](https://github.com/bsmg/BeatSaber-IPA-Reloaded)
- [BeatSaverSharp ^3.4.4](https://github.com/Auros/BeatSaverSharper)
- [WebsocketSharp ^1.0.4](assets/websocket-sharp-1.0.4.zip)
- [SongCore ^3.10.2](https://github.com/Kylemc1413/SongCore)
- [SongDetailsCache ^1.2.1](https://github.com/kinsi55/BeatSaber_SongDetails)
- [SiraUtil ^3.1.2](https://github.com/Auros/SiraUtil)

## Overlays:
There are few overlays that I know of at the moment that work with this mod but here are some:
| Overlay | Creator |
| --- | --- |
| [BSDP-Overlay](../BSDP-Overlay) | ReadieFur |
| [Freakylay](https://github.com/UnskilledFreak/Freakylay) | UnskilledFreak |
| [HyldraZolxy](https://github.com/HyldraZolxy/BeatSaber-Overlay) | HyldraZolxy |

## Project status:
This project is still maintianed though loosely, I will keep updating it to make sure it remains compatiable with the game but don't expect new features to constantly be added.  
For a detailed view on the project status, check the [TODO](./.todo) file.


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

## Developer documentation:
### Obtaining the data via the Websocket:
Data is broadcasted over an unsecure websocket (plain `ws`) that runs on port `2946`, the path to the data is `/BSDataPuller/<TYPE>`.  
The reason for the use of an unsecure websocket is because it is pratically impossible to get a verified and signed SSL certificate for redistribution, that would break the whole point of SSL.  
Each endpoint will send out a JSON object, check [Data Types](#data-types) for the specific data that each endpoint sends out.

### Obtaining the data via the C#:
It is possible to use the data that this mod exposes within your own mod if you wish to do so.  
To get started, add the [latest version](./releases/latest) of the mod to your project as a reference.  
Data types can be accessed within the `DataPuller.Data` namespace.  
All data types extend the `AData` class which contains an `OnUpdate` event that can be subscribed to that is fired whenever the data is updated.  
Check [Data Types](#data-types) for the specific data that each endpoint sends out.

### Data types:
I will format each entry in the following way:
```
<TYPE>
	<DESCRIPTION>
	<LOCATION>
	<OBJECT>
		///<COMMENT>
		<TYPE> <NAME> = <DEFAULT_VALUE>;
```
All data types contain the following properties:
```cs
///The time that the data was serialized.
long UnixTimestamp;
```
Below is a list of all the specific data types:
<details>
<summary style="font-weight: 600">MapData</summary>
Description: Contains data about the current map and mod.  
Type: `class`  

| Method | Location |
| --- | --- |
| Websocket | `/BSDataPuller/MapData` |
| C# | `DataPuller.Data.MapData` |

This data gets updated whenever:
- The map is changed
- A level is quit/paused/failed/finished

```cs
//====LEVEL====
///This can remain false even if LevelFailed is true, when Modifiers.NoFailOn0Energy is true.
bool LevelPaused = false;

bool LevelFinished = false;

bool LevelFailed = false;

bool LevelQuit = false;

//====MAP====
///The hash ID for the current map.
///null if the hash could not be determined (e.g. if the map is not a custom level).
string? Hash = null;

///The name of the current map.
string SongName = "";

///The sub-name of the current map.
string SongSubName = "";

///The author of the song.
string SongAuthor = "";

///The mapper of the current chart.
string Mapper = "";

///The BSR key of the current map.
///null if the BSR key could not be obtained.
string? BSRKey = null;

///The cover image of the current map.
///null if the cover image could not be obtained.
string? CoverImage = null;

///The duration of the map in seconds.
int Duration = 0;

//====DIFFICULTY====
///The type of map.
///i.e. Standard, 360, OneSaber, etc.
string MapType = "";

///The standard difficulty label of the map.
///i.e. Easy, Normal, Hard, etc.
string Difficulty = "";

///The custom difficulty label set by the mapper.
///null if there is none.
string? CustomDifficultyLabel = null;

///The beats per minute of the current map.
int BPM = 0;

///The note jump speed of the current map.
double NJS = 0;

///The modifiers selected by the player for the current level.
///i.e. No fail, No arrows, Ghost notes, etc.
Modifiers Modifiers = new Modifiers();

///The score multiplier set by the users selection of modifiers.
float ModifiersMultiplier = 1.0f;

bool PracticeMode = false;

///The modifiers selected by the user that are specific to practice mode.
PracticeModeModifiers PracticeModeModifiers = new PracticeModeModifiers();

///The amount Play Points this map is worth.
///0 if the map is unranked or the value was undetermined.
double PP = 0;

///0 if the value was undetermined.
double Star = 0;

//====MISC====
string GameVersion = ""; //Will be the current game version, e.g. 1.20.0

string PluginVersion = ""; //Will be the current version of the plugin, e.g. 2.1.0

bool IsMultiplayer = false;

///The previous local record set by the player for this map specific mode and difficulty.
///0 if the map variant hasn't never been played before.
int PreviousRecord = 0;

///The BSR key fore the last played map.
///null if there was no previous map or the previous maps BSR key was undetermined.
///This value won't be updated if the current map is the same as the last.
string? PreviousBSR = null;
```

##### Modifiers
This is a sub-object of `MapData` and does not get extend the `AData` class, there is no endpoint for this type.  
Type: `class`
```cs
bool NoFailOn0Energy = false;
bool OneLife = false;
bool FourLives = false;
bool NoBombs = false;
bool NoWalls = false;
bool NoArrows = false;
bool GhostNotes = false;
bool DisappearingArrows = false;
bool SmallNotes = false;
bool ProMode = false;
bool StrictAngles = false;
bool ZenMode = false;
bool SlowerSong = false;
bool FasterSong = false;
bool SuperFastSong = false;
```

##### PracticeModeModifiers
This is a sub-object of `MapData` and does not get extend the `AData` class, there is no endpoint for this type.  
Type: `class`
```cs
float SongSpeedMul;
bool StartInAdvanceAndClearNotes;
float SongStartTime;
```

</details>

<details>
<summary style="font-weight: 600">LiveData</summary>
Description: Contains data about the player status within the current map.  
Type: `class`

| Method | Location |
| --- | --- |
| Websocket | `/BSDataPuller/LiveData` |
| C# | `DataPuller.Data.LiveData` |

This data gets updated whenever:
- The players health changes
- A block is hit or missed
- The score changes
- 1 game second passes (this varies depending on the speed multiplier)

```cs
//====SCORE====
///The current raw score.
int Score = 0;

///The current score with the player selected multipliers applied.
int ScoreWithMultipliers = 0;

///The maximum possible raw score for the current number of cut notes.
int MaxScore = 0;

///The maximum possible score with the player selected multipliers applied for the current number of cut notes.
int MaxScoreWithMultipliers = 0;

///The string rank label for the current score.
///i.e. SS, S, A, B, etc.
string Rank = "SSS";

bool FullCombo = true;

///The total number of notes spawned since the start position of the song until the current position in the song.
int NotesSpawned = 0;

///The current note cut combo count without error.
///Resets back to 0 when the player: misses a note, hits a note incorrectly, takes damage or hits a bomb.
int Combo = 0;

///The total number of missed and incorrectly hit notes since the start position of the song until the current position in the song.
int Misses = 0;

double Accuracy = 100;

///The individual scores for the last hit note.
SBlockHitScore BlockHitScore = new SBlockHitScore();

double PlayerHealth = 50;

///The colour of note that was last hit.
///ColorType.None if no note was previously hit or a bomb was hit.
ColorType ColorType = ColorType.None;

//====MISC====
///The total amount of time in seconds since the start of the map.
int TimeElapsed = 0;

///The event that caused the update trigger to be fired.
ELiveDataEventTriggers EventTrigger = ELiveDataEventTriggers.Unknown;
```
##### SBlockHitScore
This is a sub-object of `LiveData` and does not get extend the `AData` class, there is no endpoint for this type.
Type: `struct`
```cs
///0 to 70.
int PreSwing = 0;
///0 to 30.
int PostSwing = 0;
///0 to 15.
int CenterSwing = 0;
```

##### ColorType
This is a sub-object of `LiveData` and does not get extend the `AData` class, there is no endpoint for this type.
Type: `enum`
```cs
ColorA = 0,
ColorB = 1,
None = -1
```

##### ELiveDataEventTriggers
This is a sub-object of `LiveData` and does not get extend the `AData` class, there is no endpoint for this type.  
Type: `enum`
```cs
Unknown = 0,
TimerElapsed,
NoteMissed,
EnergyChange,
ScoreChange
```

</details>
