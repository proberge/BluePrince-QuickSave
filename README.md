# QuickSave Mod for Blue Prince

QuickSave is an attempt to add a "Quick Save" functionality to **Blue Prince**, built on the **MelonLoader** framework. It introduces a experimental half-working in-day save system so that you can quit the game without "Calling it a day" and resume where you left off.

## Key Features

*   **Menu Integration**: The Main Menu "Continue" button automatically detects if a valid quicksave exists for your active profile and day. The in-game menu's "Save and Quit" button is replaced with a "QuickSave and Quit" button.
*   **Manor Layout Persistence**: Restores the exact arrangement of rooms across the 5x9 grid, including their rotations and map markers.
    * Known issues: the Garage is weird and not properly restored.
    * Rooms upgraded on the current day probably won't be upgraded when they are restored.
    * Not fully tested with all rooms, although rooms upgraded on previous days seem to work.
    * Outer room not implemented yet.
    * Doors of rotated rooms seem to be bugged; a second door unexpectedly appears behind. I haven't been able to identify what's missing yet. 
*   **Player & Inventory State**: Captures player position and full inventory (Steps, Keys, Gems, Gold, Dice, and Stars).
*   **Item Persistence**: Gives the player the items they had when they saved. Items still in the manor (ex. Keys and Gems) are restored to their original positions.
*   **Puzzle State Tracking**: Experimental support for persisting specific room-level puzzle progress (e.g., opened boxes in the Parlor).
    * Known issue: literally only the Parlor is supported for now, and it won't track progress towards the Parlor trophy. 

## Technical Implementation

Mostly vibe-coded: I gave instructions to Antigravity (various models, but mostly Gemini) based on what I observed in [UnityExplorer](https://github.com/sinai-dev/UnityExplorer) and then tweaked things until they mostly worked.

The code is a bit messy but should be self-explanatory; we poke at things in the game's internals (GameObjects and PlayMakerFSM components) to read and write the data we need.

The likely most interesting logic is in `FsmHookManager` - it allows us to hook into the game's FSMs and execute code when certain events are triggered, for example when the player enters a room or interacts with an object. See `Core.cs`' `OnSceneWasInitialized` method for an example: we initiate a Coroutine (to perform the quick-load) once the `DAY`'s FSM reaches a state where the player is about to get control of their character.

Other mod developers may also be interested in the `FsmLoggingManager`, which can be used to log (to the console, in real time) all state transitions and actions of a specific FSM to see how it interacts with other objects. This was used in conjunction with [PlayMakerDocumenter](https://github.com/markekraus/PlayMakerDocumenter) to learn about the game's FSMs. See the example scripts in the `DebugUtils` folder for usage examples.

## Installation

1.  Ensure you have [MelonLoader](https://melonwiki.xyz/) installed for Blue Prince.
2.  Install the install the Protocol buffer compiler. You can find it [here](https://protobuf.dev/installation/).
3.  Build the project to get `QuickSave.dll` into your game's `Mods` folder. You may need to tweak the PostBuild event in the .csproj file to point to your game's directory.

The mod was tested on version 1.6.1 and will likely break with future updates.