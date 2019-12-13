using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Events.World_Events;
using Inner_Maps;
using UnityEngine;
using Random = UnityEngine.Random;

public class Region : ILocation {

    private const float HoveredBorderAlpha = 0f / 255f;
    private const float UnhoveredBorderAlpha = 0f / 255f;

    public int id { get; private set; }
    public string name { get; private set; }
    public string description => GetDescription();
    public List<HexTile> tiles { get; private set; }
    public HexTile coreTile { get; private set; }
    public LOCATION_TYPE locationType {
        get {
            if (area != null) {
                return area.locationType;
            }
            return LOCATION_TYPE.EMPTY;
        }
    }
    public Area area { get; private set; }
    public Color regionColor { get; private set; }
    public List<Region> connections { get; private set; }
    public Minion assignedMinion { get; private set; }
    public Faction owner { get; private set; }
    public Faction previousOwner { get; private set; }
    public List<Faction> factionsHere { get; private set; }
    public List<Character> residents { get; private set; }
    public DemonicLandmarkBuildingData demonicBuildingData { get; private set; }
    public DemonicLandmarkInvasionData demonicInvasionData { get; private set; }
    public WorldEvent activeEvent { get; private set; }
    public Character eventSpawnedBy { get; private set; }
    public IWorldEventData eventData { get; private set; }
    public GameObject eventIconGo { get; private set; }
    public List<Character> charactersAtLocation { get; private set; }
    public List<RegionFeature> features { get; private set; }
    public InnerTileMap innerMap {
        get {
            if (area != null) {
                return area.areaMap;
            }
            return _regionInnerTileMap;
        }
    }
    public RegionTileObject regionTileObject { get; private set; }
    

    private RegionInnerTileMap _regionInnerTileMap; //inner map of the region, this should only be used if this region does not have an area. 
    private List<System.Action> _otherAfterInvasionActions; //list of other things to do when this landmark is invaded.
    private string _activeEventAfterEffectScheduleId;
    private List<SpriteRenderer> _borderSprites;
    private Dictionary<STRUCTURE_TYPE, List<LocationStructure>> _structures;

    public Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> awareness { get; private set; }

    #region getter/setter
    public BaseLandmark mainLandmark {
        get { return coreTile.landmarkOnTile; }
    }
    public Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures {
        get {
            if(area != null) { return area.structures; }
            return _structures;
        }
    }
    public LocationStructure mainStorage {
        get {
            if (area != null) { return area.mainStorage; }
            return structures.First().Value.First();
        }
    }
    #endregion

    private Region() {
        connections = new List<Region>();
        charactersAtLocation = new List<Character>();
        _otherAfterInvasionActions = new List<System.Action>();
        factionsHere = new List<Faction>();
        features = new List<RegionFeature>();
        residents = new List<Character>();
        awareness = new Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>>();
    }
    public Region(HexTile coreTile) : this() {
        id = Utilities.SetID(this);
        name = RandomNameGenerator.Instance.GetRegionName();
        this.coreTile = coreTile;
        tiles = new List<HexTile>();
        AddTile(coreTile);
        regionColor = Random.ColorHSV();
    }
    public Region(SaveDataRegion data) : this() {
        id = Utilities.SetID(this, data.id);
        name = data.name;
        coreTile = GridMap.Instance.hexTiles[data.coreTileID];
        tiles = new List<HexTile>();
        regionColor = data.regionColor;
    }

    public void SetName(string name) {
        this.name = name;
    }
    public void AddTile(HexTile tile) {
        if (!tiles.Contains(tile)) {
            tiles.Add(tile);
            tile.SetRegion(this);
        }
    }
    public void AddTile(List<HexTile> tiles) {
        for (int i = 0; i < tiles.Count; i++) {
            AddTile(tiles[i]);
        }
    }
    public void OnMainLandmarkChanged() {
        regionTileObject?.UpdateAdvertisements(this);
    }

    #region Utilities
    private string GetDescription() {
        if (coreTile.isCorrupted) {
            if (HasFeature(RegionFeatureDB.Hallowed_Ground_Feature)) {
                return "This region has a Hallowed Ground. You cannot build a demonic landmark here until you have defiled it.";
            } else if (mainLandmark.specificLandmarkType == LANDMARK_TYPE.NONE) {
                return "This region is empty. You may assign a minion to build a demonic landmark here.";
            }
        }
        return LandmarkManager.Instance.GetLandmarkData(mainLandmark.specificLandmarkType).description;
    }
    public void FinalizeData() {
        //outerTiles = GetOuterTiles();
        _borderSprites = GetOuterBorders();
    }
    public void RedetermineCore() {
        int maxX = tiles.Max(t => t.data.xCoordinate);
        int minX = tiles.Min(t => t.data.xCoordinate);
        int maxY = tiles.Max(t => t.data.yCoordinate);
        int minY = tiles.Min(t => t.data.yCoordinate);

        int x = (minX + maxX) / 2;
        int y = (minY + maxY) / 2;

        //coreTile.spriteRenderer.color = regionColor;

        coreTile = GridMap.Instance.map[x, y];
        //coreTile.spriteRenderer.color = Color.white;

        if (!tiles.Contains(coreTile)) {
            throw new System.Exception("Region does not contain new core tile! " + coreTile.ToString());
        }
    }
    /// <summary>
    /// Get the outer tiles of this region. NOTE: Made this into a getter instead of saving it in a variable, to save memory.
    /// </summary>
    /// <returns>List of outer tiles.</returns>
    private List<HexTile> GetOuterTiles() {
        List<HexTile> outerTiles = new List<HexTile>();
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currTile = tiles[i];
            if (currTile.AllNeighbours.Count != 6 || currTile.HasNeighbourFromOtherRegion()) {
                outerTiles.Add(currTile);
            }
        }
        return outerTiles;
    }
    private List<SpriteRenderer> GetOuterBorders() {
        List<HexTile> outerTiles = GetOuterTiles();
        List<SpriteRenderer> borders = new List<SpriteRenderer>();
        HEXTILE_DIRECTION[] dirs = Utilities.GetEnumValues<HEXTILE_DIRECTION>();
        for (int i = 0; i < outerTiles.Count; i++) {
            HexTile currTile = outerTiles[i];
            for (int j = 0; j < dirs.Length; j++) {
                HEXTILE_DIRECTION dir = dirs[j];
                if (dir == HEXTILE_DIRECTION.NONE) { continue; }
                HexTile neighbour = currTile.GetNeighbour(dir);
                if (neighbour == null || neighbour.region != currTile.region) {
                    SpriteRenderer border = currTile.GetBorder(dir);
                    //currTile.SetBorderColor(regionColor);
                    borders.Add(border);
                }
            }
        }
        return borders;
    }
    public List<Region> AdjacentRegions() {
        List<Region> adjacent = new List<Region>();
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currTile = tiles[i];
            List<Region> regions;
            if (currTile.TryGetDifferentRegionNeighbours(out regions)) {
                for (int j = 0; j < regions.Count; j++) {
                    Region currRegion = regions[j];
                    if (!adjacent.Contains(currRegion)) {
                        adjacent.Add(currRegion);
                    }
                }
            }
        }
        return adjacent;
    }
    public void OnHoverOverAction() {
        ShowSolidBorder();
    }
    public void OnHoverOutAction() {
        if (UIManager.Instance.regionInfoUI.isShowing) {
            if (UIManager.Instance.regionInfoUI.activeRegion != this) {
                ShowTransparentBorder();
            }
        } else {
            ShowTransparentBorder();
        }

    }
    public void ShowSolidBorder() {
        for (int i = 0; i < _borderSprites.Count; i++) {
            SpriteRenderer s = _borderSprites[i];
            Color color = s.color;
            color.a = HoveredBorderAlpha;
            s.color = color;
            s.gameObject.SetActive(true);
        }
    }
    public void ShowTransparentBorder() {
        for (int i = 0; i < _borderSprites.Count; i++) {
            SpriteRenderer s = _borderSprites[i];
            Color color = s.color;
            color.a = UnhoveredBorderAlpha;
            s.color = color;
            s.gameObject.SetActive(true);
        }
    }
    public void CenterCameraOnRegion() {
        coreTile.CenterCameraHere();
    }
    #endregion

    #region Invasion
    public bool CanBeInvaded() {
        if (area != null) {
            return area.CanInvadeSettlement();
        }
        return HasCorruptedConnection() && !coreTile.isCorrupted && !demonicInvasionData.beingInvaded;
    }
    public void StartInvasion(Minion assignedMinion) {
        //PlayerManager.Instance.player.SetInvadingRegion(this);
        assignedMinion.SetAssignedRegion(this);
        SetAssignedMinion(assignedMinion);

        demonicInvasionData = new DemonicLandmarkInvasionData() {
            beingInvaded = true,
            currentDuration = 0,
        };

        //ticksInInvasion = 0;
        Messenger.AddListener(Signals.TICK_STARTED, PerInvasionTick);
        TimerHubUI.Instance.AddItem("Invasion of " + (mainLandmark.tileLocation.areaOfTile != null ? mainLandmark.tileLocation.areaOfTile.name : name), mainLandmark.invasionTicks, () => UIManager.Instance.ShowRegionInfo(this));
    }
    public void LoadInvasion(SaveDataRegion data) {
        //PlayerManager.Instance.player.SetInvadingRegion(this);
        //assignedMinion.SetAssignedRegion(this);
        //SetAssignedMinion(assignedMinion);

        demonicInvasionData = data.demonicInvasionData;
        if (demonicInvasionData.beingInvaded) {
            Messenger.AddListener(Signals.TICK_STARTED, PerInvasionTick);
            TimerHubUI.Instance.AddItem("Invasion of " + (mainLandmark.tileLocation.areaOfTile != null ? mainLandmark.tileLocation.areaOfTile.name : name), mainLandmark.invasionTicks - demonicInvasionData.currentDuration, () => UIManager.Instance.ShowRegionInfo(this));
        }
    }
    private void PerInvasionTick() {
        DemonicLandmarkInvasionData tempData = demonicInvasionData;
        tempData.currentDuration++;
        demonicInvasionData = tempData;
        if (demonicInvasionData.currentDuration > mainLandmark.invasionTicks) {
            //invaded.
            Invade();
            UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "You have successfully invaded " + this.name, () => UIManager.Instance.ShowRegionInfo(this));
            Messenger.RemoveListener(Signals.TICK_STARTED, PerInvasionTick);
        }
    }
    private void Invade() {
        //corrupt region
        InvadeActions();
        LandmarkManager.Instance.OwnRegion(PlayerManager.Instance.player.playerFaction, RACE.DEMON, this);
        //PlayerManager.Instance.AddTileToPlayerArea(coreTile);
        //PlayerManager.Instance.player.SetInvadingRegion(null);
        demonicInvasionData = new DemonicLandmarkInvasionData();
        assignedMinion.SetAssignedRegion(null);
        SetAssignedMinion(null);

        //This is done so that when a region is invaded by the player, the showing Info UI will update appropriately
        if (UIManager.Instance.regionInfoUI.isShowing && UIManager.Instance.regionInfoUI.activeRegion == this) {
            UIManager.Instance.ShowRegionInfo(this);
        }
    }
    public void SetAssignedMinion(Minion minion) {
        Minion previouslyAssignedMinion = assignedMinion;
        assignedMinion = minion;
        if (assignedMinion != null) {
            AddCharacterToLocation(assignedMinion.character);
            mainLandmark.OnMinionAssigned(assignedMinion); //a new minion was assigned 
        } else if (previouslyAssignedMinion != null) {
            RemoveCharacterFromLocation(previouslyAssignedMinion.character);
            mainLandmark.OnMinionUnassigned(previouslyAssignedMinion); //a minion was unassigned
        }
    }
    #endregion

    #region Player Build Structure
    public void StartBuildingStructure(LANDMARK_TYPE landmarkType, Minion minion) {
        SetAssignedMinion(minion);
        minion.SetAssignedRegion(this);
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(landmarkType);
        demonicBuildingData = new DemonicLandmarkBuildingData() {
            landmarkType = landmarkType,
            landmarkName = landmarkData.landmarkTypeString,
            buildDuration = landmarkData.buildDuration + Mathf.RoundToInt(landmarkData.buildDuration * PlayerManager.Instance.player.constructionRatePercentageModifier),
            currentDuration = 0,
        };
        coreTile.UpdateBuildSprites();
        TimerHubUI.Instance.AddItem("Building " + demonicBuildingData.landmarkName + " at " + name, demonicBuildingData.buildDuration, () => UIManager.Instance.ShowRegionInfo(this));
        Messenger.AddListener(Signals.TICK_STARTED, PerTickBuilding);
    }
    public void LoadBuildingStructure(SaveDataRegion data) {
        demonicBuildingData = data.demonicBuildingData;
        if(demonicBuildingData.landmarkType != LANDMARK_TYPE.NONE) {
            TimerHubUI.Instance.AddItem("Building " + demonicBuildingData.landmarkName + " at " + name, demonicBuildingData.buildDuration - demonicBuildingData.currentDuration, () => UIManager.Instance.ShowRegionInfo(this));
            Messenger.AddListener(Signals.TICK_STARTED, PerTickBuilding);
        }
    }
    private void PerTickBuilding() {
        DemonicLandmarkBuildingData tempData = demonicBuildingData;
        tempData.currentDuration++;
        demonicBuildingData = tempData;
        if (demonicBuildingData.currentDuration >= demonicBuildingData.buildDuration) {
            FinishBuildingStructure();
        }
    }
    private void FinishBuildingStructure() {
        Messenger.RemoveListener(Signals.TICK_STARTED, PerTickBuilding);
        //mainLandmark.ChangeLandmarkType(demonicBuildingData.landmarkType);
        //int previousID = mainLandmark.id;
        BaseLandmark newLandmark = LandmarkManager.Instance.CreateNewLandmarkOnTile(coreTile, demonicBuildingData.landmarkType, false);
        //newLandmark.OverrideID(previousID);

        UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Finished building " + Utilities.NormalizeStringUpperCaseFirstLetters(newLandmark.specificLandmarkType.ToString()) + " at " + this.name, () => UIManager.Instance.ShowRegionInfo(this));
        demonicBuildingData = new DemonicLandmarkBuildingData();
        //assignedMinion.SetAssignedRegion(null);
        //SetAssignedMinion(null);

        newLandmark.OnFinishedBuilding();
        coreTile.UpdateBuildSprites();
        Messenger.Broadcast(Signals.AREA_INFO_UI_UPDATE_APPROPRIATE_CONTENT, this);
    }
    #endregion

    #region Connections
    public void AddConnection(Region newConnection) {
        connections.Add(newConnection);
    }
    public bool IsConnectedWith(Region region) {
        for (int i = 0; i < connections.Count; i++) {
            Region connection = connections[i];
            if (connection == region) {
                return true;
            }
        }
        return false;
    }
    public bool HasMaximumConnections() {
        return connections.Count >= LandmarkManager.Max_Connections;
    }
    /// <summary>
    /// Is this landmark connected to another landmark that has been corrupted?
    /// </summary>
    /// <returns>True or false</returns>
    public bool HasCorruptedConnection() {
        for (int i = 0; i < connections.Count; i++) {
            Region connection = connections[i];
            if (connection.coreTile.isCorrupted) {
                return true;
            }
        }
        return false;
    }
    public bool HasSettlementConnection() {
        for (int i = 0; i < connections.Count; i++) {
            Region connection = connections[i];
            if (connection.area != null) {
                return true;
            }
        }
        return false;
    }
    public bool HasSettlementOrCorruptedConnection() {
        for (int i = 0; i < connections.Count; i++) {
            Region connection = connections[i];
            if (connection.area != null || connection.coreTile.isCorrupted) {
                return true;
            }
        }
        return false;
    }
    public bool IsConnectedToRegionOwnedBy(Faction faction) {
        for (int i = 0; i < connections.Count; i++) {
            Region region = connections[i];
            if (region.owner == faction) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Corruption/Invasion
    public void InvadeActions() {
        mainLandmark?.ChangeLandmarkType(LANDMARK_TYPE.NONE);
        ActivateRegionFeatures();
        RemoveFeaturesAfterInvade();
        ExecuteEventAfterInvasion();
        ExecuteOtherAfterInvasionActions();
    }
    private void ExecuteOtherAfterInvasionActions() {
        for (int i = 0; i < _otherAfterInvasionActions.Count; i++) {
            _otherAfterInvasionActions[i].Invoke();
        }
        _otherAfterInvasionActions.Clear();
    }
    public void AddAfterInvasionAction(System.Action action) {
        _otherAfterInvasionActions.Add(action);
    }
    #endregion

    #region Events
    public void SpawnEvent(WorldEvent we, Character spawner) {
        activeEvent = we;
        //set character that spawned event
        SetCharacterEventSpawner(spawner);
        eventData = activeEvent.ConstructEventDataForLandmark(this);
        for (int i = 0; i < eventData.involvedCharacters.Length; i++) {
            Character currCharacter = eventData.involvedCharacters[i];
            //do not let the character that spawned the event go home
            if (currCharacter.stateComponent.currentState is MoveOutState) {
                MoveOutState state = currCharacter.stateComponent.currentState as MoveOutState;
                Debug.Log(GameManager.Instance.TodayLogString() + "Removing go home schedule of " + currCharacter.name);
                SchedulingManager.Instance.RemoveSpecificEntry(state.goHomeSchedID);
            } else {
                throw new System.Exception(currCharacter.name + " is at " + name + " but is not in move out state!");

            }
        }
        //spawn the event
        activeEvent.Spawn(this, spawner, eventData, out _activeEventAfterEffectScheduleId);
        Messenger.Broadcast(Signals.WORLD_EVENT_SPAWNED, this, we);
    }
    public void LoadEvent(SaveDataRegion data) {
        if (data.activeEvent != WORLD_EVENT.NONE) {
            activeEvent = WorldEventsManager.Instance.GetWorldEvent(data.activeEvent);
            Character spawner = CharacterManager.Instance.GetCharacterByID(data.eventSpawnedByCharacterID);
            SetCharacterEventSpawner(spawner);
            eventData = data.eventData.Load();
            activeEvent.Load(this, spawner, eventData, out _activeEventAfterEffectScheduleId);
            Messenger.Broadcast(Signals.WORLD_EVENT_SPAWNED, this, activeEvent);
        }
    }
    public void SetCharacterEventSpawner(Character character) {
        eventSpawnedBy = character;
    }
    private void SpawnEventThatCanProvideEffectFor(WORLD_EVENT_EFFECT[] effects, Character spawner) {
        List<WorldEvent> events = WorldEventsManager.Instance.GetEventsThatCanProvideEffects(this, spawner, effects);
        if (events.Count > 0) {
            WorldEvent chosenEvent = events[Random.Range(0, events.Count)];
            SpawnEvent(chosenEvent, spawner);
        } else {
            Debug.LogWarning($"{GameManager.Instance.TodayLogString()}No spawnable events were found for {spawner.name} at {this.name}");
        }
    }
    public void WorldEventFinished(WorldEvent we) {
        if (activeEvent != we) {
            throw new System.Exception("World event " + we.name + " finished, but it is not the active event at " + this.name);
        }
        for (int i = 0; i < eventData.involvedCharacters.Length; i++) {
            Character currCharacter = eventData.involvedCharacters[i];
            //make characters involved in the event, go home
            if (currCharacter.minion == null) {
                (currCharacter.stateComponent.currentState as MoveOutState).GoHome();
            }
        }
        DespawnEvent();
        Messenger.Broadcast(Signals.WORLD_EVENT_FINISHED_NORMAL, this, we);
    }
    public void WorldEventFailed(WorldEvent we) {
        if (activeEvent != we) {
            throw new System.Exception("World event " + we.name + " finished, but it is not the active event at " + this.name);
        }
        for (int i = 0; i < eventData.involvedCharacters.Length; i++) {
            Character currCharacter = eventData.involvedCharacters[i];
            //make characters involved in the event, go home
            if (currCharacter.minion == null && !currCharacter.isDead && currCharacter.stateComponent.currentState is MoveOutState) {
                (currCharacter.stateComponent.currentState as MoveOutState).GoHome();
            }
        }
        DespawnEvent();
        Messenger.Broadcast(Signals.WORLD_EVENT_FAILED, this, we);
    }
    private void DespawnEvent() {
        WorldEvent despawned = activeEvent;
        eventData.interferingCharacter?.minion.SetAssignedRegion(null); //return interfering character
        activeEvent = null;
        eventData = null;
        despawned.OnDespawn(this);
        Messenger.Broadcast(Signals.WORLD_EVENT_DESPAWNED, this, despawned);
    }
    private void ExecuteEventAfterInvasion() {
        if (activeEvent != null) {
            SchedulingManager.Instance.RemoveSpecificEntry(_activeEventAfterEffectScheduleId); //unschedule the active event after effect schedule
            activeEvent.ExecuteAfterInvasionEffect(this);
            DespawnEvent();
        }
        List<Character> nonMinionChaarcters = charactersAtLocation.Where(x => x.minion == null).ToList();
        //kill all remaining characters
        for (int i = 0; i < nonMinionChaarcters.Count; i++) {
            Character character = nonMinionChaarcters[i];
            character.Death("Invasion");
        }
    }
    public bool CanSpawnNewEvent() {
        return activeEvent == null;
    }
    /// <summary>
    /// Force the event to end regardless of its remaining duration.
    /// </summary>
    public void ForceResolveWorldEvent() {
        SchedulingManager.Instance.RemoveSpecificEntry(_activeEventAfterEffectScheduleId); //unschedule the active event after effect schedule
        activeEvent.TryExecuteAfterEffect(this, eventSpawnedBy);
    }
    public void SetEventIcon(GameObject go) {
        eventIconGo = go;
    }
    public void JobBasedEventGeneration(Character character) {
        //only trigger event generation if there is no active event and the character that arrived is not a minion
        if (activeEvent == null && character.minion == null) {
            if (character.stateComponent.currentState is MoveOutState) {
                WORLD_EVENT_EFFECT[] effects = WorldEventsManager.Instance.GetNeededEffectsOfJob(character.stateComponent.currentState.job.jobType);
                SpawnEventThatCanProvideEffectFor(effects, character);
            }
        }
    }
    #endregion

    #region Characters
    public void LoadCharacterHere(Character character) {
        charactersAtLocation.Add(character);
        character.SetRegionLocation(this);
        Messenger.Broadcast(Signals.CHARACTER_ENTERED_REGION, character, this);
        //if (area == null) {
        //    //JobBasedEventGeneration(character);
        //    Messenger.Broadcast(Signals.CHARACTER_ENTERED_REGION, character, this);
        //} else {
        //    //character.ownParty.SetSpecificLocation(area);
        //    Messenger.Broadcast(Signals.CHARACTER_ENTERED_AREA, area, character);
        //}
        //character.SetLandmarkLocation(this.mainLandmark);
        //Messenger.Broadcast(Signals.CHARACTER_ENTERED_REGION, character, this);
    }
    public void AddCharacterToLocation(Character character, LocationGridTile tileOverride = null, bool isInitial = false) {
        if (!charactersAtLocation.Contains(character)) {
            charactersAtLocation.Add(character);
            character.SetRegionLocation(this);
            Messenger.Broadcast(Signals.CHARACTER_ENTERED_REGION, character, this);
            //if (area == null) {
            //    Messenger.Broadcast(Signals.CHARACTER_ENTERED_REGION, character, this);
            //} else {
            //    //character.ownParty.SetSpecificLocation(area);
            //    Messenger.Broadcast(Signals.CHARACTER_ENTERED_AREA, area, character);
            //}
        }
    }
    public void RemoveCharacterFromLocation(Character character) {
        if (charactersAtLocation.Remove(character)) {
            if (character.currentStructure == null && area != null && !owner.isPlayerFaction) {
                throw new System.Exception(character.name + " doesn't have a current structure at " + name);
            }
            if (character.currentStructure != null && area == null) {
                throw new System.Exception(character.name + " has a current structure at a location which has no area: " + name);
            }
            if (character.currentStructure != null) {
                character.currentStructure.RemoveCharacterAtLocation(character);
            }
            character.SetRegionLocation(null);
            Messenger.Broadcast(Signals.CHARACTER_EXITED_REGION, character, this);
            //if (area == null) {
            //    character.SetRegionLocation(null);
            //    Messenger.Broadcast(Signals.CHARACTER_EXITED_REGION, character, this);
            //} else {
            //    if (character.currentStructure == null && !owner.isPlayerFaction) {
            //        throw new System.Exception(character.name + " doesn't have a current structure at " + area.name);
            //    }
            //    if (character.currentStructure != null) {
            //        character.currentStructure.RemoveCharacterAtLocation(character);

            //    }
            //    Messenger.Broadcast(Signals.CHARACTER_EXITED_AREA, area, character);
            //}
        }
    }
    public void RemoveCharacterFromLocation(Party party) {
        RemoveCharacterFromLocation(party.owner);
    }
    public bool IsResident(Character character) {
        return residents.Contains(character);
    }
    public bool AddResident(Character character, Dwelling chosenHome = null, bool ignoreCapacity = true) {
        if (!residents.Contains(character)) {
            if (!ignoreCapacity) {
                if (area != null && area.IsResidentsFull()) {
                    Debug.LogWarning(GameManager.Instance.TodayLogString() + "Cannot add " + character.name + " as resident of " + this.name + " because residency is already full!");
                    return false; //area is at capacity
                }
            }
            if (!CanCharacterBeAddedAsResidentBasedOnFaction(character)) {
                character.PrintLogIfActive(GameManager.Instance.TodayLogString() + character.name + " tried to become a resident of " + name + " but their factions conflicted");
                return false;
            }
            character.SetHome(this);
            residents.Add(character);
            if(area != null) {
                if(!coreTile.isCorrupted) {
                    area.classManager.OnAddResident(character);
                }
                area.AssignCharacterToDwellingInArea(character, chosenHome);
            }
            return true;
        }
        return false;
    }
    public void RemoveResident(Character character) {
        if (residents.Remove(character)) {
            character.SetHome(null);
            if (area != null && character.homeStructure != null && character.homeStructure.location == area) {
                character.homeStructure.RemoveResident(character);
            }
            //CheckForUnoccupancy();
            //Messenger.Broadcast(Signals.AREA_RESIDENT_REMOVED, this, character);
        }
    }
    private bool CanCharacterBeAddedAsResidentBasedOnFaction(Character character) {
        if (owner != null && character.faction != null) {
            //If character's faction is hostile with region's ruling faction, character cannot be a resident
            return !owner.IsHostileWith(character.faction);
        } else if (owner != null && character.faction == null) {
            //If character has no faction and region has faction, character cannot be a resident
            return false;
        }
        return true;
    }
    public bool HasAnyCharacterOfType(params CHARACTER_ROLE[] roleTypes) {
        for (int i = 0; i < charactersAtLocation.Count; i++) {
            Character character = charactersAtLocation[i];
            if (roleTypes.Contains(character.role.roleType)) {
                return true;
            }
        }
        return false;
    }
    public bool HasAnyCharacterOfType(params ATTACK_TYPE[] attackTypes) {
        for (int i = 0; i < charactersAtLocation.Count; i++) {
            Character character = charactersAtLocation[i];
            if (attackTypes.Contains(character.characterClass.attackType)) {
                return true;
            }
        }
        return false;
    }
    public Character GetAnyCharacterOfType(params CHARACTER_ROLE[] roleTypes) {
        for (int i = 0; i < charactersAtLocation.Count; i++) {
            Character character = charactersAtLocation[i];
            if (roleTypes.Contains(character.role.roleType)) {
                return character;
            }
        }
        return null;
    }
    public Character GetAnyCharacterOfType(params ATTACK_TYPE[] attackTypes) {
        for (int i = 0; i < charactersAtLocation.Count; i++) {
            Character character = charactersAtLocation[i];
            if (attackTypes.Contains(character.characterClass.attackType)) {
                return character;
            }
        }
        return null;
    }
    #endregion

    #region Faction
    public void SetOwner(Faction owner) {
        SetPreviousOwner(this.owner);
        this.owner = owner;
        if (area != null) {
            /*Whenever a location is occupied, 
                all items in structures Inside Settlement will be owned by the occupying faction.*/
            List<LocationStructure> insideStructures = area.GetStructuresAtLocation(true);
            for (int i = 0; i < insideStructures.Count; i++) {
                insideStructures[i].OwnItemsInLocation(owner);
            }
            Messenger.Broadcast(Signals.AREA_OWNER_CHANGED, area);
        }
        bool isCorrupted = this.owner != null && this.owner.isPlayerFaction;
        for (int i = 0; i < tiles.Count; i++) {
            HexTile tile = tiles[i];
            tile.SetCorruption(isCorrupted);
        }
        mainLandmark.landmarkNameplate.UpdateFactionEmblem();
        regionTileObject?.UpdateAdvertisements(this);
    }
    public void SetPreviousOwner(Faction faction) {
        previousOwner = faction;
    }
    public void AddFactionHere(Faction faction) {
        if (!IsFactionHere(faction)) {
            factionsHere.Add(faction);
            //Once a faction is added and there is no ruling faction yet, automatically let the added faction own the region
            if(owner == null) {
                LandmarkManager.Instance.OwnRegion(faction, faction.race, this);
            }
        }
    }
    public void RemoveFactionHere(Faction faction) {
        if (factionsHere.Remove(faction)) {
            //If a faction is removed and it is the ruling faction, transfer ruling faction to the next faction on the list if there's any, if not make the region part of neutral faction
            if(owner == faction) {
                LandmarkManager.Instance.UnownRegion(this);
                if(factionsHere.Count > 0) {
                    LandmarkManager.Instance.OwnRegion(factionsHere[0], factionsHere[0].race, this);
                } else {
                    FactionManager.Instance.neutralFaction.AddToOwnedRegions(this);
                }
            }
        }
    }
    public bool IsFactionHere(Faction faction) {
        return factionsHere.Contains(faction);
    }
    #endregion

    #region Area
    public void SetArea(Area area) {
        //Area previousArea = this.area;
        this.area = area;
        //if(area != null) {
        //    OnSetAreaInRegion();
        //} else {
        //    if(previousArea != null) {
        //        OnRemoveAreaInRegion(previousArea);
        //    }
        //}
    }
    //private void OnSetAreaInRegion() {
    //    for (int i = 0; i < tiles.Count; i++) {
    //        HexTile tile = tiles[i];
    //        area.OnTileAddedToArea(tile);
    //    }
    //}
    //private void OnRemoveAreaInRegion(Area previousArea) {
    //    for (int i = 0; i < tiles.Count; i++) {
    //        HexTile tile = tiles[i];
    //        previousArea.OnTileRemovedFromArea(tile);
    //        Biomes.Instance.UpdateTileVisuals(tile);
    //    }
    //}
    #endregion

    #region Features
    public void LoadFeatures(SaveDataRegion data) {
        for (int i = 0; i < data.features.Count; i++) {
            AddFeature(data.features[i]);
        }
    }
    public void AddFeature(RegionFeature feature) {
        if (!features.Contains(feature)) {
            features.Add(feature);
            //Debug.Log(GameManager.Instance.TodayLogString() + " added new region feature " + feature.name + " to " + this.name);
        }
    }
    public void AddFeature(string featureName) {
        AddFeature(LandmarkManager.Instance.CreateRegionFeature(featureName));
    }
    public bool RemoveFeature(RegionFeature feature) {
        return features.Remove(feature);
    }
    public bool RemoveFeature(string featureName) {
        RegionFeature feature = GetFeature(featureName);
        if (feature != null) {
            return RemoveFeature(feature);
        }
        return false;
    }
    public void RemoveAllFeatures() {
        List<RegionFeature> allFeatures = new List<RegionFeature>(features);
        for (int i = 0; i < allFeatures.Count; i++) {
            RemoveFeature(allFeatures[i]);
        }
        //features.Clear(); //only cleared for now because at the time of writing, features do not do anything when they are removed.
    }
    public RegionFeature GetFeature(string featureName) {
        for (int i = 0; i < features.Count; i++) {
            RegionFeature f = features[i];
            if (f.GetType().ToString() == featureName || f.name == featureName) {
                return f;
            }
        }
        return null;
    }
    public bool HasFeature(string featureName) {
        return GetFeature(featureName) != null;
    }
    private void ActivateRegionFeatures() {
        List<RegionFeature> regionFeatures = new List<RegionFeature>(features);
        for (int i = 0; i < regionFeatures.Count; i++) {
            RegionFeature f = regionFeatures[i];
            if (f.type == REGION_FEATURE_TYPE.ACTIVE) {
                f.Activate(this);
                if (f.isRemovedOnActivation) {
                    RemoveFeature(f);
                }
            }
        }
    }
    private void RemoveFeaturesAfterInvade() {
        List<RegionFeature> regionFeatures = new List<RegionFeature>(features);
        for (int i = 0; i < regionFeatures.Count; i++) {
            RegionFeature f = regionFeatures[i];
            if (f.isRemovedOnInvade) {
                RemoveFeature(f);
            }
        }
    }
    #endregion

    #region Awareness
    public bool AddAwareness(IPointOfInterest pointOfInterest) {
        if (!HasAwareness(pointOfInterest)) {
            if (!awareness.ContainsKey(pointOfInterest.poiType)) {
                awareness.Add(pointOfInterest.poiType, new List<IPointOfInterest>());
            }
            awareness[pointOfInterest.poiType].Add(pointOfInterest);
            //if (pointOfInterest is TreeObject) {
            //    List<IPointOfInterest> treeAwareness = GetTileObjectAwarenessOfType(TILE_OBJECT_TYPE.TREE_OBJECT);
            //    if (treeAwareness.Count >= Character.TREE_AWARENESS_LIMIT) {
            //        RemoveAwareness(treeAwareness[0]);
            //    }
            //}
            return true;
        }
        return false;
    }
    public void RemoveAwareness(IPointOfInterest pointOfInterest) {
        if (awareness.ContainsKey(pointOfInterest.poiType)) {
            List<IPointOfInterest> awarenesses = awareness[pointOfInterest.poiType];
            for (int i = 0; i < awarenesses.Count; i++) {
                IPointOfInterest iawareness = awarenesses[i];
                if (iawareness == pointOfInterest) {
                    awarenesses.RemoveAt(i);
                    break;
                }
            }
        }
    }
    public void RemoveAwareness(POINT_OF_INTEREST_TYPE poiType) {
        if (awareness.ContainsKey(poiType)) {
            awareness.Remove(poiType);
        }
    }
    public bool HasAwareness(IPointOfInterest poi) {
        if (awareness.ContainsKey(poi.poiType)) {
            List<IPointOfInterest> awarenesses = awareness[poi.poiType];
            for (int i = 0; i < awarenesses.Count; i++) {
                IPointOfInterest currPOI = awarenesses[i];
                if (currPOI == poi) {
                    return true;
                }
            }
            return false;
        }
        return false;
    }
    #endregion
    
    #region Structures
    public void GenerateStructures() {
        _structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
        LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.WILDERNESS, false);
    }
    public void AddStructure(LocationStructure structure) {
        if (!structures.ContainsKey(structure.structureType)) {
            structures.Add(structure.structureType, new List<LocationStructure>());
        }

        if (!structures[structure.structureType].Contains(structure)) {
            structures[structure.structureType].Add(structure);
        }
    }
    public void RemoveStructure(LocationStructure structure) {
        if (structures.ContainsKey(structure.structureType)) {
            if (structures[structure.structureType].Remove(structure)) {

                if (structures[structure.structureType].Count == 0) { //this is only for optimization
                    structures.Remove(structure.structureType);
                }
            }
        }
    }
    public LocationStructure GetRandomStructureOfType(STRUCTURE_TYPE type) {
        if (structures.ContainsKey(type)) {
            return structures[type][Utilities.rng.Next(0, structures[type].Count)];
        }
        return null;
    }
    public LocationStructure GetRandomStructure() {
        Dictionary<STRUCTURE_TYPE, List<LocationStructure>> _structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>(this.structures);
        _structures.Remove(STRUCTURE_TYPE.EXIT);
        int dictIndex = UnityEngine.Random.Range(0, _structures.Count);
        int count = 0;
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in _structures) {
            if (count == dictIndex) {
                return kvp.Value[UnityEngine.Random.Range(0, kvp.Value.Count)];
            }
            count++;
        }
        return null;
    }
    public LocationStructure GetStructureByID(STRUCTURE_TYPE type, int id) {
        if (structures.ContainsKey(type)) {
            List<LocationStructure> locStructures = structures[type];
            for (int i = 0; i < locStructures.Count; i++) {
                if(locStructures[i].id == id) {
                    return locStructures[i];
                }
            }
        }
        return null;
    }
    public List<LocationStructure> GetStructuresAtLocation(bool inside) {
        List<LocationStructure> _structures = new List<LocationStructure>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in this.structures) {
            for (int i = 0; i < kvp.Value.Count; i++) {
                LocationStructure currStructure = kvp.Value[i];
                if (currStructure.isInside == inside && currStructure.structureType != STRUCTURE_TYPE.EXIT) {
                    _structures.Add(currStructure);
                }
            }
        }
        return _structures;
    }
    public bool HasStructure(STRUCTURE_TYPE type) {
        return structures.ContainsKey(type);
    }
    #endregion

    #region Inner Map
    public void SetRegionInnerMap(RegionInnerTileMap regionInnerTileMap) {
        _regionInnerTileMap = regionInnerTileMap;
    }
    public bool AddSpecialTokenToLocation(SpecialToken token, LocationStructure structure = null, LocationGridTile gridLocation = null) {
        token.SetOwner(this.owner);
        if (innerMap != null) { //if the area map of this area has already been created.
            if (structure != null) {
                structure.AddItem(token, gridLocation);
            } else {
                //get structure for token
                LocationStructure chosen = InnerMapManager.Instance.GetRandomStructureToPlaceItem(this, token);
                chosen.AddItem(token);
            }
        }
        return true;
    }
    public void RemoveSpecialTokenFromLocation(SpecialToken token) {
        LocationStructure takenFrom = token.structureLocation;
        if (takenFrom != null) {
            takenFrom.RemoveItem(token);
        }
    }
    public bool IsRequiredByLocation(SpecialToken token) {
        return false;
    }
    public bool IsSameCoreLocationAs(ILocation location) {
        return location.coreTile == this.coreTile;
    }
    public void SetRegionTileObject(RegionTileObject _regionTileObject) {
        regionTileObject = _regionTileObject;
    }
    #endregion

    
}
