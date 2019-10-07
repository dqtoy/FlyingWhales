using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tease : GoapAction {

    public Tease(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.TEASE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Entertain_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        doesNotStopTargetCharacter = true;
    }

    #region Overrides
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Tease Success");
    }
    protected override int GetCost() {
        Character targetCharacter = poiTarget as Character;
        List<RELATIONSHIP_TRAIT> rels = actor.GetAllRelationshipTraitTypesWith(targetCharacter);
        if (rels.Contains(RELATIONSHIP_TRAIT.FRIEND)) {
            return Utilities.rng.Next(40, 61);
        } else {
            return Utilities.rng.Next(50, 71);
        }
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    #endregion

    #region State Effects
    private void PerTickTeaseSuccess() {
        actor.AdjustHappiness(500);
    }
    #endregion   
}

public class TeaseData : GoapActionData {
    public TeaseData() : base(INTERACTION_TYPE.TEASE) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }
}

