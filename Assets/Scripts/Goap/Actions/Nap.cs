using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nap : GoapAction {
    protected override string failActionState { get { return "Nap Fail"; } }

    public Nap(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.NAP, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Sleep_Icon;
        shouldIntelNotificationOnlyIfActorIsActive = true;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = null, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            SetState("Nap Success");
        } else {
            TileObject obj = poiTarget as TileObject;
            if (!obj.IsAvailable()) {
                SetState("Nap Fail");
            } else {
                SetState("Target Missing");
            }
        }
    }
    protected override int GetCost() {
        Dwelling dwelling = targetStructure as Dwelling;
        if (dwelling.IsResident(actor)) {
            return 8;
        } else {
            for (int i = 0; i < dwelling.residents.Count; i++) {
                Character resident = dwelling.residents[i];
                if (resident != actor) {
                    CharacterRelationshipData characterRelationshipData = actor.GetCharacterRelationshipData(resident);
                    if (characterRelationshipData != null) {
                        if (characterRelationshipData.HasRelationshipOfEffect(TRAIT_EFFECT.POSITIVE)) {
                            return 25;
                        }
                    }
                }
            }
            return 45;
        }
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        IAwareness awareness = actor.GetAwareness(poiTarget);
        if (awareness == null) {
            return false;
        }
        LocationGridTile knownLoc = awareness.knownGridLocation;
        if (targetStructure.structureType == STRUCTURE_TYPE.DWELLING && knownLoc != null) {
            TileObject obj = poiTarget as TileObject;
            return obj.IsAvailable();
        }
        return false;
    }
    #endregion

    #region State Effects
    private void PreNapSuccess() {
        //currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        Resting restingTrait = new Resting();
        actor.AddTrait(restingTrait);
    }
    private void PerTickNapSuccess() {
        actor.AdjustTiredness(4);
    }
    private void AfterNapSuccess() {
        RemoveTraitFrom(actor, "Resting");
    }
    //private void PreNapFail() {
    //    currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    //private void PreNapMissing() {
    //    currentState.AddLogFiller(actor.currentStructure.location, actor.currentStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    #endregion
}