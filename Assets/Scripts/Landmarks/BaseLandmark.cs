/*
 This is the base class for all landmarks.
 eg. Settlements(Cities), Resources, Dungeons, Lairs, etc.
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class BaseLandmark : ILocation, IInteractable {
    protected int _id;
    protected string _landmarkName;
    protected LANDMARK_TYPE _specificLandmarkType;
    protected bool _canBeOccupied; //can the landmark be occupied?
    protected bool _isOccupied;
    protected bool _isBeingInspected;
    protected bool _isRaided;
    protected bool _hasBeenInspected;
    protected bool _hasBeenCorrupted;
    protected bool _isAttackingAnotherLandmark;
    protected int _combatHistoryID;
    protected int _civilianCount;
    protected HexTile _location;
    protected HexTile _connectedTile;
    protected Faction _owner;
    protected StructureObj _landmarkObj;
    protected LandmarkVisual _landmarkVisual;
    //protected LandmarkInvestigation _landmarkInvestigation;
    protected List<Character> _charactersWithHomeOnLandmark;
    protected List<BaseLandmark> _connections;
    protected List<Character> _prisoners; //list of prisoners on landmark
    protected List<Log> _history;
    protected List<Party> _charactersAtLocation;
    protected List<Party> _assaultParties;
    protected List<LandmarkPartyData> _lastInspectedOfCharactersAtLocation;
    protected List<Item> _itemsInLandmark;
    protected List<Item> _lastInspectedItemsInLandmark;
    protected List<LANDMARK_TAG> _landmarkTags;
    protected List<HexTile> _nextCorruptedTilesToCheck;
    protected List<HexTile> _wallTiles;
    //protected List<Secret> _secrets;
    protected List<Token> _tokens;
    protected List<string> _encounters;
    protected List<Interaction> _currentInteractions;
    protected Dictionary<Character, GameDate> _characterTraces; //Lasts for 60 days
    protected Dictionary<int, Combat> _combatHistory;
    protected Dictionary<RESOURCE, int> _resourceInventory;
    
    public bool hasAdjacentCorruptedLandmark;
    public QuestBoard questBoard { get; private set; }
    public List<GameEvent> advertisedEvents { get; private set; } //events happening at this landmark, that other characters can partake in
    //public Party defenders { get; private set; }
    //public bool canProduceSupplies { get; private set; }
    public WeightedDictionary<INTERACTION_TYPE> scenarios { get; private set; }
    public WeightedDictionary<bool> eventTriggerWeights { get; private set; }
    public int eventTriggerWeight { get; private set; }
    public int noEventTriggerWeight { get; private set; }
    //public int maxDailySupplyProduction { get; private set; }
    //public int minDailySupplyProduction { get; private set; }
    public int maxDefenderCount { get; private set; }

    //private List<Buff> defenderBuffs;

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
    public List<BaseLandmark> connections {
        get { return _connections; }
    }
    public bool canBeOccupied {
        get { return _canBeOccupied; }
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
    public List<Character> charactersWithHomeOnLandmark {
        get { return _charactersWithHomeOnLandmark; }
    }
    //  public virtual int totalPopulation {
    //get { return civilians + CharactersCount(); }
    //  }
    //public int civilians {
    //	get { return _civiliansByRace.Sum(x => x.Value); }
    //   }
    //   public Dictionary<RACE, int> civiliansByRace {
    //       get { return _civiliansByRace; }
    //   }
    public LandmarkVisual landmarkVisual {
        get { return _landmarkVisual; }
    }
    //public LandmarkInvestigation landmarkInvestigation {
    //    get { return _landmarkInvestigation; }
    //}
    public List<Character> prisoners {
        get { return _prisoners; }
    }
    public List<Log> history {
        get { return this._history; }
    }
    public Dictionary<int, Combat> combatHistory {
        get { return _combatHistory; }
    }
    public List<Party> charactersAtLocation {
        get { return _charactersAtLocation; }
    }
    public List<LandmarkPartyData> lastInspectedOfCharactersAtLocation {
        get { return _lastInspectedOfCharactersAtLocation; }
    }
    public List<Item> itemsInLandmark {
        get { return _itemsInLandmark; }
    }
    public List<Item> lastInspectedItemsInLandmark {
        get { return _lastInspectedItemsInLandmark; }
    }
    //public List<Secret> secrets {
    //    get { return _secrets; }
    //}
    public List<Token> tokens {
        get { return _tokens; }
    }
    public List<string> encounters {
        get { return _encounters; }
    }
    public List<Interaction> currentInteractions {
        get { return _currentInteractions; }
    }
    public List<HexTile> wallTiles {
        get { return _wallTiles; }
    }
    public List<Party> assaultParties {
        get { return _assaultParties; }
    }
    public Dictionary<Character, GameDate> characterTraces {
        get { return _characterTraces; }
    }
    public int currDurability {
        get { return _landmarkObj.currentHP; }
    }
    public int totalDurability {
        get { return _landmarkObj.maxHP; }
    }
    public int civilianCount {
        get { return _civilianCount; }
    }
    public bool isAttackingAnotherLandmark {
        get { return _isAttackingAnotherLandmark; }
    }
    public bool isBeingInspected {
        get { return _isBeingInspected; }
    }
    public bool hasBeenInspected {
        get { return _hasBeenInspected; }
    }
    public bool isRaided {
        get { return _isRaided; }
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
    public StructureObj landmarkObj {
        get { return _landmarkObj; }
    }
    //public HiddenDesire hiddenDesire {
    //    get { return null; }
    //}
    public ILocation specificLocation {
        get { return this; }
    }
    #endregion

    public BaseLandmark() {
        _owner = null; //landmark has no owner yet
        _combatHistoryID = 0;
        _hasBeenCorrupted = false;
        hasAdjacentCorruptedLandmark = false;
        _connections = new List<BaseLandmark>();
        _charactersWithHomeOnLandmark = new List<Character>();
        _prisoners = new List<Character>();
        _history = new List<Log>();
        _charactersAtLocation = new List<Party>();
        _lastInspectedOfCharactersAtLocation = new List<LandmarkPartyData>();
        _itemsInLandmark = new List<Item>();
        _lastInspectedItemsInLandmark = new List<Item>();
        _nextCorruptedTilesToCheck = new List<HexTile>();
        _wallTiles = new List<HexTile>();
        advertisedEvents = new List<GameEvent>();
        //_secrets = new List<Secret>();
        _tokens = new List<Token>();
        _encounters = new List<string>();
        _currentInteractions = new List<Interaction>();
        _combatHistory = new Dictionary<int, Combat>();
        _characterTraces = new Dictionary<Character, GameDate>();
        _assaultParties = new List<Party>();
        scenarios = new WeightedDictionary<INTERACTION_TYPE>();
        eventTriggerWeights = new WeightedDictionary<bool>();
        //SetSupplyProductionState(true);
        //SetMaxDefenderCount(4);
        //defenderBuffs = new List<Buff>();
        //defenders = new Party[LandmarkManager.MAX_DEFENDERS];
        //Messenger.AddListener(Signals.TOGGLE_CHARACTERS_VISIBILITY, OnToggleCharactersVisibility);
    }
    public BaseLandmark(HexTile location, LANDMARK_TYPE specificLandmarkType) : this() {
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(specificLandmarkType);
        _id = Utilities.SetID(this);
        _location = location;
        _specificLandmarkType = specificLandmarkType;
        SetName(RandomNameGenerator.Instance.GetLandmarkName(specificLandmarkType));
        ConstructTags(landmarkData);
        SpawnInitialLandmarkItems();
    }
    public BaseLandmark(HexTile location, LandmarkSaveData data) : this() {
        _id = Utilities.SetID(this, data.landmarkID);
        _location = location;
        _specificLandmarkType = data.landmarkType;
        SetName(data.landmarkName);
        
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(specificLandmarkType);
        ConstructTags(landmarkData);
        SpawnInitialLandmarkItems();

        if (data.scenarioWeights != null) {
            scenarios = new WeightedDictionary<INTERACTION_TYPE>(data.scenarioWeights);
        }
        SetEventTriggerWeight(data.eventTriggerWeight);
        SetNoEventTriggerWeight(data.noEventTriggerWeight);
        eventTriggerWeights.AddElement(true, eventTriggerWeight);
        eventTriggerWeights.AddElement(false, noEventTriggerWeight);

        //if (data.defenderWeights != null) {
        //    defenderWeights = new WeightedDictionary<DefenderSetting>(data.defenderWeights);
        //}
        //SetMaxDailySupplyProductionAmount(data.maxDailySupplyAmount);
        //SetInitialDefenderCount(data.initialDefenderCount);
        //SetMaxDefenderCount(data.maxDefenderCount);
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

    #region Connections
    public void AddConnection(BaseLandmark connection) {
        if (!_connections.Contains(connection)) {
            _connections.Add(connection);
        }
    }
    public bool IsConnectedTo(Region region) {
        for (int i = 0; i < _connections.Count; i++) {
            BaseLandmark currConnection = _connections[i];
            if (currConnection.tileLocation.region.id == region.id) {
                return true;
            }
        }
        return false;
    }
    public bool IsConnectedTo(BaseLandmark landmark) {
        for (int i = 0; i < _connections.Count; i++) {
            BaseLandmark currConnection = _connections[i];
            if (currConnection.id == landmark.id) {
                return true;
            }
        }
        return false;
    }
    public bool IsIndirectlyConnectedTo(Region region) {
        for (int i = 0; i < _connections.Count; i++) {
            BaseLandmark currConnection = _connections[i];
            if (currConnection.IsConnectedTo(region)) {
                return true;
            }
        }
        return false;
    }
    public bool IsIndirectlyConnectedTo(BaseLandmark landmark) {
        for (int i = 0; i < _connections.Count; i++) {
            BaseLandmark currConnection = _connections[i];
            if (currConnection.IsConnectedTo(landmark)) {
                return true;
            }
        }
        return false;
    }
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
            throw new System.Exception("Landmark doesn't have an owner but something is trying to unoccupy it!");
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

    #region Characters
    public void AddCharacterHomeOnLandmark(Character character, bool ignoreAreaResidentCapacity = false, bool broadcast = true) {
        if (!_charactersWithHomeOnLandmark.Contains(character)) {
            _charactersWithHomeOnLandmark.Add(character);
            character.SetHomeLandmark(this, ignoreAreaResidentCapacity);
            if (broadcast) {
                Messenger.Broadcast(Signals.LANDMARK_RESIDENT_ADDED, this, character);
            }
        }
    }
    public void RemoveCharacterHomeOnLandmark(Character character) {
        if (_charactersWithHomeOnLandmark.Remove(character)) {
            Messenger.Broadcast(Signals.LANDMARK_RESIDENT_REMOVED, this, character);
        }
        character.SetHomeLandmark(null);
    }
    public bool IsResident(Character character) {
        return _charactersWithHomeOnLandmark.Contains(character);
    }
    public Character GetPrisonerByID(int id) {
        for (int i = 0; i < _prisoners.Count; i++) {
            if (_prisoners[i].id == id) {
                return _prisoners[i];
            }
        }
        return null;
    }
    public List<Character> GetCharactersOfType(string className) {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < charactersAtLocation.Count; i++) {
            Party currParty = charactersAtLocation[i];
            if (currParty is CharacterParty) {
                for (int j = 0; j < currParty.characters.Count; j++) {
                    Character character = currParty.characters[j];
                    if (character.characterClass.className.Contains(className)) {
                        characters.Add(character);
                    }
                }
            }
        }
        return characters;
    }
    public Character GetResidentCharacterOfClass(string className) {
        for (int i = 0; i < _charactersWithHomeOnLandmark.Count; i++) {
            if(_charactersWithHomeOnLandmark[i].characterClass != null && _charactersWithHomeOnLandmark[i].characterClass.className == className && _charactersWithHomeOnLandmark[i].currentParty.specificLocation == this) {
                return _charactersWithHomeOnLandmark[i];
            }
        }
        return null;
    }
    public void SpawnRandomCharacters(int howMany) {
        if (tileLocation.areaOfTile.IsResidentsFull()) {
            return;
        }
        WeightedDictionary<AreaCharacterClass> classWeights = tileLocation.areaOfTile.GetClassWeights();
        for (int i = 0; i < howMany; i++) {
            if (tileLocation.areaOfTile.IsResidentsFull()) {
                break;
            }
            string classNameToBeSpawned = classWeights.PickRandomElementGivenWeights().className;
            Character createdCharacter = CharacterManager.Instance.CreateNewCharacter(classNameToBeSpawned, tileLocation.areaOfTile.raceType, Utilities.GetRandomGender(), tileLocation.areaOfTile.owner, this);
        }
    }
    #endregion

    #region Location
    public void AddCharacterToLocation(Party iparty) {
        //if(iparty.owner.homeLandmark == this) {
        if (!_charactersAtLocation.Contains(iparty)) {
            //if (!IsDefenderOfLandmark(iparty)) {
            _charactersAtLocation.Add(iparty); //only add to characters list if the party is not a defender of the landmark
            //}
            //this.tileLocation.RemoveCharacterFromLocation(iparty);
            //if (iparty.specificLocation != null) {
            //    iparty.specificLocation.RemoveCharacterFromLocation(iparty);
            //}
            iparty.SetSpecificLocation(this);
            tileLocation.areaOfTile.AddCharacterAtLocation(iparty.owner);
#if !WORLD_CREATION_TOOL
            //_landmarkVisual.OnCharacterEnteredLandmark(iparty);
            Messenger.Broadcast<Party, BaseLandmark>(Signals.PARTY_ENTERED_LANDMARK, iparty, this);
#endif
        }
        //}
    }
    public void RemoveCharacterFromLocation(Party iparty, bool addToTile = false) {
        if (_charactersAtLocation.Remove(iparty)) {
            if (addToTile) {
                this.tileLocation.AddCharacterToLocation(iparty);
            }
            tileLocation.areaOfTile.RemoveCharacterAtLocation(iparty.owner);
#if !WORLD_CREATION_TOOL
            Messenger.Broadcast<Party, BaseLandmark>(Signals.PARTY_EXITED_LANDMARK, iparty, this);
#endif
        } else {
            Debug.LogWarning("Cannot remove character from " + this.name + " because he/she is not here");
        }
    }
    public void ReplaceCharacterAtLocation(Party ipartyToReplace, Party ipartyToAdd) {
        if (_charactersAtLocation.Contains(ipartyToReplace)) {
            int indexOfCharacterToReplace = _charactersAtLocation.IndexOf(ipartyToReplace);
            _charactersAtLocation.Insert(indexOfCharacterToReplace, ipartyToAdd);
            _charactersAtLocation.Remove(ipartyToReplace);
            ipartyToAdd.SetSpecificLocation(this);
        }
    }
    public bool IsCharacterAtLocation(Character character) {
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            Party currParty = _charactersAtLocation[i];
            if (currParty.characters.Contains(character)) {
                return true;
            }
        }
        return false;
    }
    public int GetInspectedCharactersCount() {
        int count = 0;
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            if (_charactersAtLocation[i].IsPartyBeingInspected()) {
                count++;
            }
        }
        return count;
    }
    public int GetCharactersInsideLandmarkCount() {
        int count = 0;
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            if (!_charactersAtLocation[i].icon.isMovingToHex) {
                count++;
            }
        }
        return count;
    }
    public CharacterParty GetArmyWithMostOccupiedSlots() {
        CharacterParty mostArmy = null;
        for (int i = 0; i < charactersWithHomeOnLandmark.Count; i++) {
            Character currCharacter = charactersWithHomeOnLandmark[i];
            if (currCharacter is CharacterArmyUnit) {
                CharacterArmyUnit currUnit = (currCharacter as CharacterArmyUnit);
                if (mostArmy == null || mostArmy.characters.Count > currUnit.currentParty.characters.Count) {
                    mostArmy = currUnit.currentParty as CharacterParty;
                }
            }
        }
        return mostArmy;
    }
    public List<Character> GetIdleResidents() {
        List<Character> idleResidents = new List<Character>();
        for (int i = 0; i < charactersWithHomeOnLandmark.Count; i++) {
            Character currCharacter = charactersWithHomeOnLandmark[i];
            if (currCharacter.isDefender) {
                continue; //skip
            }
            if (currCharacter.ownParty is CharacterParty && (currCharacter.ownParty as CharacterParty).isBusy) {
                continue; //skip
            }
            idleResidents.Add(currCharacter);
        }
        return idleResidents;
    }
    public bool HasResidentAtHome() {
        for (int i = 0; i < charactersWithHomeOnLandmark.Count; i++) {
            Character currCharacter = charactersWithHomeOnLandmark[i];
            if (currCharacter.isDefender) {
                continue; //skip
            }
            if (currCharacter.ownParty.icon.isTravelling) {
                continue; //skip
            }
            if (currCharacter.ownParty.specificLocation.id == this.id) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Combat
    public bool HasHostileCharactersWith(Character character) {
        if (_charactersAtLocation.Where(x => x is MonsterParty).Any()) {
            return true;
        }
        return false;
    }
    #endregion

    #region Utilities
    public void SetLandmarkObject(LandmarkVisual obj) {
        _landmarkVisual = obj;
        _landmarkVisual.SetLandmark(this);
    }
    internal void ChangeLandmarkType(LANDMARK_TYPE newLandmarkType) {
        _specificLandmarkType = newLandmarkType;
        Initialize();
    }
    public void CenterOnLandmark() {
        CameraMove.Instance.CenterCameraOn(this.tileLocation.gameObject);
    }
    public override string ToString() {
        return this.landmarkName;
    }
    public void SetIsAttackingAnotherLandmarkState(bool state) {
        _isAttackingAnotherLandmark = state;
    }
    private void OnToggleCharactersVisibility() {
        //_landmarkVisual.ToggleCharactersVisibility();
    }
    //private void RemoveListeners() {
    //Messenger.RemoveListener(Signals.DAY_START, DailySupplyProduction);
    //}
    #endregion

    #region Prisoner
    internal void AddPrisoner(Character character) {
        _prisoners.Add(character);
    }
    internal void RemovePrisoner(Character character) {
        _prisoners.Remove(character);
    }
    #endregion

    #region History
    internal void AddHistory(Log log) {
        if (!_history.Contains(log)) {
            _history.Add(log);
            if (this._history.Count > 60) {
                this._history.RemoveAt(0);
            }
            //tileLocation.areaOfTile.AddHistory(log);
            Messenger.Broadcast(Signals.HISTORY_ADDED, this as object);
        }
    }
    #endregion

    #region Materials
    public void AdjustDurability(int amount) {
        //_landmarkObj.AdjustHP(amount);
        //_currDurability += amount;
        //_currDurability = Mathf.Clamp (_currDurability, 0, _totalDurability);
    }
    #endregion

    #region Items
    private void SpawnInitialLandmarkItems() {
        //LandmarkData data = LandmarkManager.Instance.GetLandmarkData(_specificLandmarkType);
        //for (int i = 0; i < data.itemData.Length; i++) {
        //    LandmarkItemData currItemData = data.itemData[i];
        //    Item createdItem = ItemManager.Instance.CreateNewItemInstance(currItemData.itemName);
        //    if (ItemManager.Instance.IsLootChest(createdItem)) {
        //        //chosen item is a loot crate, generate a random item
        //        string[] words = createdItem.itemName.Split(' ');
        //        int tier = System.Int32.Parse(words[1]);
        //        if (createdItem.itemName.Contains("Armor")) {
        //            createdItem = ItemManager.Instance.GetRandomTier(tier, ITEM_TYPE.ARMOR);
        //        } else if (createdItem.itemName.Contains("Weapon")) {
        //            createdItem = ItemManager.Instance.GetRandomTier(tier, ITEM_TYPE.WEAPON);
        //        }
        //    } else {
        //        //only set as unlimited if not from loot chest, since gear from loot chests are not unlimited
        //        createdItem.SetIsUnlimited(currItemData.isUnlimited);
        //    }
        //    //createdItem.SetExploreWeight(currItemData.exploreWeight);
        //    AddItemInLandmark(createdItem);
        //}
    }
    private QUALITY GetEquipmentQuality() {
        int crudeChance = 30;
        int exceptionalChance = crudeChance + 20;
        int chance = UnityEngine.Random.Range(0, 100);
        if (chance < crudeChance) {
            return QUALITY.CRUDE;
        } else if (chance >= crudeChance && chance < exceptionalChance) {
            return QUALITY.EXCEPTIONAL;
        }
        return QUALITY.NORMAL;
    }
    public void AddItem(Item item) {
        if (_itemsInLandmark.Contains(item)) {
            throw new System.Exception(this.landmarkName + " already has an instance of " + item.itemName);
        }
        _itemsInLandmark.Add(item);
        //item.SetPossessor (this);
        item.OnItemPlacedOnLandmark(this);
    }
    public void AddItem(List<Item> item) {
        for (int i = 0; i < item.Count; i++) {
            AddItem(item[i]);
        }
        //_itemsInLandmark.AddRange (item);
    }
    public void RemoveItemInLandmark(Item item) {
        _itemsInLandmark.Remove(item);
        Messenger.Broadcast(Signals.ITEM_REMOVED_FROM_LANDMARK, item, this);
    }
    public void RemoveItemInLandmark(string itemName) {
        for (int i = 0; i < itemsInLandmark.Count; i++) {
            Item currItem = itemsInLandmark[i];
            if (currItem.itemName.Equals(itemName)) {
                RemoveItemInLandmark(currItem);
                break;
            }
        }
    }
    public bool HasItem(string itemName) {
        for (int i = 0; i < _itemsInLandmark.Count; i++) {
            if (_itemsInLandmark[i].itemName == itemName) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Traces
    public void AddTrace(Character character) {
        GameDate expDate = GameManager.Instance.Today();
        expDate.AddMonths(90);
        if (!_characterTraces.ContainsKey(character)) {
            _characterTraces.Add(character, expDate);
        } else {
            SchedulingManager.Instance.RemoveSpecificEntry(_characterTraces[character], () => RemoveTrace(character));
            _characterTraces[character] = expDate;
        }
        SchedulingManager.Instance.AddEntry(expDate, () => RemoveTrace(character));
    }
    public void RemoveTrace(Character character) {
        if (_characterTraces.ContainsKey(character)) {
            if (GameManager.Instance.Today().IsSameDate(_characterTraces[character])) {
                _characterTraces.Remove(character);
            }
        }
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

    #region Objects
    public void SetObject(StructureObj obj) {
        //_landmarkObj = obj;
        //obj.SetObjectLocation(this);
        //obj.OnAddToLandmark(this);
    }
    #endregion

    #region Corruption
    public void ToggleCorruption(bool state) {
        if (state) {
            LandmarkManager.Instance.corruptedLandmarksCount++;
            if (!_hasBeenCorrupted) {
                _hasBeenCorrupted = true;
                _nextCorruptedTilesToCheck.Add(tileLocation);
            }
            //_diagonalLeftBlocked = 0;
            //_diagonalRightBlocked = 0;
            //_horizontalBlocked = 0;
            //tileLocation.region.LandmarkStartedCorruption(this);
            PutWallDown();
            Messenger.AddListener(Signals.DAY_ENDED, DoCorruption);
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
        if (!hasAdjacentCorruptedLandmark && LandmarkManager.Instance.corruptedLandmarksCount > 1) {
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
        Messenger.RemoveListener(Signals.DAY_ENDED, DoCorruption);
        //Messenger.Broadcast<BaseLandmark>("StopCorruption", this);
    }
    private void DoCorruption() {
        if (_nextCorruptedTilesToCheck.Count > 0) {
            int index = UnityEngine.Random.Range(0, _nextCorruptedTilesToCheck.Count);
            HexTile currentCorruptedTileToCheck = _nextCorruptedTilesToCheck[index];
            _nextCorruptedTilesToCheck.RemoveAt(index);
            SpreadCorruption(currentCorruptedTileToCheck);
        } else {
            StopSpreadCorruption();
        }
    }
    private void SpreadCorruption(HexTile originTile) {
        if (!originTile.CanThisTileBeCorrupted()) {
            return;
        }
        for (int i = 0; i < originTile.AllNeighbours.Count; i++) {
            HexTile neighbor = originTile.AllNeighbours[i];
            if (neighbor.uncorruptibleLandmarkNeighbors <= 0) {
                if (!neighbor.isCorrupted) { //neighbor.region.id == originTile.region.id && neighbor.CanThisTileBeCorrupted()
                    neighbor.SetCorruption(true, this);
                    _nextCorruptedTilesToCheck.Add(neighbor);
                }
                //if(neighbor.landmarkNeighbor != null && !neighbor.landmarkNeighbor.tileLocation.isCorrupted) {
                //    neighbor.landmarkNeighbor.CreateWall();
                //}
                if (originTile.corruptedLandmark.id != neighbor.corruptedLandmark.id) {
                    originTile.corruptedLandmark.hasAdjacentCorruptedLandmark = true;
                }
            }
            //else {
            //if cannot be corrupted it means that it has a landmark still owned by a kingdom
            //neighbor.landmarkOnTile.CreateWall();
            //}
        }
    }
    public void ALandmarkHasStartedCorruption(BaseLandmark corruptedLandmark) {
        //Messenger.RemoveListener<BaseLandmark>("StartCorruption", ALandmarkHasStartedCorruption);

        //int corruptedX = corruptedLandmark.tileLocation.xCoordinate;
        //int corruptedY = corruptedLandmark.tileLocation.yCoordinate;

        //string direction = "horizontal";
        ////if same column, the wall is automatically horizontal, if not, enter here
        //if (tileLocation.xCoordinate != corruptedX) {
        //    if (tileLocation.yCoordinate == corruptedY) {
        //        int chance = UnityEngine.Random.Range(0, 2);
        //        if (chance == 0) {
        //            direction = "diagonalleft";
        //        } else {
        //            direction = "diagonalright";
        //        }
        //    } else if (tileLocation.yCoordinate < corruptedY) {
        //        if (tileLocation.xCoordinate < corruptedX) {
        //            direction = "diagonalleft";
        //        } else {
        //            direction = "diagonalright";
        //        }
        //    } else {
        //        if (tileLocation.xCoordinate < corruptedX) {
        //            direction = "diagonalright";
        //        } else {
        //            direction = "diagonalleft";
        //        }
        //    }
        //}
        //int chance = UnityEngine.Random.Range(0, 3);
        //if (chance == 0) {
        //    direction = "diagonalleft";
        //} else {
        //    direction = "diagonalright";
        //}
        PutWallUp();
        // if (tileLocation.xCoordinate != corruptedX) {
        //    if (tileLocation.xCoordinate < corruptedX) {
        //        if(tileLocation.yCoordinate == corruptedY) {
        //            if(_diagonalLeftBlocked > 0 && _diagonalRightBlocked <= 0) {
        //                direction = "diagonalright";
        //            }else if (_diagonalLeftBlocked <= 0 && _diagonalRightBlocked > 0) {
        //                direction = "diagonalleft";
        //            } else {
        //                if (chance == 0) {
        //                    direction = "diagonalleft";
        //                } else {
        //                    direction = "diagonalright";
        //                }
        //            }
        //        } else {
        //            if(tileLocation.yCoordinate < corruptedY) {
        //                if (_diagonalLeftBlocked <= 0 && _horizontalBlocked > 0) {
        //                    direction = "diagonalleft";
        //                } else if (_diagonalLeftBlocked > 0 && _horizontalBlocked <= 0) {
        //                    direction = "horizontal";
        //                } else {
        //                    if (chance == 0) {
        //                        direction = "diagonalleft";
        //                    }
        //                }
        //            } else {
        //                if (_diagonalRightBlocked <= 0 && _horizontalBlocked > 0) {
        //                    direction = "diagonalright";
        //                } else if (_diagonalRightBlocked > 0 && _horizontalBlocked <= 0) {
        //                    direction = "horizontal";
        //                } else {
        //                    if (chance == 0) {
        //                        direction = "diagonalright";
        //                    }
        //                }
        //            }
        //        }
        //    } else {
        //        if (tileLocation.yCoordinate == corruptedY) {
        //            if (_diagonalLeftBlocked > 0 && _diagonalRightBlocked <= 0) {
        //                direction = "diagonalright";
        //            } else if (_diagonalLeftBlocked <= 0 && _diagonalRightBlocked > 0) {
        //                direction = "diagonalleft";
        //            } else {
        //                if (chance == 0) {
        //                    direction = "diagonalleft";
        //                } else {
        //                    direction = "diagonalright";
        //                }
        //            }
        //        } else {
        //            if (tileLocation.yCoordinate < corruptedY) {
        //                if (_diagonalRightBlocked <= 0 && _horizontalBlocked > 0) {
        //                    direction = "diagonalright";
        //                } else if (_diagonalRightBlocked > 0 && _horizontalBlocked <= 0) {
        //                    direction = "horizontal";
        //                } else {
        //                    if (chance == 0) {
        //                        direction = "diagonalright";
        //                    }
        //                }
        //            } else {
        //                if (_diagonalLeftBlocked <= 0 && _horizontalBlocked > 0) {
        //                    direction = "diagonalleft";
        //                } else if (_diagonalLeftBlocked > 0 && _horizontalBlocked <= 0) {
        //                    direction = "horizontal";
        //                } else {
        //                    if (chance == 0) {
        //                        direction = "diagonalleft";
        //                    }
        //                }
        //            }
        //        }
        //    }
        //} else {
        //    if (_horizontalBlocked > 0 && _diagonalLeftBlocked > 0 && _diagonalRightBlocked <= 0) {
        //        direction = "diagonalright";
        //    } else if (_horizontalBlocked > 0 && _diagonalLeftBlocked <= 0 && _diagonalRightBlocked > 0) {
        //        direction = "diagonalleft";
        //    } else if (_horizontalBlocked <= 0 && _diagonalLeftBlocked > 0 && _diagonalRightBlocked > 0) {
        //        direction = "horizontal";
        //    } else {
        //        if (chance == 0) {
        //            direction = "diagonalleft";
        //        } else {
        //            direction = "diagonalright";
        //        }
        //    }
        //}
        //AdjustDirectionBlocked(direction, 1);
        //_blockedLandmarkDirection.Add(corruptedLandmark, direction);
    }
    public void PutWallUp() {
        //_wallDirection = direction;
        //List<HexTile> wallTiles = _horizontalTiles;
        //if(direction == "diagonalleft") {
        //    wallTiles = _diagonalLeftTiles;
        //} else if (direction == "diagonalright") {
        //    wallTiles = _diagonalRightTiles;
        //}
        //for (int i = 0; i < wallTiles.Count; i++) {
        //    wallTiles[i].AdjustUncorruptibleLandmarkNeighbors(1);
        //}
        for (int i = 0; i < _wallTiles.Count; i++) {
            _wallTiles[i].AdjustUncorruptibleLandmarkNeighbors(1);
        }
    }
    private void PutWallDown() {
        for (int i = 0; i < _wallTiles.Count; i++) {
            _wallTiles[i].AdjustUncorruptibleLandmarkNeighbors(-1);
        }
        //for (int i = 0; i < tileLocation.AllNeighbours.Count; i++) {
        //    tileLocation.AllNeighbours[i].AdjustUncorruptibleLandmarkNeighbors(-1);
        //}
        //if(_wallDirection != string.Empty) {
        //    List<HexTile> wallTiles = _horizontalTiles;
        //    if (_wallDirection == "diagonalleft") {
        //        wallTiles = _diagonalLeftTiles;
        //    } else if (_wallDirection == "diagonalright") {
        //        wallTiles = _diagonalRightTiles;
        //    }
        //    for (int i = 0; i < wallTiles.Count; i++) {
        //        wallTiles[i].AdjustUncorruptibleLandmarkNeighbors(-1);
        //    }
        //    _wallDirection = string.Empty;
        //}
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
    #endregion

    #region Hextiles
    public void GenerateDiagonalLeftTiles() {
        AddTileRecursivelyByDirection(HEXTILE_DIRECTION.NORTH_WEST, tileLocation);
        AddTileRecursivelyByDirection(HEXTILE_DIRECTION.SOUTH_EAST, tileLocation);
    }
    public void GenerateDiagonalRightTiles() {
        AddTileRecursivelyByDirection(HEXTILE_DIRECTION.NORTH_EAST, tileLocation);
        AddTileRecursivelyByDirection(HEXTILE_DIRECTION.SOUTH_WEST, tileLocation);
    }
    public void GenerateHorizontalTiles() {
        AddTileRecursivelyByDirection(HEXTILE_DIRECTION.EAST, tileLocation);
        AddTileRecursivelyByDirection(HEXTILE_DIRECTION.WEST, tileLocation);
    }
    public void GenerateWallTiles() {
        //_wallTiles.AddRange(_diagonalLeftTiles);
        //_wallTiles.AddRange(_diagonalRightTiles);
        //_wallTiles.AddRange(_horizontalTiles);
        //_wallTiles.AddRange(tileLocation.AllNeighbours);
        _wallTiles = tileLocation.GetTilesInRange(2);
    }
    private void AddTileRecursivelyByDirection(HEXTILE_DIRECTION direction, HexTile originTile) {
        if (originTile.tileLocation.neighbourDirections.ContainsKey(direction)) {
            if (!originTile.tileLocation.neighbourDirections[direction].neighbourDirections.ContainsKey(direction)) {
                return;
            }
            HexTile directionTile = originTile.tileLocation.neighbourDirections[direction].neighbourDirections[direction];
            if (directionTile.landmarkOnTile == null) { //directionTile.region.id == originTile.region.id
                //string strDirection = "diagonalleft";
                //if (direction == HEXTILE_DIRECTION.NORTH_WEST || direction == HEXTILE_DIRECTION.SOUTH_EAST) {
                //    _diagonalLeftTiles.Add(directionTile);
                //} else if (direction == HEXTILE_DIRECTION.NORTH_EAST || direction == HEXTILE_DIRECTION.SOUTH_WEST) {
                //    _diagonalRightTiles.Add(directionTile);
                //    //strDirection = "diagonalright";
                //} else if (direction == HEXTILE_DIRECTION.EAST || direction == HEXTILE_DIRECTION.WEST) {
                //    _horizontalTiles.Add(directionTile);
                //    //strDirection = "horizontal";
                //}
                //directionTile.landmarkDirection.Add(this, strDirection);
                //AddTileRecursivelyByDirection(direction, directionTile);
            }
        }
    }
    #endregion

    #region Civilians
    public void SetCivilianCount(int count) {
        _civilianCount = count;
    }
    public void AdjustCivilianCount(int amount) {
        _civilianCount += amount;
    }
    #endregion

    #region Quest Board
    public void CreateQuestBoard() {
        questBoard = new QuestBoard(this);
    }
    public bool HasQuestBoard() {
        return questBoard != null;
    }
    #endregion

    #region IInteractable
    public void SetIsBeingInspected(bool state) {
        _isBeingInspected = state;
        //_landmarkVisual.ToggleCharactersVisibility();
        //if (_isBeingInspected) {
        //    Messenger.Broadcast(Signals.LANDMARK_INSPECTED, this);
        //}
    }
    public void SetHasBeenInspected(bool state) {
        _hasBeenInspected = state;
        //if (state) {
        //    if (owner != null && owner.id != PlayerManager.Instance.player.playerFaction.id) {
        //        PlayerManager.Instance.player.AddIntel(owner.factionIntel);
        //    }
        //    if (tileLocation.areaOfTile != null && tileLocation.areaOfTile.id != PlayerManager.Instance.player.playerArea.id) {
        //        PlayerManager.Instance.player.AddIntel(tileLocation.areaOfTile.locationIntel);
        //    }
        //}
    }
    public void EndedInspection() {
        UpdateLastInspection();
    }
    private void UpdateLastInspection() {
        _lastInspectedOfCharactersAtLocation.Clear();
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            LandmarkPartyData landmarkPartyData = new LandmarkPartyData {
                partyMembers = new List<Character>(_charactersAtLocation[i].characters),
                action = _charactersAtLocation[i].currentAction,
                currentDuration = _charactersAtLocation[i].currentDay
            };
            _lastInspectedOfCharactersAtLocation.Add(landmarkPartyData);
        }
        _lastInspectedItemsInLandmark.Clear();
        _lastInspectedItemsInLandmark.AddRange(_itemsInLandmark);
    }
    #endregion

    #region Camp
    //public void MigrateCharactersToAnotherLandmark() {
    //    List<BaseLandmark> homeLandmarks = GetNewHomeLandmarksFromArea();
    //    if (homeLandmarks.Count > 0) {
    //        BaseLandmark chosenLandmark = homeLandmarks[0];
    //        while (_charactersWithHomeOnLandmark.Count > 0) {
    //            Character character = _charactersWithHomeOnLandmark[0];
    //            RemoveCharacterHomeOnLandmark(character);
    //            chosenLandmark.AddCharacterHomeOnLandmark(character);
    //        }
    //        int civiliansPerLandmark = _civilianCount / homeLandmarks.Count;
    //        int civiliansRemainder = _civilianCount % homeLandmarks.Count;
    //        homeLandmarks[0].AdjustCivilianCount(civiliansRemainder);
    //        for (int i = 0; i < homeLandmarks.Count; i++) {
    //            homeLandmarks[i].AdjustCivilianCount(civiliansPerLandmark);
    //        }
    //        SetCivilianCount(0);
    //    } else {
    //        //Camp
    //        if (_specificLandmarkType == LANDMARK_TYPE.CAMP) {
    //            //If this is the last camp, cannot be migrated anymore, kill all civilians, characters will make a camp on their own
    //            SetCivilianCount(0);
    //            return;
    //        }
    //        int numOfCamps = 0;
    //        if (_charactersWithHomeOnLandmark.Count > 0) {
    //            numOfCamps = _charactersWithHomeOnLandmark.Count / 3;
    //            if (numOfCamps <= 0) {
    //                numOfCamps = 1;
    //            }
    //        } else if (_civilianCount > 0) {
    //            numOfCamps = _civilianCount / 50;
    //            if (numOfCamps <= 0) {
    //                numOfCamps = 1;
    //            }
    //        }
    //        if (numOfCamps > 0) {
    //            int civiliansPerCamp = _civilianCount / numOfCamps;
    //            int civiliansRemainder = _civilianCount % numOfCamps;
    //            for (int i = 0; i < numOfCamps; i++) {
    //                BaseLandmark camp = tileLocation.areaOfTile.CreateCampForHouse(this.tileLocation);
    //                camp.tileLocation.SetArea(tileLocation.areaOfTile);
    //                camp.AdjustCivilianCount(civiliansPerCamp);
    //                if (i == numOfCamps - 1) {
    //                    //Last camp
    //                    camp.AdjustCivilianCount(civiliansRemainder);
    //                    while (_charactersWithHomeOnLandmark.Count > 0) {
    //                        Character character = _charactersWithHomeOnLandmark[0];
    //                        RemoveCharacterHomeOnLandmark(character);
    //                        camp.AddCharacterHomeOnLandmark(character);
    //                    }
    //                } else {
    //                    int count = 3;
    //                    while (count > 0) {
    //                        if (_charactersWithHomeOnLandmark.Count > 0) {
    //                            Character character = _charactersWithHomeOnLandmark[0];
    //                            RemoveCharacterHomeOnLandmark(character);
    //                            camp.AddCharacterHomeOnLandmark(character);
    //                        } else {
    //                            break;
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}
    //private List<BaseLandmark> GetNewHomeLandmarksFromArea() {
    //    List<BaseLandmark> chosenLandmarks = new List<BaseLandmark>();
    //    if (tileLocation.areaOfTile != null) {
    //        for (int i = 0; i < tileLocation.areaOfTile.landmarks.Count; i++) {
    //            BaseLandmark landmark = tileLocation.areaOfTile.landmarks[i];
    //            if (landmark != this && landmark.landmarkObj.specificObjectType == _landmarkObj.specificObjectType) {
    //                chosenLandmarks.Add(landmark);
    //            }
    //        }
    //        if (chosenLandmarks.Count > 1) {
    //            chosenLandmarks = chosenLandmarks.OrderBy(x => x._charactersWithHomeOnLandmark.Count).ToList();
    //        }
    //    }
    //    return chosenLandmarks;
    //}
    #endregion

    #region Events
    public void AddAdvertisedEvent(GameEvent gameEvent) {
        if (!advertisedEvents.Contains(gameEvent)) {
            advertisedEvents.Add(gameEvent);
        }
    }
    public void RemoveAdvertisedEvent(GameEvent gameEvent) {
        advertisedEvents.Remove(gameEvent);
    }
    public bool HasEventOfType(GAME_EVENT eventType) {
        for (int i = 0; i < advertisedEvents.Count; i++) {
            if (advertisedEvents[i].type == eventType) {
                return true;
            }
        }
        return false;
    }
    #endregion

    //#region Supplies
    //public void DisableSupplyProductionUntil(GameDate dueDate) {
    //    SetSupplyProductionState(false);
    //    SchedulingManager.Instance.AddEntry(dueDate, () => SetSupplyProductionState(true));
    //}
    //public void SetSupplyProductionState(bool state) {
    //    canProduceSupplies = state;
    //}
    //public bool MeetsSupplyProductionRequirements() {
    //    switch (specificLandmarkType) {
    //        case LANDMARK_TYPE.FARM:
    //            return charactersWithHomeOnLandmark.Where(x => x.characterClass.className == "Farmer").Count() > 0; //(Requires at least 1 Farmer)
    //    }
    //    return true;
    ////}
    //#endregion

    //#region Defenders
    //public void AddDefender(ICharacter newDefender) {
    //    LandmarkData data = LandmarkManager.Instance.GetLandmarkData(specificLandmarkType);
    //    if (maxDefenderCount <= 0) {
    //        return; //no defender slots
    //    }
    //    if (defenders == null) {
    //        //set the defenders party as the party of the new defender
    //        defenders = newDefender.ownParty;
    //        defenders.SetMaxCharacters(maxDefenderCount);
    //        //apply buffs, if any, to new defender party
    //        for (int i = 0; i < defenderBuffs.Count; i++) {
    //            Buff currBuff = defenderBuffs[i];
    //            defenders.AddBuff(currBuff);
    //        }
    //    }

    //    if (defenders.icharacters.Count >= maxDefenderCount) {
    //        return; //if the current defender party members is more or equal to the maximum defenders allowed for the landmark type
    //    }
    //    if (newDefender is Character) {
    //        (newDefender as Character).OnSetAsDefender(this.tileLocation.areaOfTile);
    //    }
    //    defenders.AddCharacter(newDefender);
    //    //for (int i = 0; i < defenders.Length; i++) {
    //    //    Party currDefender = defenders[i];
    //    //    if (currDefender == null) {
    //    //        defenders[i] = newDefender;
    //    //        (newDefender.owner as Character).OnSetAsDefender(this);
    //    //        break;
    //    //    }
    //    //}
    //}
    //public void RemoveDefender(ICharacter defender) {
    //    if (defenders != null) {
    //        if (defenders.owner.id == defender.id) {
    //            //if the character that needs to be removed is the owner of the defender party
    //            //check if there are any other characters left in the defender party
    //            List<ICharacter> otherCharacters = new List<ICharacter>();
    //            for (int i = 0; i < defenders.icharacters.Count; i++) {
    //                ICharacter currCharacter = defenders.icharacters[i];
    //                if (defenders.owner.id != currCharacter.id) {
    //                    otherCharacters.Add(currCharacter);
    //                }
    //            }
    //            if (otherCharacters.Count > 0) {
    //                //if there are other characters left
    //                //set the defender party to the party of the first remaining character, 
    //                //then add all other characters to that party
    //                Party partyToUse = otherCharacters[0].ownParty;
    //                defenders = partyToUse;
    //                for (int i = 1; i < otherCharacters.Count; i++) {
    //                    partyToUse.AddCharacter(otherCharacters[i]);
    //                }
    //            } else {
    //                //there are no more other characters other than the owner
    //                //set the defenders to null
    //                defenders = null;
    //            }
    //            if (defender is Character) {
    //                (defender as Character).OnRemoveAsDefender();
    //            }
    //        } else {
    //            if (defenders.icharacters.Contains(defender)) {
    //                defenders.RemoveCharacter(defender);
    //                if (defender is Character) {
    //                    (defender as Character).OnRemoveAsDefender();
    //                }
    //            }
    //        }
    //    }
        
    //    //for (int i = 0; i < defenders.Length; i++) {
    //    //    Party currDefender = defenders[i];
    //    //    if (currDefender != null && currDefender.id == defender.id) {
    //    //        defenders[i] = null;
    //    //        (currDefender.owner as Character).OnRemoveAsDefender();
    //    //        break;
    //    //    }
    //    //}
    //}
    //public bool IsDefenderOfLandmark(Party party) {
    //    if (defenders != null) {
    //        if (defenders.id == party.id) {
    //            return true;
    //        } else {
    //            if (defenders.icharacters.Contains(party.owner)) {
    //                return true;
    //            }
    //        }
    //    }
    //    return false;
    //}
    //public bool HasEmptyDefenderSlot() {
    //    if (defenders == null || !defenders.isFull) {
    //        return true;
    //    }
    //    return false;
    //}
    ////public void SetInitialDefenderCount(int count) {
    ////    this.initialDefenderCount = count;
    ////}
    ////public void SetMaxDefenderCount(int count) {
    ////    this.maxDefenderCount = count;
    ////}
    //#endregion

    #region Interactions
    public void ConstructInitialInteractions() {
        //_landmarkInvestigation = new LandmarkInvestigation(this);
        //Interaction investigateInteraction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.INVESTIGATE, this);
        //Interaction pointOfInterest1 = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.ABANDONED_HOUSE, this);
        //Interaction pointOfInterest2 = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.UNEXPLORED_CAVE, this);
        //AddInteraction(investigateInteraction);
        //AddInteraction(pointOfInterest1);
        //AddInteraction(pointOfInterest2);
    }
    public void AddInteraction(Interaction interaction) {
        tileLocation.areaOfTile.AddInteraction(interaction);
    }
    public void RemoveInteraction(Interaction interaction) {
        tileLocation.areaOfTile.RemoveInteraction(interaction);
    }
    public Interaction GetInteractionOfType(INTERACTION_TYPE type) {
        for (int i = 0; i < _currentInteractions.Count; i++) {
            Interaction currInteraction = _currentInteractions[i];
            if (currInteraction.type == type) {
                return currInteraction;
            }
        }
        return null;
    }
    public void SetEventTriggerWeight(int weight) {
        this.eventTriggerWeight = weight;
    }
    public void SetNoEventTriggerWeight(int weight) {
        this.noEventTriggerWeight = weight;
    }
    public WeightedDictionary<INTERACTION_TYPE> GetInteractionWeights(BaseLandmark landmark) {
        WeightedDictionary<INTERACTION_TYPE> weights = new WeightedDictionary<INTERACTION_TYPE>();
        foreach (KeyValuePair<INTERACTION_TYPE, int> kvp in scenarios.dictionary) {
            if (GetInteractionOfType(kvp.Key) == null && InteractionManager.Instance.CanCreateInteraction(kvp.Key, landmark)) {
                weights.AddElement(kvp.Key, kvp.Value);
            }
        }
        return weights;
    }
    //public void SetMaxDailySupplyProductionAmount(int amount) {
    //    this.maxDailySupplyProduction = amount;
    //}
    //public void SetMinDailySupplyProductionAmount(int amount) {
    //    this.minDailySupplyProduction = amount;
    //}
    #endregion

    #region Raid
    public void SetRaidedState(bool state) {
        if (_isRaided != state) {
            _isRaided = state;
            if (state) {
                StartRaidedState();
            }
        }
    }
    private void StartRaidedState() {
        GameDate endRaidedDate = GameManager.Instance.Today();
        endRaidedDate.AddMonths(5);
        SchedulingManager.Instance.AddEntry(endRaidedDate, () => SetRaidedState(false));
    }
    #endregion

    //#region Buffs
    //public void AddDefenderBuff(Buff buff) {
    //    defenderBuffs.Add(buff);
    //    //apply buff to current defender
    //    if (defenders != null) {
    //        defenders.AddBuff(buff);
    //    }
    //}
    //public void RemoveDefenderBuff(Buff buff) {
    //    if (defenderBuffs.Contains(buff)) {
    //        defenderBuffs.Remove(buff);
    //        //apply debuff to current defender
    //        if (defenders != null) {
    //            defenders.RemoveBuff(buff);
    //        }
    //    }
        
    //}
    //#endregion

    #region Assault Army Party
    public void AddAssaultArmyParty(Party party) {
        _assaultParties.Add(party);
    }
    public void RemoveAssaultArmyParty(Party party) {
        _assaultParties.Remove(party);
    }
    #endregion

    #region Mobilization
    public void StartMobilization() {
        List<Character> charactersNotAssigned = new List<Character>();
        for (int i = 0; i < _charactersWithHomeOnLandmark.Count; i++) {
            charactersNotAssigned.Add(_charactersWithHomeOnLandmark[i]);
            //if((_charactersWithHomeOnLandmark[i] is CharacterArmyUnit || _charactersWithHomeOnLandmark[i] is MonsterArmyUnit) && _charactersWithHomeOnLandmark[i].currentParty.icharacters.Count == 1 && _charactersWithHomeOnLandmark[i].currentParty.specificLocation == this) {
            //}
        }

        //Assign army units to external tiles of area
        for (int i = 0; i < tileLocation.areaOfTile.exposedTiles.Count; i++) {
            BaseLandmark exposedTile = tileLocation.areaOfTile.exposedTiles[i];
            //if (exposedTile.defenders == null || exposedTile.defenders.icharacters.Count < 4) {
            //    if(charactersNotAssigned.Count > 0) {
            //        charactersNotAssigned[0].currentParty.specificLocation.RemoveCharacterFromLocation(charactersNotAssigned[0].currentParty);
            //        charactersNotAssigned[0].homeLandmark.RemoveCharacterHomeOnLandmark(charactersNotAssigned[0]);
            //        exposedTile.AddCharacterToLocation(charactersNotAssigned[0].currentParty);
            //        exposedTile.AddCharacterHomeOnLandmark(charactersNotAssigned[0]);
            //        //exposedTile.AddDefender(charactersNotAssigned[0]);
            //        charactersNotAssigned.RemoveAt(0);
            //    }
            //}
        }

        //Form or fill an assault party
        if(charactersNotAssigned.Count > 0) {
            if(_assaultParties.Count > 0) {
                bool alreadyHasLegitAssaultParty = false;
                for (int i = 0; i < _assaultParties.Count; i++) {
                    if (_assaultParties[i].characters.Count >= 4) {
                        alreadyHasLegitAssaultParty = true;
                        break;
                    }
                }
                if (!alreadyHasLegitAssaultParty) {
                    //Do this only once?
                    bool canFillAssaultParty = false;
                    for (int i = 0; i < _assaultParties.Count; i++) {
                        while (_assaultParties[i].characters.Count < 4) {
                            canFillAssaultParty = true;
                            if (charactersNotAssigned.Count > 0) {
                                _assaultParties[i].AddCharacter(charactersNotAssigned[0]);
                                charactersNotAssigned.RemoveAt(0);
                            } else {
                                break;
                            }
                        }
                        if (canFillAssaultParty) {
                            break;
                        }
                    }
                }
            } else {
                //Create an Assault party
                if (charactersNotAssigned.Count > 1) {
                    Party partyToBeFormed = charactersNotAssigned[0].currentParty;
                    charactersNotAssigned.RemoveAt(0);
                    while (partyToBeFormed.characters.Count < 4) {
                        if (charactersNotAssigned.Count > 0) {
                            partyToBeFormed.AddCharacter(charactersNotAssigned[0]);
                            charactersNotAssigned.RemoveAt(0);
                        } else {
                            break;
                        }
                    }
                    AddAssaultArmyParty(partyToBeFormed);
                }
            }
        }

        //Assign army units to internal tiles of area
        if (charactersNotAssigned.Count > 0) {
            for (int i = 0; i < tileLocation.areaOfTile.unexposedTiles.Count; i++) {
                BaseLandmark unexposedTile = tileLocation.areaOfTile.unexposedTiles[i];
                //if (unexposedTile.defenders == null || unexposedTile.defenders.icharacters.Count < 4) {
                //    if (charactersNotAssigned.Count > 0) {
                //        charactersNotAssigned[0].currentParty.specificLocation.RemoveCharacterFromLocation(charactersNotAssigned[0].currentParty);
                //        charactersNotAssigned[0].homeLandmark.RemoveCharacterHomeOnLandmark(charactersNotAssigned[0]);
                //        unexposedTile.AddCharacterToLocation(charactersNotAssigned[0].currentParty);
                //        unexposedTile.AddCharacterHomeOnLandmark(charactersNotAssigned[0]);
                //        //unexposedTile.AddDefender(charactersNotAssigned[0]);
                //        charactersNotAssigned.RemoveAt(0);
                //    }
                //}
            }
        }

        //Form additional assault units
        while (charactersNotAssigned.Count > 1) {
            Party partyToBeFormed = charactersNotAssigned[0].currentParty;
            charactersNotAssigned.RemoveAt(0);
            while (partyToBeFormed.characters.Count < 4) {
                if (charactersNotAssigned.Count > 0) {
                    partyToBeFormed.AddCharacter(charactersNotAssigned[0]);
                    charactersNotAssigned.RemoveAt(0);
                } else {
                    break;
                }
            }
            AddAssaultArmyParty(partyToBeFormed);
        }
    }
    #endregion
}
