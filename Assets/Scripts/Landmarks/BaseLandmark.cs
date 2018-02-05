/*
 This is the base class for all landmarks.
 eg. Settlements(Cities), Resources, Dungeons, Lairs, etc.
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseLandmark : ILocation {
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
    protected Dictionary<RESOURCE, int> _resourceInventory; //list of resources available on landmark
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

    #region getters/setters
    public int id {
        get { return _id; }
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
		get { return civiliansWithReserved + _location.CharactersCount(); }
    }
	public int civilians {
		get { return (int)_civilians; }
    }
	public int civiliansWithReserved{
		get { return (int)_civilians + _reservedCivilians; }
	}
    public Dictionary<RESOURCE, int> resourceInventory {
        get { return _resourceInventory; }
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
        _resourceInventory = new Dictionary<RESOURCE, int>();
		_prisoners = new List<ECS.Character> ();
		_history = new List<string> ();
		_combatHistory = new Dictionary<int, ECS.CombatPrototype> ();
		_combatHistoryID = 0;
        _charactersAtLocation = new List<ICombatInitializer>();
        ConstructTechnologiesDictionary();
        InititalizeEncounterables();
    }

    public void SetLandmarkObject(LandmarkObject obj) {
        _landmarkObject = obj;
        _landmarkObject.SetLandmark(this);
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
        faction.AddLandmarkAsOwned(this);
        _location.Occupy();
        EnableInitialTechnologies(faction);
    }
    public virtual void UnoccupyLandmark() {
        if(_owner == null) {
            throw new System.Exception("Landmark doesn't have an owner but something is trying to unoccupy it!");
        }
        _isOccupied = false;
        _owner.RemoveLandmarkAsOwned(this);
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
            _technologies.Add(allTechnologies[i], false);
        }
    }
    /*
     Set the initial technologies of a faction as enabled on this landmark.
         */
    private void EnableInitialTechnologies(Faction faction) {
        SetTechnologyState(faction.inititalTechnologies, true);
    }
    /*
     Set the initital technologies of a faction as disabled on this landmark.
         */
    private void DisableInititalTechnologies(Faction faction) {
        SetTechnologyState(faction.inititalTechnologies, false);
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
            if(_owner != null && _owner.inititalTechnologies.Contains(technology)) {
                //Do not disable technology, since the owner of the landmark has that technology inherent to itself
            } else {
                SetTechnologyState(technology, false);
            }
        }
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
		this._history.Add (text);
		if(this._history.Count > 20){
			this._history.RemoveAt (0);
		}
	}
	#endregion
}
