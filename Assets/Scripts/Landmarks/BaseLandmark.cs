/*
 This is the base class for all landmarks.
 eg. Settlements(Cities), Resources, Dungeons, Lairs, etc.
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECS;

public class BaseLandmark : ILocation, IInteractable {
    protected int _id;
    protected string _landmarkName;
    protected LANDMARK_TYPE _specificLandmarkType;
    protected bool _canBeOccupied; //can the landmark be occupied?
    protected bool _isOccupied;
    protected bool _isBeingInspected;
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
    protected List<ICharacter> _charactersWithHomeOnLandmark;
    protected List<BaseLandmark> _connections;
    protected List<Character> _prisoners; //list of prisoners on landmark
    protected List<Log> _history;
    protected List<Party> _charactersAtLocation;
    protected List<LandmarkPartyData> _lastInspectedOfCharactersAtLocation;
    protected List<Item> _itemsInLandmark;
    protected List<Item> _lastInspectedItemsInLandmark;
    protected List<LANDMARK_TAG> _landmarkTags;
    protected List<HexTile> _nextCorruptedTilesToCheck;
    protected List<HexTile> _wallTiles;
    protected List<Secret> _secrets;
    protected List<Intel> _intels;
    protected List<string> _encounters;
    protected List<Interaction> _currentInteractions;
    protected Dictionary<Character, GameDate> _characterTraces; //Lasts for 60 days
    protected Dictionary<int, Combat> _combatHistory;
    protected Dictionary<RESOURCE, int> _resourceInventory;

    public bool hasAdjacentCorruptedLandmark;
    public QuestBoard questBoard { get; private set; }
    public List<GameEvent> advertisedEvents { get; private set; } //events happening at this landmark, that other characters can partake in
    //public int suppliesAtLandmark { get; private set; }
    public Party[] defenders { get; private set; }

    #region getters/setters
    public int id {
        get { return _id; }
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
    public List<ICharacter> charactersWithHomeOnLandmark {
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
	public List<Character> prisoners {
		get { return _prisoners; }
	}
	public List<Log> history{
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
    public List<Secret> secrets {
        get { return _secrets; }
    }
    public List<Intel> intels {
        get { return _intels; }
    }
    public List<string> encounters {
        get { return _encounters; }
    }
    public List<Interaction> currentInteractions {
        get { return _currentInteractions; }
    }
    public Dictionary<Character, GameDate> characterTraces {
		get { return _characterTraces; }
	}
    public List<HexTile> wallTiles {
        get { return _wallTiles; }
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
    public HiddenDesire hiddenDesire {
        get { return null; }
    }
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
        _charactersWithHomeOnLandmark = new List<ICharacter>();
        _prisoners = new List<Character>();
        _history = new List<Log>();
        _charactersAtLocation = new List<Party>();
        _lastInspectedOfCharactersAtLocation = new List<LandmarkPartyData>();
        _itemsInLandmark = new List<Item>();
        _lastInspectedItemsInLandmark = new List<Item>();
        _nextCorruptedTilesToCheck = new List<HexTile>();
        _wallTiles = new List<HexTile>();
        advertisedEvents = new List<GameEvent>();
        _secrets = new List<Secret>();
        _intels = new List<Intel>();
        _encounters = new List<string>();
        _currentInteractions = new List<Interaction>();
        _combatHistory = new Dictionary<int, Combat>();
        _characterTraces = new Dictionary<Character, GameDate>();
        defenders = new Party[LandmarkManager.MAX_DEFENDERS];
        Messenger.AddListener(Signals.TOGGLE_CHARACTERS_VISIBILITY, OnToggleCharactersVisibility);
    }
    public BaseLandmark(HexTile location, LANDMARK_TYPE specificLandmarkType) : this(){
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(specificLandmarkType);
        _id = Utilities.SetID(this);
        _location = location;
        _specificLandmarkType = specificLandmarkType;
        SetName(RandomNameGenerator.Instance.GetLandmarkName(specificLandmarkType));
        ConstructTags(landmarkData);
        SpawnInitialLandmarkItems();
    }
    public BaseLandmark(HexTile location, LandmarkSaveData data) : this(){
        _id = Utilities.SetID(this, data.landmarkID);
        _location = location;
        _specificLandmarkType = data.landmarkType;
        SetName(data.landmarkName);

        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(specificLandmarkType);
        ConstructTags(landmarkData);
        SpawnInitialLandmarkItems();
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
    public virtual void Initialize() {}
	public virtual void DestroyLandmark(){
        ObjectState ruined = landmarkObj.GetState("Ruined");
        landmarkObj.ChangeState(ruined);
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
        if(_owner == null) {
            throw new System.Exception("Landmark doesn't have an owner but something is trying to unoccupy it!");
        }
        _isOccupied = false;
        _location.Unoccupy();
        _owner = null;
    }
	public void ChangeOwner(Faction newOwner){
		_owner = newOwner;
		_isOccupied = true;
		_location.Occupy();
	}
    #endregion

    #region Characters
    public void AddCharacterHomeOnLandmark(ICharacter character) {
        if (!_charactersWithHomeOnLandmark.Contains(character)) {
            _charactersWithHomeOnLandmark.Add(character);
            character.SetHomeLandmark(this);
        }
    }
    public void RemoveCharacterHomeOnLandmark(ICharacter character) {
        _charactersWithHomeOnLandmark.Remove(character);
        character.SetHomeLandmark(null);
    }
    public bool IsResident(ICharacter character) {
        return _charactersWithHomeOnLandmark.Contains(character);
    }
	public Character GetPrisonerByID(int id){
		for (int i = 0; i < _prisoners.Count; i++) {
			if (_prisoners [i].id == id){
				return _prisoners [i];
			}
		}
		return null;
	}
    #endregion

    #region Location
    public void AddCharacterToLocation(Party iparty) {
        if (!_charactersAtLocation.Contains(iparty)) {
            if (!IsDefenderOfLandmark(iparty)) {
                _charactersAtLocation.Add(iparty); //only add to characters list if the party is not a defender of the landmark
            }
            //this.tileLocation.RemoveCharacterFromLocation(iparty);
            if(iparty.specificLocation != null) {
                iparty.specificLocation.RemoveCharacterFromLocation(iparty);
            }
            iparty.SetSpecificLocation(this);
#if !WORLD_CREATION_TOOL
            _landmarkVisual.OnCharacterEnteredLandmark(iparty);
            Messenger.Broadcast<Party, BaseLandmark>(Signals.PARTY_ENTERED_LANDMARK, iparty, this);
#endif
        }
    }
    public void RemoveCharacterFromLocation(Party iparty, bool addToTile = true) {
        _charactersAtLocation.Remove(iparty);
        if (addToTile) {
            this.tileLocation.AddCharacterToLocation(iparty);
        }
#if !WORLD_CREATION_TOOL
        _landmarkVisual.OnCharacterExitedLandmark(iparty);
        Messenger.Broadcast<Party, BaseLandmark>(Signals.PARTY_EXITED_LANDMARK, iparty, this);
#endif
    }
    public void ReplaceCharacterAtLocation(Party ipartyToReplace, Party ipartyToAdd) {
        if (_charactersAtLocation.Contains(ipartyToReplace)) {
            int indexOfCharacterToReplace = _charactersAtLocation.IndexOf(ipartyToReplace);
            _charactersAtLocation.Insert(indexOfCharacterToReplace, ipartyToAdd);
            _charactersAtLocation.Remove(ipartyToReplace);
            ipartyToAdd.SetSpecificLocation(this);
        }
    }
    public bool IsCharacterAtLocation(ICharacter character) {
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            Party currParty = _charactersAtLocation[i];
            if (currParty.icharacters.Contains(character)) {
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
	internal void ChangeLandmarkType(LANDMARK_TYPE newLandmarkType){
		_specificLandmarkType = newLandmarkType;
		Initialize ();
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
        _landmarkVisual.ToggleCharactersVisibility();
    }
    //private void RemoveListeners() {
        //Messenger.RemoveListener(Signals.DAY_START, DailySupplyProduction);
    //}
    #endregion

    #region Prisoner
    internal void AddPrisoner(Character character){
		_prisoners.Add (character);
	}
	internal void RemovePrisoner(Character character){
		_prisoners.Remove (character);
	}
	#endregion

	#region History
    internal void AddHistory(Log log) {
        ////check if the new log is a duplicate of the latest log
        //Log latestLog = history.ElementAtOrDefault(history.Count - 1);
        //if (latestLog != null) {
        //    if (Utilities.AreLogsTheSame(log, latestLog)) {
        //        string text = landmarkName + " has duplicate logs!";
        //        text += "\n" + log.id + Utilities.LogReplacer(log) + " ST:" + log.logCallStack;
        //        text += "\n" + latestLog.id + Utilities.LogReplacer(latestLog) + " ST:" + latestLog.logCallStack;
        //        throw new System.Exception(text);
        //    }
        //}
        log.SetInspected(_isBeingInspected);
        _history.Add(log);
        if (this._history.Count > 60) {
            this._history.RemoveAt(0);
        }
        Messenger.Broadcast(Signals.HISTORY_ADDED, this as object);
    }
    #endregion

    #region Materials
    public void AdjustDurability(int amount){
        _landmarkObj.AdjustHP(amount);
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
    public void AddItem(Item item){
        if (_itemsInLandmark.Contains(item)) {
            throw new System.Exception(this.landmarkName + " already has an instance of " + item.itemName);
        }
		_itemsInLandmark.Add (item);
		//item.SetPossessor (this);
        item.OnItemPlacedOnLandmark(this);
	}
	public void AddItem(List<Item> item){
        for (int i = 0; i < item.Count; i++) {
            AddItem(item[i]);
        }
		//_itemsInLandmark.AddRange (item);
	}
	public void RemoveItemInLandmark(Item item){
        _itemsInLandmark.Remove(item);
        Messenger.Broadcast(Signals.ITEM_REMOVED_FROM_LANDMARK, item, this);
    }
    public void RemoveItemInLandmark(string itemName) {
        for (int i = 0; i < itemsInLandmark.Count; i++) {
            ECS.Item currItem = itemsInLandmark[i];
            if (currItem.itemName.Equals(itemName)) {
                RemoveItemInLandmark(currItem);
                break;
            }
        }
    }
	public bool HasItem(string itemName){
		for (int i = 0; i < _itemsInLandmark.Count; i++) {
			if (_itemsInLandmark [i].itemName == itemName) {
				return true;
			}
		}
		return false;
	}
    #endregion

	#region Traces
	public void AddTrace(Character character){
		GameDate expDate = GameManager.Instance.Today ();
		expDate.AddDays (90);
		if(!_characterTraces.ContainsKey(character)){
			_characterTraces.Add (character, expDate);
		}else{
			SchedulingManager.Instance.RemoveSpecificEntry (_characterTraces[character], () => RemoveTrace (character));
			_characterTraces [character] = expDate;
		}
		SchedulingManager.Instance.AddEntry (expDate, () => RemoveTrace (character));
	}
	public void RemoveTrace(Character character){
		if(_characterTraces.ContainsKey(character)){
			if(GameManager.Instance.Today().IsSameDate(_characterTraces[character])){
				_characterTraces.Remove (character);
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
        _landmarkObj = obj;
        //obj.SetObjectLocation(this);
        obj.OnAddToLandmark(this);
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
            Messenger.AddListener(Signals.HOUR_ENDED, DoCorruption);
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
            while(chosenTile == null) {
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
        Messenger.RemoveListener(Signals.HOUR_ENDED, DoCorruption);
        //Messenger.Broadcast<BaseLandmark>("StopCorruption", this);
    }
    private void DoCorruption() {
        if(_nextCorruptedTilesToCheck.Count > 0) {
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
        if(pathTiles != null) {
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
            if(directionTile.landmarkOnTile == null) { //directionTile.region.id == originTile.region.id
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
        _landmarkVisual.ToggleCharactersVisibility();
        if (_isBeingInspected) {
            Messenger.Broadcast(Signals.LANDMARK_INSPECTED, this);
        }
    }
    public void SetHasBeenInspected(bool state) {
        _hasBeenInspected = state;
    }
    public void EndedInspection() {
        UpdateLastInspection();
    }
    private void UpdateLastInspection() {
        _lastInspectedOfCharactersAtLocation.Clear();
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            LandmarkPartyData landmarkPartyData = new LandmarkPartyData {
                partyMembers = new List<ICharacter>(_charactersAtLocation[i].icharacters),
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
    public void MigrateCharactersToAnotherLandmark() {
        List<BaseLandmark> homeLandmarks = GetNewHomeLandmarksFromArea();
        if(homeLandmarks.Count > 0) {
            BaseLandmark chosenLandmark = homeLandmarks[0];
            while(_charactersWithHomeOnLandmark.Count > 0) {
                ICharacter character = _charactersWithHomeOnLandmark[0];
                RemoveCharacterHomeOnLandmark(character);
                chosenLandmark.AddCharacterHomeOnLandmark(character);
            }
            int civiliansPerLandmark = _civilianCount / homeLandmarks.Count;
            int civiliansRemainder = _civilianCount % homeLandmarks.Count;
            homeLandmarks[0].AdjustCivilianCount(civiliansRemainder);
            for (int i = 0; i < homeLandmarks.Count; i++) {
                homeLandmarks[i].AdjustCivilianCount(civiliansPerLandmark);
            }
            SetCivilianCount(0);
        } else {
            //Camp
            if(_specificLandmarkType == LANDMARK_TYPE.CAMP) {
                //If this is the last camp, cannot be migrated anymore, kill all civilians, characters will make a camp on their own
                SetCivilianCount(0);
                return;
            }
            int numOfCamps = 0;
            if(_charactersWithHomeOnLandmark.Count > 0) {
                numOfCamps = _charactersWithHomeOnLandmark.Count / 3;
                if (numOfCamps <= 0) {
                    numOfCamps = 1;
                }
            } else if (_civilianCount > 0) {
                numOfCamps = _civilianCount / 50;
                if (numOfCamps <= 0) {
                    numOfCamps = 1;
                }
            }
            if(numOfCamps > 0) {
                int civiliansPerCamp = _civilianCount / numOfCamps;
                int civiliansRemainder = _civilianCount % numOfCamps;
                for (int i = 0; i < numOfCamps; i++) {
                    BaseLandmark camp = tileLocation.areaOfTile.CreateCampForHouse(this.tileLocation);
                    camp.tileLocation.SetArea(tileLocation.areaOfTile);
                    camp.AdjustCivilianCount(civiliansPerCamp);
                    if (i == numOfCamps - 1) {
                        //Last camp
                        camp.AdjustCivilianCount(civiliansRemainder);
                        while (_charactersWithHomeOnLandmark.Count > 0) {
                            ICharacter character = _charactersWithHomeOnLandmark[0];
                            RemoveCharacterHomeOnLandmark(character);
                            camp.AddCharacterHomeOnLandmark(character);
                        }
                    } else {
                        int count = 3;
                        while (count > 0) {
                            if (_charactersWithHomeOnLandmark.Count > 0) {
                                ICharacter character = _charactersWithHomeOnLandmark[0];
                                RemoveCharacterHomeOnLandmark(character);
                                camp.AddCharacterHomeOnLandmark(character);
                            } else {
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
    private List<BaseLandmark> GetNewHomeLandmarksFromArea() {
        List<BaseLandmark> chosenLandmarks = new List<BaseLandmark>();
        if (tileLocation.areaOfTile != null) {
            for (int i = 0; i < tileLocation.areaOfTile.landmarks.Count; i++) {
                BaseLandmark landmark = tileLocation.areaOfTile.landmarks[i];
                if (landmark != this && landmark.landmarkObj.specificObjectType == _landmarkObj.specificObjectType && landmark.landmarkObj.currentState.stateName != "Ruined") {
                    chosenLandmarks.Add(landmark);
                }
            }
            if (chosenLandmarks.Count > 1) {
                chosenLandmarks = chosenLandmarks.OrderBy(x => x._charactersWithHomeOnLandmark.Count).ToList();
            }
        }
        return chosenLandmarks;
    }
    #endregion

    #region Events
    public void AddAdvertisedEvent(GameEvent gameEvent){
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
    //private void StartSupplyProduction(LandmarkData landmarkData) {
    //    if (landmarkData.dailySupplyProduction > 0) {
    //        Messenger.AddListener(Signals.DAY_START, DailySupplyProduction);
    //    }
    //}
    //private void DailySupplyProduction() {
    //    LandmarkData data = LandmarkManager.Instance.GetLandmarkData(specificLandmarkType);
    //    if (tileLocation.areaOfTile != null) {
    //        //give supplies to area immediately
    //        tileLocation.areaOfTile.AdjustSuppliesInBank(data.dailySupplyProduction);
    //    } else {
    //        AdjustSupplies(data.dailySupplyProduction);
    //    }
    //}
    //public void SetSupplies(int amount) {
    //    suppliesAtLandmark = amount;
    //    suppliesAtLandmark = Mathf.Max(suppliesAtLandmark, 0);
    //}
    //public void AdjustSupplies(int amount) {
    //    suppliesAtLandmark += amount;
    //    suppliesAtLandmark = Mathf.Max(suppliesAtLandmark, 0);
    //}
    //#endregion

    #region Defenders
    public void AddDefender(Party newDefender) {
        for (int i = 0; i < defenders.Length; i++) {
            Party currDefender = defenders[i];
            if (currDefender == null) {
                defenders[i] = newDefender;
                (newDefender.owner as Character).OnSetAsDefender(this);
                break;
            }
        }
    }
    public void RemoveDefender(Party defender) {
        for (int i = 0; i < defenders.Length; i++) {
            Party currDefender = defenders[i];
            if (currDefender != null && currDefender.id == defender.id) {
                defenders[i] = null;
                (currDefender.owner as Character).OnRemoveAsDefender();
                break;
            }
        }
    }
    public bool IsDefenderOfLandmark(Party party) {
        for (int i = 0; i < defenders.Length; i++) {
            if (defenders[i] != null && defenders[i].id == party.id) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Interactions
    public void AddInteraction(Interaction interaction) {
        _currentInteractions.Add(interaction);
    }
    public bool HasActiveInteraction() {
        //if this landmark already has a landmark other than investigate
        for (int i = 0; i < _currentInteractions.Count; i++) {
            Interaction currInteraction = _currentInteractions[i];
            if (!(currInteraction is InvestigateInteraction)) {
                return true;
            }
        }
        return false;
    }
    #endregion
}
