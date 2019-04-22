using System.Collections.Generic;

public class EatAtTable : GoapAction {

    public Trait poisonedTrait { get; private set; }

    public EatAtTable(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.EAT_DWELLING_TABLE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Eat_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = actor });
    }
    public override void PerformActualAction() {
        if (poiTarget.gridTileLocation != null && (actor.gridTileLocation == poiTarget.gridTileLocation || actor.gridTileLocation.IsAdjacentTo(poiTarget))) {
            poisonedTrait = poiTarget.GetTrait("Poisoned");
            if (poisonedTrait != null) {
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
        LocationGridTile knownLoc = actor.GetAwareness(poiTarget).knownGridLocation;
        Dwelling dwelling = knownLoc.structure as Dwelling;
        if (!dwelling.IsOccupied()) {
            return 10;
        } else {
            if (dwelling.IsResident(actor)) {
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
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Target Missing");
    //}
    #endregion

    #region Effects
    private void PreEatSuccess() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        actor.AdjustDoNotGetHungry(1);
        //actor.AddTrait("Eating");
    }
    private void PerTickEatSuccess() {
        actor.AdjustFullness(12);
    }
    private void AfterEatSuccess() {
        actor.AdjustDoNotGetHungry(-1);
    }
    private void PreEatPoisoned() {
        currentState.SetShouldAddLogs(false);
        //currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        actor.AdjustDoNotGetHungry(1);
        RemoveTraitFrom(poiTarget, "Poisoned");
    }
    private void PerTickEatPoisoned() {
        actor.AdjustFullness(12);
    }
    private void AfterEatPoisoned() {
        actor.AdjustDoNotGetHungry(-1);
        int chance = UnityEngine.Random.Range(0, 2);
        Log log = null;
        if(chance == 0) {
            log = new Log(GameManager.Instance.Today(), "GoapAction", "EatAtTable", "eat poisoned_sick");
            log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(poiTarget.gridTileLocation.structure.location, poiTarget.gridTileLocation.structure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
            Sick sick = new Sick();
            AddTraitTo(actor, sick);
        } else {
            if (parentPlan.job != null) {
                parentPlan.job.SetCannotCancelJob(true);
            }
            log = new Log(GameManager.Instance.Today(), "GoapAction", "EatAtTable", "eat poisoned_killed");
            log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(poiTarget.gridTileLocation.structure.location, poiTarget.gridTileLocation.structure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
            actor.Death("normal", false);
            AddActualEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, targetPOI = actor });
        }
        log.AddLogToInvolvedObjects();
        currentState.OverrideDescriptionLog(log);
        //UIManager.Instance.Pause();
    }
    private void PreTargetMissing() {
        currentState.AddLogFiller(actor.currentStructure.location, actor.currentStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    private void AfterTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        IAwareness awareness = actor.GetAwareness(poiTarget);
        if (awareness == null) {
            return false;
        }
        LocationGridTile knownLoc = awareness.knownGridLocation;
        return knownLoc.structure.structureType == STRUCTURE_TYPE.DWELLING;
    }
    #endregion
}
