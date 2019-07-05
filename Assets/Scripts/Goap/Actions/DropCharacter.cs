using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropCharacter : GoapAction {
    private LocationStructure _prison;

    public override LocationStructure targetStructure {
        get { return _prison; }
    }

    public DropCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.DROP_CHARACTER, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        _prison = actor.homeArea.prison;
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION_B;
        actionIconString = GoapActionStateDB.Hostile_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_PARTY, conditionKey = actor, targetPOI = poiTarget }, IsInActorParty);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = actor.homeArea, targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Drop Success");
    }
    protected override int GetCost() {
        return 1;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Target Missing");
    //}
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    public override void SetTargetStructure() {
        base.SetTargetStructure();
    }
    public override void OnStopActionWhileTravelling() {
        base.OnStopActionWhileTravelling();
        Character targetCharacter = poiTarget as Character;
        actor.ownParty.RemoveCharacter(targetCharacter);
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return actor != poiTarget;
    }
    #endregion

    #region Preconditions
    private bool IsInActorParty() {
        Character target = poiTarget as Character;
        return target.currentParty == actor.currentParty;
    }
    #endregion

    #region State Effects
    public void PreDropSuccess() {
        //currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        currentState.AddLogFiller(_prison.location, _prison.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        currentState.SetIntelReaction(DropSuccessIntelReaction);
    }
    public void AfterDropSuccess() {
        Character target = poiTarget as Character;
        actor.ownParty.RemoveCharacter(target);
        //target.MoveToAnotherStructure(_workAreaStructure);
        AddActualEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = actor.homeArea, targetPOI = poiTarget });

        if(parentPlan != null && parentPlan.job != null && parentPlan.job.jobType == JOB_TYPE.SAVE_CHARACTER) {
            RemoveTraitFrom(target, "Restrained");
        } else {
            Restrained restrainedTrait = target.GetNormalTrait("Restrained") as Restrained;
            restrainedTrait.SetIsPrisoner(true);
        }
    }
    #endregion

    #region Intel Reactions
    private List<string> DropSuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        //Recipient and Target have at least one non-negative relationship and Actor is not from the same faction:
        Character targetCharacter = poiTarget as Character;

        if (isOldNews) {
            //Old News
            reactions.Add("This is old news.");
        } else {
            //Not Yet Old News
            if (awareCharactersOfThisAction.Contains(recipient)) {
                //- If Recipient is Aware
                reactions.Add("I know that already.");
            } else {
                //- Recipient is Actor
                if (recipient == actor) {
                    reactions.Add("I know what I did.");
                }
                //- Recipient is Target
                else if (recipient == targetCharacter) {
                    if (recipient.faction.id != actor.faction.id) {
                        reactions.Add("Please help me!");
                    } else {
                        reactions.Add(string.Format("I am grateful for {0}'s help.", actor.name));
                    }
                }
                //- Recipient Has Positive Relationship with Target
                else if (recipient.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.POSITIVE) {
                    if (recipient.faction.id != actor.faction.id) {
                        reactions.Add(string.Format("Thank you for letting me know about this. I've got to find a way to free {0}!", targetCharacter.name));
                        recipient.CreateSaveCharacterJob(targetCharacter);
                    } else {
                        reactions.Add(string.Format("I am grateful that {0} saved {1}.", actor.name, targetCharacter.name));
                        CharacterManager.Instance.RelationshipImprovement(actor, recipient, this);
                    }
                }
                //- Recipient Has Negative Relationship with Target
                else if (recipient.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.NEGATIVE) {
                    if (recipient.faction.id != actor.faction.id) {
                        reactions.Add("This news is music to my ears!");
                        AddTraitTo(recipient, "Cheery");
                    } else {
                        reactions.Add(string.Format("I am pissed that {0} had been brought back.", targetCharacter.name));
                        AddTraitTo(recipient, "Annoyed");
                    }
                }
                //- Recipient Has No Relationship with Target
                else {
                    if (recipient.faction.id != actor.faction.id) {
                        reactions.Add(string.Format("Thank you for letting me know about this. I've got to find a way to free {0}!", targetCharacter.name));
                        recipient.CreateSaveCharacterJob(targetCharacter);
                    } else {
                        reactions.Add(string.Format("It is good that {0} saved {1}.", actor.name, targetCharacter.name));
                    }
                }
            }
        }
        return reactions;
    }
    #endregion
}
