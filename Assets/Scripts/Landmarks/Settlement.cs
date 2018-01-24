/*
 This will replace the city script.
 */
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class Settlement : BaseLandmark {

    private CHARACTER_CLASS classToCreate;
    private CHARACTER_ROLE roleToCreate;

    private ECS.Character _headOfSettlement;

    private const int CHARACTER_LIMIT = 10;

    public Settlement(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) {
        _canBeOccupied = true;
        _isHidden = false;
    }

    #region Ownership
    public override void OccupyLandmark(Faction faction) {
        base.OccupyLandmark(faction);
        if (location.isHabitable) {
            //Create structures on location
            location.region.HighlightRegionTiles(faction.factionColor, 69f / 255f);
            location.CreateStructureOnTile(faction, STRUCTURE_TYPE.CITY);
            location.emptyCityGO.SetActive(false);
            _landmarkName = RandomNameGenerator.Instance.GenerateCityName(faction.race);
        }
        DecideCharacterToCreate(); //Start Character Creation Process
        IncreasePopulationPerMonth(); //Start Population Increase Process
    }
    #endregion

    #region Characters
    /*
     At the start of the month, the settlement will
     decide what character class and role to create.
         */
    protected void DecideCharacterToCreate() {
        if (civilians >= 1f && _charactersWithHomeOnLandmark.Count < CHARACTER_LIMIT) {
            //Check first if the settlement has enough civilians to create a new character
            //and that it has not exceeded the max number of characters that consider this settlement as home
            WeightedDictionary<CHARACTER_ROLE> characterRoleProductionDictionary = LandmarkManager.Instance.GetCharacterRoleProductionDictionary(this.owner);
            WeightedDictionary<CHARACTER_CLASS> characterClassProductionDictionary = LandmarkManager.Instance.GetCharacterClassProductionDictionary(this);
            if (characterRoleProductionDictionary.GetTotalOfWeights() > 0 && characterClassProductionDictionary.GetTotalOfWeights() > 0) {
                roleToCreate = characterRoleProductionDictionary.PickRandomElementGivenWeights();
                classToCreate = characterClassProductionDictionary.PickRandomElementGivenWeights();
            } else {
                roleToCreate = CHARACTER_ROLE.NONE;
                classToCreate = CHARACTER_CLASS.NONE;
            }
        } else {
            roleToCreate = CHARACTER_ROLE.NONE;
            classToCreate = CHARACTER_CLASS.NONE;
        }

        GameDate createCharacterDate = new GameDate(GameManager.Instance.month, GameManager.daysInMonth[GameManager.Instance.month], GameManager.Instance.year);
        SchedulingManager.Instance.AddEntry(createCharacterDate.month, createCharacterDate.day, createCharacterDate.year, () => CreateScheduledCharacter());
    }
    /*
     At the end of the month, the settlement will create a
     character given the attributes it decided at the start of the month.
         */
    protected void CreateScheduledCharacter() {
        if(roleToCreate != CHARACTER_ROLE.NONE && classToCreate != CHARACTER_CLASS.NONE) {
            if (civilians >= 1f && _charactersWithHomeOnLandmark.Count < CHARACTER_LIMIT) { 
                //Check first if the settlement has enough civilians to create a new character
                //and that it has not exceeded the max number of characters that consider this settlement as home
				CreateNewCharacter(roleToCreate, Utilities.NormalizeString(classToCreate.ToString()));
            }
        }
        GameDate decideCharacterDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
        decideCharacterDate.AddDays(1);
        SchedulingManager.Instance.AddEntry(decideCharacterDate.month, decideCharacterDate.day, decideCharacterDate.year, () => DecideCharacterToCreate());
    }
    /*
     Does the settlement have the required technology
     to produce a class?
         */
    public bool CanProduceClass(CHARACTER_CLASS charClass) {
        TECHNOLOGY neededTech = Utilities.GetTechnologyForCharacterClass(charClass);
        if (_technologies[neededTech]) {
            return true;
        }
        return false;
    }
    /*
     Create a new character, given a role and class.
     This will also subtract from the civilian population.
         */
	public ECS.Character CreateNewCharacter(CHARACTER_ROLE charRole, string className) {
        ECS.Character newCharacter = CharacterManager.Instance.CreateNewCharacter(charRole, className, _owner.race);
//        newCharacter.AssignRole(charRole);
        newCharacter.SetFaction(_owner);
        newCharacter.SetLocation(location);
		newCharacter.SetHome (this);
        this.AdjustPopulation(-1); //Adjust population by -1
        this.owner.AddNewCharacter(newCharacter);
        this._location.AddCharacterOnTile(newCharacter);
        this.AddCharacterHomeOnLandmark(newCharacter);
        newCharacter.DetermineAction();
        UIManager.Instance.UpdateFactionSummary();
        return newCharacter;
    }
    public List<Party> GetPartiesInSettlement() {
        List<Party> parties = new List<Party>();
		for (int i = 0; i < this._location.charactersOnTile.Count; i++) {
			ECS.Character currCharacter = this._location.charactersOnTile[i];
            if(currCharacter.party != null) {
                if (!parties.Contains(currCharacter.party)) {
                    parties.Add(currCharacter.party);
                }
            }
        }
        return parties;
    }
    public void SetHead(ECS.Character head) {
        _headOfSettlement = head;
        if(_owner.leader != null) {
            if(_headOfSettlement.GetRelationshipWith(_owner.leader) == null) {
                //Create Relationship between the head of the settlement and the leader of the faction that owns the settlement
                CharacterManager.Instance.CreateNewRelationshipBetween(_owner.leader, _headOfSettlement);
            }
        }

        for (int i = 0; i < _charactersWithHomeOnLandmark.Count; i++) {
            ECS.Character otherCharacter = _charactersWithHomeOnLandmark[i];
            if(_headOfSettlement.id != otherCharacter.id) {
                if(_headOfSettlement.GetRelationshipWith(otherCharacter) == null) {
                    //Village Elders will have relationship with the characters within their village.
                    CharacterManager.Instance.CreateNewRelationshipBetween(_headOfSettlement, otherCharacter);
                }
            }
        }
    }
    /*
     Make a character consider this landmark as it's home.
         */
    public override void AddCharacterHomeOnLandmark(ECS.Character character) {
        if (!_charactersWithHomeOnLandmark.Contains(character)) {
            _charactersWithHomeOnLandmark.Add(character);
            //Add new relationship with head of the settlement
            if(_headOfSettlement != null) {
                if (character.GetRelationshipWith(_headOfSettlement) == null) {
                    //Village Elders will have relationship with the characters within their village.
                    CharacterManager.Instance.CreateNewRelationshipBetween(_headOfSettlement, character);
                }
            }
            
        }
    }
    #endregion

    #region Population
    /*
     Reschedule monthly population growth.
     NOTE: Not to be used when inititally scheduling monthly population increase!
         */
    private void ScheduleMonthlyPopulationIncrease() {
        GameDate increasePopulationDate = GameManager.Instance.Today();
        increasePopulationDate.AddMonths(1);
        SchedulingManager.Instance.AddEntry(increasePopulationDate.month, 1, increasePopulationDate.year, () => IncreasePopulationPerMonth());
    }
    /*
     Increase population of this settlement every end of the month.
         */
    private void IncreasePopulationPerMonth() {
        float populationGrowth = this.totalPopulation * this.location.region.populationGrowth;
        AdjustPopulation(populationGrowth);
        UIManager.Instance.UpdateFactionSummary();
        ScheduleMonthlyPopulationIncrease();
    }
    #endregion
}
