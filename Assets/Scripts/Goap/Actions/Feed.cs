using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Feed : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public Feed() : base(INTERACTION_TYPE.FEED) {
        actionIconString = GoapActionStateDB.FirstAid_Icon;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.LUNCH_TIME,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
            TIME_IN_WORDS.LATE_NIGHT,
        };
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_WOOD, conditionKey = "0", isKeyANumber = true, target = GOAP_EFFECT_TARGET.ACTOR }, ActorHasSupply);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = string.Empty, target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Feed Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 1;
    }
    public override void OnStopWhilePerforming(Character actor, IPointOfInterest target, object[] otherData) {
        base.OnStopWhilePerforming(actor, target, otherData);
        (target as Character).AdjustDoNotGetHungry(-1);
    }
    public override GoapActionInvalidity IsInvalid(Character actor, IPointOfInterest target, object[] otherData) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(actor, target, otherData);
        if (goapActionInvalidity.isInvalid == false) {
            if ((target as Character).IsInOwnParty() == false) {
                goapActionInvalidity.isInvalid = true;
            }
        }
        return goapActionInvalidity;
    }
    #endregion

    #region Effects
    private void PreFeedSuccess(ActualGoapNode goapNode) {
        Character targetCharacter = goapNode.poiTarget as Character;
        targetCharacter.AdjustDoNotGetHungry(1);
        goapNode.actor.AdjustFood(-20);
        //TODO: goapNode.action.states[goapNode.currentStateName].SetIntelReaction(FeedSuccessReactions);
    }
    private void PerTickFeedSuccess(ActualGoapNode goapNode) {
        Character targetCharacter = goapNode.poiTarget as Character;
        targetCharacter.AdjustFullness(585);
    }
    private void AfterFeedSuccess(ActualGoapNode goapNode) {
        Character targetCharacter = goapNode.poiTarget as Character;
        targetCharacter.AdjustDoNotGetHungry(-1);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
                return false;
            }
        }
        return false;
    }
    #endregion

    //#region Intel Reactions
    //private List<string> FeedSuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    Character targetCharacter = poiTarget as Character;

    //    if (isOldNews) {
    //        //Old News
    //        reactions.Add("This is old news.");
    //    } else {
    //        //Not Yet Old News
    //        if (awareCharactersOfThisAction.Contains(recipient)) {
    //            //- If Recipient is Aware
    //            reactions.Add("I know that already.");
    //        } else {
    //            //- Recipient is Actor
    //            if (recipient == actor) {
    //                reactions.Add("I know what I did.");
    //            }
    //            //- Recipient is Target
    //            else if (recipient == targetCharacter) {
    //                if(targetCharacter.isAtHomeRegion) {
    //                    reactions.Add("I am paying for my mistakes.");
    //                } else {
    //                    reactions.Add("Please help me!");
    //                }
    //            }
    //            //- Recipient Has Positive Relationship with Target
    //            else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE) {
    //                if (targetCharacter.isAtHomeRegion) {
    //                    reactions.Add(string.Format("{0} is paying for {1} mistakes.", targetCharacter.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.POSSESSIVE, false)));
    //                } else {
    //                    reactions.Add(string.Format("I've got to figure out how to save {0}!", targetCharacter.name));
    //                    recipient.CreateSaveCharacterJob(targetCharacter);
    //                }
    //            }
    //            //- Recipient Has Negative Relationship with Target
    //            else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                //if (targetCharacter.isAtHomeArea) {
    //                //    reactions.Add(string.Format("I hope {0} rots in there!", targetCharacter.name));
    //                //    AddTraitTo(recipient, "Satisfied");
    //                //} else {
    //                //    reactions.Add(string.Format("I hope {0} rots in there!", targetCharacter.name));
    //                //    AddTraitTo(recipient, "Satisfied");
    //                //}
    //                reactions.Add(string.Format("I hope {0} rots in there!", targetCharacter.name));
    //                AddTraitTo(recipient, "Satisfied");
    //            }
    //            //- Recipient Has No Relationship with Target
    //            else {
    //                if(recipient.faction.id == targetCharacter.faction.id) {
    //                    if (targetCharacter.isAtHomeRegion) {
    //                        reactions.Add(string.Format("{0} is a criminal!", targetCharacter.name));
    //                    } else {
    //                        reactions.Add(string.Format("I've got to figure out how to save {0}!", targetCharacter.name));
    //                        recipient.CreateSaveCharacterJob(targetCharacter);
    //                    }
    //                } else {
    //                    //if (targetCharacter.isAtHomeArea) {
    //                    //    reactions.Add("This isn't relevant to me.");
    //                    //} else {
    //                    //    reactions.Add("This isn't relevant to me.");
    //                    //}
    //                    reactions.Add("This isn't relevant to me.");
    //                }
    //            }
    //        }
    //    }
    //    return reactions;
    //}
    //#endregion

    #region Preconditions
    private bool ActorHasSupply(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor.supply >= 10;
    }
    #endregion
}

public class FeedData : GoapActionData {
    public FeedData() : base(INTERACTION_TYPE.FEED) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return true;
    }
}
