using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Inner_Maps {
    public class InnerMapManager : MonoBehaviour {

        public static InnerMapManager Instance;
        
        public static readonly Vector2Int BuildingSpotSize = new Vector2Int(7, 7);
        public static readonly int BuildingSpotBorderSize = 1; //how many tiles, per side of the build spot, should not be occupied by the structure.

        public const int DefaultCharacterSortingOrder = 82;
        public const int GroundTilemapSortingOrder = 10;
        public const int DetailsTilemapSortingOrder = 40;
   
        private Vector3 _nextMapPos = Vector3.zero;
        public GameObject characterCollisionTriggerPrefab;

        [Header("Pathfinding")]
        [SerializeField] private AstarPath pathfinder;

        [Header("Tile Object")]
        [SerializeField] private TileObjectSlotDictionary tileObjectSlotSettings;
        public GameObject tileObjectSlotsParentPrefab;
        public GameObject tileObjectSlotPrefab;
    
        [Header("Lighting")]
        [SerializeField] private Light areaMapLight;

        [Header("Structures")]
        [SerializeField] private LocationStructurePrefabDictionary structurePrefabs;

        [Header("Tilemap Assets")] 
        public InnerMapAssetManager assetManager;
        [SerializeField] private ItemAsseteDictionary itemTiles;
        [SerializeField] private TileObjectAssetDictionary tileObjectTiles;
        [SerializeField] private WallResourceAssetDictionary wallResourceAssets; //wall assets categorized by resource.
        [SerializeField] private List<TileBase> allTileAssets;

        //Area Map Objects
        [FormerlySerializedAs("areaMapObjectFactory")] public MapVisualFactory mapObjectFactory;

        //structure templates
        private string templatePath;
        
        //this specifies what light intensity is to be used while inside the specific range in ticks
        private readonly Dictionary<int, float> lightSettings = new Dictionary<int, float>() { 
#if UNITY_EDITOR
            { 228, 1f }, { 61, 1.8f }
#else
            { 228, 0.3f }, { 61, 0.8f }
#endif
        };
        public Dictionary<TILE_OBJECT_TYPE, List<TileObject>> allTileObjects { get; private set; }
        public AreaInnerTileMap currentlyShowingMap { get; private set; }
        public Area currentlyShowingArea { get; private set; }
        public List<InnerTileMap> innerMaps { get; private set; }
        public bool isAnAreaMapShowing => currentlyShowingMap != null;

        public IPointOfInterest currentlyHoveredPoi {
            get {
                if (isAnAreaMapShowing) {
                    if (currentlyShowingMap.hoveredCharacter != null) {
                        return currentlyShowingMap.hoveredCharacter;
                    } else if (GetTileFromMousePosition() != null) {
                        LocationGridTile hoveredTile = GetTileFromMousePosition();
                        return hoveredTile.objHere;
                    }
                }
                return null;
            }
        }
        public LocationGridTile currentlyHoveredTile => GetTileFromMousePosition();
        public List<LocationGridTile> currentlyHighlightedTiles { get; private set; }

        #region Monobehaviours
        private void Awake() {
            Instance = this;
            templatePath = Application.dataPath + "/StreamingAssets/Structure Templates/";
        }
        public void LateUpdate() {
            if (GameManager.showAllTilesTooltip) {
                if (UIManager.Instance.IsMouseOnUI() || IsMouseOnMapObject() || currentlyShowingMap == null) {
                    // if (UIManager.Instance.IsSmallInfoShowing() && UIManager.Instance.smallInfoShownFrom == "ShowTileData") {
                    //     UIManager.Instance.HideSmallInfo();
                    // }
                    return;
                }
                LocationGridTile hoveredTile = GetTileFromMousePosition();
                if (hoveredTile.objHere == null) {
                    ShowTileData(hoveredTile);
                }
            }

            if (Input.GetMouseButtonDown(0)) {
                if (UIManager.Instance.IsMouseOnUI() == false && IsMouseOnMapObject() == false && currentlyShowingMap != null) {
                    Messenger.Broadcast(Signals.HIDE_MENUS);    
                }
            }
//             if (UIManager.Instance.IsMouseOnUI() || IsMouseOnMarker() || currentlyShowingMap == null) {
//                 if (UIManager.Instance.IsSmallInfoShowing() && UIManager.Instance.smallInfoShownFrom == "ShowTileData") {
//                     UIManager.Instance.HideSmallInfo();
//                 }
//                 return;
//             }
//             LocationGridTile hoveredTile = GetTileFromMousePosition();
//             if (hoveredTile != null) {
//                 //CursorManager.Instance.SetSparkleEffectState(hoveredTile.objHere != null);
//                 if (GameManager.showAllTilesTooltip) {
//                     ShowTileData(hoveredTile);
//                     if (hoveredTile.objHere != null) {
//                         if (Input.GetMouseButtonDown(0)) {
//                             hoveredTile.OnClickTileActions(PointerEventData.InputButton.Left);
//                         } else if (Input.GetMouseButtonDown(1)) {
//                             hoveredTile.OnClickTileActions(PointerEventData.InputButton.Right);
//                         }
//                     } else {
//                         if (Input.GetMouseButtonDown(0)) {
//                             hoveredTile.OnClickTileActions(PointerEventData.InputButton.Left);
//                         } else if (Input.GetMouseButtonDown(1)) {
//                             hoveredTile.OnClickTileActions(PointerEventData.InputButton.Right);
//                         }
//                     }
//                 } else {
//                     if (hoveredTile.objHere != null) {
//                         if(hoveredTile.objHere != null) {
//                             ShowTileData(hoveredTile);
//                         } 
// //                        else {
// //                            ShowCharacterData(hoveredTile.parentMap.hoveredCharacter);
// //                        }
//                         if (Input.GetMouseButtonDown(0)) {
//                             hoveredTile.OnClickTileActions(PointerEventData.InputButton.Left);
//                         } else if (Input.GetMouseButtonDown(1)) {
//                             hoveredTile.OnClickTileActions(PointerEventData.InputButton.Right);
//                         }
//                     } else {
//                         if (Input.GetMouseButtonDown(0)) {
//                             hoveredTile.OnClickTileActions(PointerEventData.InputButton.Left);
//                         } else if (Input.GetMouseButtonDown(1)) {
//                             hoveredTile.OnClickTileActions(PointerEventData.InputButton.Right);
//                         }
//                         UIManager.Instance.HideSmallInfo();
//
//                     }
//                 }
//
//             } else {
//                 UIManager.Instance.HideSmallInfo();
//             }
        }
        #endregion

        #region Main
        public void Initialize() {
            allTileObjects = new Dictionary<TILE_OBJECT_TYPE, List<TileObject>>();
            innerMaps = new List<InnerTileMap>();
            mapObjectFactory = new MapVisualFactory();
            AreaMapCameraMove.Instance.Initialize();
            Messenger.AddListener(Signals.TICK_ENDED, CheckForChangeLight);
        }
        /// <summary>
        /// Try and show the area map of an area. If it does not have one, this will generate one instead.
        /// </summary>
        /// <param name="area"></param>
        public void TryShowAreaMap(Area area) {
            if (area.areaMap != null) {
                //show existing map
                ShowAreaMap(area);
            } else {
                throw new System.Exception($"{area.name} does not have a generated areaMap");
            }
        }
        public void ShowAreaMap(Area area, bool centerCameraOnMapCenter = true, bool instantCenter = true) {
            if (area.locationType == LOCATION_TYPE.DEMONIC_INTRUSION) {
                UIManager.Instance.portalPopup.SetActive(true);
                return;
            }
            area.areaMap.Open();
            currentlyShowingMap = area.areaMap;
            currentlyShowingArea = area;
            Messenger.Broadcast(Signals.AREA_MAP_OPENED, area);

            if (centerCameraOnMapCenter) {
                AreaMapCameraMove.Instance.JustCenterCamera(instantCenter);
            }
        }
        public Area HideAreaMap() {
            if (currentlyShowingMap == null) {
                return null;
            }
            currentlyShowingMap.Close();
            Area closedArea = currentlyShowingArea;
            AreaMapCameraMove.Instance.CenterCameraOn(null);
            currentlyShowingMap = null;
            currentlyShowingArea = null;
            PlayerManager.Instance.player.SetCurrentlyActivePlayerJobAction(null);
            Messenger.Broadcast(Signals.AREA_MAP_CLOSED, closedArea);
            return closedArea;
        }
        public void OnCreateInnerMap(InnerTileMap newMap) {
            innerMaps.Add(newMap);
            //newMap.transform.localPosition = nextMapPos;
            //set the next map position based on the new maps height
            newMap.transform.localPosition = _nextMapPos;
            newMap.UpdateTilesWorldPosition();
            PathfindingManager.Instance.CreatePathfindingGraphForLocation(newMap);
            _nextMapPos = new Vector3(_nextMapPos.x, _nextMapPos.y + newMap.height + 10, _nextMapPos.z);
        }
        public void DestroyAreaMap(Area area) {
            foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in area.structures) {
                for (var i = 0; i < keyValuePair.Value.Count; i++) {
                    keyValuePair.Value[i].DoCleanup();
                }
            }
            pathfinder.data.RemoveGraph(area.areaMap.pathfindingGraph);
            area.areaMap.CleanUp();
            innerMaps.Remove(area.areaMap);
            GameObject.Destroy(area.areaMap.gameObject);
            area.SetAreaMap(null);
            Debug.LogError("Area map of " + area.name + " is destroyed!");
        }
        #endregion

        #region Utilities
        public LocationGridTile GetTileFromMousePosition() {
            Vector3 mouseWorldPos = (currentlyShowingMap.worldUiCanvas.worldCamera.ScreenToWorldPoint(Input.mousePosition));
            Vector3 localPos = currentlyShowingMap.grid.WorldToLocal(mouseWorldPos);
            Vector3Int coordinate = currentlyShowingMap.grid.LocalToCell(localPos);
            if (coordinate.x >= 0 && coordinate.x < currentlyShowingMap.width
                                  && coordinate.y >= 0 && coordinate.y < currentlyShowingMap.height) {
                return currentlyShowingMap.map[coordinate.x, coordinate.y];
            }
            return null;
        }
        public LocationStructure GetRandomStructureToPlaceItem(ILocation location, SpecialToken token) {
            //Items are now placed specifically in a structure when spawning at world creation. 
            //Randomly place it at any non-Dwelling structure in the location.
            List<LocationStructure> choices = new List<LocationStructure>();
            foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in location.structures) {
                if (kvp.Key != STRUCTURE_TYPE.DWELLING && kvp.Key != STRUCTURE_TYPE.EXIT && kvp.Key != STRUCTURE_TYPE.CEMETERY
                    && kvp.Key != STRUCTURE_TYPE.INN && kvp.Key != STRUCTURE_TYPE.WORK_AREA && kvp.Key != STRUCTURE_TYPE.PRISON && kvp.Key != STRUCTURE_TYPE.POND) {
                    choices.AddRange(kvp.Value);
                }
            }
            if (choices.Count > 0) {
                return choices[UnityEngine.Random.Range(0, choices.Count)];
            }
            return null;
        }
        #endregion

        #region UI
        private bool IsMouseOnMapObject() {
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = Input.mousePosition;

            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, raycastResults);

            if (raycastResults.Count > 0) {
                foreach (var go in raycastResults) {
                    if (go.gameObject.CompareTag("Character Marker") || go.gameObject.CompareTag("Map Object")) {
                        //Debug.Log(go.gameObject.name, go.gameObject);
                        return true;
                    }

                }
            }
            return false;
        }
        public void HighlightTiles(List<LocationGridTile> tiles) {
            if (tiles != null) {
                for (int i = 0; i < tiles.Count; i++) {
                    tiles[i].HighlightTile();
                }
            }
            currentlyHighlightedTiles = tiles;
        }
        public void UnhighlightTiles() {
            if (currentlyHighlightedTiles != null) {
                for (int i = 0; i < currentlyHighlightedTiles.Count; i++) {
                    currentlyHighlightedTiles[i].UnhighlightTile();
                }
            }
            currentlyHighlightedTiles = null;
        }
        public void UnhighlightTiles(List<LocationGridTile> tiles) {
            for (int i = 0; i < tiles.Count; i++) {
                tiles[i].UnhighlightTile();
            }
        }
        #endregion

        #region Structure Templates
        public List<StructureTemplate> GetStructureTemplates(STRUCTURE_TYPE structure) {
            List<StructureTemplate> templates = new List<StructureTemplate>();
            string path = templatePath + structure.ToString() + "/";
            if (Directory.Exists(path)) {
                DirectoryInfo info = new DirectoryInfo(path);
                FileInfo[] files = info.GetFiles();
                for (int i = 0; i < files.Length; i++) {
                    FileInfo currInfo = files[i];
                    if (currInfo.Extension.Equals(".json")) {
                        string dataAsJson = File.ReadAllText(currInfo.FullName);
                        StructureTemplate loaded = JsonUtility.FromJson<StructureTemplate>(dataAsJson);
                        loaded.name = currInfo.Name;
                        templates.Add(loaded);
                    }
                }
            }
            return templates;
        }
        public List<StructureTemplate> GetStructureTemplates(string folderName, List<string> except = null) {
            List<StructureTemplate> templates = new List<StructureTemplate>();
            string path = templatePath + folderName + "/";
            if (Directory.Exists(path)) {
                DirectoryInfo info = new DirectoryInfo(path);
                FileInfo[] files = info.GetFiles();
                for (int i = 0; i < files.Length; i++) {
                    FileInfo currInfo = files[i];
                    if (currInfo.Extension.Equals(".json")) {
                        string dataAsJson = File.ReadAllText(currInfo.FullName);
                        StructureTemplate loaded = JsonUtility.FromJson<StructureTemplate>(dataAsJson);
                        loaded.name = currInfo.Name;
                        if (except != null && except.Contains(loaded.name)) {
                            continue; //skip
                        }
                        //Debug.Log(loaded.name);
#if TRAILER_BUILD
                    if (folderName == "TOWN CENTER/" && loaded.name != "TC_Template_3.json") {
                        continue; //only use Template 3 on Trailer Build
                    }
#endif
                        templates.Add(loaded);
                    }
                }
            }
            return templates;
        }
        /// <summary>
        /// Get Tile asset based on name. NOTE: Should only be used on the start of the game when building the area maps.
        /// </summary>
        /// <param name="name">Name of the asset</param>
        public TileBase GetTileAsset(string name, bool logMissing = false) {
            //List<TileBase> allTileAssets = LoadAllTilesAssets();
            for (int i = 0; i < allTileAssets.Count; i++) {
                TileBase currTile = allTileAssets[i];
                if (currTile.name == name) {
                    return currTile;
                }
            }
            if (logMissing) {
                Debug.LogWarning("There is no tilemap asset with name " + name);
            }
            return null;
        }
        /// <summary>
        /// Convert all tile list to a dictionary for easier accessing of data. Meant to be used when a function is expected to 
        /// heavily use the tile database, to prevent constant looping of tile database list.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, TileBase> GetTileAssetDatabase() {
            Dictionary<string, TileBase> tileDB = new Dictionary<string, TileBase>();
            for (int i = 0; i < allTileAssets.Count; i++) {
                TileBase currTile = allTileAssets[i];
                tileDB.Add(currTile.name, currTile);
            }
            return tileDB;
        }
        public TileBase TryGetTileAsset(string name, Dictionary<string, TileBase> assets) {
            if (assets.ContainsKey(name)) {
                return assets[name];
            }
            return null;
        }
        private List<TileBase> LoadAllTilesAssets() {
            return Resources.LoadAll("Tile Map Assets", typeof(TileBase)).Cast<TileBase>().ToList();
        }
        [ContextMenu("Load Assets")]
        public void LoadTileAssets() {
            allTileAssets = LoadAllTilesAssets().Distinct().ToList();
            allTileAssets.Sort((x, y) => string.Compare(x.name, y.name));
        }
        #endregion

        #region For Testing
        public void ShowTileData(LocationGridTile tile, Character character = null) {
            if (tile == null) {
                return;
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            string summary = tile.localPlace.ToString();
            summary += "\n<b>Local Location:</b>" + tile.localLocation;
            summary += " <b>World Location:</b>" + tile.worldLocation;
            summary += " <b>Centered World Location:</b>" + tile.centeredWorldLocation;
            summary += " <b>Ground Type:</b>" + tile.groundType;
            summary += " <b>Is Occupied:</b>" + tile.isOccupied;
            summary += " <b>Tile Type:</b>" + tile.tileType;
            summary += " <b>Tile State:</b>" + tile.tileState;
            summary += " <b>Reserved Tile Object Type:</b>" + tile.reservedObjectType;
            summary += " <b>Previous Tile Asset:</b>" + (tile.previousGroundVisual?.name ?? "Null");
            summary += " <b>Current Tile Asset:</b>" + (tile.parentTileMap.GetSprite(tile.localPlace)?.name ?? "Null");
            if (tile.hasFurnitureSpot) {
                summary += " <b>Furniture Spot:</b>" + tile.furnitureSpot;
            }
            summary += "\nTile Traits: ";
            if (tile.normalTraits.Count > 0) {
                summary += "\n";
                for (int i = 0; i < tile.normalTraits.Count; i++) {
                    summary += "|" + tile.normalTraits[i].name + "|";
                }

            } else {
                summary += "None";
            }

            IPointOfInterest poi = tile.objHere;
            if (poi == null) {
                poi = tile.genericTileObject;
            }
            summary += "\nContent: " + poi ?? "None";
            if (poi != null) {
                summary += "\nHP: " + poi.currentHP + "/" + poi.maxHP;
                summary += "\n\tObject State: " + poi.state;
                summary += "\n\tIs Available: " + poi.IsAvailable();

                if (poi is TreeObject) {
                    summary += "\n\tYield: " + (poi as TreeObject).yield;
                } else if (poi is Ore) {
                    summary += "\n\tYield: " + (poi as Ore).yield;
                } else if (poi is ResourcePile) {
                    summary += "\n\tResource in Pile: " + (poi as ResourcePile).resourceInPile;
                }  else if (poi is Table) {
                    summary += "\n\tFood in Table: " + (poi as Table).food;
                } else if (poi is SpecialToken) {
                    summary += "\n\tCharacter Owner: " + (poi as SpecialToken).characterOwner?.name ?? "None";
                    summary += "\n\tFaction Owner: " + (poi as SpecialToken).factionOwner?.name ?? "None";
                }
                summary += "\n\tAdvertised Actions: ";
                if (poi.advertisedActions.Count > 0) {
                    for (int i = 0; i < poi.advertisedActions.Count; i++) {
                        summary += "|" + poi.advertisedActions[i] + "|";
                    }
                } else {
                    summary += "None";
                }
                summary += "\n\tObject Traits: ";
                if (poi.traitContainer.allTraits.Count > 0) {
                    for (int i = 0; i < poi.traitContainer.allTraits.Count; i++) {
                        summary += "\n\t\t- " + poi.traitContainer.allTraits[i].name + " - " + poi.traitContainer.allTraits[i].GetTestingData();
                    }
                } else {
                    summary += "None";
                }
                summary += "\n\tJobs Targeting this: ";
                if (poi.allJobsTargettingThis.Count > 0) {
                    for (int i = 0; i < poi.allJobsTargettingThis.Count; i++) {
                        summary += "\n\t\t- " + poi.allJobsTargettingThis[i];
                    }
                } else {
                    summary += "None";
                }
            }
            if (tile.structure != null) {
                summary += "\nStructure: " + tile.structure + ", Has Owner: " + tile.structure.IsOccupied();
                summary += "\nCharacters at " + tile.structure + ": ";
                if (tile.structure.charactersHere.Count > 0) {
                    for (int i = 0; i < tile.structure.charactersHere.Count; i++) {
                        Character currCharacter = tile.structure.charactersHere[i];
                        if (character == currCharacter) {
                            summary += "\n<b>" + currCharacter.name + "</b>";
                            summary += "\n\t" + GetCharacterHoverData(currCharacter) + "\n";
                        } else {
                            summary += currCharacter.name + ",";
                        }
                    }
                } else {
                    summary += "None";
                }
            
            } else {
                summary += "\nStructure: None";
            }
            UIManager.Instance.ShowSmallInfo(summary);
#else
         //For build only
        if (tile.objHere != null) {
            UIManager.Instance.ShowSmallInfo(tile.objHere.ToString());
        }
#endif
        }
        public void ShowCharacterData(Character character) {
            string summary = "<b>" + character.name + "</b>";
            summary += "\n\t" + GetCharacterHoverData(character) + "\n";
            UIManager.Instance.ShowSmallInfo(summary);
        }
        private string GetCharacterHoverData(Character character) {
            Character activeCharacter = UIManager.Instance.characterInfoUI.activeCharacter;
            string summary = "Character: " + character.name;
            summary += "\n<b>Mood:</b>" + character.currentMoodType;
            summary += " <b>Supply:</b>" + character.supply;
            summary += " <b>Can Move:</b>" + character.canMove;
            summary += " <b>Can Witness:</b>" + character.canWitness;
            summary += " <b>Can Be Attacked:</b>" + character.canBeAtttacked;
            summary += " <b>Move Speed:</b>" + character.marker.pathfindingAI.speed;
            summary += " <b>Attack Range:</b>" + character.characterClass.attackRange;
            summary += " <b>Attack Speed:</b>" + character.attackSpeed;
            summary += " <b>Target POI:</b>" + (character.marker.targetPOI?.name ?? "None");
            summary += " <b>Base Structure:</b>" + (character.trapStructure.structure != null ? character.trapStructure.structure.ToString() : "None");
            //if (activeCharacter != null && activeCharacter != character) {
            //    summary += "\n\tOpinion of " + activeCharacter.name + ":";
            //    if (activeCharacter.opinionComponent.HasOpinion(character)) {
            //        Dictionary<string, int> opinion = activeCharacter.opinionComponent.GetOpinion(character);
            //        foreach (KeyValuePair<string, int> kvp in opinion) {
            //            summary += "\n\t\t" + kvp.Key + ": " + kvp.Value;
            //        }
            //    } else {
            //        summary += " None";
            //    }
            //}
            summary += "\n\tDestination Tile: ";
            if (character.marker.destinationTile == null) {
                summary += "None";
            } else {
                summary += character.marker.destinationTile + " at " + character.marker.destinationTile.parentMap.location.name;
            }
            summary += "\n\tPOI's in Vision: ";
            if (character.marker.inVisionPOIs.Count > 0) {
                for (int i = 0; i < character.marker.inVisionPOIs.Count; i++) {
                    IPointOfInterest poi = character.marker.inVisionPOIs[i];
                    summary += poi.name + ", ";
                }
            } else {
                summary += "None";
            }
            summary += "\n\tCharacters in Vision: ";
            if (character.marker.inVisionCharacters.Count > 0) {
                for (int i = 0; i < character.marker.inVisionCharacters.Count; i++) {
                    Character poi = character.marker.inVisionCharacters.ElementAt(i);
                    summary += poi.name + ", ";
                }
            } else {
                summary += "None";
            }
            summary += "\n\tPOI's in Range but different structures: ";
            if (character.marker.visionCollision.poisInRangeButDiffStructure.Count > 0) {
                for (int i = 0; i < character.marker.visionCollision.poisInRangeButDiffStructure.Count; i++) {
                    IPointOfInterest poi = character.marker.visionCollision.poisInRangeButDiffStructure[i];
                    summary += poi.name + ", ";
                }
            } else {
                summary += "None";
            }
            summary += "\n\tHostiles in Range: ";
            if (character.marker.hostilesInRange.Count > 0) {
                for (int i = 0; i < character.marker.hostilesInRange.Count; i++) {
                    IPointOfInterest poi = character.marker.hostilesInRange[i];
                    summary += poi.name + ", ";
                }
            } else {
                summary += "None";
            }
            summary += "\n\tAvoid in Range: ";
            if (character.marker.avoidInRange.Count > 0) {
                for (int i = 0; i < character.marker.avoidInRange.Count; i++) {
                    IPointOfInterest poi = character.marker.avoidInRange[i];
                    summary += poi.name + ", ";
                }
            } else {
                summary += "None";
            }
            summary += "\n\tTerrifying Characters: ";
            if (character.marker.terrifyingObjects.Count > 0) {
                for (int i = 0; i < character.marker.terrifyingObjects.Count; i++) {
                    IPointOfInterest currObj = character.marker.terrifyingObjects[i];
                    summary += currObj.name + ", ";
                }
            } else {
                summary += "None";
            }
            summary += "\n\tPersonal Job Queue: ";
            if (character.jobQueue.jobsInQueue.Count > 0) {
                for (int i = 0; i < character.jobQueue.jobsInQueue.Count; i++) {
                    JobQueueItem poi = character.jobQueue.jobsInQueue[i];
                    summary += poi + ", ";
                }
            } else {
                summary += "None";
            }
            return summary;
        }
        #endregion

        #region Town Map Generation
        public TownMapSettings GetTownMapSettings(Dictionary<int, Dictionary<int, LocationGridTileSettings>> allSettings) {
            int minX;
            int maxX;
            int minY;
            int maxY;
            GetBounds(allSettings, out minX, out maxX, out minY, out maxY);
            TownMapSettings s = new TownMapSettings();
            int shiftXBy = 0; //shift x position of all objects by n
            int shiftYBy = 0;//shift y position of all objects by n
            if (minX != 0) { shiftXBy = minX * -1; }
            if (minY != 0) { shiftYBy = minY * -1; }

            Dictionary<int, Dictionary<int, LocationGridTileSettings>> shiftedSettings = ShiftSettingsBy(new Vector2Int(shiftXBy, shiftYBy), allSettings);
            GetBounds(shiftedSettings, out minX, out maxX, out minY, out maxY);

            if (minX < 0 || minY < 0) {
                throw new System.Exception("Minimum bounds of shifted settings has negative value! X: " + minX.ToString() + ", Y: " + minY.ToString());
            }
            s.size = new Point(maxX, maxY);

            List<TileTemplateData> groundTiles = new List<TileTemplateData>();
            List<TileTemplateData> groundWallTiles = new List<TileTemplateData>();
            List<TileTemplateData> structureTiles = new List<TileTemplateData>();
            List<TileTemplateData> objectTiles = new List<TileTemplateData>();
            List<TileTemplateData> detailTiles = new List<TileTemplateData>();
            List<BuildingSpotData> buildingSpotData = new List<BuildingSpotData>();

            int count = 0;
            for (int x = 0; x < maxX; x++) {
                for (int y = 0; y < maxY; y++) {
                    if (shiftedSettings.ContainsKey(x) && shiftedSettings[x].ContainsKey(y)) {
                        LocationGridTileSettings currSetting = shiftedSettings[x][y];
                        currSetting.UpdatePositions(new Vector3(x, y, 0f));
                        groundTiles.Add(currSetting.groundTile);
                        groundWallTiles.Add(currSetting.groundWallTile);
                        structureTiles.Add(currSetting.structureWallTile);
                        objectTiles.Add(currSetting.objectTile);
                        detailTiles.Add(currSetting.detailTile);
                        if (currSetting.hasBuildingSpot) {
                            buildingSpotData.Add(currSetting.buildingSpot);
                        }
                    } else {
                        TileTemplateData emptyData = TileTemplateData.Empty;
                        emptyData.tilePosition = new Vector3(x, y, 0);
                        groundTiles.Add(emptyData);
                        structureTiles.Add(emptyData);
                        objectTiles.Add(emptyData);
                        detailTiles.Add(emptyData);
                    }
                    count++;
                }
            }

            if (count != (maxX * maxY)) {
                throw new System.Exception("Total tiles are inconsistent with size! Count is: " + count.ToString() + ". MaxX is: " + maxX.ToString() + ". MaxY is: " + maxY.ToString());
            }

            foreach (KeyValuePair<int, Dictionary<int, LocationGridTileSettings>> kvp in shiftedSettings) {
                foreach (KeyValuePair<int, LocationGridTileSettings> kvp2 in kvp.Value) {
                    LocationGridTileSettings currSetting = kvp2.Value;
                    currSetting.UpdatePositions(new Vector3(kvp.Key, kvp2.Key, 0f));
                    groundTiles.Add(currSetting.groundTile);
                    groundWallTiles.Add(currSetting.groundWallTile);
                    structureTiles.Add(currSetting.structureWallTile);
                    objectTiles.Add(currSetting.objectTile);
                    detailTiles.Add(currSetting.detailTile);
                    count++;
                }
            }

            s.groundTiles = groundTiles.ToArray();
            s.groundWallTiles = groundWallTiles.ToArray();
            s.structureTiles = structureTiles.ToArray();
            s.objectTiles = objectTiles.ToArray();
            s.detailTiles = detailTiles.ToArray();
            s.buildSpots = buildingSpotData;
            return s;
        }
        private void GetBounds(Dictionary<int, Dictionary<int, LocationGridTileSettings>> allSettings, out int minX, out int maxX, out int minY, out int maxY) {
            minX = 99999;
            maxX = 0;
            minY = 99999;
            maxY = 0;
            foreach (KeyValuePair<int, Dictionary<int, LocationGridTileSettings>> kvp in allSettings) {
                if (kvp.Key < minX) { minX = kvp.Key; }
                else if (kvp.Key > maxX) { maxX = kvp.Key; }
                foreach (KeyValuePair<int, LocationGridTileSettings> kvp2 in kvp.Value) {
                    if (kvp2.Key < minY) { minY = kvp2.Key; } 
                    else if (kvp2.Key > maxY) { maxY = kvp2.Key; }
                }
            }
            maxX += 1;
            maxY += 1; //because collections start at 0, and I need the max length of the collections.
        }
        private Dictionary<int, Dictionary<int, LocationGridTileSettings>> ShiftSettingsBy(Vector2Int shiftBy, Dictionary<int, Dictionary<int, LocationGridTileSettings>> settings) {
            Dictionary<int, Dictionary<int, LocationGridTileSettings>> shifted = new Dictionary<int, Dictionary<int, LocationGridTileSettings>>();
            foreach (KeyValuePair<int, Dictionary<int, LocationGridTileSettings>> kvp in settings) {
                foreach (KeyValuePair<int, LocationGridTileSettings> kvp2 in kvp.Value) {
                    int shiftedX = kvp.Key + shiftBy.x;
                    int shiftedY = kvp2.Key + shiftBy.y;
                    LocationGridTileSettings currSetting = kvp2.Value;
                    if (!shifted.ContainsKey(shiftedX)) {
                        shifted.Add(shiftedX, new Dictionary<int, LocationGridTileSettings>());
                    }
                    shifted[shiftedX].Add(shiftedY, currSetting);
                }
            }
            return shifted;
        }
        public Dictionary<int, Dictionary<int, LocationGridTileSettings>> GenerateTownCenterTemplateForGeneration(StructureTemplate template, Vector3Int startPos) {
            Dictionary<int, Dictionary<int, LocationGridTileSettings>> generated = new Dictionary<int, Dictionary<int, LocationGridTileSettings>>();
            for (int i = 0; i < template.groundTiles.Length; i++) {
                TileTemplateData ground = template.groundTiles[i];
                Vector3 tilePos = ground.tilePosition;

                BuildingSpotData buildingSpot;
                bool hasBuildingSpot = template.TryGetBuildingSpotDataAtLocation(tilePos, out buildingSpot);

                tilePos.x += startPos.x;
                tilePos.y += startPos.y;
                TileTemplateData detail = template.detailTiles[i];
                TileTemplateData groundWall;
                if (template.groundWallTiles != null) {
                    groundWall = template.groundWallTiles[i];
                } else {
                    groundWall = TileTemplateData.Empty;
                }
            
                TileTemplateData structureWall = template.structureWallTiles[i];
                TileTemplateData obj = template.objectTiles[i];
                if (!generated.ContainsKey((int)tilePos.x)) {
                    generated.Add((int)tilePos.x, new Dictionary<int, LocationGridTileSettings>());
                }
                generated[(int)tilePos.x].Add((int)tilePos.y, new LocationGridTileSettings() {
                    groundTile = ground,
                    groundWallTile = groundWall,
                    detailTile = detail,
                    structureWallTile = structureWall,
                    objectTile = obj,
                    hasBuildingSpot = hasBuildingSpot,
                    buildingSpot = buildingSpot
                });
            }
            return generated;

        }
        #endregion

        #region Tile Object
        public bool HasSettingForTileObjectAsset(Sprite asset) {
            return tileObjectSlotSettings.ContainsKey(asset);
        }
        /// <summary>
        /// Get the slot settings for a given tile object asset.
        /// NOTE: should be used in conjunction with <see cref="HasSettingForTileObjectAsset"/> to check if any settings are available, since TileObjectSettings cannot be null.
        /// </summary>
        /// <param name="asset">The asset used by the tile object.</param>
        /// <returns>The list of slot settings</returns>
        public List<TileObjectSlotSetting> GetTileObjectSlotSettings(Sprite asset) {
            return tileObjectSlotSettings[asset];
        }
        public void AddTileObject(TileObject to) {
            if (!allTileObjects.ContainsKey(to.tileObjectType)) {
                allTileObjects.Add(to.tileObjectType, new List<TileObject>());
            }
            if (!allTileObjects[to.tileObjectType].Contains(to)) {
                allTileObjects[to.tileObjectType].Add(to);
            }
        }
        public void RemoveTileObject(TileObject to) {
            if (allTileObjects.ContainsKey(to.tileObjectType)) {
                allTileObjects[to.tileObjectType].Remove(to);
            }
        }
        public TileObject GetTileObject(TILE_OBJECT_TYPE type, int id) {
            if (allTileObjects.ContainsKey(type)) {
                for (int i = 0; i < allTileObjects[type].Count; i++) {
                    TileObject to = allTileObjects[type][i];
                    if(to.id == id) {
                        return to;
                    }
                }
            }
            return null;
        }
        public T CreateNewTileObject<T>(TILE_OBJECT_TYPE tileObjectType) where T : TileObject {
            var typeName = Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(tileObjectType.ToString());
            System.Type type = System.Type.GetType(typeName);
            if (type != null) {
                T obj = System.Activator.CreateInstance(type) as T;
                return obj;
            }
            throw new System.Exception("Could not create new instance of tile object of type " + tileObjectType.ToString());
        }
        #endregion

        #region Lighting
        public void UpdateLightBasedOnTime(GameDate date) {
            foreach (KeyValuePair<int, float> keyValuePair in lightSettings) {
                if (date.tick > keyValuePair.Key) {
                    areaMapLight.intensity = keyValuePair.Value;
                }
            }
        }
        private void CheckForChangeLight() {
            if (lightSettings.ContainsKey(GameManager.Instance.tick)) {
                StartCoroutine(TransitionLightTo(lightSettings[GameManager.Instance.tick]));
            }
        }
        private IEnumerator TransitionLightTo(float intensity) {
            while (true) {
                if (GameManager.Instance.isPaused) {
                    yield return null;
                }
                if (intensity > areaMapLight.intensity) {
                    areaMapLight.intensity += 0.05f;
                } else if (intensity < areaMapLight.intensity) {
                    areaMapLight.intensity -= 0.05f;
                }
                if (Mathf.Approximately(areaMapLight.intensity, intensity)) {
                    break;
                }
                yield return new WaitForSeconds(0.1f);
            }
        }
        #endregion

        #region Structures
        public List<GameObject> GetStructurePrefabsForStructure(STRUCTURE_TYPE type) {
            return structurePrefabs[type];
        }
        #endregion

        #region Assets
        public Sprite GetItemAsset(SPECIAL_TOKEN itemType) {
            return itemTiles[itemType];
        }
        public Sprite GetTileObjectAsset(TILE_OBJECT_TYPE objectType, POI_STATE state, BIOMES biome) {
            TileObjectTileSetting setting = tileObjectTiles[objectType];
            BiomeTileObjectTileSetting biomeSetting;
            if (setting.biomeAssets.ContainsKey(biome)) {
                biomeSetting = setting.biomeAssets[biome];
            } else {
                biomeSetting = setting.biomeAssets[BIOMES.NONE];
            }
            if (state == POI_STATE.ACTIVE) {
                return biomeSetting.activeTile;
            } else {
                return biomeSetting.inactiveTile;
            }
        
        }
        public WallAsset GetWallAsset(RESOURCE wallResource, string assetName) {
            return wallResourceAssets[wallResource].GetWallAsset(assetName);
        }
        #endregion
    }
}