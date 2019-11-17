﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Spit : GoapAction {
    protected override string failActionState { get { return "Target Missing"; } }

    public Spit() : base(INTERACTION_TYPE.SPIT, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Entertain_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        if (!isTargetMissing) {
            SetState("Spit Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        //**Cost**: randomize between 5-35
        return Utilities.rng.Next(5, 36);
    }
    #endregion

    #region Requirement
   protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null) {
            return false;
        }
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        if (poiTarget is Tombstone) {
            Tombstone tombstone = poiTarget as Tombstone;
            Character target = tombstone.character;
            return actor.relationshipContainer.GetRelationshipEffectWith(target) == RELATIONSHIP_EFFECT.NEGATIVE;
        }
        return false;
    }
    #endregion

    #region Effects
    private void PreSpitSuccess() {
        Tombstone tombstone = poiTarget as Tombstone;
        currentState.AddLogFiller(null, tombstone.character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        currentState.SetIntelReaction(SpitSuccessReactions);
    }
    private void AfterSpitSuccess() {
        actor.AdjustHappiness(5000);
    }
    private void PreTargetMissing() {
        Tombstone tombstone = poiTarget as Tombstone;
        currentState.AddLogFiller(null, tombstone.character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    #endregion

    #region Intel Reactions
    private List<string> SpitSuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Tombstone tombstone = poiTarget as Tombstone;
        Character targetCharacter = tombstone.character;

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
                    if(RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                        reactions.Add(string.Format("{0} does not respect me.", actor.name));
                        AddTraitTo(recipient, "Annoyed");
                    } else {
                        reactions.Add(string.Format("{0} should not do that again.", actor.name));
                    }
                }
                //- Recipient Has Positive Relationship with Target
                else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE) {
                    if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                        reactions.Add("That was very rude!");
                        AddTraitTo(recipient, "Annoyed");
                    } else {
                        reactions.Add(string.Format("{0} should not do that again.", actor.name));
                    }
                }
                //- Recipient Has Negative Relationship with Target
                else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.NEGATIVE) {
                    reactions.Add("That was not nice.");
                }
                //- Recipient Has No Relationship with Target
                else {
                    reactions.Add("That was not nice.");
                }
            }
        }
        return reactions;
    }
    #endregion
}

public class SpitData : GoapActionData {
    public SpitData() : base(INTERACTION_TYPE.SPIT) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null) {
            return false;
        }
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        if (poiTarget is Tombstone) {
            Tombstone tombstone = poiTarget as Tombstone;
            Character target = tombstone.character;
            return actor.relationshipContainer.GetRelationshipEffectWith(target) == RELATIONSHIP_EFFECT.NEGATIVE;
        }
        return false;
    }
}