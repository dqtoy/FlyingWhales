/*
 This is the base class for all landmarks.
 eg. Settlements(Cities), Resources, Dungeons, Lairs, etc.
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseLandmark : ILocation, TaskCreator {
    protected int _id;
    protected HexTile _location;
    protected LANDMARK_TYPE _specificLandmarkType;
    protected List<object> _connections;
    protected bool _canBeOccupied; //can the landmark be occupied?
    protected bool _isOccupied;
    protected bool _isHidden; //is landmark hidden or discovered?
    protected bool _isExplored; //has landmark been explored?
    protected string _landmarkName;
    protected Faction _owner;
    protected float _civilians; //This only contains the number of civilians (not including the characters) refer to totalPopulation to get the sum of the 2
	protected int _reservedCivilians;
    protected List<ECS.Character> _charactersWithHomeOnLandmark;
    protected Dictionary<MATERIAL, MaterialValues> _materialsInventory; //list of materials in landmark
	protected Dictionary<PRODUCTION_TYPE, MATERIAL> _neededMaterials; //list of materials in landmark

    //TODO: Add list of items on landmark
    protected List<TECHNOLOGY> _technologiesOnLandmark;
    protected Dictionary<TECHNOLOGY, bool> _technologies; //list of technologies and whether or not the landmark has that type of technology
    protected LandmarkObject _landmarkObject;
    protected WeightedDictionary<ENCOUNTERABLE> _encounterables;
	protected IEncounterable _landmarkEncounterable;
	protected ENCOUNTERABLE _landmarkEncounterableType;
	protected List<ECS.Character> _prisoners; //list of prisoners on landmark
    protected List<string> _history;
	protected int _combatHistoryID;
	protected Dictionary<int, ECS.CombatPrototype> _combatHistory;
    protected List<ICombatInitializer> _charactersAtLocation;
    protected ECS.CombatPrototype _currentCombat;
	protected List<Quest> _activeQuests;

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
		get { return "[url=" + this._id.ToString() + "_landmark]" + _landmarkName + "[/url]"; }
	}
    public HexTile location {
        get { return _location; }
    }
    public LANDMARK_TYPE specificLandmarkType {
        get { return _specificLandmarkType; }
    }
    public List<object> connections {
        get { return _connections; }
    }
    public bool canBeOccupied {
        get { return _canBeOccupied; }
    }
    public bool isOccupied {
        get { return _isOccupied; }
    }
    public bool isHidden {
        get { return _isHidden; }
    }
    public bool isExplored {
        get { return _isExplored; }
    }
    public Faction owner {
        get { return _owner; }
    }
    public int totalPopulation {
		get { return civiliansWithReserved + _location.CharactersCount() + CharactersCount(); }
    }
	public int civilians {
		get { return (int)_civilians; }
    }
	public int civiliansWithReserved{
		get { return (int)_civilians + _reservedCivilians; }
	}
	public Dictionary<MATERIAL, MaterialValues> materialsInventory {
		get { return _materialsInventory; }
    }
    public Dictionary<TECHNOLOGY, bool> technologies {
        get { return _technologies; }
    }
    public LandmarkObject landmarkObject {
        get { return _landmarkObject; }
    }
    public WeightedDictionary<ENCOUNTERABLE> encounterables {
        get { return _encounterables; }
    }
	public IEncounterable landmarkEncounterable {
		get { return _landmarkEncounterable; }
	}
	public List<ECS.Character> prisoners {
		get { return _prisoners; }
	}
	public List<string> history{
		get { return this._history; }
	}
	public Dictionary<int, ECS.CombatPrototype> combatHistory {
		get { return _combatHistory; }
	}
    public List<ICombatInitializer> charactersAtLocation {
        get { return _charactersAtLocation; }
    }
	public List<Quest> activeQuests {
		get { return _activeQuests; }
	}
    #endregion

    public BaseLandmark(HexTile location, LANDMARK_TYPE specificLandmarkType) {
        _id = Utilities.SetID(this);
        _location = location;
        _specificLandmarkType = specificLandmarkType;
        _connections = new List<object>();
        _isHidden = true;
        _isExplored = false;
        _landmarkName = string.Empty; //TODO: Add name generation
        _owner = null; //landmark has no owner yet
        _civilians = 0f;
		_reservedCivilians = 0;
        _charactersWithHomeOnLandmark = new List<ECS.Character>();
		_prisoners = new List<ECS.Character> ();
		_history = new List<string> ();
		_combatHistory = new Dictionary<int, ECS.CombatPrototype> ();
		_combatHistoryID = 0;
        _charactersAtLocation = new List<ICombatInitializer>();
		_activeQuests = new List<Quest> ();
        ConstructTechnologiesDictionary();
		ConstructMaterialValues ();
        InititalizeEncounterables();
    }

    public void SetLandmarkObject(LandmarkObject obj) {
        _landmarkObject = obj;
        _landmarkObject.SetLandmark(this);
        if(this is ResourceLandmark) {
            SetHiddenState(false);
            SetExploredState(true);
        }
    }

    #region Connections
    public void AddConnection(BaseLandmark connection) {
        if (!_connections.Contains(connection)) {
            _connections.Add(connection);
        }
    }
    public void AddConnection(Region connection) {
        if (!_connections.Contains(connection)) {
            _connections.Add(connection);
        }
    }
    #endregion

    #region Ownership
    public virtual void OccupyLandmark(Faction faction) {
        _owner = faction;
        _isOccupied = true;
        SetHiddenState(false);
        SetExploredState(true);
        _location.Occupy();
        EnableInitialTechnologies(faction);
    }
    public virtual void UnoccupyLandmark() {
        if(_owner == null) {
            throw new System.Exception("Landmark doesn't have an owner but something is trying to unoccupy it!");
        }
        _isOccupied = false;
        _location.Unoccupy();
        DisableInititalTechnologies(_owner);
        _owner = null;
    }
    #endregion

    #region Technologies
    /*
     Initialize the technologies dictionary with all the available technologies
     and set them as disabled.
         */
    private void ConstructTechnologiesDictionary() {
        TECHNOLOGY[] allTechnologies = Utilities.GetEnumValues<TECHNOLOGY>();
        _technologies = new Dictionary<TECHNOLOGY, bool>();
        for (int i = 0; i < allTechnologies.Length; i++) {
            TECHNOLOGY currTech = allTechnologies[i];
            _technologies.Add(currTech, false);
        }
    }
    /*
     Set the initial technologies of a faction as enabled on this landmark.
         */
    private void EnableInitialTechnologies(Faction faction) {
        SetTechnologyState(faction.initialTechnologies, true);
    }
    /*
     Set the initital technologies of a faction as disabled on this landmark.
         */
    private void DisableInititalTechnologies(Faction faction) {
        SetTechnologyState(faction.initialTechnologies, false);
    }
    /*
     Enable/Disable technologies in a landmark.
         */
    public void SetTechnologyState(TECHNOLOGY technology, bool state) {
        if (!state) {
            if (!_technologiesOnLandmark.Contains(technology)) {
                //technology is not inherent to the landmark, so allow action
                _technologies[technology] = state;
            }
        } else {
            _technologies[technology] = state;
        }
    }
    /*
     Set multiple technologies states.
         */
    public void SetTechnologyState(List<TECHNOLOGY> technology, bool state) {
        for (int i = 0; i < technology.Count; i++) {
            TECHNOLOGY currTech = technology[i];
            SetTechnologyState(currTech, state);
        }
    }
    /*
     Add a technology that is inherent to the current landmark.
         */
    public void AddTechnologyOnLandmark(TECHNOLOGY technology) {
        if (!_technologiesOnLandmark.Contains(technology)) {
            _technologiesOnLandmark.Add(technology);
            SetTechnologyState(technology, true);
        }
    }
    /*
     Remove a technology that is inherent to the current landmark.
         */
    public void RemoveTechnologyOnLandmark(TECHNOLOGY technology) {
        if (_technologiesOnLandmark.Contains(technology)) {
            _technologiesOnLandmark.Remove(technology);
            if(_owner != null && _owner.initialTechnologies.Contains(technology)) {
                //Do not disable technology, since the owner of the landmark has that technology inherent to itself
            } else {
                SetTechnologyState(technology, false);
            }
        }
    }
    /*
     Does this landmark have a specific technology?
         */
    public bool HasTechnology(TECHNOLOGY technology) {
        return technologies[technology];
    }
    #endregion

    #region Population
    public void AdjustPopulation(float adjustment) {
        _civilians += adjustment;
    }
	public void AdjustReservedPopulation(int amount){
		_reservedCivilians += amount;
	}
    #endregion

    #region Characters
    /*
     Make a character consider this landmark as it's home.
         */
    public virtual void AddCharacterHomeOnLandmark(ECS.Character character) {
        if (!_charactersWithHomeOnLandmark.Contains(character)) {
            _charactersWithHomeOnLandmark.Add(character);
        }
    }
    public void RemoveCharacterHomeOnLandmark(ECS.Character character) {
        _charactersWithHomeOnLandmark.Remove(character);
    }
	public ECS.Character GetCharacterAtLocationByID(int id){
		for (int i = 0; i < _charactersAtLocation.Count; i++) {
			if(_charactersAtLocation[i]	is ECS.Character){
				if(((ECS.Character)_charactersAtLocation[i]).id == id){
					return (ECS.Character)_charactersAtLocation [i];
				}
			}
		}
		return null;
	}
	public Party GetPartyAtLocationByLeaderID(int id){
		for (int i = 0; i < _charactersAtLocation.Count; i++) {
			if(_charactersAtLocation[i]	is Party){
				if(((Party)_charactersAtLocation[i]).partyLeader.id == id){
					return (Party)_charactersAtLocation [i];
				}
			}
		}
		return null;
	}
    #endregion

    #region Encounterables
    protected virtual void InititalizeEncounterables() {
        _encounterables = new WeightedDictionary<ENCOUNTERABLE>();
    }
    #endregion

    #region Party
    public List<Party> GetPartiesOnLandmark() {
        List<Party> parties = new List<Party>();
        for (int i = 0; i < _location.charactersAtLocation.Count; i++) {
			if(_location.charactersAtLocation[i] is Party){
				parties.Add((Party)_location.charactersAtLocation[i]);
			}
        }
        return parties;
    }
    #endregion

    #region Location
    public void AddCharacterToLocation(ICombatInitializer character, bool startCombat = true) {
        if (!_charactersAtLocation.Contains(character)) {
            _charactersAtLocation.Add(character);
            if (character is ECS.Character) {
                ECS.Character currChar = character as ECS.Character;
                this.location.RemoveCharacterFromLocation(currChar);
                currChar.SetLocation(this.location);
                currChar.SetSpecificLocation(this);
            } else if (character is Party) {
                Party currParty = character as Party;
                this.location.RemoveCharacterFromLocation(currParty);
                currParty.SetLocation(this.location);
                currParty.SetSpecificLocation(this);
            }
            if (startCombat) {
                StartCombatAtLocation();
            }
        }
    }
    public void RemoveCharacterFromLocation(ICombatInitializer character) {
        _charactersAtLocation.Remove(character);
        if (character is ECS.Character) {
            ECS.Character currChar = character as ECS.Character;
            currChar.SetSpecificLocation(this.location); //make the characters location, the hex tile that this landmark is on, meaning that the character exited the structure
        } else if (character is Party) {
            Party currParty = character as Party;
            currParty.SetSpecificLocation(this.location);//make the party's location, the hex tile that this landmark is on, meaning that the party exited the structure
        }
    }
    public int CharactersCount() {
        int count = 0;
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            if (_charactersAtLocation[i] is Party) {
                count += ((Party)_charactersAtLocation[i]).partyMembers.Count;
            } else {
                count += 1;
            }
        }
        return count;
    }
    #endregion

    #region Combat
    public void StartCombatAtLocation() {
        if (!CombatAtLocation()) {
            this._currentCombat = null;
            for (int i = 0; i < _charactersAtLocation.Count; i++) {
                _charactersAtLocation[i].SetIsDefeated(false);
            }
        }
    }
    public bool CombatAtLocation() {
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            if (_charactersAtLocation[i].InitializeCombat()) {
                return true;
            }
        }
        return false;
    }
    public ICombatInitializer GetCombatEnemy(ICombatInitializer combatInitializer) {
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            if (_charactersAtLocation[i] != combatInitializer) {
                if (_charactersAtLocation[i] is Party) {
                    if (((Party)_charactersAtLocation[i]).isDefeated) {
                        continue;
                    }
                }
                if (combatInitializer.CanBattleThis(_charactersAtLocation[i])) {
                    return _charactersAtLocation[i];
                }
            }
        }
        return null;
    }
    public void SetCurrentCombat(ECS.CombatPrototype combat) {
        _currentCombat = combat;
    }
    #endregion

    public void SetHiddenState(bool isHidden) {
        _isHidden = isHidden;
        landmarkObject.UpdateLandmarkVisual();
    }

    public void SetExploredState(bool isExplored) {
        _isExplored = isExplored;
        landmarkObject.UpdateLandmarkVisual();
    }
	internal bool IsBorder(){
		if(this.owner == null){
			return false;
		}
		for (int i = 0; i < this.location.region.connections.Count; i++) {
			if(this.location.region.connections[i] is Region){
				Region adjacentRegion = (Region)this.location.region.connections [i];
				if(adjacentRegion.centerOfMass.landmarkOnTile.owner != null && adjacentRegion.centerOfMass.landmarkOnTile.owner.id != this.owner.id){
					return true;
				}
			}
		}
		return false;
	}
	internal bool IsAdjacentToEnemyTribe(){
		if(this.owner == null || (this.owner != null && !(this.owner is Tribe))){
			return false;
		}
		for (int i = 0; i < this.location.region.connections.Count; i++) {
			if(this.location.region.connections[i] is Region){
				Region adjacentRegion = (Region)this.location.region.connections [i];
				if(adjacentRegion.centerOfMass.landmarkOnTile.owner != null && this.owner is Tribe && adjacentRegion.centerOfMass.landmarkOnTile.owner.id != this.owner.id){
					FactionRelationship factionRel = this._owner.GetRelationshipWith(adjacentRegion.centerOfMass.landmarkOnTile.owner);
					if(factionRel != null && factionRel.isAtWar){
						return true;
					}
				}
			}
		}
		return false;
	}
	internal bool HasWarlordOnAdjacentVillage(){
		if(this.owner == null){
			return false;
		}
		for (int i = 0; i < this.location.region.connections.Count; i++) {
			if(this.location.region.connections[i] is Region){
				Region adjacentRegion = (Region)this.location.region.connections [i];
				if(adjacentRegion.centerOfMass.landmarkOnTile.owner != null && adjacentRegion.centerOfMass.landmarkOnTile.owner.id != this.owner.id){
					FactionRelationship factionRel = this._owner.GetRelationshipWith(adjacentRegion.centerOfMass.landmarkOnTile.owner);
					if (factionRel != null && factionRel.isAtWar) {
						if (adjacentRegion.centerOfMass.landmarkOnTile.HasWarlord ()) {
							return true;
						}
					}
				}
			}
		}
		return false;
	}
	internal bool HasWarlord(){
		for (int i = 0; i < this._location.charactersAtLocation.Count; i++) {
			if(this._location.charactersAtLocation[i] is ECS.Character){
				if(((ECS.Character)this._location.charactersAtLocation[i]).role.roleType == CHARACTER_ROLE.WARLORD){
					return true;
				}
			}else if(this._location.charactersAtLocation[i] is Party){
				if(((Party)this._location.charactersAtLocation[i]).partyLeader.role.roleType == CHARACTER_ROLE.WARLORD){
					return true;
				}
			}
		}
		return false;
	}
	internal int GetTechnologyCount(){
		int count = 0;
		foreach (bool isTrue in _technologies.Values) {
			if(isTrue){
				count += 1;
			}
		}
		return count;
	}
	internal bool HasAdjacentUnoccupiedTile(){
		for (int i = 0; i < this._location.region.connections.Count; i++) {
			if(this._location.region.connections[i] is Region){
				Region adjacentRegion = (Region)this._location.region.connections [i];
				if(!adjacentRegion.centerOfMass.isOccupied){
					return true;
				}
			}
		}
		return false;
	}
	internal HexTile GetRandomAdjacentUnoccupiedTile(){
		List<HexTile> allUnoccupiedCenterOfMass = new List<HexTile> ();
		for (int i = 0; i < this._location.region.connections.Count; i++) {
			if(this._location.region.connections[i] is Region){
				Region adjacentRegion = (Region)this._location.region.connections [i];
				if(!adjacentRegion.centerOfMass.isOccupied && !this.owner.internalQuestManager.AlreadyHasQuestOfType(QUEST_TYPE.EXPAND, adjacentRegion.centerOfMass)){
					allUnoccupiedCenterOfMass.Add (adjacentRegion.centerOfMass);
				}
			}
		}
		if(allUnoccupiedCenterOfMass.Count > 0){
			return allUnoccupiedCenterOfMass [UnityEngine.Random.Range (0, allUnoccupiedCenterOfMass.Count)];
		}else{
			return null;
		}
	}

	#region Prisoner
	internal void AddPrisoner(ECS.Character character){
		character.SetPrisoner (true, this);
		_prisoners.Add (character);
	}
	internal void RemovePrisoner(ECS.Character character){
		_prisoners.Remove (character);
	}
	#endregion

	#region History
	internal void AddHistory(string text, object obj = null){
		GameDate today = GameManager.Instance.Today ();
		string date = "[" + ((MONTH)today.month).ToString() + " " + today.day + ", " + today.year + "]";
		if(obj != null){
			if(obj is ECS.CombatPrototype){
				ECS.CombatPrototype combat = (ECS.CombatPrototype)obj;
				if(this.combatHistory.Count > 20){
					this.combatHistory.Remove (0);
				}
				_combatHistoryID += 1;
				combatHistory.Add (_combatHistoryID, combat);
				string combatText = "[url=" + _combatHistoryID.ToString() + "_combat]" + text + "[/url]";
				text = combatText;
			}
		}
		this._history.Insert (0, date + " " + text);
		if(this._history.Count > 20){
			this._history.RemoveAt (this._history.Count - 1);
		}
	}
	#endregion

	#region Materials
	private void ConstructMaterialValues(){
		_materialsInventory = new Dictionary<MATERIAL, MaterialValues>();
		MATERIAL[] materials = Utilities.GetEnumValues<MATERIAL> ();
		for (int i = 1; i < materials.Length; i++) {
			_materialsInventory.Add (materials [i], new MaterialValues ());
		}
	}
	protected void ConstructNeededMaterials(){
		_neededMaterials = new Dictionary<PRODUCTION_TYPE, MATERIAL>();
		PRODUCTION_TYPE[] production = Utilities.GetEnumValues<PRODUCTION_TYPE> ();
		for (int i = 1; i < production.Length; i++) {
			_neededMaterials.Add (production [i], MATERIAL.NONE);
		}
	}
	internal virtual void AdjustMaterial(MATERIAL material, int amount){
		_materialsInventory [material].count += amount;
        _materialsInventory[material].count = Mathf.Max(_materialsInventory[material].count, 0);
	}
	internal void SetMaterial(MATERIAL material, int amount){
		_materialsInventory [material].count = amount;
	}
	internal void ReserveMaterial(MATERIAL material, int amount){
		AdjustMaterial (material, -amount);
		_materialsInventory [material].reserved += amount;
		if(_materialsInventory [material].reserved < 0){
			_materialsInventory [material].reserved = 0;
		}
	}
	internal void ReduceReserveMaterial(MATERIAL material, int amount){
		_materialsInventory [material].reserved -= amount;
		if(_materialsInventory [material].reserved < 0){
			_materialsInventory [material].reserved = 0;
		}
	}
	internal int GetTotalFoodCount(){
		int count = 0;
		for (int i = 0; i < MaterialManager.Instance.edibleMaterials.Count; i++) {
			MATERIAL material = MaterialManager.Instance.edibleMaterials [i];
			count += _materialsInventory [material].count;
		}
		return count;
	}
	internal Dictionary<MATERIAL, int> ReduceTotalFoodCount(int amount){
        Dictionary<MATERIAL, int> foodReduced = new Dictionary<MATERIAL, int>();
		int totalAmount = amount;
		for (int i = 0; i < MaterialManager.Instance.edibleMaterials.Count; i++) {
			MATERIAL material = MaterialManager.Instance.edibleMaterials [i];
			if(totalAmount > 0){
				if(totalAmount > _materialsInventory [material].count){
                    foodReduced.Add(material, materialsInventory[material].count);
                    totalAmount -= _materialsInventory [material].count;
					SetMaterial (material, 0);
                } else{
                    foodReduced.Add(material, totalAmount);
                    AdjustMaterial (material, -totalAmount);
					break;
				}
			}else{
				break;
			}
		}
        return foodReduced;

    }
    internal bool HasAvailableMaterial(MATERIAL material, int amount) {
        if(_materialsInventory[material].count >= amount) {
            return true;
        }
        return false;
    }
    /*
     Can this landmark afford to construct a structure?
         */
    public bool CanAffordConstruction(Construction constructionData) {
        if (GetTotalFoodCount() < constructionData.production.foodCost) {
            return false; //this landmark does not have enough food to build the structure
        }
        if(civilians < constructionData.production.civilianCost) {
            return false; //this landmark does not have enough civilians to build the structure
        }
        if(GetMaterialForConstruction(constructionData) == MATERIAL.NONE) {
            return false; //the landmark does not have any materials that can build the structure
        }
        return true; //this landmark meets all the requirements
    }
    public MATERIAL GetMaterialForConstruction(Construction constructionData) {
        List<MATERIAL> preferredMats = _owner.productionPreferences[PRODUCTION_TYPE.CONSTRUCTION].prioritizedMaterials;
        for (int i = 0; i < preferredMats.Count; i++) {
            MATERIAL currMat = preferredMats[i];
            if (constructionData.materials.Contains(currMat)) {
                if (HasAvailableMaterial(currMat, constructionData.production.resourceCost)) {
                    return currMat; //Check if this landmark has a resource with the required amount, that can build the structure
                }
            }
        }
        return MATERIAL.NONE;
    }
    /*
     This will reduce this landmarks assets based on
     a given Production Cost and a material. This will return
     a list of food materials that was reduced.
         */
    public Dictionary<MATERIAL, int> ReduceAssets(Production productionCost, MATERIAL materialToUse) {
        AdjustPopulation(-productionCost.civilianCost);
        AdjustMaterial(materialToUse, -productionCost.resourceCost);
        return ReduceTotalFoodCount(productionCost.foodCost);
    }
    /*
     This will reduce a landmarks assets based on Construction Data,
     this will determine what material to use on it's own. This will return
     a list of food materials that was reduced.
         */
    public Dictionary<MATERIAL, int> ReduceAssets(Construction constructionData) {
        AdjustPopulation(-constructionData.production.civilianCost);
        MATERIAL matToUse = GetMaterialForConstruction(constructionData);
        if(matToUse == MATERIAL.NONE) {
            throw new System.Exception("There is no materials to build a " + constructionData.structure.name);
        }
        AdjustMaterial(matToUse, -constructionData.production.resourceCost);
        return ReduceTotalFoodCount(constructionData.production.foodCost);
    }
    #endregion

    #region Quests
    public void AddNewQuest(Quest quest) {
		if (!_activeQuests.Contains(quest)) {
			_activeQuests.Add(quest);
			_owner.AddNewQuest(quest);
			if(quest.postedAt != null) {
				quest.postedAt.AddQuestToBoard(quest);
			}
			//quest.ScheduleDeadline(); //Once a quest has been added to active quest, scedule it's deadline
		}
	}
	public void RemoveQuest(Quest quest) {
		_activeQuests.Remove(quest);
		_owner.RemoveQuest(quest);
	}
	public List<Quest> GetQuestsOfType(QUEST_TYPE questType) {
		List<Quest> quests = new List<Quest>();
		for (int i = 0; i < _activeQuests.Count; i++) {
			Quest currQuest = _activeQuests[i];
			if(currQuest.questType == questType) {
				quests.Add(currQuest);
			}
		}
		return quests;
	}
	public bool AlreadyHasQuestOfType(QUEST_TYPE questType, object identifier){
		for (int i = 0; i < _activeQuests.Count; i++) {
			Quest currQuest = _activeQuests[i];
			if(currQuest.questType == questType) {
				if(questType == QUEST_TYPE.EXPLORE_REGION){
					Region region = (Region)identifier;
					if(((ExploreRegion)currQuest).regionToExplore.id == region.id){
						return true;
					}
				} else if(questType == QUEST_TYPE.EXPAND){
					if(identifier is HexTile){
						HexTile hexTile = (HexTile)identifier;
						if(((Expand)currQuest).targetUnoccupiedTile.id == hexTile.id){
							return true;
						}
					}else if(identifier is BaseLandmark){
						BaseLandmark landmark = (BaseLandmark)identifier;
						if(((Expand)currQuest).originTile.id == landmark.location.id){
							return true;
						}
					}

				} else if (questType == QUEST_TYPE.EXPLORE_TILE) {
					BaseLandmark landmark = (BaseLandmark)identifier;
					if (((ExploreTile)currQuest).landmarkToExplore.id == landmark.id) {
						return true;
					}
				} else if (questType == QUEST_TYPE.BUILD_STRUCTURE) {
					BaseLandmark landmark = (BaseLandmark)identifier;
					if (((BuildStructure)currQuest).target.id == landmark.id) {
						return true;
					}
				} else if (questType == QUEST_TYPE.OBTAIN_MATERIAL) {
					MATERIAL material = (MATERIAL)identifier;
					if (((ObtainMaterial)currQuest).materialToObtain == material) {
						return true;
					}
				}
			}
		}
		return false;
	}
    #endregion
}
