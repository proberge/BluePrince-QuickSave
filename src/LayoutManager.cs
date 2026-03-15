using System.Collections.Generic;
using UnityEngine;
using MelonLoader;
using BluePrince.Modding.QuickSave;
using Il2Cpp;
using QuickSave.Utils;
using Vector3 = UnityEngine.Vector3;
using Rotation = BluePrince.Modding.QuickSave.Rotation;
using Column = BluePrince.Modding.QuickSave.Column;
using Rank = BluePrince.Modding.QuickSave.Rank;

namespace QuickSave
{
    public static class LayoutManager
    {
        public static MelonLogger.Instance Logger => Melon<Core>.Instance.LoggerInstance;

        public static Layout GetLayout()
        {
            var layout = new Layout();
            
            (string name, float rotationY)[,] roomData = new (string, float)[5, 9];
             for (int x = 0; x < 5; x++)
            {
                for (int z = 0; z < 9; z++)
                {
                    roomData[x, z] = ("Empty", 0f);
                }
            }
            roomData[2, 0] = ("Entrance Hall", 0f);
            
            GameObject roomContainer = GameObject.Find("__SYSTEM/Room Spawn Pools/");
            string outerRoomName = "Empty";

            if (roomContainer != null)
            {
                for (int i = 0; i < roomContainer.transform.childCount; i++)
                {
                    Transform childTransform = roomContainer.transform.GetChild(i);
                    string name = childTransform.name;

                    int cloneIndex = name.IndexOf("(Clone)");
                    if (cloneIndex >= 0) name = name.Substring(0, cloneIndex);

                    if (UnityEngine.Vector3.Distance(childTransform.position, new UnityEngine.Vector3(-33.7815f, -1.328f, 10.1183f)) < 2.0f)
                    {
                        outerRoomName = name;
                        layout.DraftOrder.Add($"{name} (Outer)");
                        continue;
                    }

                    ManorPosition pos = ManorExtensions.FromWorldPosition(childTransform.position);

                    if (pos.Column != Column.Unspecified && pos.Rank != Rank.Unspecified)
                    {
                        roomData[pos.Column.ToX(), pos.Rank.ToZ()] = (name, childTransform.eulerAngles.y);
                        layout.DraftOrder.Add($"{name} ({pos})");
                    }
                }
            }
            else
            {
                Logger.Warning("Could not find Room Spawn Pools.");
            }
            
            layout.OuterRoomName = outerRoomName;

            for (int z = 0; z < 9; z++)
            {
                for (int x = 0; x < 5; x++)
                {
                    var data = roomData[x, z];
                    if (data.name == "Empty") continue;

                    var room = new BluePrince.Modding.QuickSave.Room();
                    room.Name = data.name;
                    room.Rotation = ManorExtensions.ToRotation(data.rotationY);
                    room.Column = ManorExtensions.ToColumn(x);
                    room.Rank = ManorExtensions.ToRank(z);

                    layout.Rooms.Add(room);
                }
            }
            
            return layout;
        }

        public static void RestoreLayout(Layout layout)
        {
            if (layout == null)
            {
                Logger.Warning("No layout data in save state.");
                return;
            }

            GameObject roomContainer = GameObject.Find("__SYSTEM/Room Spawn Pools/");
            if (roomContainer == null)
            {
                Logger.Warning("Could not find Room Spawn Pools for loading.");
                return;
            }

            bool[,] occupancy = new bool[5, 9];
            foreach (var r in layout.Rooms)
            {
                if (r.Column != Column.Unspecified && r.Rank != Rank.Unspecified)
                    occupancy[r.Column.ToX(), r.Rank.ToZ()] = true;
            }
            occupancy[Column.C.ToX(), Rank._9.ToZ()] = true; // C9 is the Antechamber

            var existingRooms = new Dictionary<string, List<GameObject>>();
            for (int i = 0; i < roomContainer.transform.childCount; i++)
            {
                GameObject roomGo = roomContainer.transform.GetChild(i).gameObject;
                string cleanName = roomGo.name;
                int cloneIndex = cleanName.IndexOf("(Clone)");
                if (cloneIndex >= 0) cleanName = cleanName.Substring(0, cloneIndex);

                if (!existingRooms.ContainsKey(cleanName))
                    existingRooms[cleanName] = new List<GameObject>();
                
                existingRooms[cleanName].Add(roomGo);
                
                if (cleanName != "Entrance Hall")
                    roomGo.transform.position = new UnityEngine.Vector3(-1000, -1000, -1000);
            }

            GameObject engineBase = GameObject.Find("/__SYSTEM/The Room Engines");
            var placedRooms = new List<(BluePrince.Modding.QuickSave.Room saved, GameObject go)>();

            foreach (var savedRoom in layout.Rooms)
            {
                GameObject roomToPlace = null;

                if (existingRooms.ContainsKey(savedRoom.Name) && existingRooms[savedRoom.Name].Count > 0)
                {
                    roomToPlace = existingRooms[savedRoom.Name][0];
                    existingRooms[savedRoom.Name].RemoveAt(0);
                }
                else if (savedRoom.Name == "Entrance Hall")
                {
                    roomToPlace = GameObject.Find("ROOMS/Entrance Hall");
                    if (roomToPlace == null) roomToPlace = GameObject.Find("__SYSTEM/Room Spawn Pools/Entrance Hall");
                }
                else
                {
                    if (engineBase != null)
                    {
                        Transform engineTransform = engineBase.transform.Find(savedRoom.Name.ToUpper());
                        if (engineTransform != null)
                        {
                            var fsm = engineTransform.gameObject.GetComponent<PlayMakerFSM>();
                            if (fsm != null)
                            {
                                GameObject prefab = fsm.FsmVariables.GetFsmGameObject("Room Prefab").Value;
                                if (prefab == null) prefab = fsm.FsmVariables.GetFsmGameObject("RoomPrefabRef").Value;

                                if (prefab != null)
                                {
                                    roomToPlace = GameObject.Instantiate(prefab, roomContainer.transform);
                                    roomToPlace.name = savedRoom.Name;
                                    Logger.Msg($"  Instantiated missing room: {savedRoom.Name}");
                                }
                            }
                        }
                    }
                }

                if (roomToPlace != null)
                {
                    bool isEntranceHall = savedRoom.Name == "Entrance Hall";
                    
                    if (!isEntranceHall)
                    {
                        Vector3 targetPos = ManorExtensions.GetWorldPosition(savedRoom.Rank, savedRoom.Column);
                        roomToPlace.transform.position = targetPos;
                        
                        if (savedRoom.Name != "Garage")
                        {
                            roomToPlace.transform.rotation = Quaternion.Euler(0, savedRoom.Rotation.ToRotationY(), 0);
                        }
                    }
                    
                    roomToPlace.SetActive(true);
                    RoomHelper.TriggerRoomBegin(roomToPlace);
                    placedRooms.Add((savedRoom, roomToPlace));

                    if (isEntranceHall)
                        Logger.Msg($"  Updated doors for 'Entrance Hall' (kept original position)");
                    else
                        Logger.Msg($"  Restored room '{savedRoom.Name}' to {new ManorPosition(savedRoom.Rank, savedRoom.Column)} rotation={savedRoom.Rotation}");
                }
                else
                {
                    Logger.Warning($"  Could not find or instantiate room: {savedRoom.Name}");
                }
            }

            ProcessAllDoors(placedRooms, occupancy);
        }

        public static void RestoreMap(Layout layout)
        {
            if (layout == null) return;

            GameObject grid = GameObject.Find("__SYSTEM/THE GRID");
            if (grid == null)
            {
                Logger.Warning("Could not find __SYSTEM/THE GRID");
                return;
            }

            GameObject engineBase = GameObject.Find("/__SYSTEM/The Room Engines");
            if (engineBase == null)
            {
                Logger.Warning("Could not find /__SYSTEM/The Room Engines");
                return;
            }

            foreach (var room in layout.Rooms)
            {
                if (room.Name == "Entrance Hall") continue;

                int tileIndex = (room.Rank.ToZ() * 5) + room.Column.ToX() + 1;

                if (tileIndex < 1 || tileIndex > 46)
                {
                    Logger.Warning($"  Map tile index {tileIndex} for {room.Name} is out of range.");
                    continue;
                }

                Transform tile = grid.transform.Find($"Tile {tileIndex}");
                if (tile == null) continue;

                Transform mapTile = tile.Find($"Map Tile {tileIndex}");
                if (mapTile == null) continue;

                MeshRenderer renderer = mapTile.GetComponent<MeshRenderer>();
                if (renderer == null) continue;

                Transform engine = engineBase.transform.Find(room.Name.ToUpper());
                if (engine == null)
                {
                    Logger.Warning($"  Could not find Engine for room: {room.Name}");
                    continue;
                }

                PlayMakerFSM fsm = engine.gameObject.GetComponent<PlayMakerFSM>();
                if (fsm == null) continue;

                Rotation rot = room.Rotation;

                string dirVar = rot switch
                {
                    Rotation.Rotate0 => "NORTH",
                    Rotation.Rotate90 => "EAST",
                    Rotation.Rotate180 => "SOUTH",
                    Rotation.Rotate270 => "WEST",
                    _ => "NORTH"
                };

                var texVar = fsm.FsmVariables.GetFsmTexture(dirVar);
                if (texVar != null && texVar.Value != null)
                {
                    renderer.material.mainTexture = texVar.Value;
                    mapTile.gameObject.SetActive(true);
                }
                else
                {
                     Logger.Warning($"  Could not find texture for room: {dirVar} in {room.Name}");
                }
            }
        }

        private static void ProcessAllDoors(List<(BluePrince.Modding.QuickSave.Room saved, GameObject go)> placedRooms, bool[,] occupancy)
        {
            var doorMap = new Dictionary<(Column c, Rank r), HashSet<int>>();
            doorMap[(Column.C, Rank._1)] = new HashSet<int> { 0, 1, 3 }; // C1 (Entrance Hall): N, E, W
            doorMap[(Column.C, Rank._9)] = new HashSet<int> { 0, 2, 3 }; // C9 (Antechamber): N, S, W

            foreach (var (saved, go) in placedRooms)
            {
                Transform doorsContainer = go.transform.Find("_GAMEPLAY/_DOORS");
                if (doorsContainer == null) doorsContainer = go.transform.Find("_GAMEPLAY /_DOORS");
                
                var dirs = new HashSet<int>();

                if (doorsContainer != null && doorsContainer.childCount == 1) {
                    Logger.Msg($"  Room '{saved.Name}' has only one child in _DOORS, probably needs special handling.");
                    var worldDir = RoomHelper.GetDoorWorldDir("S Door", saved.Rotation.ToRotationY());
                    if (worldDir.HasValue) dirs.Add((int)worldDir.Value);
                    worldDir = RoomHelper.GetDoorWorldDir("N Door", saved.Rotation.ToRotationY());
                    if (worldDir.HasValue) dirs.Add((int)worldDir.Value);
                }

                if (doorsContainer != null)
                {
                    for (int i = 0; i < doorsContainer.childCount; i++)
                    {
                        var dir = RoomHelper.GetDoorWorldDir(doorsContainer.GetChild(i).name, saved.Rotation.ToRotationY());
                        if (dir.HasValue) dirs.Add((int)dir.Value);
                    }
                }
                else
                {
                    var dir = RoomHelper.GetDoorWorldDir("S Door", saved.Rotation.ToRotationY());
                    if (dir.HasValue) dirs.Add((int)dir.Value);
                    Logger.Msg($"  Room '{saved.Name}' has no _DOORS container. Assuming South door (world dir {dir})");
                }
                doorMap[(saved.Column, saved.Rank)] = dirs;
            }

            foreach (var (saved, go) in placedRooms)
            {
                Transform doorsContainer = go.transform.Find("_GAMEPLAY/_DOORS");
                if (doorsContainer == null) doorsContainer = go.transform.Find("_GAMEPLAY /_DOORS");
                
                int doorCount = (doorsContainer != null) ? doorsContainer.childCount : 1;

                for (int i = 0; i < doorCount; i++)
                {
                    Transform door = (doorsContainer != null) ? doorsContainer.GetChild(i) : null;
                    int? worldDir;
                    
                    if (door != null) worldDir = RoomHelper.GetDoorWorldDir(door.name, saved.Rotation.ToRotationY());
                    else worldDir = RoomHelper.GetDoorWorldDir("S Door", saved.Rotation.ToRotationY());

                    if (!worldDir.HasValue) continue;

                    var (dx, dz) = RoomHelper.DirectionToDelta(worldDir.Value);
                    Column ncol = (Column)((int)saved.Column + dx);
                    Rank nrank = (Rank)((int)saved.Rank + dz);

                    if (!RoomHelper.GetOccupancy(occupancy, ncol, nrank)) continue;

                    int oppositeDir = ((int)worldDir.Value + 2) % 4;
                    bool neighborHasDoorBack = doorMap.ContainsKey((ncol, nrank)) && doorMap[(ncol, nrank)].Contains(oppositeDir);

                    if (door != null) door.gameObject.SetActive(false);

                    if (!neighborHasDoorBack)
                    {
                        Vector3 edgePos = ManorExtensions.GetWorldPosition(saved.Rank, saved.Column);
                        edgePos.x += dx * 5f;
                        edgePos.z += dz * 5f;
                        RoomHelper.PlaceDeadEnd(edgePos, worldDir.Value);
                        Logger.Msg($"  Placed DeadEnd between {new ManorPosition(saved.Rank, saved.Column)} and {new ManorPosition(nrank, ncol)} dir={worldDir}");
                    }
                }
            }
        }
    }
}
