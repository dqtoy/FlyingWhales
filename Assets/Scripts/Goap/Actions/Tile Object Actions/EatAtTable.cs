using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatAtTable : GoapAction {

    public Poisoned poisonedTrait { get; private set; }
    private string poisonedResult;
    public Table targetTable { get; private set; }

    public EatAtTable(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.EAT_DWELLING_TABLE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Eat_Icon;
        shouldIntelNotificationOnlyIfActorIsActive = true;
        //isNotificationAnIntel = false;
        targetTable = poiTarget as Table;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            poisonedTrait = poiTarget.GetNormalTrait("Poisoned") as Poisoned;
            if (poisonedTrait != null) {
                SetState("Eat Poisoned");
            } else {
                SetState("Eat Success");
            }
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        //if the table is poisoned, check if the actor knows about it, if he/she does, increase cost
        Poisoned poisoned = poiTarget.GetNormalTrait("Poisoned") as Poisoned;
        if (poisoned != null) {
            if (poisoned.awareCharacters.Contains(actor)) {
                return 50;
            }
        }

        if(poiTarget.gridTileLocation.structure.structureType == STRUCTURE_TYPE.DWELLING) {
            Dwelling dwelling = poiTarget.gridTileLocation.structure as Dwelling;
            if (!dwelling.IsOccupied()) {
                return 12;
            } else {
                if (dwelling.IsResident(actor)) {
                    return 1;
                } else {
                    for (int i = 0; i < dwelling.residents.Count; i++) {
                        Character owner = dwelling.residents[i];
                        CharacterRelationshipData characterRelationshipData = actor.GetCharacterRelationshipData(owner);
                        if (characterRelationshipData != null) {
                            if (characterRelationshipData.HasRelationshipOfEffect(TRAIT_EFFECT.POSITIVE)) {
                                return 18;
                            }
                        }
                    }
                    return 28;
                }
            }
        } else if(poiTarget.gridTileLocation.structure.structureType == STRUCTURE_TYPE.INN) {
            return 28;
        }
        return 100;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Target Missing");
    //}
    public override void OnStopActionDuringCurrentState() {
        if (currentState.name == "Eat Success") {
            actor.AdjustDoNotGetHungry(-1);
        }else if(currentState.name == "Eat Poisoned") {
            actor.AdjustDoNotGetHungry(-1);
            Sick sick = new Sick();
            for (int i = 0; i < poisonedTrait.responsibleCharacters.Count; i++) {
                AddTraitTo(actor, sick, poisonedTrait.responsibleCharacters[i]);
            }
        }
    }
    public override int GetArrangedLogPriorityIndex(string priorityID) {
        if (priorityID == "description") {
            return 0;
        } else if (priorityID == "sick") {
            return 1;
        }
        return base.GetArrangedLogPriorityIndex(priorityID);
    }
    #endregion

    #region Effects
    private void PreEatSuccess() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        actor.AdjustDoNotGetHungry(1);
        currentState.SetIntelReaction(EatSuccessReactions);
        //actor.AddTrait("Eating");
    }
    private void PerTickEatSuccess() {
        actor.AdjustFullness(18);
        if (currentState.currentDuration <= 8) {
            targetTable.AdjustFood(-2);
        } else {
            targetTable.AdjustFood(-1);
        }
    }
    private void AfterEatSuccess() {
        actor.AdjustDoNotGetHungry(-1);
    }
    private void PreEatPoisoned() {
        //currentState.SetShouldAddLogs(false);
        //currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        actor.AdjustDoNotGetHungry(1);
        RemoveTraitFrom(poiTarget, "Poisoned");
        Log log = null;
        WeightedDictionary<string> result = poisonedTrait.GetResultWeights();
        string res = result.PickRandomElementGivenWeights();
        if (res == "Sick") {
            log = new Log(GameManager.Instance.Today(), "GoapAction", "EatAtTable", "eat poisoned_sick", this);
            log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(poiTarget.gridTileLocation.structure.location, poiTarget.gridTileLocation.structure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
            poisonedResult = "Sick";
        } else if (res == "Death"){
            log = new Log(GameManager.Instance.Today(), "GoapAction", "EatAtTable", "eat poisoned_killed", this);
            log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(poiTarget.gridTileLocation.structure.location, poiTarget.gridTileLocation.structure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
            poisonedResult = "Death";
        }
        //log.AddLogToInvolvedObjects();
        currentState.OverrideDescriptionLog(log);
        currentState.SetIntelReaction(EatPoisonedReactions);
    }
    private void PerTickEatPoisoned() {
        actor.AdjustFullness(18);
        if(currentState.currentDuration <= 10) {
            targetTable.AdjustFood(-2);
        } else {
            targetTable.AdjustFood(-1);
        }
    }
   
    private void AfterEatPoisoned() {
        actor.AdjustDoNotGetHungry(-1);
        //UIManager.Instance.Pause();
        if (poisonedResult == "Sick") {
            Sick sick = new Sick();
            for (int i = 0; i < poisonedTrait.responsibleCharacters.Count; i++) {
                sick.AddCharacterResponsibleForTrait(poisonedTrait.responsibleCharacters[i]);
            }
            AddTraitTo(actor, sick);
        } else {
            if (parentPlan.job != null) {
                parentPlan.job.SetCannotCancelJob(true);
            }
            SetCannotCancelAction(true);
            actor.Death("poisoned", deathFromAction: this);
            AddActualEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, targetPOI = actor });
#if TRAILER_BUILD
            if (actor.name == "Fiona") {
                Character jamie = CharacterManager.Instance.GetCharacterByName("Jamie");
                jamie.CancelAllJobsAndPlans();
                //make jaime go to fiona's home
                GoapAction eat = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.EAT_DWELLING_TABLE, jamie, poiTarget);
                GoapPlan plan = new GoapPlan(new GoapNode(null, eat.cost, eat), new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY }, GOAP_CATEGORY.FULLNESS);
                plan.ConstructAllNodes();
                jamie.AddPlan(plan, true);
            }
#endif
        }
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
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        bool isFoodEnough = false;
        if(poiTarget is Table) {
            Table table = poiTarget as Table;
            isFoodEnough = table.food >= 20;
        }
        return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null && isFoodEnough;
    }
    #endregion

    #region Intel Reactions
    private List<string> EatSuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();

        if (isOldNews) {
            reactions.Add("This is old news.");
        } else {
            //Recipient is Actor
            if (recipient == actor) {
                reactions.Add("I know what I did.");
            } else {
                reactions.Add("This is not relevant to me.");
            }
        }
        return reactions;
    }
    private List<string> EatPoisonedReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        if (isOldNews) {
            reactions.Add("This is old news.");
        } else {
            //Recipient is Actor
            if (recipient == actor) {
                reactions.Add("Yes! I ate poisoned food but thankfully, I survived. Do you know who did it?!");
            } 
            else if (poisonedTrait.IsResponsibleForTrait(recipient)) {
                if(recipient.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.NEGATIVE) {
                    reactions.Add("Yes I did that and it worked! Muahahaha!");
                    AddTraitTo(recipient, "Cheery");
                } else {
                    reactions.Add(string.Format("{0} wasn't my target when I poisoned the food.", actor.name));
                }
            } 
            else if ((recipient.faction == actor.faction && !recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.ENEMY)) 
                || (recipient.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.POSITIVE)) {

                Trait sickTrait = actor.GetNormalTrait("Sick");
                if (actor.isDead) {
                    reactions.Add(string.Format("Poor {0}, may {1} rest in peace.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
                    Trait deadTrait = actor.GetNormalTrait("Dead");
                    deadTrait.CreateJobsOnEnterVisionBasedOnTrait(actor, recipient);
                }else if (sickTrait != null) {
                    reactions.Add(string.Format("Poor {0}, maybe I can help.", actor.name));
                    sickTrait.CreateJobsOnEnterVisionBasedOnTrait(actor, recipient);
                }
            } 
            else if (recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.ENEMY)) {
                reactions.Add(string.Format("{0} deserves that.", actor.name));
                AddTraitTo(recipient, "Cheery");
            }
            else {
                reactions.Add("This is not relevant to me.");
            }
        }
        return reactions;
    }
    #endregion
}
