public class EatAtTable : GoapAction {
    public EatAtTable(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.EAT_DWELLING_TABLE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
    }

    #region Overrides
    //protected override void ConstructRequirement() {
    //    _requirementAction = Requirement;
    //}
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = actor });
    }
    public override void PerformActualAction() {
        if (targetStructure == actor.gridTileLocation.structure) {
            ////TODO: CHECKER IF TABLE IS POISONED
            //SetState("Eat Success");

            if (poiTarget.GetTrait("Poisoned") != null) {
                SetState("Eat Poisoned");
            } else {
                SetState("Eat Success");
            }
        } else {
            SetState("Target Missing");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        if(poiTarget.gridTileLocation.structure.structureType == STRUCTURE_TYPE.DWELLING && poiTarget is Table) {
            Dwelling dwelling = poiTarget.gridTileLocation.structure as Dwelling;
            if (!dwelling.IsOccupied()) {
                return 10;
            } else {
                if(dwelling.IsResident(actor)) {
                    return 1;
                } else {
                    for (int i = 0; i < dwelling.residents.Count; i++) {
                        Character owner = dwelling.residents[i];
                        CharacterRelationshipData characterRelationshipData = actor.GetCharacterRelationshipData(owner);
                        if (characterRelationshipData != null) {
                            if (characterRelationshipData.HasRelationshipOfEffect(TRAIT_EFFECT.POSITIVE)) {
                                return 4;
                            }
                        }
                    }
                    return 10;
                }
            }
        }
        return 0;
    }
    public override void FailAction() {
        base.FailAction();
        SetState("Target Missing");
    }
    #endregion

    #region Effects
    private void PreEatSuccess() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        actor.AdjustDoNotGetHungry(1);
        //actor.AddTrait("Eating");
    }
    private void PerTickEatSuccess() {
        actor.AdjustFullness(10);
    }
    private void AfterEatSuccess() {
        actor.AdjustDoNotGetHungry(-1);
    }
    private void PreEatPoisoned() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        actor.AdjustDoNotGetHungry(1);
        poiTarget.RemoveTrait("Poisoned");
    }
    private void PerTickEatPoisoned() {
        actor.AdjustFullness(10);
    }
    private void AfterEatPoisoned() {
        actor.AdjustDoNotGetHungry(-1);
        int chance = UnityEngine.Random.Range(0, 2);
        if(chance == 0) {
            actor.AddTrait("Sick");
        } else {
            actor.Death();
        }
        //TODO: DIFFERENT DESCRIPTION LOGS IN SAME STATE
    }
    private void PreTargetMissing() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    private void AfterTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion

    #region Requirements
    //protected bool Requirement() {
    //    return poiTarget.state == POI_STATE.ACTIVE;
    //}
    #endregion
}
