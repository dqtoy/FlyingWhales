/*
 This will replace the city script.
 */
using UnityEngine;
using System.Collections;

public class Settlement : BaseLandmark, QuestCreator {

    private CHARACTER_CLASS classToCreate;
    private CHARACTER_ROLE roleToCreate;

    public Settlement(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) {
        _canBeOccupied = true;
    }

    #region Ownership
    public override void OccupyLandmark(Faction faction) {
        base.OccupyLandmark(faction);
        if (location.isHabitable) {
            //Create structures on location
            location.region.HighlightRegionTiles(faction.factionColor, 69f / 255f);
            location.CreateStructureOnTile(faction, STRUCTURE_TYPE.CITY);
            location.emptyCityGO.SetActive(false);
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
        WeightedDictionary<CHARACTER_ROLE> characterRoleProductionDictionary = LandmarkManager.Instance.GetCharacterRoleProductionDictionary(this.owner);
        WeightedDictionary<CHARACTER_CLASS> characterClassProductionDictionary = LandmarkManager.Instance.GetCharacterClassProductionDictionary(this);
        if (characterRoleProductionDictionary.GetTotalOfWeights() > 0 && characterClassProductionDictionary.GetTotalOfWeights() > 0) {
            roleToCreate = characterRoleProductionDictionary.PickRandomElementGivenWeights();
            classToCreate = characterClassProductionDictionary.PickRandomElementGivenWeights();
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
            if (civilians >= 1f) { //Check first if the settlement has enough civilians to create a new character
                CreateNewCharacter(roleToCreate, classToCreate);
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
    public void CreateNewCharacter(CHARACTER_ROLE charRole, CHARACTER_CLASS charClass) {
        Character newCharacter = new Character(this._owner.race);
        newCharacter.AssignClass(charClass);
        newCharacter.AssignRole(charRole);
        newCharacter.SetFaction(this._owner);
        this.AdjustPopulation(-1); //Adjust population by -1
        this.owner.AddNewCharacter(newCharacter);
        this.AddCharacterOnLandmark(newCharacter);
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
        ScheduleMonthlyPopulationIncrease();
    }
    #endregion
}
