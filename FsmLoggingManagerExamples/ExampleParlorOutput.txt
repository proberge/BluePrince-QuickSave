# Example Output from EnableFullEventLogging.cs on a few GameObject's FSMs in the Parlor Room.

[13:33:31.504] [QuickSave] [FsmLogging] 'WIND-UP KEY' State 'State 4' Action 'SetFsmInt': [Global Manager] 'NewCursor' -> 3
[13:33:31.506] [QuickSave] [FsmLogging] 'WIND-UP KEY' State 'State 4' Action 'MousePickEvent': Target:
[13:33:31.585] [QuickSave] [FsmLogging] 'WIND-UP KEY' State 'Click 3' Action 'PmtDeSpawn': Despawning GameObject [WIND-UP KEY] in [Pickup] with 1.2s
[13:33:31.587] [QuickSave] [FsmLogging] 'WIND-UP KEY' State 'Click 3' Action 'SendEvent': Event 'Wind-Up Key pickup' to 'Global Manager'
[13:33:31.588] [QuickSave] [FsmLogging] 'WIND-UP KEY' State 'Click 3' Action 'iTweenScaleTo': <iTweenScaleTo>
[13:33:31.588] [QuickSave] [FsmLogging] 'WIND-UP KEY' State 'Click 3' Action 'PlaySound': <PlaySound>
[13:33:36.572] [QuickSave] [FsmLogging] 'Keyhole' State 'State 4' Action 'SetFsmInt': [Global Manager] 'NewCursor' -> 16
[13:33:36.572] [QuickSave] [FsmLogging] 'Keyhole' State 'State 4' Action 'MousePickEvent': Target:
[13:33:36.574] [QuickSave] [FsmLogging] 'Keyhole' State 'State 8' Action 'ArrayListContains': Check '' for (WIND-UP KEY) (If True: change state to State 4, if False: do nothing)
[13:33:36.575] [QuickSave] [FsmLogging] 'Keyhole' State 'State 8' Action 'GetFsmGameObject': [Global Manager] Get 'Inv PickedUp' -> Store in 'Inventory EstateItems'
[13:33:37.023] [QuickSave] [FsmLogging] 'Keyhole' State 'Click' Action 'SetFsmInt': [Global Manager] 'NewCursor' -> 0
[13:33:37.023] [QuickSave] [FsmLogging] 'Keyhole' State 'Click' Action 'iTweenMoveTo': <iTweenMoveTo>
[13:33:37.024] [QuickSave] [FsmLogging] 'Keyhole' State 'Click' Action 'ActivateGameObject': Activate 'Wind Up Key': True
[13:33:37.024] [QuickSave] [FsmLogging] 'Keyhole' State 'Click' Action 'SendEvent': Event 'disable' to 'PARLOR GAME'
[13:33:37.024] [QuickSave] [FsmLogging] 'Keyhole' State 'Click' Action 'SendEvent': Event 'Begin' to 'box sound'
[13:33:37.173] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'State 2' Action 'IntCompare': Test 'Solution' (1) == '' (2) (If Equal: change state to White Check, if Less: change state to Blue Check, if Greater: change state to Black Check)
[13:33:37.173] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'State 2' Action 'SendEvent': Event 'disable' to 'PARLOR GAME'
[13:33:37.174] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'State 2' Action 'SetBoolValue': 'ready' -> False
[13:33:37.174] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'Blue Check' Action 'IntCompare': Test '' (1) == 'Selection' (1) (If Equal: change state to extra gem, if Less: do nothing, if Greater: do nothing)
[13:33:37.174] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'Blue Gems' Action 'SendEvent': Event 'done' to 'Self/Unknown'
[13:33:37.174] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'Blue Gems' Action 'PmtSpawn': Spawning GameObject [GEM SAPHIRE] in [Pickup] at position [2]
[13:33:37.174] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'Blue Gems' Action 'PmtSpawn': Spawning GameObject [GEM SAPHIRE] in [Pickup] at position [1]
[13:33:37.175] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'Blue Gems' Action 'BoolTest': Test 'funeral gems' == False (If True: change state to Funeral, if False: do nothing)
[13:33:37.175] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'Blue Gems' Action 'SetBoolValue': 'Correct' -> True
[13:33:37.175] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'Scoring' Action 'BoolTest': Test 'Correct' == True (If True: change state to State 3, if False: change state to Dare Calming)
[13:33:37.175] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'Scoring' Action 'GetFsmInt': [PARLOR TIMER] Get 'Timer' -> Store in 'Timer'
[13:33:37.175] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'Scoring' Action 'GetFsmInt': [Global Persitent Manager] Get 'Parlor Score' -> Store in 'Parlor Score'
[13:33:37.175] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'Scoring' Action 'BoolTest': Test 'one guess' == False (If True: do nothing, if False: do nothing)
[13:33:37.175] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'Correct' Action 'IntCompare': Test 'Timer' (268) == '' (200) (If Equal: do nothing, if Less: do nothing, if Greater: change state to Correct but over 200 seconds)
[13:33:37.175] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'Correct' Action 'IntCompare': Test 'Timer' (268) == '' (350) (If Equal: do nothing, if Less: do nothing, if Greater: change state to Correct but over 350 seconds)
[13:33:37.175] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'Correct' Action 'IntCompare': Test 'Timer' (268) == '' (80) (If Equal: do nothing, if Less: change state to Correct in under 80 seconds, if Greater: do nothing)
[13:33:37.175] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'Correct' Action 'IntCompare': Test 'Timer' (268) == '' (40) (If Equal: do nothing, if Less: change state to Correct in under 40 seconds, if Greater: do nothing)
[13:33:37.175] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'Correct but over 200 seconds' Action 'SendEvent': Event 'Event 8' to 'Self/Unknown'
[13:33:37.175] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'Correct but over 200 seconds' Action 'SetFsmInt': [Global Persitent Manager] 'Parlor Score' -> 16
[13:33:37.175] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'Correct but over 200 seconds' Action 'IntAdd': 'Parlor Score' += -1
[13:33:37.176] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'Correct Tally Add' Action 'SetFsmInt': [Global Persitent Manager] 'ParlorPuzzles Correct' -> 13
[13:33:37.176] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'Correct Tally Add' Action 'IntAdd': 'PuzzlesCorrect' += 1
[13:33:37.176] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'Correct Tally Add' Action 'GetFsmInt': [Global Persitent Manager] Get 'ParlorPuzzles Correct' -> Store in 'PuzzlesCorrect'
[13:33:37.176] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'Trophy' Action 'IntCompare': Test 'PuzzlesCorrect' (13) == '' (39) (If Equal: do nothing, if Less: change state to Blue Gems, if Greater: do nothing)
[13:33:37.176] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'Trophy' Action 'GetFsmInt': [Global Persitent Manager] Get 'ParlorPuzzles Correct' -> Store in 'PuzzlesCorrect'
[13:33:37.176] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'extra gem' Action 'PmtSpawn': Spawning GameObject [GEM SAPHIRE] in [Pickup] at position [5]
[13:33:37.176] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'extra gem' Action 'BoolTest': Test 'extra gem' == True (If True: do nothing, if False: change state to Trophy)
[13:33:37.176] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'State 3' Action 'SendEvent': Event 'Update' to 'DARE ENGINE'
[13:33:37.176] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'State 3' Action 'SetFsmInt': [DARE ENGINE] 'dare break' -> 7
[13:33:37.176] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'State 3' Action 'GetFsmGameObject': [Global Manager] Get 'DARE ENGINE' -> Store in 'DARE ENGINE'
[13:33:37.176] [QuickSave] [FsmLogging] 'Keyhole' State 'State 2' Action 'SetFsmInt': [Global Manager] 'NewCursor' -> 0
[13:33:37.176] [QuickSave] [FsmLogging] 'Keyhole' State 'State 2' Action 'iTweenRotateBy': <iTweenRotateBy>
[13:33:37.176] [QuickSave] [FsmLogging] 'Keyhole' State 'State 2' Action 'SendEvent': Event 'Begin' to 'Inventory Remove'
[13:33:37.176] [QuickSave] [FsmLogging] 'Keyhole' State 'State 2' Action 'SendEvent': Event 'Event 0' to 'PARLOR GAME'
[13:33:37.176] [QuickSave] [FsmLogging] 'Keyhole' State 'State 7' Action 'SetFsmInt': [PARLOR GAME] 'Selection' -> 1
[13:33:37.176] [QuickSave] [FsmLogging] 'Keyhole' State 'KEY SPEND' Action 'SetFsmInt': [] 'Wind-Up Key' -> -1
[13:33:37.176] [QuickSave] [FsmLogging] 'Keyhole' State 'KEY SPEND' Action 'IntAdd': 'keytotal' += -1
[13:33:37.176] [QuickSave] [FsmLogging] 'Keyhole' State 'KEY SPEND' Action 'GetFsmInt': [] Get 'Wind-Up Key' -> Store in 'keytotal'
[13:33:37.176] [QuickSave] [FsmLogging] 'Keyhole' State 'KEY SPEND' Action 'GetFsmGameObject': [Global Manager] Get 'Special Key Tracker' -> Store in 'Special Key Tracker'
[13:33:37.206] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'Waiting for selection' Action 'SetBoolValue': 'ready' -> True
[13:33:37.206] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'State 4' Action 'NextFrameEvent': -> Waiting for selection
[13:33:37.206] [QuickSave] [FsmLogging] 'PARLOR GAME' State 'State 4' Action 'SetBoolValue': 'one guess' -> True
[13:33:37.640] [QuickSave] [FsmLogging] 'Keyhole' State 'State 3' Action 'SendEvent': Event 'activate' to 'Lid Collider - Click blocker'
[13:33:37.640] [QuickSave] [FsmLogging] 'Keyhole' State 'State 3' Action 'SetFsmInt': [Global Manager] 'NewCursor' -> 2
[13:33:37.640] [QuickSave] [FsmLogging] 'Lid Collider - Click blocker' State 'State 2' Action 'iTweenRotateBy': <iTweenRotateBy>
[13:33:37.640] [QuickSave] [FsmLogging] 'Lid Collider - Click blocker' State 'State 2' Action 'SetFsmInt': [Global Manager] 'NewCursor' -> 2
[13:33:38.257] [QuickSave] [FsmLogging] 'Lid Collider - Click blocker' State 'Prestate' Action 'MousePickEvent': Target: