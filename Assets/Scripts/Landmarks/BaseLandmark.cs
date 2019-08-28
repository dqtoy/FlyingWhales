/*
 This is the base class for all landmarks.
 eg. Settlements(Cities), Resources, Dungeons, Lairs, etc.
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class BaseLandmark {
    protected int _id;
    protected string _landmarkName;
    protected LANDMARK_TYPE _specificLandmarkType;
    protected bool _isOccupied;
    protected bool _hasBeenCorrupted;
    protected HexTile _location;
    protected HexTile _connectedTile;
    protected Faction _owner;
    protected LandmarkVisual _landmarkVisual;
    protected LandmarkNameplate nameplate;
    protected List<Item> _itemsInLandmark;
    protected List<LANDMARK_TAG> _landmarkTags;
    public List<BaseLandmark> connections { get; private set; }
    public Character skirmishEnemy { get; private set; }
    public IWorldObject worldObj { get; private set; }
    public int invasionTicks { get; private set; } //how many ticks until this landmark is invaded. NOTE: This is in raw ticks so if the landmark should be invaded in 1 hour, this should be set to the number of ticks in an hour.
    public List<Character> charactersHere { get; private set; }
    public WorldEvent activeEvent { get; private set; }
    public Character eventSpawnedBy { get; private set; }
    public GameObject eventIconGO { get; private set; }
    public IWorldEventData eventData { get; private set; }
    public Vector2 nameplatePos { get; private set; }

    private List<System.Action> otherAfterInvasionActions; //list of other things to do when this landmark is invaded.

    private string activeEventAfterEffectScheduleID;

    #region getters/setters
    public int id {
        get { return _id; }
    }
    public string name {
        get { return landmarkName; }
    }
    public string thisName {
        get { return landmarkName; }
    }
    public string locationName {
        get { return landmarkName + " " + tileLocation.locationName; }
    }
    public string landmarkName {
        get { return _landmarkName; }
    }
    public string urlName {
        get { return "<link=" + '"' + this._id.ToString() + "_landmark" + '"' + ">" + _landmarkName + "</link>"; }
    }
    public LANDMARK_TYPE specificLandmarkType {
        get { return _specificLandmarkType; }
    }
    public bool isOccupied {
        get { return _isOccupied; }
    }
    public Faction owner {
        get { return _owner; }
    }
    public Faction faction {
        get { return _owner; }
    }
    public LandmarkVisual landmarkVisual {
        get { return _landmarkVisual; }
    }
    public LOCATION_IDENTIFIER locIdentifier {
        get { return LOCATION_IDENTIFIER.LANDMARK; }
    }
    public HexTile tileLocation {
        get { return _location; }
    }
    public HexTile connectedTile {
        get { return _connectedTile; }
    }
    #endregion

    public BaseLandmark() {
        _owner = null; //landmark has no owner yet
        _hasBeenCorrupted = false;
        _itemsInLandmark = new List<Item>();
        connections = new List<BaseLandmark>();
        charactersHere = new List<Character>();
        otherAfterInvasionActions = new List<System.Action>();
        invasionTicks = GameManager.ticksPerDay;
        //invasionTicks = 2;
    }
    public BaseLandmark(HexTile location, LANDMARK_TYPE specificLandmarkType) : this() {
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(specificLandmarkType);
        _id = Utilities.SetID(this);
        _location = location;
        _specificLandmarkType = specificLandmarkType;
        SetName(RandomNameGenerator.Instance.GetLandmarkName(specificLandmarkType));
        ConstructTags(landmarkData);
        nameplatePos = LandmarkManager.Instance.GetNameplatePosition(this.tileLocation);
        //nameplate = UIManager.Instance.CreateLandmarkNameplate(this);
    }
    public BaseLandmark(HexTile location, LandmarkSaveData data) : this() {
        _id = Utilities.SetID(this, data.landmarkID);
        _location = location;
        _specificLandmarkType = data.landmarkType;
        SetName(data.landmarkName);
        
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(specificLandmarkType);
        ConstructTags(landmarkData);
        nameplatePos = LandmarkManager.Instance.GetNameplatePosition(this.tileLocation);
        //nameplate = UIManager.Instance.CreateLandmarkNameplate(this);
    }
    public BaseLandmark(HexTile location, SaveDataLandmark data) : this() {
        _id = Utilities.SetID(this, data.id);
        _location = location;
        _specificLandmarkType = data.landmarkType;
        SetName(data.landmarkName);

        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(specificLandmarkType);
        ConstructTags(landmarkData);
        nameplatePos = LandmarkManager.Instance.GetNameplatePosition(this.tileLocation);
        //nameplate = UIManager.Instance.CreateLandmarkNameplate(this);
    }

    public void SetName(string name) {
        _landmarkName = name;
        if (_landmarkVisual != null) {
            _landmarkVisual.UpdateName();
        }
    }
    public void SetConnectedTile(HexTile connectedTile) {
        _connectedTile = connectedTile;
    }
    public void ChangeLandmarkType(LANDMARK_TYPE type) {
        _specificLandmarkType = type;
        tileLocation.UpdateLandmarkVisuals();
        //if (type == LANDMARK_TYPE.NONE) {
        //    ObjectPoolManager.Instance.DestroyObject(nameplate.gameObject);
        //}
    }

    #region Virtuals
    public virtual void Initialize() { }
    public virtual void DestroyLandmark() {
        //if (!_landmarkObj.isRuined) {
            //ObjectState ruined = landmarkObj.GetState("Ruined");
            //landmarkObj.ChangeState(ruined);
            //tileLocation.areaOfTile.CheckDeath();
        //}
        //RemoveListeners();
    }
    /*
     What should happen when a character searches this landmark
         */
    public virtual void SearchLandmark(Character character) { }
    #endregion

    #region Ownership
    public virtual void OccupyLandmark(Faction faction) {
        _owner = faction;
        _isOccupied = true;
        _location.Occupy();
        _owner.OwnLandmark(this);
    }
    public virtual void UnoccupyLandmark() {
        if (_owner == null) {
            //throw new System.Exception("Landmark doesn't have an owner but something is trying to unoccupy it!");
        }
        _isOccupied = false;
        _location.Unoccupy();
        _owner = null;
    }
    public void ChangeOwner(Faction newOwner) {
        _owner = newOwner;
        _isOccupied = true;
        _location.Occupy();
    }
    #endregion

    #region Utilities
    public void SetLandmarkObject(LandmarkVisual obj) {
        _landmarkVisual = obj;
        _landmarkVisual.SetLandmark(this);
    }
    public void CenterOnLandmark() {
        CameraMove.Instance.CenterCameraOn(this.tileLocation.gameObject);
    }
    public override string ToString() {
        return this.landmarkName;
    }
    public bool HasAnyCharacterOfType(params CHARACTER_ROLE[] roleTypes) {
        for (int i = 0; i < charactersHere.Count; i++) {
            Character character = charactersHere[i];
            if (roleTypes.Contains(character.role.roleType)) {
                return true;
            }
        }
        return false;
    }
    public bool HasAnyCharacterOfType(params ATTACK_TYPE[] attackTypes) {
        for (int i = 0; i < charactersHere.Count; i++) {
            Character character = charactersHere[i];
            if (attackTypes.Contains(character.characterClass.attackType)) {
                return true;
            }
        }
        return false;
    }
    public Character GetAnyCharacterOfType(params CHARACTER_ROLE[] roleTypes) {
        for (int i = 0; i < charactersHere.Count; i++) {
            Character character = charactersHere[i];
            if (roleTypes.Contains(character.role.roleType)) {
                return character;
            }
        }
        return null;
    }
    public Character GetAnyCharacterOfType(params ATTACK_TYPE[] attackTypes) {
        for (int i = 0; i < charactersHere.Count; i++) {
            Character character = charactersHere[i];
            if (attackTypes.Contains(character.characterClass.attackType)) {
                return character;
            }
        }
        return null;
    }
    #endregion

    #region Tags
    private void ConstructTags(LandmarkData landmarkData) {
        _landmarkTags = new List<LANDMARK_TAG>(landmarkData.uniqueTags); //add unique tags
        ////add common tags from base landmark type
        //BaseLandmarkData baseLandmarkData = LandmarkManager.Instance.GetBaseLandmarkData(landmarkData.baseLandmarkType);
        //_landmarkTags.AddRange(baseLandmarkData.baseLandmarkTags);
    }
    #endregion

    #region Corruption/Invasion
    public void ToggleCorruption(bool state) {
        if (state) {
            LandmarkManager.Instance.corruptedLandmarksCount++;
            if (!_hasBeenCorrupted) {
                _hasBeenCorrupted = true;
            }
            //_diagonalLeftBlocked = 0;
            //_diagonalRightBlocked = 0;
            //_horizontalBlocked = 0;
            //tileLocation.region.LandmarkStartedCorruption(this);
            Messenger.AddListener(Signals.TICK_ENDED, DoCorruption);
            //if (Messenger.eventTable.ContainsKey("StartCorruption")) {
            //    Messenger.RemoveListener<BaseLandmark>("StartCorruption", ALandmarkHasStartedCorruption);
            //    Messenger.Broadcast<BaseLandmark>("StartCorruption", this);
            //}
        } else {
            LandmarkManager.Instance.corruptedLandmarksCount--;
            StopSpreadCorruption();
        }
    }
    private void StopSpreadCorruption() {
        if (LandmarkManager.Instance.corruptedLandmarksCount > 1) {
            HexTile chosenTile = null;
            int range = 3;
            while (chosenTile == null) {
                List<HexTile> tilesToCheck = tileLocation.GetTilesInRange(range, true);
                for (int i = 0; i < tilesToCheck.Count; i++) {
                    if (tilesToCheck[i].corruptedLandmark != null && tilesToCheck[i].corruptedLandmark.id != this.id) {
                        chosenTile = tilesToCheck[i];
                        break;
                    }
                }
                range++;
            }
            PathGenerator.Instance.CreatePath(this, this.tileLocation, chosenTile, PATHFINDING_MODE.UNRESTRICTED);
        }
        //tileLocation.region.LandmarkStoppedCorruption(this);
        Messenger.RemoveListener(Signals.TICK_ENDED, DoCorruption);
        //Messenger.Broadcast<BaseLandmark>("StopCorruption", this);
    }
    private void DoCorruption() {
        //if (_nextCorruptedTilesToCheck.Count > 0) {
        //    int index = UnityEngine.Random.Range(0, _nextCorruptedTilesToCheck.Count);
        //    HexTile currentCorruptedTileToCheck = _nextCorruptedTilesToCheck[index];
        //    _nextCorruptedTilesToCheck.RemoveAt(index);
        //    SpreadCorruption(currentCorruptedTileToCheck);
        //} else {
        //    StopSpreadCorruption();
        //}
    }
    private void SpreadCorruption(HexTile originTile) {
        //if (!originTile.CanThisTileBeCorrupted()) {
        //    return;
        //}
        for (int i = 0; i < originTile.AllNeighbours.Count; i++) {
            HexTile neighbor = originTile.AllNeighbours[i];
            if (neighbor.uncorruptibleLandmarkNeighbors <= 0) {
                if (!neighbor.isCorrupted) { //neighbor.region.id == originTile.region.id && neighbor.CanThisTileBeCorrupted()
                    neighbor.SetCorruption(true, this);
                    //_nextCorruptedTilesToCheck.Add(neighbor);
                }
                //if(neighbor.landmarkNeighbor != null && !neighbor.landmarkNeighbor.tileLocation.isCorrupted) {
                //    neighbor.landmarkNeighbor.CreateWall();
                //}
                if (originTile.corruptedLandmark.id != neighbor.corruptedLandmark.id) {
                    //originTile.corruptedLandmark.hasAdjacentCorruptedLandmark = true;
                }
            }
            //else {
            //if cannot be corrupted it means that it has a landmark still owned by a kingdom
            //neighbor.landmarkOnTile.CreateWall();
            //}
        }
    }
    public void ReceivePath(List<HexTile> pathTiles) {
        if (pathTiles != null) {
            ConnectCorruption(pathTiles);
        }
    }
    private void ConnectCorruption(List<HexTile> pathTiles) {
        for (int i = 0; i < pathTiles.Count; i++) {
            pathTiles[i].SetUncorruptibleLandmarkNeighbors(0);
            pathTiles[i].SetCorruption(true, this);
        }
    }
    public void InvadeThisLandmark() {
        switch (specificLandmarkType) {
            case LANDMARK_TYPE.NONE:
            case LANDMARK_TYPE.CAVE:
            case LANDMARK_TYPE.MONSTER_LAIR:
            case LANDMARK_TYPE.ANCIENT_RUIN:
            case LANDMARK_TYPE.TEMPLE:
            case LANDMARK_TYPE.BANDIT_CAMP:
                //No base effect upon invading
                break;
            case LANDMARK_TYPE.BARRACKS:    
            case LANDMARK_TYPE.OUTPOST:
                PlayerManager.Instance.player.LevelUpAllMinions();
                PlayerUI.Instance.ShowGeneralConfirmation("Congratulations!", "All your minions gained 1 level.");
                break;
            case LANDMARK_TYPE.FARM:
                PlayerManager.Instance.player.UnlockASummonSlotOrUpgradeExisting();
                break;
            case LANDMARK_TYPE.MINES:
            case LANDMARK_TYPE.FACTORY: //This is FACTORY
            case LANDMARK_TYPE.WORKSHOP:
                PlayerManager.Instance.player.UnlockAnArtifactSlotOrUpgradeExisting();
                break;
        }
        ChangeLandmarkType(LANDMARK_TYPE.NONE);
        ObtainWorldObject();
        ExecuteEventAfterInvasion();
        ExecuteOtherAfterInvasionActions();
    }
    private void ExecuteOtherAfterInvasionActions() {
        for (int i = 0; i < otherAfterInvasionActions.Count; i++) {
            otherAfterInvasionActions[i].Invoke();
        }
        otherAfterInvasionActions.Clear();
    }
    public void AddAfterInvasionAction(System.Action action) {
        otherAfterInvasionActions.Add(action);
    }
    /// <summary>
    /// Is this landmark connected to another landmark that has been corrupted?
    /// </summary>
    /// <returns>True or false</returns>
    public bool HasCorruptedConnection() {
        for (int i = 0; i < connections.Count; i++) {
            BaseLandmark connection = connections[i];
            if (connection.tileLocation.isCorrupted) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Connections
    public void AddConnection(BaseLandmark newConnection) {
        connections.Add(newConnection);
    }
    public bool IsConnectedWith(BaseLandmark otherLandmark) {
        return connections.Contains(otherLandmark);
    }
    public bool HasMaximumConnections() {
        return connections.Count >= LandmarkManager.Max_Connections;
    }
    #endregion

    #region Events
    public void SpawnEvent(WorldEvent we, Character spawner = null) {
        activeEvent = we;
        //set character that spawned event
        if (spawner == null) {
            eventSpawnedBy = activeEvent.GetCharacterThatCanSpawnEvent(this);
        } else {
            eventSpawnedBy = spawner;
        }
        eventData = activeEvent.ConstructEventDataForLandmark(this);        
        for (int i = 0; i < eventData.involvedCharacters.Length; i++) {
            Character currCharacter = eventData.involvedCharacters[i];
            //do not let the character that spawned the event go home
            if (currCharacter.stateComponent.currentState is MoveOutState) {
                MoveOutState state = currCharacter.stateComponent.currentState as MoveOutState;
                Debug.Log(GameManager.Instance.TodayLogString() + "Removing go home schedule of " + currCharacter.name);
                SchedulingManager.Instance.RemoveSpecificEntry(state.goHomeSchedID);
            } else {
                throw new System.Exception(currCharacter.name + " is at " + tileLocation.region.name + " but is not in move out state!");

            }
        }
        //spawn the event
        activeEvent.Spawn(this, out activeEventAfterEffectScheduleID);
        Messenger.Broadcast(Signals.WORLD_EVENT_SPAWNED, this, we);
    }
    private void SpawnBasicEvent(Character spawner) {
        string summary = GameManager.Instance.TodayLogString() + spawner.name + " arrived at " + tileLocation.region.name + " will try to spawn random event.";
        List<WorldEvent> events = StoryEventsManager.Instance.GetEventsThatCanSpawnAt(this, true);
        if (events.Count > 0) {
            summary += "\nPossible events are: ";
            for (int i = 0; i < events.Count; i++) {
                summary += "|" + events[i].name + "|";
            }
            WorldEvent chosenEvent = events[Random.Range(0, events.Count)];
            summary += "\nChosen Event is: " + chosenEvent.name;
            SpawnEvent(chosenEvent, spawner);
        } else {
            summary += "\nNo possible events to spawn.";
        }
        Debug.Log(summary);
    }
    public void WorldEventFinished(WorldEvent we) {
        if (activeEvent != we) {
            throw new System.Exception("World event " + we.name + " finished, but it is not the active event at " + this.tileLocation.region.name);
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
    private void DespawnEvent() {
        WorldEvent despawned = activeEvent;
        activeEvent = null;
        despawned.OnDespawn(this);
        Messenger.Broadcast(Signals.WORLD_EVENT_DESPAWNED, this, despawned);
    }
    private void ExecuteEventAfterInvasion() {
        if (activeEvent != null) {
            SchedulingManager.Instance.RemoveSpecificEntry(activeEventAfterEffectScheduleID); //unschedule the active event after effect schedule
            activeEvent.ExecuteAfterInvasionEffect(this);
            DespawnEvent();
        }
        //kill all remaining characters
        while (charactersHere.Count > 0) {
            Character character = charactersHere[0];
            character.Death("Invasion");
        }
    }
    public bool CanSpawnNewEvent() {
        return !tileLocation.isCorrupted && activeEvent == null;
    }
    public void SetEventIcon(GameObject go) {
        eventIconGO = go;
    }
    private void AutomaticEventGeneration(Character characterThatArrived) {
        if (activeEvent == null) {
            SpawnBasicEvent(characterThatArrived);
        }
    }
    #endregion

    #region Skirmish
    //public void GenerateSkirmishEnemy() {
    //    if(skirmishEnemy != null) {
    //        return;
    //    }
    //    Area area = GetUpcomingSettlement();
    //    if (area != null) {
    //        if (area.owner.leader is Character) {
    //            Character areaLeader = area.owner.leader as Character;

    //            WeightedDictionary<CharacterRole> roleChoices = new WeightedDictionary<CharacterRole>();
    //            roleChoices.AddElement(CharacterRole.CIVILIAN, 30);
    //            roleChoices.AddElement(CharacterRole.ADVENTURER, 35);
    //            roleChoices.AddElement(CharacterRole.SOLDIER, 35);

    //            Character enemy = new Character(roleChoices.PickRandomElementGivenWeights(), areaLeader.race, Utilities.GetRandomGender());
    //            enemy.OnUpdateRace();
    //            enemy.SetLevel(areaLeader.level - 1);

    //            skirmishEnemy = enemy;
    //        }
    //    } else {
    //        throw new System.Exception(tileLocation.name + " has no upcoming settlement!");
    //    }
    //}
    #endregion

    #region World Objects
    public void SetWorldObject(IWorldObject obj) {
        worldObj = obj;
    }
    private void ObtainWorldObject() {
        worldObj?.Obtain();
        SetWorldObject(null);
    }
    #endregion

    #region Characters
    public void AddCharacterHere(Character character) {
        charactersHere.Add(character);
        character.SetLandmarkLocation(this);
        AutomaticEventGeneration(character);
        Messenger.Broadcast(Signals.CHARACTER_ENTERED_LANDMARK, character, this);
    }
    public void RemoveCharacterHere(Character character) {
        charactersHere.Remove(character);
        character.SetLandmarkLocation(null);
        Messenger.Broadcast(Signals.CHARACTER_EXITED_LANDMARK, character, this);
    }
    #endregion
}
