using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puke : GoapAction {

    public Puke(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.PUKE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.No_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
    }

    #region Overrides
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Puke Success");
    }
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    protected override int GetCost() {
        return 5;
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    #endregion

    #region State Effects
    private void PrePukeSuccess() {
        actor.SetPOIState(POI_STATE.INACTIVE);
        currentState.SetIntelReaction(SuccessReactions);
    }
    private void AfterPukeSuccess() {
        actor.SetPOIState(POI_STATE.ACTIVE);
    }
    #endregion

    #region Intel Reactions
    private List<string> SuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        RELATIONSHIP_EFFECT relWithActor = recipient.GetRelationshipEffectWith(actor);
        //- Is Actor
        if (recipient == actor) {
            //  - If Informed: "That was embarrassing."
            if (status == SHARE_INTEL_STATUS.INFORMED) {
                reactions.Add("That was embarrassing.");
            }
        }
        //- Neutral or Positive Relationship with Actor
        else if (relWithActor == RELATIONSHIP_EFFECT.NONE || relWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
            if (status == SHARE_INTEL_STATUS.WITNESSED) {
                //- If witnessed: 35% chance to also Puke if also on the move
                if (Random.Range(0, 100) < 35 && recipient.marker.isMoving) {
                    AlsoPuke(recipient);
                    return reactions; //do not do anything else
                }
            }
            //- If character has Doctor trait
            if (recipient.GetNormalTrait("Doctor") != null) {
                //- attempt to Cure the Actor
                if (!actor.isDead && !actor.HasJobTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, "Plagued") && !actor.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
                    GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Plagued", targetPOI = actor };
                    GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_TRAIT, goapEffect,
                        new Dictionary<INTERACTION_TYPE, object[]>() { { INTERACTION_TYPE.CRAFT_ITEM_GOAP, new object[] { SPECIAL_TOKEN.HEALING_POTION } }, });
                    if (CanCharacterTakeRemoveIllnessesJob(recipient, actor, null)) {
                        //job.SetCanTakeThisJobChecker(CanCharacterTakeRemoveTraitJob);
                        recipient.jobQueue.AddJobInQueue(job);
                    } else {
                        job.SetCanTakeThisJobChecker(CanCharacterTakeRemoveIllnessesJob);
                        recipient.specificLocation.jobQueue.AddJobInQueue(job);
                    }
                }
                if (status == SHARE_INTEL_STATUS.INFORMED) {
                    //- if informed: "I'm a doctor. I should help [Actor Name]."
                    reactions.Add(string.Format("I'm a doctor. I should help {0}.", recipient.name));
                }
            }
            //- If character does not have Doctor trait
            else {
                //- if informed: "I hope [he/she] gets well soon."
                if (status == SHARE_INTEL_STATUS.INFORMED) {
                    reactions.Add(string.Format("I hope {0} gets well soon.", Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
                }
            }
        }
        //- Negative Relationship with Actor
        else if (relWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
            if (status == SHARE_INTEL_STATUS.WITNESSED) {
                //- If witnessed: 35% chance to also Puke if also on the move
                if (Random.Range(0, 100) < 35 && recipient.marker.isMoving) {
                    AlsoPuke(recipient);
                    return reactions; //do not do anything else
                }
            } else if (status == SHARE_INTEL_STATUS.INFORMED) {
                //- If Informed: "Stop sharing gross things about that vile person."
                reactions.Add("Stop sharing gross things about that vile person.");
            }
        }

        return reactions;
    }
    #endregion

    private GoapAction stoppedAction;
    private CharacterState pausedState;
    private void AlsoPuke(Character character) {
        if (character.currentAction != null && character.currentAction.goapType != INTERACTION_TYPE.PUKE) {
            stoppedAction = character.currentAction;
            character.StopCurrentAction(false);
            character.marker.StopMovement();

            GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PUKE, character, character);

            GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
            GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.REACTION);
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.PUKE);
            job.SetAssignedPlan(goapPlan);
            goapPlan.ConstructAllNodes();

            goapAction.CreateStates();
            character.SetCurrentAction(goapAction);
            character.currentAction.SetEndAction(ResumeLastAction);
            character.currentAction.DoAction();
        } else if (character.stateComponent.currentState != null) {
            pausedState = character.stateComponent.currentState;
            character.stateComponent.currentState.PauseState();
            character.marker.StopMovement();
            GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PUKE, character, character);

            GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
            GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.REACTION);
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.PUKE);
            job.SetAssignedPlan(goapPlan);
            goapPlan.ConstructAllNodes();

            goapAction.CreateStates();
            character.SetCurrentAction(goapAction);
            character.currentAction.SetEndAction(ResumePausedState);
            character.currentAction.DoAction();
        } else {
            GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PUKE, character, character);

            GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
            GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.REACTION);
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.PUKE);
            job.SetAssignedPlan(goapPlan);
            goapPlan.ConstructAllNodes();

            goapAction.CreateStates();
            character.SetCurrentAction(goapAction);
            character.currentAction.DoAction();
        }
    }

    private void ResumeLastAction(string result, GoapAction action) {
        if (InteractionManager.Instance.CanSatisfyGoapActionRequirements(stoppedAction.goapType, stoppedAction.actor, stoppedAction.poiTarget, stoppedAction.otherData)) {
            stoppedAction.DoAction();
        } else {
            action.actor.GoapActionResult(result, action);
        }

    }
    private void ResumePausedState(string result, GoapAction action) {
        action.actor.GoapActionResult(result, action);
        pausedState.ResumeState();
    }

    private bool CanCharacterTakeRemoveIllnessesJob(Character character, Character targetCharacter, JobQueueItem job) {
        if (character != targetCharacter && character.faction == targetCharacter.faction && character.isAtHomeArea) {
            if (character.faction.id == FactionManager.Instance.neutralFaction.id) {
                return character.race == targetCharacter.race && character.homeArea == targetCharacter.homeArea && !targetCharacter.HasRelationshipOfTypeWith(character, RELATIONSHIP_TRAIT.ENEMY);
            }
            return !character.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.ENEMY) && character.GetNormalTrait("Doctor") != null;
        }
        return false;
    }
}

public class PukeData : GoapActionData {
    public PukeData() : base(INTERACTION_TYPE.PUKE) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }
}
