using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class RitualKilling : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public RitualKilling() : base(INTERACTION_TYPE.RITUAL_KILLING) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        isNotificationAnIntel = true;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, target = GOAP_EFFECT_TARGET.TARGET }, IsTargetInWildernessOrHome);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Killing Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = "\n" + name + " " + target.nameWithID + ": +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            if (actor.marker.CanDoStealthActionToTarget(poiTarget) == false) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.stateName = "Killing Fail";
            }
        }
        return goapActionInvalidity;
    }
    public override string ReactionToActor(Character witness, ActualGoapNode node) {
        string response = base.ReactionToActor(witness, node);
        Character actor = node.actor;
        IPointOfInterest target = node.poiTarget;
        if (target is Character) {
            if (witness.traitContainer.GetNormalTrait<Trait>("Coward") != null) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, actor);
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor);
            } else {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor);
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disgust, witness, actor);
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor);

                Character targetCharacter = target as Character;
                if (witness.opinionComponent.IsFriendsWith(actor) && !targetCharacter.isSerialKiller) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, witness, actor);
                }
                if (witness.opinionComponent.IsFriendsWith(targetCharacter)) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor);
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor);
                }
            }
        }
        CrimeManager.Instance.ReactToCrime(witness, actor, node, node.associatedJobType, CRIME_TYPE.SERIOUS);
        return response;
    }
    public override string ReactionToTarget(Character witness, ActualGoapNode node) {
        string response = base.ReactionToTarget(witness, node);
        Character actor = node.actor;
        IPointOfInterest target = node.poiTarget;
        if (target is Character) {
            Character targetCharacter = target as Character;
            if (!targetCharacter.isSerialKiller) {
                string opinionLabel = witness.opinionComponent.GetOpinionLabel(targetCharacter);
                if (opinionLabel == OpinionComponent.Acquaintance || opinionLabel == OpinionComponent.Friend || opinionLabel == OpinionComponent.Close_Friend) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor);
                }
            }
        }
        return response;
    }
    public override string ReactionOfTarget(ActualGoapNode node) {
        string response = base.ReactionOfTarget(node);
        Character actor = node.actor;
        IPointOfInterest target = node.poiTarget;
        if (target is Character) {
            Character targetCharacter = target as Character;
            if (targetCharacter.traitContainer.GetNormalTrait<Trait>("Coward") != null) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, targetCharacter, actor);
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, targetCharacter, actor);
            } else {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, targetCharacter, actor);

                if (targetCharacter.opinionComponent.IsFriendsWith(actor) && !targetCharacter.isSerialKiller) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, targetCharacter, actor);
                }
            }
            CrimeManager.Instance.ReactToCrime(targetCharacter, actor, node, node.associatedJobType, CRIME_TYPE.SERIOUS);
        }
        return response;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return actor != poiTarget && actor.isSerialKiller;
        }
        return false;
    }
    private bool IsTargetInWildernessOrHome(Character actor, IPointOfInterest target, object[] otherData) {
        if(target is Character) {
            Character targetCharacter = target as Character;
            return targetCharacter.IsInOwnParty() && (targetCharacter.currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS || targetCharacter.currentStructure == actor.homeStructure);
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreKillingSuccess(ActualGoapNode goapNode) {
        
    }
    public void AfterKillingSuccess(ActualGoapNode goapNode) {
        //goapNode.actor.needsComponent.AdjustHappiness(10000);
        if (goapNode.poiTarget is Character) {
            Character targetCharacter = goapNode.poiTarget as Character;
            targetCharacter.Death(deathFromAction: goapNode, responsibleCharacter: goapNode.actor);
            goapNode.actor.jobComponent.TriggerBurySerialKillerVictim(targetCharacter);
        }
    }
    #endregion
}

public class RitualKillingData : GoapActionData {
    public RitualKillingData() : base(INTERACTION_TYPE.RITUAL_KILLING) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor != poiTarget && actor.isSerialKiller;
    }
}

