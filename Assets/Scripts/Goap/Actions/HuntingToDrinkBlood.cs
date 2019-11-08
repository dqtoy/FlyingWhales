using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HuntingToDrinkBlood : GoapAction {

    public HuntingToDrinkBlood(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Eat_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        isNotificationAnIntel = false;
        isRoamingAction = true;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        if (actor.GetNormalTrait("Vampiric") != null) {
            AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = actor });
        }
    }
    public override void Perform() {
        base.Perform();
        SetState("In Progress");
    }
    protected override int GetBaseCost() {
        return 1;
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null);
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if (actor == poiTarget) {
            return true;
        }
        return false;
    }
    #endregion

    #region Effects
    private void PreInProgress() {
        Vampiric vampiric = actor.GetNormalTrait("Vampiric") as Vampiric;
        if(actor.marker.inVisionCharacters.Count > 0) {
            for (int i = 0; i < actor.marker.inVisionCharacters.Count; i++) {
                if(vampiric.CreateJobsOnEnterVisionBasedOnTrait(actor.marker.inVisionCharacters[i], actor)) {
                    return;
                }
            }
        }
        RoamAround();
    }
    private void AfterInProgress() {
        if (actor.currentParty.icon.isTravelling) {
            actor.marker.StopMovement();
        }
    }
    #endregion

    private void RoamAround() {
        actor.marker.GoTo(PickRandomTileToGoTo(), RoamAround);
    }
    private LocationGridTile PickRandomTileToGoTo() {
        LocationStructure chosenStructure = null;
        List<LocationStructure> dwellings = actor.specificLocation.GetStructuresOfType(STRUCTURE_TYPE.DWELLING, actor.currentStructure);
        if (dwellings != null && dwellings.Count > 0) {
            List<LocationStructure> dwellingsOfEnemies = dwellings.Where(x => (x as Dwelling).HasEnemyOrNoRelationshipWithAnyResident(actor)).ToList();
            if (dwellingsOfEnemies != null && dwellingsOfEnemies.Count > 0) {
                chosenStructure = dwellingsOfEnemies[UnityEngine.Random.Range(0, dwellingsOfEnemies.Count)];
            } else {
                chosenStructure = dwellings[UnityEngine.Random.Range(0, dwellings.Count)];
            }
        } else {
            chosenStructure = actor.specificLocation.GetRandomStructure();
        }

        LocationGridTile chosenTile = chosenStructure.GetRandomTile();
        if (chosenTile != null) {
            return chosenTile;
        } else {
            throw new System.Exception("No tile in " + chosenStructure.name + " for " + actor.name + " to go to in " + goapType.ToString());
        }
    }
}

public class HuntingToDrinkBloodData : GoapActionData {
    public HuntingToDrinkBloodData() : base(INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (actor == poiTarget) {
            return true;
        }
        return false;
    }
}
