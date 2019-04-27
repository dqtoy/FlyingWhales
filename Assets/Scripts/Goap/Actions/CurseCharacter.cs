using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurseCharacter : GoapAction {

    public CurseCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.CURSE_CHARACTER, INTERACTION_ALIGNMENT.EVIL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.Hostile_Icon;
        shouldAddLogs = false; //set to false because this action has a special case for logs
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Ritualized", targetPOI = actor }, () => HasTrait(actor, "Ritualized"));
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Cursed", targetPOI = poiTarget });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, conditionKey = "Negative", targetPOI = poiTarget });

        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, targetPOI = poiTarget });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = poiTarget });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing && (poiTarget as Character).IsInOwnParty()) {
            SetState("Curse Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 3;
    }
    protected override void MoveToDoAction(GoapPlan plan, Character targetCharacter) {
        if (actor.specificLocation != targetStructure.location) {
            actor.currentParty.GoToLocation(targetStructure.location, PATHFINDING_MODE.NORMAL, targetStructure, () => actor.PerformGoapAction(plan), null, poiTarget, targetTile);
        } else {
            //if the actor is already at the area where the target structure is, immediately do the action, since this action is performed in place
            actor.PerformGoapAction(plan);
        }
    }
    #endregion

    #region State Effects
    public void PreCurseSuccess() {
        currentState.SetIntelReaction(CurseSuccessReactions);
        //(poiTarget as Character).marker.pathfindingAI.AdjustDoNotMove(1);
    }
    public void AfterCurseSuccess() {
        //**After Effect 1**: Target gains Cursed trait.
        Cursed cursed = new Cursed();
        AddTraitTo(poiTarget, cursed);
        //**After Effect 2**: Actor loses Ritualized trait.
        RemoveTraitFrom(actor, "Ritualized");

        Log actorLog = new Log(GameManager.Instance.Today(), "GoapAction", this.GetType().ToString(), currentState.name.ToLower() + "_description_actor");
        actorLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        actorLog.AddToFillers(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        Log targetLog = new Log(GameManager.Instance.Today(), "GoapAction", this.GetType().ToString(), currentState.name.ToLower() + "_description_target");
        targetLog.AddToFillers(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        actor.AddHistory(actorLog);
        (poiTarget as Character).AddHistory(targetLog);
        currentState.OverrideDescriptionLog(actorLog);

        //(poiTarget as Character).marker.pathfindingAI.AdjustDoNotMove(-1);
    }
    #endregion

    #region Requirement
    private bool Requirement() {
        return actor != poiTarget;
    }
    #endregion

    #region Intel Reactions
    public List<string> CurseSuccessReactions(Character reciepient, Intel sharedIntel) {
        List<string> reactions = new List<string>();
        Character target = poiTarget as Character;
        //Recipient is the target:
        if (reciepient == poiTarget) {
            //- **Recipient Response Text**:  "[Actor Name] cursed me? What a horrible person."
            reactions.Add(string.Format("{0} cursed me? What a horrible person.", actor.name));
            //-**Recipient Effect * *: Remove Friend/ Lover / Paramour relationship between Actor and Recipient.
            CharacterManager.Instance.RemoveRelationshipBetween(actor, reciepient, RELATIONSHIP_TRAIT.FRIEND);
            CharacterManager.Instance.RemoveRelationshipBetween(actor, reciepient, RELATIONSHIP_TRAIT.LOVER);
            CharacterManager.Instance.RemoveRelationshipBetween(actor, reciepient, RELATIONSHIP_TRAIT.PARAMOUR);
            //Apply Crime System handling as if the Recipient witnessed Actor commit Assault.
            reciepient.ReactToCrime(CRIME.ASSAULT, actor);
        }
        //Recipient is the actor:
        else if (reciepient == actor) {
            //-**Recipient Response Text**: I know what I did.
            reactions.Add("I know what I did.");
            //- **Recipient Effect * *: no effect
        }
        //Recipient and Actor have a positive relationship:
        else if (reciepient.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE, RELATIONSHIP_TRAIT.RELATIVE)) {
            //-**Recipient Response Text**: "[Actor Name] may have cursed somebody but I know that [he/she] is a good person."
            reactions.Add(string.Format("{0} may have cursed somebody but I know that {1} is a good person.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
            //- **Recipient Effect * *: no effect
        }
        //Recipient and Actor have a negative relationship:
        else if (reciepient.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.NEGATIVE)) {
            //- **Recipient Response Text**: "[Actor Name] cursed someone!? Why am I not surprised."
            reactions.Add(string.Format("{0} cursed someone!? Why am I not surprised.", actor.name));
            //-**Recipient Effect * *: Apply Crime System handling as if the Recipient witnessed Actor commit Assault.
            reciepient.ReactToCrime(CRIME.ASSAULT, actor);
        }
        //Recipient and Actor have no relationship but are from the same faction:
        else if (!reciepient.HasRelationshipWith(actor) && reciepient.faction == actor.faction) {
            //- **Recipient Response Text**: "[Actor Name] cursed someone!? That's forbidden."
            reactions.Add(string.Format("{0} cursed someone!? That's forbidden.", actor.name));
            //-**Recipient Effect * *: Apply Crime System handling as if the Recipient witnessed Actor commit Assault.
            reciepient.ReactToCrime(CRIME.ASSAULT, actor);
        }
        return reactions;
    }
    #endregion
}
