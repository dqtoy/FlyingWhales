/*
 This is the base class for all landmarks.
 eg. Settlements(Cities), Resources, Dungeons, Lairs, etc.
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
    protected Dictionary<RACE, int> _civiliansByRace;

    //TODO: Add list of items on landmark
    protected List<TECHNOLOGY> _technologiesOnLandmark;
    protected Dictionary<TECHNOLOGY, bool> _technologies; //list of technologies and whether or not the landmark has that type of technology
    protected LandmarkObject _landmarkObject;
	protected List<ECS.Character> _prisoners; //list of prisoners on landmark
    protected List<string> _history;
	protected int _combatHistoryID;
	protected Dictionary<int, ECS.CombatPrototype> _combatHistory;
    protected List<ICombatInitializer> _charactersAtLocation;
    protected ECS.CombatPrototype _currentCombat;
	protected List<OldQuest.Quest> _activeQuests;
	protected List<ECS.Item> _itemsInLandmark;

    private bool _hasScheduledCombatCheck = false;

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
    public virtual int totalPopulation {
		get { return civilians + CharactersCount(); }
    }
	public int civilians {
		get { return _civiliansByRace.Sum(x => x.Value); }
    }
    public Dictionary<RACE, int> civiliansByRace {
        get { return _civiliansByRace; }
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
	public List<OldQuest.Quest> activeQuests {
		get { return _activeQuests; }
	}
	public HexTile tileLocation{
		get { return _location; }
	}
	public LOCATION_IDENTIFIER locIdentifier{
		get { return LOCATION_IDENTIFIER.LANDMARK; }
	}
	public List<ECS.Item> itemsInLandmark {
		get { return _itemsInLandmark; }
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
        _charactersWithHomeOnLandmark = new List<ECS.Character>();
		_prisoners = new List<ECS.Character>();
		_history = new List<string>();
		_combatHistory = new Dictionary<int, ECS.CombatPrototype>();
		_combatHistoryID = 0;
        _charactersAtLocation = new List<ICombatInitializer>();
		_activeQuests = new List<OldQuest.Quest>();
		_itemsInLandmark = new List<ECS.Item> ();
        ConstructTechnologiesDictionary();
		ConstructMaterialValues();
        ConstructCiviliansDictionary();
        Inititalize();
    }

	#region Virtuals
	protected virtual void Inititalize() {}
	#endregion

    public void SetLandmarkObject(LandmarkObject obj) {
        _landmarkObject = obj;
        _landmarkObject.SetLandmark(this);
        //if(this is ResourceLandmark) {
        //    SetHiddenState(false);
        //    SetExploredState(true);
        //}
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
		AddHistory ("Occupied by " + _owner.name + ".");
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
    private void ConstructCiviliansDictionary() {
        _civiliansByRace = new Dictionary<RACE, int>();
        RACE[] allRaces = Utilities.GetEnumValues<RACE>();
        for (int i = 0; i < allRaces.Length; i++) {
            RACE currRace = allRaces[i];
            if(currRace != RACE.NONE) {
                _civiliansByRace.Add(currRace, 0);
            }
        }
    }
    public void AdjustCivilians(RACE race, int amount) {
        _civiliansByRace[race] += amount;
        _civiliansByRace[race] = Mathf.Max(0, _civiliansByRace[race]);
    }
    public void AdjustCivilians(Dictionary<RACE, int> civilians) {
        foreach (KeyValuePair<RACE, int> kvp in civilians) {
            AdjustCivilians(kvp.Key, kvp.Value);
        }
    }
    public Dictionary<RACE, int> ReduceCivilians(int amount) {
        Dictionary<RACE, int> reducedCivilians = new Dictionary<RACE, int>();
        for (int i = 0; i < Mathf.Abs(amount); i++) {
            RACE chosenRace = GetRaceBasedOnProportion();
            AdjustCivilians(chosenRace, -1);
            if (reducedCivilians.ContainsKey(chosenRace)) {
                reducedCivilians[chosenRace] += 1;
            } else {
                reducedCivilians.Add(chosenRace, 1);
            }
        }
        return reducedCivilians;
    }
    //public void AdjustPopulation(float adjustment) {
    //    _civilians += adjustment;
    //}
	//public void AdjustReservedPopulation(int amount){
	//	_reservedCivilians += amount;
	//}
    protected RACE GetRaceBasedOnProportion() {
        WeightedDictionary<RACE> raceDict = new WeightedDictionary<RACE>(_civiliansByRace);
        if (raceDict.GetTotalOfWeights() > 0) {
            return raceDict.PickRandomElementGivenWeights();
        }
        throw new System.Exception("Cannot get race to produce!");
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
	public ECS.Character GetPrisonerByID(int id){
		for (int i = 0; i < _prisoners.Count; i++) {
			if (_prisoners [i].id == id){
				return _prisoners [i];
			}
		}
		return null;
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
                currChar.SetSpecificLocation(this);
            } else if (character is Party) {
                Party currParty = character as Party;
                this.location.RemoveCharacterFromLocation(currParty);
                currParty.SetSpecificLocation(this);
            }
            if (!_hasScheduledCombatCheck) {
                ScheduleCombatCheck();
            }
            //if (startCombat) {
            //    StartCombatAtLocation();
            //}
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
        if (_charactersAtLocation.Count == 0 && _hasScheduledCombatCheck) {
            UnScheduleCombatCheck();
        }
    }
	public int CharactersCount(bool includeHostile = false) {
        int count = 0;
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
			if (includeHostile && this._owner != null) {
				if(_charactersAtLocation[i].faction == null){
					continue;
				}else{
					FactionRelationship fr = this._owner.GetRelationshipWith (_charactersAtLocation [i].faction);
					if(fr != null && fr.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE){
						continue;
					}
				}
			}
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
    public void ScheduleCombatCheck() {
        _hasScheduledCombatCheck = true;
        Messenger.AddListener("OnDayEnd", CheckForCombat);
    }
    public void UnScheduleCombatCheck() {
        _hasScheduledCombatCheck = false;
        Messenger.RemoveListener("OnDayEnd", CheckForCombat);
    }
    /*
     Check this location for encounters, start if any.
     Mechanics can be found at https://trello.com/c/PgK25YvC/837-encounter-mechanics.
         */
    public void CheckForCombat() {
        //At the start of each day:
        if (HasHostilities()) {
            //1. Attacking characters will attempt to initiate combat:
            CheckAttackingGroupsCombat();
            //2. Patrolling characters will attempt to initiate combat:
            CheckPatrollingGroupsCombat();
        }
        //3. Pillaging and Hunting characters will perform their daily action if they havent been engaged in combat
        //4. Exploring and Stealing characters will perform their daily action if they havent been engaged in combat
        //5. Resting and Hibernating characters will recover HP if they havent been engaged in combat
        ContinueDailyActions();
    }
    public void CheckAttackingGroupsCombat() {
        List<ICombatInitializer> attackingGroups = GetAttackingGroups();
        for (int i = 0; i < attackingGroups.Count; i++) {
            ICombatInitializer currAttackingGroup = attackingGroups[i];
            if (currAttackingGroup.isInCombat) {
                continue; //this current group is already in combat, skip it
            }
            //- If there are hostile parties in combat stance who are not engaged in combat, the attacking character will initiate combat with one of them at random
            List<ICombatInitializer> combatGroups = new List<ICombatInitializer>(GetGroupsBasedOnStance(STANCE.COMBAT, true, currAttackingGroup).Where(x => x.IsHostileWith(currAttackingGroup)));
            if (combatGroups.Count > 0) {
                ICombatInitializer chosenEnemy = combatGroups[Random.Range(0, combatGroups.Count)];
                StartCombatBetween(currAttackingGroup, chosenEnemy);
                continue; //the attacking group has found an enemy! skip to the next group
            }

            //Otherwise, if there are hostile parties in neutral stance who are not engaged in combat, the attacking character will initiate combat with one of them at random
            List<ICombatInitializer> neutralGroups = new List<ICombatInitializer>(GetGroupsBasedOnStance(STANCE.NEUTRAL, true, currAttackingGroup).Where(x => x.IsHostileWith(currAttackingGroup)));
            if (neutralGroups.Count > 0) {
                ICombatInitializer chosenEnemy = neutralGroups[Random.Range(0, neutralGroups.Count)];
                StartCombatBetween(currAttackingGroup, chosenEnemy);
                continue; //the attacking group has found an enemy! skip to the next group
            }

            //- Otherwise, if there are hostile parties in stealthy stance who are not engaged in combat, the attacking character will attempt to initiate combat with one of them at random.
            List<ICombatInitializer> stealthGroups = new List<ICombatInitializer>(GetGroupsBasedOnStance(STANCE.STEALTHY, true, currAttackingGroup).Where(x => x.IsHostileWith(currAttackingGroup)));
            if (stealthGroups.Count > 0) {
                //The chance of initiating combat is 35%
                if (Random.Range(0, 100) < 35) {
                    ICombatInitializer chosenEnemy = stealthGroups[Random.Range(0, stealthGroups.Count)];
                    StartCombatBetween(currAttackingGroup, chosenEnemy);
                    continue; //the attacking group has found an enemy! skip to the next group
                }
            }
        }
    }
    public void CheckPatrollingGroupsCombat() {
        List<ICombatInitializer> patrollingGroups = GetPatrollingGroups();
        for (int i = 0; i < patrollingGroups.Count; i++) {
            ICombatInitializer currPatrollingGroup = patrollingGroups[i];
            if (currPatrollingGroup.isInCombat) {
                continue; //this current group is already in combat, skip it
            }
            //- If there are hostile parties in combat stance who are not engaged in combat, the attacking character will initiate combat with one of them at random
            List<ICombatInitializer> combatGroups = new List<ICombatInitializer>(GetGroupsBasedOnStance(STANCE.COMBAT, true, currPatrollingGroup).Where(x => x.IsHostileWith(currPatrollingGroup)));
            if (combatGroups.Count > 0) {
                ICombatInitializer chosenEnemy = combatGroups[Random.Range(0, combatGroups.Count)];
                StartCombatBetween(currPatrollingGroup, chosenEnemy);
                continue; //the attacking group has found an enemy! skip to the next group
            }

            //Otherwise, if there are hostile parties in neutral stance who are not engaged in combat, the attacking character will initiate combat with one of them at random
            List<ICombatInitializer> neutralGroups = new List<ICombatInitializer>(GetGroupsBasedOnStance(STANCE.NEUTRAL, true, currPatrollingGroup).Where(x => x.IsHostileWith(currPatrollingGroup)));
            if (neutralGroups.Count > 0) {
                ICombatInitializer chosenEnemy = neutralGroups[Random.Range(0, neutralGroups.Count)];
                StartCombatBetween(currPatrollingGroup, chosenEnemy);
                continue; //the attacking group has found an enemy! skip to the next group
            }

            //- Otherwise, if there are hostile parties in stealthy stance who are not engaged in combat, the attacking character will attempt to initiate combat with one of them at random
            List<ICombatInitializer> stealthGroups = new List<ICombatInitializer>(GetGroupsBasedOnStance(STANCE.STEALTHY, true, currPatrollingGroup).Where(x => x.IsHostileWith(currPatrollingGroup)));
            if (stealthGroups.Count > 0) {
                //The chance of initiating combat is 35%
                if (Random.Range(0, 100) < 35) {
                    ICombatInitializer chosenEnemy = stealthGroups[Random.Range(0, stealthGroups.Count)];
                    StartCombatBetween(currPatrollingGroup, chosenEnemy);
                    continue; //the attacking group has found an enemy! skip to the next group
                }
            }
        }
    }
    public bool HasHostilities() {
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            ICombatInitializer currItem = _charactersAtLocation[i];
            for (int j = 0; j < _charactersAtLocation.Count; j++) {
                ICombatInitializer otherItem = _charactersAtLocation[j];
                if (currItem != otherItem) {
                    if (currItem.IsHostileWith(otherItem)) {
                        return true; //there are characters with hostilities
                    }
                }
            }
        }
        return false;
    }
    public List<ICombatInitializer> GetAttackingGroups() {
        List<ICombatInitializer> groups = new List<ICombatInitializer>();
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            ICombatInitializer currGroup = _charactersAtLocation[i];
            if (currGroup.currentTask is Attack) {
                groups.Add(currGroup);
            }
        }
        return groups;
    }
    public List<ICombatInitializer> GetPatrollingGroups() {
        List<ICombatInitializer> groups = new List<ICombatInitializer>();
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            ICombatInitializer currGroup = _charactersAtLocation[i];
            if (currGroup.currentTask is Defend) {
                groups.Add(currGroup);
            }
        }
        return groups;
    }
    public List<ICombatInitializer> GetGroupsBasedOnStance(STANCE stance, bool notInCombatOnly, ICombatInitializer except = null) {
        List<ICombatInitializer> groups = new List<ICombatInitializer>();
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            ICombatInitializer currGroup = _charactersAtLocation[i];
            if (notInCombatOnly) {
                if (currGroup.isInCombat) {
                    continue; //skip
                }
            }
            if (currGroup.GetCurrentStance() == stance) {
                if (except != null && currGroup == except) {
                    continue; //skip
                }
                groups.Add(currGroup);
            }
        }
        return groups;
    }
    public void StartCombatBetween(ICombatInitializer combatant1, ICombatInitializer combatant2) {
        ECS.CombatPrototype combat = new ECS.CombatPrototype(combatant1, combatant2, this);
        combatant1.SetIsInCombat(true);
        combatant2.SetIsInCombat(true);
        if (combatant1 is Party) {
            combat.AddCharacters(ECS.SIDES.A, (combatant1 as Party).partyMembers);
        } else {
            combat.AddCharacter(ECS.SIDES.A, combatant1 as ECS.Character);
        }
        if (combatant2 is Party) {
            combat.AddCharacters(ECS.SIDES.B, (combatant2 as Party).partyMembers);
        } else {
            combat.AddCharacter(ECS.SIDES.B, combatant2 as ECS.Character);
        }
        //this.specificLocation.SetCurrentCombat(combat);
        CombatThreadPool.Instance.AddToThreadPool(combat);
    }
    public void ContinueDailyActions() {
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            ICombatInitializer currItem = _charactersAtLocation[i];
            currItem.ContinueDailyAction();
        }
    }

    //public void StartCombatAtLocation() {
    //    if (!CombatAtLocation()) {
    //        this._currentCombat = null;
    //        for (int i = 0; i < _charactersAtLocation.Count; i++) {
    //            ICombatInitializer currItem = _charactersAtLocation[i];
    //            currItem.SetIsDefeated(false);
    //currItem.SetIsInCombat (false);
    //if(currItem.currentFunction != null){
    //	currItem.currentFunction ();
    //}
    //currItem.SetCurrentFunction(null);
    //        }
    //    } else {
    //        for (int i = 0; i < _charactersAtLocation.Count; i++) {
    //            ICombatInitializer currItem = _charactersAtLocation[i];
    //currItem.SetIsInCombat (true);
    //        }
    //    }
    //}
    //public bool CombatAtLocation() {
    //    for (int i = 0; i < _charactersAtLocation.Count; i++) {
    //        if (_charactersAtLocation[i].InitializeCombat()) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    //public ICombatInitializer GetCombatEnemy(ICombatInitializer combatInitializer) {
    //    for (int i = 0; i < _charactersAtLocation.Count; i++) {
    //        if (_charactersAtLocation[i] != combatInitializer) {
    //            if (_charactersAtLocation[i] is Party) {
    //                if (((Party)_charactersAtLocation[i]).isDefeated) {
    //                    continue;
    //                }
    //            }
    //            if (combatInitializer.IsHostileWith(_charactersAtLocation[i])) {
    //                return _charactersAtLocation[i];
    //            }
    //        }
    //    }
    //    return null;
    //}
    //public void SetCurrentCombat(ECS.CombatPrototype combat) {
    //    _currentCombat = combat;
    //}
    #endregion

    public void SetHiddenState(bool isHidden) {
        _isHidden = isHidden;
        if(landmarkObject != null) {
            landmarkObject.UpdateLandmarkVisual();
        }
    }

    public void SetExploredState(bool isExplored) {
        _isExplored = isExplored;
        if (landmarkObject != null) {
            landmarkObject.UpdateLandmarkVisual();
        }
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
            if (ProductionManager.Instance.constructionMaterials.Contains(currMat)) {
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
     NOTE: This will only reduce materials, not civilians
     civilian adjustment needs a separate call.
         */
    public Dictionary<MATERIAL, int> ReduceAssets(Production productionCost, MATERIAL materialToUse) {
        //AdjustPopulation(-productionCost.civilianCost);
        AdjustMaterial(materialToUse, -productionCost.resourceCost);
        return ReduceTotalFoodCount(productionCost.foodCost);
    }
    /*
     This will reduce a landmarks assets based on Construction Data,
     this will determine what material to use on it's own. This will return
     a list of food materials that was reduced.
     NOTE: This will only reduce materials, not civilians
     civilian adjustment needs a separate call.
         */
    public Dictionary<MATERIAL, int> ReduceAssets(Construction constructionData) {
        //AdjustPopulation(-constructionData.production.civilianCost);
        MATERIAL matToUse = GetMaterialForConstruction(constructionData);
        if(matToUse == MATERIAL.NONE) {
            throw new System.Exception("There is no materials to build a " + constructionData.structure.name);
        }
        AdjustMaterial(matToUse, -constructionData.production.resourceCost);
        return ReduceTotalFoodCount(constructionData.production.foodCost);
    }
    public bool HasMaterialsFor(PRODUCTION_TYPE prodType) {
        foreach (KeyValuePair<MATERIAL, MaterialValues> kvp in _materialsInventory) {
            MATERIAL currMat = kvp.Key;
            MaterialValues matVal = kvp.Value;
            if (matVal.count > 0) {
                if(MaterialManager.Instance.CanMaterialBeUsedFor(currMat, prodType)) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasAccessTo(PRODUCTION_TYPE prodType) {
        for (int i = 0; i < location.region.tilesInRegion.Count; i++) {
            HexTile currTile = location.region.tilesInRegion[i];
            if (currTile.materialOnTile != MATERIAL.NONE) {
                if (MaterialManager.Instance.CanMaterialBeUsedFor(currTile.materialOnTile, prodType)) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasAccessToFood() {
        for (int i = 0; i < location.region.tilesInRegion.Count; i++) {
            HexTile currTile = location.region.tilesInRegion[i];
            if (currTile.materialOnTile != MATERIAL.NONE) {
                if (MaterialManager.Instance.materialsLookup[currTile.materialOnTile].isEdible) {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region Quests
    public void AddNewQuest(OldQuest.Quest quest) {
		if (!_activeQuests.Contains(quest)) {
			_activeQuests.Add(quest);
			_owner.AddNewQuest(quest);
			if(quest.postedAt != null) {
				quest.postedAt.AddQuestToBoard(quest);
			}
			//quest.ScheduleDeadline(); //Once a quest has been added to active quest, scedule it's deadline
		}
	}
	public void RemoveQuest(OldQuest.Quest quest) {
		_activeQuests.Remove(quest);
		_owner.RemoveQuest(quest);
	}
	public List<OldQuest.Quest> GetQuestsOfType(QUEST_TYPE questType) {
		List<OldQuest.Quest> quests = new List<OldQuest.Quest>();
		for (int i = 0; i < _activeQuests.Count; i++) {
			OldQuest.Quest currQuest = _activeQuests[i];
			if(currQuest.questType == questType) {
				quests.Add(currQuest);
			}
		}
		return quests;
	}
	public bool AlreadyHasQuestOfType(QUEST_TYPE questType, object identifier){
		for (int i = 0; i < _activeQuests.Count; i++) {
			OldQuest.Quest currQuest = _activeQuests[i];
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
