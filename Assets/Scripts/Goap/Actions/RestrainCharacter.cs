using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestrainCharacter : GoapAction {

    private bool isForCriminal;

    public RestrainCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.RESTRAIN_CHARACTER, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Hostile_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_NON_POSITIVE_TRAIT, conditionKey = "Disabler", targetPOI = poiTarget }, HasNonPositiveDisablerTrait);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = poiTarget });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, conditionKey = "Negative", targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        Character target = poiTarget as Character;
        isForCriminal = parentPlan != null && parentPlan.job != null && (parentPlan.job.jobType == JOB_TYPE.APPREHEND || parentPlan.job.jobType == JOB_TYPE.RESTRAIN);
        if (!isTargetMissing && target.IsInOwnParty()) {
            SetState("Restrain Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 1;
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if (actor != poiTarget) {
            Character target = poiTarget as Character;
            return target.GetNormalTrait("Restrained") == null;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreRestrainSuccess() {
        currentState.SetIntelReaction(SuccessReactions);
    }
    public void AfterRestrainSuccess() {
        //**Effect 1**: Target gains Restrained trait.
        if(parentPlan.job != null) {
            parentPlan.job.SetCannotCancelJob(true);
        }
        AddTraitTo(poiTarget, new Restrained(), actor);
    }
    public void PreTargetMissing() {
        //**Effect 1**: Remove Target from Actor's Awareness list
        actor.RemoveAwareness(poiTarget);
    }
    #endregion

    #region Intel Reactions
    private List<string> SuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character target = poiTarget as Character;

        //If to imprison a criminal:
        if (isForCriminal) {
            if (recipient == actor) {
                //-Is Actor
                if (status == SHARE_INTEL_STATUS.INFORMED) {
                    //- If Informed: "[Target Name] did something wrong."
                    reactions.Add(string.Format("{0} did something wrong.", target.name));
                }
            } else if (recipient == target) {
                //- Is Target
                if (status == SHARE_INTEL_STATUS.INFORMED) {
                    //- If Informed: "I got caught."
                    reactions.Add("I got caught.");
                }
            } else {
                //- Otherwise:
                if (status == SHARE_INTEL_STATUS.INFORMED) {
                    //-If Informed: "If you do something bad here, you get imprisoned. That's the law."
                    reactions.Add("If you do something bad here, you get imprisoned. That's the law.");
                }
            }
        }
        //Otherwise (usually criminal stuff like Serial Killing):
        else {
            RELATIONSHIP_EFFECT relWithActor = recipient.GetRelationshipEffectWith(actor);
            RELATIONSHIP_EFFECT relWithTarget = recipient.GetRelationshipEffectWith(target);
            if (recipient == actor) {
                if (status == SHARE_INTEL_STATUS.INFORMED) {
                    //- If Informed: "Do not tell anybody, please!"
                    reactions.Add("Do not tell anybody, please!");
                }
            } else if (recipient == target) {
                if (status == SHARE_INTEL_STATUS.INFORMED) {
                    // - If Informed: "That was a traumatic experience."
                    reactions.Add("That was a traumatic experience.");
                }
            } else if (relWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
                if (relWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
                    CharacterManager.Instance.RelationshipDegradation(actor, recipient, this);
                    //- Considers it an Assault
                    recipient.ReactToCrime(CRIME.ASSAULT, this, actorAlterEgo, status);
                    if (status == SHARE_INTEL_STATUS.WITNESSED && actor.currentAction != null && actor.currentAction.parentPlan != null && actor.currentAction.parentPlan.job != null) {
                        //-If witnessed: Add Attempt to Stop Job targeting Actor
                        recipient.CreateAttemptToStopCurrentActionAndJob(target, actor.currentAction.parentPlan.job);
                    }
                    if (status == SHARE_INTEL_STATUS.INFORMED) {
                        //- If informed: "[Actor Name] shouldn't have done that to [Target Name]!"
                        reactions.Add(string.Format("{0} shouldn't have done that to {1}!", actor.name, target.name));
                    }
                } else if (relWithTarget == RELATIONSHIP_EFFECT.NONE) {
                    if (status == SHARE_INTEL_STATUS.INFORMED) {
                        // - If informed: "I'm sure there's a reason [Actor Name] did that."
                        reactions.Add(string.Format("I'm sure there's a reason {0} did that.", actor.name));
                    }
                } else if (relWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
                    if (status == SHARE_INTEL_STATUS.INFORMED) {
                        // - If informed: "I'm sure there's a reason [Actor Name] did that."
                        reactions.Add(string.Format("I'm sure there's a reason {0} did that.", actor.name));
                    }
                }
            } else if (relWithActor == RELATIONSHIP_EFFECT.NONE) {
                if (relWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
                    CharacterManager.Instance.RelationshipDegradation(actor, recipient, this);
                    //- Considers it an Assault
                    recipient.ReactToCrime(CRIME.ASSAULT, this, actorAlterEgo, status);
                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
                        //- If witnessed: Add Assault Job targeting Actor
                        recipient.CreateKnockoutJob(actor);
                    } else if (status == SHARE_INTEL_STATUS.INFORMED) {
                        //- If informed: "[Actor Name] shouldn't have done that to [Target Name]!"
                        reactions.Add(string.Format("{0} shouldn't have done that to {1}!", actor.name, target.name));
                    }
                } else if (relWithTarget == RELATIONSHIP_EFFECT.NONE) {
                    CharacterManager.Instance.RelationshipDegradation(actor, recipient, this);
                    //- Considers it an Assault
                    recipient.ReactToCrime(CRIME.ASSAULT, this, actorAlterEgo, status);
                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
                        //- If witnessed: Temporarily add Actor to Avoid List
                        recipient.marker.AddAvoidInRange(actor);
                    } else if (status == SHARE_INTEL_STATUS.INFORMED) {
                        //- If informed: "[Actor Name] shouldn't have done that to [Target Name]!"
                        reactions.Add(string.Format("{0} shouldn't have done that to {1}!", actor.name, target.name));
                    }
                } else if (relWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
                        //- If witnessed: Temporarily add Actor to Avoid List
                        recipient.marker.AddAvoidInRange(actor);
                    } else if (status == SHARE_INTEL_STATUS.INFORMED) {
                        //- If informed: "I am not fond of [Target Name] at all so I don't care what happens to [him/her]."
                        reactions.Add(string.Format("I am not fond of {0} at all so I don't care what happens to {1}.", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.OBJECTIVE, false)));
                    }
                }
            } else if (relWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
                if (relWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
                    CharacterManager.Instance.RelationshipDegradation(actor, recipient, this);
                    //- Considers it an Assault
                    recipient.ReactToCrime(CRIME.ASSAULT, this, actorAlterEgo, status);
                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
                        //- If witnessed: Add Assault Job targeting Actor
                        recipient.CreateKnockoutJob(actor);
                    } else if (status == SHARE_INTEL_STATUS.INFORMED) {
                        // - If informed:  Add Undermine Job targeting Actor
                        recipient.CreateUndermineJobOnly(actor, "informed");
                        //- If informed: "[Actor Name] is such a vile creature!"
                        reactions.Add(string.Format("{0} is such a vile creature!", actor.name));
                    }
                } else if (relWithTarget == RELATIONSHIP_EFFECT.NONE) {
                    CharacterManager.Instance.RelationshipDegradation(actor, recipient, this);
                    //- Considers it Aberration
                    recipient.ReactToCrime(CRIME.ABERRATION, this, actorAlterEgo, status);
                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
                        //- If witnessed: Add Assault Job targeting Actor
                        recipient.CreateKnockoutJob(actor);
                    } else if (status == SHARE_INTEL_STATUS.INFORMED) {
                        // - If informed:  Add Undermine Job targeting Actor
                        recipient.CreateUndermineJobOnly(actor, "informed");
                        //- If informed: "[Actor Name] is such a vile creature!"
                        reactions.Add(string.Format("{0} is such a vile creature!", actor.name));
                    }
                } else if (relWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
                    //- Considers it an Assault
                    recipient.ReactToCrime(CRIME.ASSAULT, this, actorAlterEgo, status);
                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
                        //- If witnessed: Temporarily add Actor to Avoid List
                        recipient.marker.AddAvoidInRange(actor);
                    } else if (status == SHARE_INTEL_STATUS.INFORMED) {
                        //- If informed: "My enemies fighting each other. What a happy day!"
                        reactions.Add("My enemies fighting each other. What a happy day!");
                    }
                }
            }
        }
        return reactions;
    }
    #endregion
}

public class RestrainCharacterData : GoapActionData {
    public RestrainCharacterData() : base(INTERACTION_TYPE.RESTRAIN_CHARACTER) {
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (actor != poiTarget) {
            Character target = poiTarget as Character;
            return target.GetNormalTrait("Restrained") == null;
        }
        return false;
    }
}
