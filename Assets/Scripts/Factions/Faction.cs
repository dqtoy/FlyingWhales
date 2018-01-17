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
    protected RACE _race;
    protected FACTION_TYPE _factionType;
    protected FACTION_SIZE _factionSize;
    private Sprite _emblem;
    private Sprite _emblemBG;
    [SerializeField] private List<Sprite> usedEmblems = new List<Sprite>();
    protected List<BaseLandmark> _ownedLandmarks;//List of settlements (cities/landmarks) owned by this faction
    protected List<TECHNOLOGY> _inititalTechnologies;
    internal Color factionColor;
    protected List<ECS.Character> _characters;
    protected InternalQuestManager _internalQuestManager;
    //TODO: Add list for characters that are part of the faction

    #region getters/setters
	public int id {
		get { return _id; }
	}
    public string name {
        get { return _name; }
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
    public List<BaseLandmark> ownedLandmarks {
        get { return _ownedLandmarks; }
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
    public InternalQuestManager internalQuestManager {
        get { return _internalQuestManager; }
    }
    #endregion

    public Faction(RACE race, FACTION_TYPE factionType) {
		this._id = Utilities.SetID<Faction> (this);
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
        _internalQuestManager = new InternalQuestManager(this);
    }

    public void SetRace(RACE race) {
        _race = race;
    }

    #region Settlements
    public void AddLandmarkAsOwned(BaseLandmark landmark) {
        if (!_ownedLandmarks.Contains(landmark)) {
            _ownedLandmarks.Add(landmark);
            RecalculateFactionSize();
        }
    }
    public void RemoveLandmarkAsOwned(BaseLandmark landmark) {
        _ownedLandmarks.Remove(landmark);
        RecalculateFactionSize();
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
        _characters.Add(character);
    }
    public void RemoveCharacter(ECS.Character character) {
        _characters.Remove(character);
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

	public ECS.Character GetCharacterByID(int id){
		for (int i = 0; i < _characters.Count; i++) {
			if(_characters[i].id == id){
				return _characters [i];
			}
		}
		return null;
	}
	public BaseLandmark GetLandmarkByID(int id){
		for (int i = 0; i < _ownedLandmarks.Count; i++) {
			if(_ownedLandmarks[i].id == id){
				return _ownedLandmarks [i];
			}
		}
		return null;
	}
	public Settlement GetSettlementWithHighestPopulation(){
		Settlement highestPopulationSettlement = null;
		for (int i = 0; i < _ownedLandmarks.Count; i++) {
			if(_ownedLandmarks[i] is Settlement){
				Settlement settlement = (Settlement)_ownedLandmarks [i];
				if(highestPopulationSettlement == null){
					highestPopulationSettlement = settlement;
				}else{
					if((int)settlement.civilians > (int)highestPopulationSettlement.civilians){
						highestPopulationSettlement = settlement;
					}
				}
			}
		}
		return highestPopulationSettlement;
	}
}
