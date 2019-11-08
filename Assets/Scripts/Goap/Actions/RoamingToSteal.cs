using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoamingToSteal : GoapAction {

    public RoamingToSteal(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.ROAMING_TO_STEAL, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Entertain_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        isNotificationAnIntel = false;
        isRoamingAction = true;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.EARLY_NIGHT,
            TIME_IN_WORDS.LATE_NIGHT,
            TIME_IN_WORDS.AFTER_MIDNIGHT,
        };
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        if (actor.GetNormalTrait("Kleptomaniac") != null) {
            AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
        }
    }
    public override void Perform() {
        base.Perform();
        SetState("In Progress");
    }
    protected override int GetBaseCost() {
        if (actor.GetNormalTrait("Kleptomaniac") != null) {
            return Utilities.rng.Next(5, 46);
        }
        return Utilities.rng.Next(35, 56);
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
        Kleptomaniac kleptomaniac = actor.GetNormalTrait("Kleptomaniac") as Kleptomaniac;
        if (actor.marker.inVisionPOIs.Count > 0) {
            for (int i = 0; i < actor.marker.inVisionPOIs.Count; i++) {
                if (kleptomaniac.CreateJobsOnEnterVisionBasedOnTrait(actor.marker.inVisionPOIs[i], actor)) {
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
        if(dwellings != null && dwellings.Count > 0) {
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

public class RoamingToStealData : GoapActionData {
    public RoamingToStealData() : base(INTERACTION_TYPE.ROAMING_TO_STEAL) {
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