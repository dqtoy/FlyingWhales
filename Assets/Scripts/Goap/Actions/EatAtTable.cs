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
        base.PerformActualAction();
        if (!isTargetMissing) {
            poisonedTrait = poiTarget.GetTrait("Poisoned");
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
        LocationGridTile knownLoc = actor.GetAwareness(poiTarget).knownGridLocation;
        Dwelling dwelling = knownLoc.structure as Dwelling;
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
            SetCannotCancelAction(true);
            log = new Log(GameManager.Instance.Today(), "GoapAction", "EatAtTable", "eat poisoned_killed");
            log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(poiTarget.gridTileLocation.structure.location, poiTarget.gridTileLocation.structure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
            actor.Death("normal");
            AddActualEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, targetPOI = actor });
        }
        log.AddLogToInvolvedObjects();
        currentState.OverrideDescriptionLog(log);
        currentState.SetIntelReaction(State2Reactions);
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

    #region Intel Reactions
    private List<string> State2Reactions(Character recipient, Intel sharedIntel) {
        List<string> reactions = new List<string>();

        //Recipient and Actor are the same
        if (recipient == actor) {
            //- **Recipient Response Text**: "I know what I've done!"
            reactions.Add(string.Format("I know what I've done!", actor.name));
            //-**Recipient Effect**:  no effect
        }

        //Recipient poisoned the table and he has a negative relationship with the Actor:
        else if (poisonedTrait.IsResponsibleForTrait(recipient) && recipient.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.NEGATIVE)) {
            //- **Recipient Response Text**: "Yes, I did that. Hahaha!"
            reactions.Add("Yes, I did that. Hahaha!");
            //-**Recipient Effect * *: no effect
        }

        //Recipient has a negative relationship with the Actor:
        else if (recipient.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.NEGATIVE)) {
            //- **Recipient Response Text**: "It's what [Actor Name] deserves!"
            reactions.Add(string.Format("It's what {0} deserves!", actor.name));
            //-**Recipient Effect**: no effect
        }

        //Actor became Sick, Recipient has a positive relationship with the Actor but is not part of the same faction:
        else if (recipient.faction != actor.faction && !recipient.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE, RELATIONSHIP_TRAIT.RELATIVE)
            && HasActualEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Sick", actor)) {
            //- **Recipient Response Text**: "Poor [Actor Name]. Maybe I can help!"
            reactions.Add(string.Format("Poor {0}. Maybe I can help!", actor.name));
            //-**Recipient Effect * *: Add a Remove[Sick] Job to Actor's personal job queue.
            GoapPlanJob job = new GoapPlanJob("Remove Sick", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Sick", targetPOI = actor });
            recipient.jobQueue.AddJobInQueue(job);
        }

        //Actor became Sick, Recipient does not have a negative relationship with the Actor and is part of the same faction:
        else if (recipient.faction == actor.faction && !recipient.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.NEGATIVE)
            && HasActualEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Sick", actor)) {
            //- **Recipient Response Text**: "Poor [Actor Name]. Maybe I can help!"
            reactions.Add(string.Format("Poor {0}. Maybe I can help!", actor.name));
            //-**Recipient Effect * *: Add a Remove[Sick] Job to location job queue and assign it to Recipient.
            GoapPlanJob job = new GoapPlanJob("Remove Sick", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Sick", targetPOI = actor });
            recipient.jobQueue.AddJobInQueue(job);
        }

        //Actor died, Recipient has a positive relationship with the Actor or is part of the same faction and not enemies:
        else if (HasActualEffect(GOAP_EFFECT_CONDITION.DEATH, null, actor) 
            && (recipient.faction == actor.faction || recipient.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE, RELATIONSHIP_TRAIT.RELATIVE))
            && !recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.ENEMY)) {
            //- **Recipient Response Text**: "Poor [Actor Name]. May [he/she] rest in peace."
            reactions.Add(string.Format("Poor {0}. May {1} rest in peace.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
            //-**Recipient Effect * *: No effect.
        }

        return reactions;
    }
    #endregion
}
