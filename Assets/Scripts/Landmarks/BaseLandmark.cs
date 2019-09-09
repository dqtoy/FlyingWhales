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

    public List<LANDMARK_TAG> landmarkTags { get; private set; }
    public List<BaseLandmark> connections { get; private set; }
    public IWorldObject worldObj { get; private set; }
    public int invasionTicks { get; private set; } //how many ticks until this landmark is invaded. NOTE: This is in raw ticks so if the landmark should be invaded in 1 hour, this should be set to the number of ticks in an hour.
    public List<Character> charactersHere { get; private set; }
    public WorldEvent activeEvent { get; private set; }
    public Character eventSpawnedBy { get; private set; }
    public GameObject eventIconGO { get; private set; }
    public IWorldEventData eventData { get; private set; }
    public Vector2 nameplatePos { get; private set; }

    //Player Landmark
    public PlayerLandmark playerLandmark { get; private set; } //If this is not, null then the player has constructed a landmark at this location.

    private List<System.Action> otherAfterInvasionActions; //list of other things to do when this landmark is invaded.
    private string activeEventAfterEffectScheduleID;

    #region getters/setters
    public int id {
        get { return _id; }
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
    public LandmarkVisual landmarkVisual {
        get { return _landmarkVisual; }
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
        connections = new List<BaseLandmark>();
        charactersHere = new List<Character>();
        otherAfterInvasionActions = new List<System.Action>();
        invasionTicks = GameManager.ticksPerDay;
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
        if (_specificLandmarkType.IsPlayerLandmark()) {
            SetPlayerLandmark(CreateNewPlayerLandmark(specificLandmarkType));
        }
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
        if (_specificLandmarkType.IsPlayerLandmark()) {
            SetPlayerLandmark(CreateNewPlayerLandmark(specificLandmarkType)); //TODO: Change this to save/load system
        }
    }
    public BaseLandmark(HexTile location, SaveDataLandmark data) : this() {
        _id = Utilities.SetID(this, data.id);
        _location = location;
        if(data.connectedTileID != -1) {
            _connectedTile = GridMap.Instance.hexTiles[data.connectedTileID];
        }
        _specificLandmarkType = data.landmarkType;
        SetName(data.landmarkName);
        landmarkTags = data.landmarkTags;
        invasionTicks = GameManager.ticksPerDay;

        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(specificLandmarkType);
        ConstructTags(landmarkData);
        nameplatePos = LandmarkManager.Instance.GetNameplatePosition(this.tileLocation);
        //nameplate = UIManager.Instance.CreateLandmarkNameplate(this);
        if (_specificLandmarkType.IsPlayerLandmark()) {
            SetPlayerLandmark(CreateNewPlayerLandmark(specificLandmarkType)); //TODO: Change this to save/load system
        }
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
        if (type.IsPlayerLandmark()) {
            SetPlayerLandmark(CreateNewPlayerLandmark(specificLandmarkType));
        } else {
            //if the type of this landmark changed to a non player landmark, and this was previously a player landmark, destroy the player landmark.
            SetPlayerLandmark(null);
        }
        //if (type == LANDMARK_TYPE.NONE) {
        //    ObjectPoolManager.Instance.DestroyObject(nameplate.gameObject);
        //}
    }

    #region Virtuals
    public virtual void Initialize() { }
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
        tileLocation.CenterCameraHere();
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
        landmarkTags = new List<LANDMARK_TAG>(landmarkData.uniqueTags); //add unique tags
        ////add common tags from base landmark type
        //BaseLandmarkData baseLandmarkData = LandmarkManager.Instance.GetBaseLandmarkData(landmarkData.baseLandmarkType);
        //_landmarkTags.AddRange(baseLandmarkData.baseLandmarkTags);
    }
    #endregion

    #region Corruption/Invasion
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
            SetCharacterEventSpawner(activeEvent.GetCharacterThatCanSpawnEvent(this));
        } else {
            SetCharacterEventSpawner(spawner);
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
        activeEvent.Spawn(this, eventData, out activeEventAfterEffectScheduleID);
        Messenger.Broadcast(Signals.WORLD_EVENT_SPAWNED, this, we);
    }
    public void LoadEventAndWorldObject(SaveDataLandmark data) {
        if (data.activeEvent != WORLD_EVENT.NONE) {
            activeEvent = StoryEventsManager.Instance.GetWorldEvent(data.activeEvent);
            SetCharacterEventSpawner(CharacterManager.Instance.GetCharacterByID(data.eventSpawnedByCharacterID));
            eventData = data.eventData.Load();
            activeEvent.Load(this, eventData, out activeEventAfterEffectScheduleID);
            Messenger.Broadcast(Signals.WORLD_EVENT_SPAWNED, this, activeEvent);
        }
        if (data.hasWorldObject) {
            SetWorldObject(data.worldObj.Load());
        }
    }
    public void SetCharacterEventSpawner(Character character) {
        eventSpawnedBy = character;
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
    public void LoadCharacterHere(Character character) {
        charactersHere.Add(character);
        character.SetLandmarkLocation(this);
        Messenger.Broadcast(Signals.CHARACTER_ENTERED_LANDMARK, character, this);
    }
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

    #region Player Landmark
    public void SetPlayerLandmark(PlayerLandmark landmark) {
        playerLandmark?.OnLandmarkDestroyed();  //Destroy the previous landmark
        playerLandmark = landmark;
        playerLandmark?.OnLandmarkPlaced(); //Place new landmark
    }
    /// <summary>
    /// Create a new instance of a player landmark. NOTE: This should only be supplied with landmark types that are sure to be player landmarks. Did not add checking here to save on performance.
    /// </summary>
    /// <param name="type">The type of player landmark</param>
    /// <returns>A new player landmark instance</returns>
    private PlayerLandmark CreateNewPlayerLandmark(LANDMARK_TYPE type) {
        var typeName = Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(type.ToString());
        System.Type systemType = System.Type.GetType(typeName);
        if (systemType != null) {
           return System.Activator.CreateInstance(systemType, this) as PlayerLandmark;
        }
        return null;
    }
    #endregion
}
