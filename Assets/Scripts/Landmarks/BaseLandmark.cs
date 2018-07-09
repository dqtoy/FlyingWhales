/*
 This is the base class for all landmarks.
 eg. Settlements(Cities), Resources, Dungeons, Lairs, etc.
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECS;

//[System.Serializable]
public class BaseLandmark : ILocation {
    protected int _id;
    protected HexTile _location;
    protected LANDMARK_TYPE _specificLandmarkType;
    protected List<BaseLandmark> _connections;
    protected bool _canBeOccupied; //can the landmark be occupied?
    protected bool _isOccupied;
    protected string _landmarkName;
    protected Faction _owner;
    //protected List<Character> _charactersWithHomeOnLandmark;
    //protected Dictionary<RACE, int> _civiliansByRace;
    //protected int _currDurability;
    //protected int _totalDurability;
    protected LandmarkObject _landmarkVisual;
    protected List<Character> _prisoners; //list of prisoners on landmark
    protected List<Log> _history;
    protected int _combatHistoryID;
    protected Dictionary<int, Combat> _combatHistory;
    protected List<NewParty> _charactersAtLocation;
    protected List<Item> _itemsInLandmark;
    protected Dictionary<Character, GameDate> _characterTraces; //Lasts for 60 days
    protected List<LANDMARK_TAG> _landmarkTags;
    protected StructureObj _landmarkObj;
    //protected List<IObject> _objects;
    private Dictionary<RESOURCE, int> _resourceInventory;
    private List<HexTile> _nextCorruptedTilesToCheck;
    private bool _hasBeenCorrupted;
    protected bool _isAttackingAnotherLandmark;
    //private List<HexTile> _diagonalRightTiles;
    //private List<HexTile> _diagonalLeftTiles;
    //private List<HexTile> _horizontalTiles;
    private List<HexTile> _wallTiles;
    public bool hasAdjacentCorruptedLandmark;
    //private int _diagonalLeftBlocked;
    //private int _diagonalRightBlocked;
    //private int _horizontalBlocked;
    //private Dictionary<BaseLandmark, string> _blockedLandmarkDirection;
    private int _civilianCount;

    #region getters/setters
    public int id {
        get { return _id; }
    }
    public string locationName {
        get { return landmarkName; }
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
  //  public virtual int totalPopulation {
		//get { return civilians + CharactersCount(); }
  //  }
	//public int civilians {
	//	get { return _civiliansByRace.Sum(x => x.Value); }
 //   }
 //   public Dictionary<RACE, int> civiliansByRace {
 //       get { return _civiliansByRace; }
 //   }
    public LandmarkObject landmarkVisual {
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
    public List<NewParty> charactersAtLocation {
        get { return _charactersAtLocation; }
    }
	public HexTile tileLocation{
		get { return _location; }
	}
	public LOCATION_IDENTIFIER locIdentifier{
		get { return LOCATION_IDENTIFIER.LANDMARK; }
	}
	public List<Item> itemsInLandmark {
		get { return _itemsInLandmark; }
	}
    public int currDurability {
        get { return _landmarkObj.currentHP; }
    }
    public int totalDurability {
		get { return _landmarkObj.maxHP; }
    }
	public Dictionary<Character, GameDate> characterTraces {
		get { return _characterTraces; }
	}
    public StructureObj landmarkObj {
        get { return _landmarkObj; }
    }
    public List<HexTile> wallTiles {
        get { return _wallTiles; }
    }
    public bool isAttackingAnotherLandmark {
        get { return _isAttackingAnotherLandmark; }
    }
    //public List<IObject> objects {
    //    get { return _objects; }
    //}
    //public int diagonalLeftBlocked {
    //    get { return _diagonalLeftBlocked; }
    //}
    //public int diagonalRightBlocked {
    //    get { return _diagonalRightBlocked; }
    //}
    //public int horizontalBlocked {
    //    get { return _horizontalBlocked; }
    //}
    //public List<Character> charactersWithHomeOnLandmark {
    //    get { return _charactersWithHomeOnLandmark; }
    //}
    public int civilianCount {
        get { return _civilianCount; }
    }
    #endregion

    public BaseLandmark() {
        _connections = new List<BaseLandmark>();
        _owner = null; //landmark has no owner yet
        //_charactersWithHomeOnLandmark = new List<Character>();
        _prisoners = new List<Character>();
        _history = new List<Log>();
        _combatHistory = new Dictionary<int, Combat>();
        _combatHistoryID = 0;
        _charactersAtLocation = new List<NewParty>();
        _itemsInLandmark = new List<Item>();
        _characterTraces = new Dictionary<Character, GameDate>();
        //_totalDurability = landmarkData.hitPoints;
        //_currDurability = _totalDurability;
        //_objects = new List<IObject>();
        _nextCorruptedTilesToCheck = new List<HexTile>();
        _hasBeenCorrupted = false;
        //_diagonalLeftTiles = new List<HexTile>();
        //_diagonalRightTiles = new List<HexTile>();
        //_horizontalTiles = new List<HexTile>();
        _wallTiles = new List<HexTile>();
        hasAdjacentCorruptedLandmark = false;
        //_diagonalLeftBlocked = 0;
        //_diagonalRightBlocked = 0;
        //_horizontalBlocked = 0;
        //_blockedLandmarkDirection = new Dictionary<BaseLandmark, string>();
        //Messenger.AddListener<BaseLandmark>("StartCorruption", ALandmarkHasStartedCorruption);
        //Messenger.AddListener<BaseLandmark>("StopCorruption", ALandmarkHasStoppedCorruption);

        //ConstructResourceInventory();
    }
    public BaseLandmark(HexTile location, LANDMARK_TYPE specificLandmarkType) : this(){
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(specificLandmarkType);
        _id = Utilities.SetID(this);
        _location = location;
        _specificLandmarkType = specificLandmarkType;
        SetName(RandomNameGenerator.Instance.GetLandmarkName(specificLandmarkType));
        ConstructTags(landmarkData);
        //ConstructCiviliansDictionary();
        //GenerateCivilians();
        SpawnInitialLandmarkItems();
    }
    public BaseLandmark(HexTile location, LandmarkSaveData data) : this(){
        _id = Utilities.SetID(this, data.landmarkID);
        _location = location;
        _specificLandmarkType = data.landmarkType;
        SetName(data.landmarkName);

        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(specificLandmarkType);
        ConstructTags(landmarkData);
        //ConstructCiviliansDictionary();
        //GenerateCivilians();
        SpawnInitialLandmarkItems();
    }

    public void SetName(string name) {
        _landmarkName = name;
        if (_landmarkVisual != null) {
            _landmarkVisual.UpdateName();
        }
    }

    #region Virtuals
    public virtual void Initialize() {}
	public virtual void DestroyLandmark(bool putRuinStructure){}
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

    #region Population
    //   private void ConstructCiviliansDictionary() {
    //       _civiliansByRace = new Dictionary<RACE, int>();
    //       RACE[] allRaces = Utilities.GetEnumValues<RACE>();
    //       for (int i = 0; i < allRaces.Length; i++) {
    //           RACE currRace = allRaces[i];
    //           if(currRace != RACE.NONE) {
    //               _civiliansByRace.Add(currRace, 0);
    //           }
    //       }
    //   }
    //   private void GenerateCivilians() {
    //       Faction ownerOfRegion = tileLocation.region.owner;
    //       LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(specificLandmarkType);
    //       //int civilians = Random.Range(landmarkData.minCivilians, landmarkData.maxCivilians);
    //       RACE civiliansRace = RACE.NONE;
    //       //if (specificLandmarkType == LANDMARK_TYPE.GOBLIN_CAMP) {
    //       //    civiliansRace = RACE.GOBLIN;
    //       //} else {
    //           if (ownerOfRegion != null) {
    //               civiliansRace = ownerOfRegion.race;
    //           } else {
    //               civiliansRace = RACE.HUMANS;
    //               if (Random.Range(0, 2) == 1) {
    //                   civiliansRace = RACE.ELVES;
    //               }
    //           }
    //       //}
    //       AdjustCivilians(civiliansRace, civilians);
    //   }
    //   public void AdjustCivilians(RACE race, int amount, Character culprit = null) {
    //       _civiliansByRace[race] += amount;
    //       _civiliansByRace[race] = Mathf.Max(0, _civiliansByRace[race]);
    //	if(culprit != null){
    //		QuestManager.Instance.CreateHuntQuest (culprit);
    //	}
    //   }
    //   public void AdjustCivilians(Dictionary<RACE, int> civilians) {
    //       foreach (KeyValuePair<RACE, int> kvp in civilians) {
    //           AdjustCivilians(kvp.Key, kvp.Value);
    //       }
    //   }
    //   public Dictionary<RACE, int> ReduceCivilians(int amount) {
    //       Dictionary<RACE, int> reducedCivilians = new Dictionary<RACE, int>();
    //       for (int i = 0; i < Mathf.Abs(amount); i++) {
    //           RACE chosenRace = GetRaceBasedOnProportion();
    //           AdjustCivilians(chosenRace, -1);
    //           if (reducedCivilians.ContainsKey(chosenRace)) {
    //               reducedCivilians[chosenRace] += 1;
    //           } else {
    //               reducedCivilians.Add(chosenRace, 1);
    //           }
    //       }
    //       return reducedCivilians;
    //   }
    //public void KillAllCivilians(){
    //	RACE[] races = _civiliansByRace.Keys.ToArray ();
    //	for (int i = 0; i < races.Length; i++) {
    //		_civiliansByRace [races [i]] = 0;
    //	}
    //}
    //   protected RACE GetRaceBasedOnProportion() {
    //       WeightedDictionary<RACE> raceDict = new WeightedDictionary<RACE>(_civiliansByRace);
    //       if (raceDict.GetTotalOfWeights() > 0) {
    //           return raceDict.PickRandomElementGivenWeights();
    //       }
    //       throw new System.Exception("Cannot get race to produce!");
    //   }
    #endregion

    #region Characters
    /*
     Create a new character, given a role and class.
     This will also subtract from the civilian population.
         */
    public Character CreateNewCharacter(RACE raceOfChar, CHARACTER_ROLE charRole, string className, bool determineAction = true) {
        //RACE raceOfChar = GetRaceBasedOnProportion();
        Character newCharacter = CharacterManager.Instance.CreateNewCharacter(charRole, className, raceOfChar, Utilities.GetRandomGender(), _owner);
        newCharacter.SetHome(this.tileLocation.areaOfTile);
        //if (reduceCivilians) {
        //    AdjustCivilians(raceOfChar, -1);
        //}
        NewParty party = newCharacter.CreateNewParty();
        party.CreateIcon();
        this.owner.AddNewCharacter(newCharacter);
        this.AddCharacterToLocation(party);
        //this.AddCharacterHomeOnLandmark(newCharacter);
        party.icon.SetPosition(this.tileLocation.transform.position);
        //if (charRole != CHARACTER_ROLE.FOLLOWER) {
        //    //newCharacter.CreateNewParty(); //Automatically create a new party lead by this new character.
        //    if (determineAction) {
        //        newCharacter.DetermineAction();
        //    }
        //}
        return newCharacter;
    }
    /*
     Create a new character, given a role and class.
     This will also subtract from the civilian population.
         */
    //public Character CreateNewCharacter(RACE raceOfChar, CHARACTER_ROLE charRole, string className, bool determineAction = true) {
    //    Character newCharacter = CharacterManager.Instance.CreateNewCharacter(charRole, className, raceOfChar);
        
    //    newCharacter.SetHome(this);
    //    //if (reduceCivilians) {
    //    //    AdjustCivilians(raceOfChar, -1);
    //    //}
    //    if (owner != null) {
    //        newCharacter.SetFaction(owner);
    //        owner.AddNewCharacter(newCharacter);
    //    }
    //    newCharacter.CreateIcon();
    //    AddCharacterToLocation(newCharacter);
    //    AddCharacterHomeOnLandmark(newCharacter);
    //    newCharacter.icon.SetPosition(this.tileLocation.transform.position);
    //    if (determineAction) {
    //        newCharacter.DetermineAction();
    //    }
    //    Messenger.Broadcast(Signals.CHARACTER_CREATED, newCharacter);
    //    return newCharacter;
    //}

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
    public void AddCharacterToLocation(NewParty iparty) {
        if (!_charactersAtLocation.Contains(iparty)) {
            _charactersAtLocation.Add(iparty);
            //if (character.icharacterType == ICHARACTER_TYPE.CHARACTER) {
            //Character currChar = character as Character;
            this.tileLocation.RemoveCharacterFromLocation(iparty);
            iparty.SetSpecificLocation(this);
#if !WORLD_CREATION_TOOL
            _landmarkVisual.OnCharacterEnteredLandmark(iparty);
            Messenger.Broadcast<NewParty>(Signals.PARTY_ENTERED_LANDMARK, iparty);
#endif
        }
         //character.SetSpecificLocation(this);
    //        if (character.icharacterType == ICHARACTER_TYPE.CHARACTER) {
    //            Character currChar = character as Character;
				//this.tileLocation.RemoveCharacterFromLocation(currChar);
    //            currChar.SetSpecificLocation(this);
    //        } else if (character is Party) {
    //            Party currParty = character as Party;
				//this.tileLocation.RemoveCharacterFromLocation(currParty);
    //            currParty.SetSpecificLocation(this);
    //        }
            //if (!_hasScheduledCombatCheck) {
            //    ScheduleCombatCheck();
            //}
        //}
    }
    public void RemoveCharacterFromLocation(NewParty iparty) {
        _charactersAtLocation.Remove(iparty);
        //if (character.icharacterType == ICHARACTER_TYPE.CHARACTER) {
        //Character currChar = character as Character;
        iparty.SetSpecificLocation(null);
#if !WORLD_CREATION_TOOL
        _landmarkVisual.OnCharacterExitedLandmark(iparty);
#endif
    //}
        //character.SetSpecificLocation(null);
   //     if (character.icharacterType == ICHARACTER_TYPE.CHARACTER) {
   //         Character currChar = character as Character;
			//currChar.SetSpecificLocation(null);
   //     } else if (character is Party) {
   //         Party currParty = character as Party;
			//currParty.SetSpecificLocation(null);
   //     }
        //if (_charactersAtLocation.Count == 0 && _hasScheduledCombatCheck) {
        //    UnScheduleCombatCheck();
        //}
    }

    public void ReplaceCharacterAtLocation(NewParty ipartyToReplace, NewParty ipartyToAdd) {
        if (_charactersAtLocation.Contains(ipartyToReplace)) {
            int indexOfCharacterToReplace = _charactersAtLocation.IndexOf(ipartyToReplace);
            _charactersAtLocation.Insert(indexOfCharacterToReplace, ipartyToAdd);
            _charactersAtLocation.Remove(ipartyToReplace);
            ipartyToAdd.SetSpecificLocation(this);
    //        if (characterToAdd.icharacterType == ICHARACTER_TYPE.CHARACTER) {
    //            Character currChar = characterToAdd as Character;
				//this.tileLocation.RemoveCharacterFromLocation(currChar);
    //            currChar.SetSpecificLocation(this);
    //        } else if (characterToAdd is Party) {
    //            Party currParty = characterToAdd as Party;
				//this.tileLocation.RemoveCharacterFromLocation(currParty);
    //            currParty.SetSpecificLocation(this);
    //        }
            //if (!_hasScheduledCombatCheck) {
            //    ScheduleCombatCheck();
            //}
        }
    }
    // public int CharactersCount(bool includeHostile = false) {
    //     int count = 0;
    //     for (int i = 0; i < _charactersAtLocation.Count; i++) {
    //if (includeHostile && this._owner != null) {
    //	if(_charactersAtLocation[i].faction == null){
    //		continue;
    //	}else{
    //		FactionRelationship fr = this._owner.GetRelationshipWith (_charactersAtLocation [i].faction);
    //		if(fr != null && fr.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE){
    //			continue;
    //		}
    //	}
    //}
    //         if (_charactersAtLocation[i] is Party) {
    //             count += ((Party)_charactersAtLocation[i]).partyMembers.Count;
    //         } else {
    //             count += 1;
    //         }
    //     }
    //     return count;
    // }
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
    public void SetLandmarkObject(LandmarkObject obj) {
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

        _history.Add(log);
        if (this._history.Count > 20) {
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
    public void AddItemInLandmark(Item item){
        if (_itemsInLandmark.Contains(item)) {
            throw new System.Exception(this.landmarkName + " already has an instance of " + item.itemName);
        }
		_itemsInLandmark.Add (item);
		//item.SetPossessor (this);
        item.OnItemPlacedOnLandmark(this);
	}
	public void AddItemsInLandmark(List<Item> item){
		_itemsInLandmark.AddRange (item);
	}
	public void RemoveItemInLandmark(Item item){
		if(!item.isUnlimited){
			_itemsInLandmark.Remove (item);
			//item.SetPossessor (null);
		}
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
    //private WeightedDictionary<Item> GetExploreItemWeights() {
    //    WeightedDictionary<Item> itemWeights = new WeightedDictionary<Item>();
    //    for (int i = 0; i < _itemsInLandmark.Count; i++) {
    //        Item currItem = _itemsInLandmark[i];
    //        itemWeights.AddElement(currItem, currItem.exploreWeight);
    //    }
    //    return itemWeights;
    //}

	//public void SpawnItemInLandmark(string itemName, int exploreWeight, bool isUnlimited){
	//	Item item = ItemManager.Instance.CreateNewItemInstance (itemName);
	//	item.exploreWeight = exploreWeight;
	//	item.isUnlimited = isUnlimited;
	//	AddItemInLandmark (item);
	//}

	//public void SpawnItemInLandmark(Item item, int exploreWeight, bool isUnlimited){
	//	Item newItem = item.CreateNewCopy();
    //  newItem.exploreWeight = exploreWeight;
	//	newItem.isUnlimited = isUnlimited;
	//	AddItemInLandmark (newItem);
	//}
	public Item SpawnItemInLandmark(string itemName){
		Item item = ItemManager.Instance.CreateNewItemInstance (itemName);
		AddItemInLandmark (item);
		return item;
	}
	public Item SpawnItemInLandmark(Item item){
		Item newItem = item.CreateNewCopy();
		AddItemInLandmark (newItem);
		return item;
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
    //public bool AddObject(IObject obj) {
    //    if (!_objects.Contains(obj)) {
    //        _objects.Add(obj);
    //        obj.OnAddToLandmark(this);
    //        return true;
    //    }
    //    return false;
    //}
    //public void RemoveObject(IObject obj) {
    //    _objects.Remove(obj);
    //    obj.SetObjectLocation(null);
    //}
    //public IObject GetObject(OBJECT_TYPE objectType, string name) {
    //    for (int i = 0; i < _objects.Count; i++) {
    //        if(_objects[i].objectType == objectType && _objects[i].objectName == name) {
    //            return _objects[i];
    //        }
    //    }
    //    return null;
    //}
    //public IObject GetObject(string name) {
    //    for (int i = 0; i < _objects.Count; i++) {
    //        if (_objects[i].objectName == name) {
    //            return _objects[i];
    //        }
    //    }
    //    return null;
    //}
    #endregion

    #region Resource Inventory
    //public void ConstructResourceInventory() {
    //    _resourceInventory = new Dictionary<RESOURCE, int>();
    //    RESOURCE[] allResources = Utilities.GetEnumValues<RESOURCE>();
    //    for (int i = 0; i < allResources.Length; i++) {
    //        if (allResources[i] != RESOURCE.NONE) {
    //            _resourceInventory.Add(allResources[i], 0);
    //        }
    //    }
    //}
    //public void AdjustResource(RESOURCE resource, int amount) {
    //    _resourceInventory[resource] += amount;
    //}
    //public void TransferResourceTo(RESOURCE resource, int amount, BaseLandmark target) {
    //    AdjustResource(resource, -amount);
    //    target.AdjustResource(resource, amount);
    //}
    //public void TransferResourceTo(RESOURCE resource, int amount, CharacterObj target) {
    //    AdjustResource(resource, -amount);
    //    target.AdjustResource(resource, amount);
    //}
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
    //public void AdjustDiagonalLeftBlocked(int amount) {
    //    _diagonalLeftBlocked += amount;
    //    if(_diagonalLeftBlocked < 0) {
    //        _diagonalLeftBlocked = 0;
    //    }
    //}
    //public void AdjustDiagonalRightBlocked(int amount) {
    //    _diagonalRightBlocked += amount;
    //    if (_diagonalRightBlocked < 0) {
    //        _diagonalRightBlocked = 0;
    //    }
    //}
    //public void AdjustHorizontalBlocked(int amount) {
    //    _horizontalBlocked += amount;
    //    if (_horizontalBlocked < 0) {
    //        _horizontalBlocked = 0;
    //    }
    //}
    //public void AdjustDirectionBlocked(string direction, int amount) {
    //    if(direction == "diagonalleft") {
    //        AdjustDiagonalLeftBlocked(amount);
    //    }else if (direction == "diagonalright") {
    //        AdjustDiagonalRightBlocked(amount);
    //    }else if (direction == "horizontal") {
    //        AdjustHorizontalBlocked(amount);
    //    }
    //}
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
    //public void ALandmarkHasStoppedCorruption(BaseLandmark corruptedLandmark) {
    //    string direction = _blockedLandmarkDirection[corruptedLandmark];
    //    AdjustDirectionBlocked(direction, -1);
    //    _blockedLandmarkDirection.Remove(corruptedLandmark);
    //}

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
    //public bool IsDirectionBlocked(string direction) {
    //    if (!tileLocation.isCorrupted) {
    //        if(direction == "diagonalleft") {
    //            return _diagonalLeftBlocked > 0;
    //        }else if (direction == "diagonalright") {
    //            return _diagonalRightBlocked > 0;
    //        } else if (direction == "horizontal") {
    //            return _horizontalBlocked > 0;
    //        }
    //    }
    //    return false;
    //}
    #endregion

    #region Civilians
    public void SetCivilianCount(int count) {
        _civilianCount = count;
    }
    #endregion
}
