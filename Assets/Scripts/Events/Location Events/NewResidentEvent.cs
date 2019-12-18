using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewResidentEvent : LocationEvent {

	public NewResidentEvent() {
        name = "New Resident Event";
        triggerTick = 96;
        triggerChance = 35;
        triggerCondition = Condition;
    }

    private bool Condition(Area location) {
        //if (location.structures.ContainsKey(STRUCTURE_TYPE.DWELLING)) {
        //    List<LocationStructure> structures = location.structures[STRUCTURE_TYPE.DWELLING];
        //    for (int i = 0; i < structures.Count; i++) {
        //        if (!structures[i].IsOccupied()) {
        //            return true;
        //        }
        //    }
        //}
        return !location.region.coreTile.isCorrupted && !location.IsResidentsFull();
    }

    #region Overrides
    public override void TriggerEvent(Area location) {
        base.TriggerEvent(location);
        //List<LocationStructure> structures = location.structures[STRUCTURE_TYPE.DWELLING];
        //int numberOfUnoccupiedDwellings = 0;
        //for (int i = 0; i < structures.Count; i++) {
        //    if (!structures[i].IsOccupied()) {
        //        numberOfUnoccupiedDwellings++;
        //    }
        //}
        int numberOfUnoccupiedDwellings = location.GetNumberOfUnoccupiedStructure(STRUCTURE_TYPE.DWELLING);
        int numOfSoonToBeOccupiedDwellings = UnityEngine.Random.Range(1, numberOfUnoccupiedDwellings + 1);
        int maxCouple = 2;
        int currentCouple = 0;
        for (int i = 0; i < numOfSoonToBeOccupiedDwellings; i++) {
            if (currentCouple >= maxCouple) {
                GenerateSingleResident(location);
            } else {
                //GenerateCoupleResidents(location);

                if (UnityEngine.Random.Range(0, 2) == 0) {
                    GenerateCoupleResidents(location);
                    currentCouple++;
                } else {
                    GenerateSingleResident(location);
                }
            }
        }
        PlayerUI.Instance.ShowGeneralConfirmation("New Residents", "New residents have arrived at " + location.region.name);
    }
    #endregion
    private void GenerateSingleResident(Area location) {
        RACE race = GetRaceForNewResident(location);
        Character newResident = location.AddNewResident(race, location.region.owner);
        Debug.Log(GameManager.Instance.TodayLogString() + "Generated new Single Resident " + newResident + " from New Resident Event");
        //CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, race, Utilities.GetRandomGender(), location.region.owner, location.region);
    }
    private void GenerateCoupleResidents(Area location) {
        RACE race = GetRaceForNewResident(location);
        //string className = location.locationClassManager.GetCurrentClassToCreate();
        Character spouse1 = location.AddNewResident(race, location.region.owner);

        race = GetRaceForNewResident(location);
        SEXUALITY sexuality = Utilities.GetCompatibleSexuality(spouse1.sexuality);
        GENDER gender = Utilities.GetOppositeGender(spouse1.gender);
        if(spouse1.sexuality == SEXUALITY.BISEXUAL) {
            if(sexuality == SEXUALITY.GAY) {
                gender = spouse1.gender;
            }else if (sexuality == SEXUALITY.BISEXUAL) {
                if(UnityEngine.Random.Range(0, 2) == 0) {
                    gender = spouse1.gender;
                }
            }
        } else if (spouse1.sexuality == SEXUALITY.GAY) {
            if (sexuality == SEXUALITY.GAY) {
                gender = spouse1.gender;
            }
        }
        //className = location.locationClassManager.GetNextClassToCreate();
        Character spouse2 = location.AddNewResident(race, gender, sexuality, location.region.owner);

        RelationshipManager.Instance.CreateNewRelationshipBetween(spouse1, spouse2, RELATIONSHIP_TYPE.LOVER);

        //if (location.region != null) {
        //    spouse1.ownParty.icon.SetPosition(location.region.coreTile.transform.position);
        //    spouse1.MigrateHomeTo(location.region, null, false);
        //    location.region.AddCharacterToLocation(spouse1);

        //    spouse2.ownParty.icon.SetPosition(location.region.coreTile.transform.position);
        //    spouse2.MigrateHomeTo(location.region, null, false);
        //    location.region.AddCharacterToLocation(spouse2);
        //}

        //location.PlaceNewResidentInInnerMap(spouse1);
        //location.PlaceNewResidentInInnerMap(spouse2);

        Debug.Log(GameManager.Instance.TodayLogString() + "Generated new Couple Resident " + spouse1 + " and " + spouse2 + " from New Resident Event");
    }
    private RACE GetRaceForNewResident(Area location) {
        if(location.region.owner != null) {
            int chance = UnityEngine.Random.Range(0, 100);
            if(chance < 75) {
                return location.region.owner.leader.race;
            }
        }
        return UnityEngine.Random.Range(0, 2) == 0 ? RACE.HUMANS : RACE.ELVES;
    }
}
