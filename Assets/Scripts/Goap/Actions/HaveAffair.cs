using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaveAffair : GoapAction {

    public HaveAffair(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.HAVE_AFFAIR, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Flirt_Icon;
        shouldIntelNotificationOnlyIfActorIsActive = true;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            SetState("Affair Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 1;
    }
    #endregion

    #region Effects
    private void AfterAffairSuccess() {
        CharacterManager.Instance.CreateNewRelationshipBetween(actor, poiTarget as Character, RELATIONSHIP_TRAIT.PARAMOUR);
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if (!poiTarget.IsAvailable()) {
            return false;
        }
        Character targetCharacter = poiTarget as Character;
        if (CharacterManager.Instance.IsSexuallyCompatible(actor, targetCharacter) && actor.CanHaveRelationshipWith(RELATIONSHIP_TRAIT.PARAMOUR, targetCharacter)) {
            return true;
        }
        return false;
    }
    #endregion
}

public class HaveAffairData : GoapActionData {
    public HaveAffairData() : base(INTERACTION_TYPE.HAVE_AFFAIR) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (!poiTarget.IsAvailable()) {
            return false;
        }
        Character targetCharacter = poiTarget as Character;
        if (CharacterManager.Instance.IsSexuallyCompatible(actor, targetCharacter) && actor.CanHaveRelationshipWith(RELATIONSHIP_TRAIT.PARAMOUR, targetCharacter)) {
            return true;
        }
        return false;
    }
}