/*
 This is the base class for each faction (major/minor)
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Faction {
	protected int _id;
    protected string _name;
    protected ECS.Character _leader;
    protected RACE _race;
    protected FACTION_TYPE _factionType;
    protected FACTION_SIZE _factionSize;
    private Sprite _emblem;
    private Sprite _emblemBG;
    [SerializeField] private List<Sprite> usedEmblems = new List<Sprite>();
    protected List<BaseLandmark> _ownedLandmarks;//List of settlements (cities/landmarks) owned by this faction
    protected List<TECHNOLOGY> _inititalTechnologies;
    internal Color factionColor;
    protected List<ECS.Character> _characters; //List of characters that are part of the faction
    protected List<Quest> _activeQuests;
    protected InternalQuestManager _internalQuestManager;
    protected Dictionary<Faction, FactionRelationship> _relationships;
	protected MilitaryManager _militaryManager;


    #region getters/setters
	public int id {
		get { return _id; }
	}
    public string name {
        get { return _name; }
    }
    public ECS.Character leader {
        get { return _leader; }
    }
    public RACE race {
        get { return _race; }
    }
    public FACTION_TYPE factionType {
        get { return _factionType; }
    }
    public FACTION_SIZE factionSize {
        get { return _factionSize; }
    }
    public Sprite emblem {
        get { return _emblem; }
    }
    public Sprite emblemBG {
        get { return _emblemBG; }
    }
    public List<BaseLandmark> ownedLandmarks {
        get { return _ownedLandmarks; }
    }
    public int totalPopulation {
        get { return settlements.Sum(x => x.totalPopulation); }
    }
    public List<Settlement> settlements {
        get { return _ownedLandmarks.Where(x => x is Settlement).Select(x => (Settlement)x).ToList(); }
    }
    public List<TECHNOLOGY> inititalTechnologies {
        get { return _inititalTechnologies; }
    }
    public List<ECS.Character> characters {
        get { return _characters; }
    }
    public List<Quest> activeQuests {
        get { return _activeQuests; }
    }
    public InternalQuestManager internalQuestManager {
        get { return _internalQuestManager; }
    }
    public Dictionary<Faction, FactionRelationship> relationships {
        get { return _relationships; }
    }
	public MilitaryManager militaryManager {
		get { return _militaryManager; }
	}
    #endregion

    public Faction(RACE race, FACTION_TYPE factionType) {
		this._id = Utilities.SetID<Faction>(this);
        SetRace(race);
        _name = RandomNameGenerator.Instance.GenerateKingdomName(race);
        _factionType = factionType;
        _factionSize = FACTION_SIZE.SMALL;
        _emblem = FactionManager.Instance.GenerateFactionEmblem(this);
        _emblemBG = FactionManager.Instance.GenerateFactionEmblemBG();
        _ownedLandmarks = new List<BaseLandmark>();
        factionColor = Utilities.GetColorForFaction();
        _characters = new List<ECS.Character>();
        ConstructInititalTechnologies();
        _activeQuests = new List<Quest>();
        _internalQuestManager = new InternalQuestManager(this);
        _relationships = new Dictionary<Faction, FactionRelationship>();
		_militaryManager = new MilitaryManager (this);
    }

    public void SetRace(RACE race) {
        _race = race;
    }

    #region virtuals
    /*
     Set the leader of this faction, change this per faction type if needed.
     This creates relationships between the leader and it's village heads by default.
         */
    public virtual void SetLeader(ECS.Character leader) {
        _leader = leader;
        List<ECS.Character> villageHeads = GetCharactersOfType(CHARACTER_ROLE.VILLAGE_HEAD);
        for (int i = 0; i < villageHeads.Count; i++) {
            ECS.Character currHead = villageHeads[i];
            //leaders have relationships with their Village heads and vise versa.
            CharacterManager.Instance.CreateNewRelationshipBetween(leader, currHead);
        }

    }
    #endregion

    #region Settlements
    public void AddLandmarkAsOwned(BaseLandmark landmark) {
        if (!_ownedLandmarks.Contains(landmark)) {
            _ownedLandmarks.Add(landmark);
            RecalculateFactionSize();
            FactionManager.Instance.UpdateFactionOrderBy();
        }
    }
    public void RemoveLandmarkAsOwned(BaseLandmark landmark) {
        _ownedLandmarks.Remove(landmark);
        RecalculateFactionSize();
        FactionManager.Instance.UpdateFactionOrderBy();
    }
    /*
     Recalculate the size of this faction given the 
     number of settlements it has.
         */
    private void RecalculateFactionSize() {
        int settlementCount = settlements.Count;
        if (settlementCount < FactionManager.Instance.smallToMediumReq) {
            _factionSize = FACTION_SIZE.SMALL;
        } else if (settlementCount >= FactionManager.Instance.smallToMediumReq && settlementCount < FactionManager.Instance.mediumToLargeReq) {
            _factionSize = FACTION_SIZE.MEDIUM;
        } else if (settlementCount >= FactionManager.Instance.mediumToLargeReq) {
            _factionSize = FACTION_SIZE.LARGE;
        }
    }
    #endregion

    #region Technologies
    protected void ConstructInititalTechnologies() {
        _inititalTechnologies = new List<TECHNOLOGY>();
        if (FactionManager.Instance.inititalRaceTechnologies.ContainsKey(this.race)) {
            _inititalTechnologies.AddRange(FactionManager.Instance.inititalRaceTechnologies[this.race]);
        }
    }
    #endregion

    #region Characters
    public void AddNewCharacter(ECS.Character character) {
        if (!_characters.Contains(character)) {
            _characters.Add(character);
            FactionManager.Instance.UpdateFactionOrderBy();
        }
    }
    public void RemoveCharacter(ECS.Character character) {
        _characters.Remove(character);
        FactionManager.Instance.UpdateFactionOrderBy();
    }
    public List<ECS.Character> GetCharactersOfType(CHARACTER_ROLE role) {
        List<ECS.Character> chars = new List<ECS.Character>();
        for (int i = 0; i < _characters.Count; i++) {
            ECS.Character currCharacter = _characters[i];
            if(currCharacter.role.roleType == role) {
                chars.Add(currCharacter);
            }
        }
        return chars;
    }
    #endregion

    #region Utilities
    public ECS.Character GetCharacterByID(int id) {
        for (int i = 0; i < _characters.Count; i++) {
            if (_characters[i].id == id) {
                return _characters[i];
            }
        }
        return null;
    }
    public BaseLandmark GetLandmarkByID(int id) {
        for (int i = 0; i < _ownedLandmarks.Count; i++) {
            if (_ownedLandmarks[i].id == id) {
                return _ownedLandmarks[i];
            }
        }
        return null;
    }
    public Settlement GetSettlementWithHighestPopulation() {
        Settlement highestPopulationSettlement = null;
        for (int i = 0; i < _ownedLandmarks.Count; i++) {
			if(_ownedLandmarks[i] is Settlement && _ownedLandmarks[i].specificLandmarkType == LANDMARK_TYPE.CITY){
                Settlement settlement = (Settlement)_ownedLandmarks[i];
                if (highestPopulationSettlement == null) {
                    highestPopulationSettlement = settlement;
                } else {
                    if ((int)settlement.civilians > (int)highestPopulationSettlement.civilians) {
                        highestPopulationSettlement = settlement;
                    }
                }
            }
        }
        return highestPopulationSettlement;
	}
	public bool IsAtWar(){
		//TODO: check if this faction is hostile to other factions, or in short, if this faction is at war
		return false;
	}
	public List<BaseLandmark> GetAllPossibleLandmarksToAttack(){
		List<BaseLandmark> allPossibleLandmarksToAttack = new List<BaseLandmark> ();
		for (int i = 0; i < _ownedLandmarks.Count; i++) {
			BaseLandmark ownedLandmark = _ownedLandmarks [i];
			for (int j = 0; j < ownedLandmark.location.region.landmarks.Count; j++) {
				BaseLandmark regionLandmark = ownedLandmark.location.region.landmarks [j];
				if(regionLandmark.owner != null && regionLandmark.owner.id != this._id && regionLandmark.owner.factionType == FACTION_TYPE.MINOR && regionLandmark.isExplored){
					//TODO: check if minor faction is hostile
					if(!_militaryManager.IsAlreadyBeingAttacked(regionLandmark)){
						allPossibleLandmarksToAttack.Add(regionLandmark);
					}
				}
			}
			for (int j = 0; j < ownedLandmark.location.region.connections.Count; j++) {
				if(ownedLandmark.location.region.connections[j] is Region){
					Region adjacentRegion = (Region)ownedLandmark.location.region.connections [j];
					if(adjacentRegion.centerOfMass.landmarkOnTile.owner != null && adjacentRegion.centerOfMass.landmarkOnTile.owner.id != this._id){
						//TODO: check if adjacentregion owner is at war with this faction
						if (!_militaryManager.IsAlreadyBeingAttacked (adjacentRegion.centerOfMass.landmarkOnTile)) {
							if(!allPossibleLandmarksToAttack.Contains(adjacentRegion.centerOfMass.landmarkOnTile)){
								allPossibleLandmarksToAttack.Add (adjacentRegion.centerOfMass.landmarkOnTile);
							}
						}
					}
				}
			}
		}
		return allPossibleLandmarksToAttack;
    }
    #endregion

    #region Relationships
    public void AddNewRelationship(Faction relWith, FactionRelationship relationship) {
        if (!_relationships.ContainsKey(relWith)) {
            _relationships.Add(relWith, relationship);
        } else {
            throw new System.Exception(this.name + " already has a relationship with " + relWith.name + ", but something is trying to create a new one!");
        }
    }
    public void RemoveRelationshipWith(Faction relWith) {
        if (_relationships.ContainsKey(relWith)) {
            _relationships.Remove(relWith);
        }
    }
    public FactionRelationship GetRelationshipWith(Faction faction) {
        if (_relationships.ContainsKey(faction)) {
            return _relationships[faction];
        }
        return null;
    }
    #endregion

    #region Quests
    public void AddNewQuest(Quest quest) {
        if (!_activeQuests.Contains(quest)) {
            _activeQuests.Add(quest);
        }
    }
    public void RemoveQuest(Quest quest) {
        _activeQuests.Remove(quest);
    }
    #endregion

    #region Death
    public void Death() {
        FactionManager.Instance.RemoveRelationshipsWith(this);
    }
    #endregion
}
