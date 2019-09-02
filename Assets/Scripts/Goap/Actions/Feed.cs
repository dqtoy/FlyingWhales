using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feed : GoapAction {
    private Character _target;

    public Feed(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.FEED, INTERACTION_ALIGNMENT.GOOD, actor, poiTarget) {
        _target = poiTarget as Character;
        actionIconString = GoapActionStateDB.FirstAid_Icon;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
            TIME_IN_WORDS.LATE_NIGHT,
        };
        _stayInArea = true;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = 0, targetPOI = actor }, () => HasSupply(10));
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing && (poiTarget as Character).IsInOwnParty()) {
            SetState("Feed Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 1;
    }
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    public override void OnStopActionDuringCurrentState() {
        if (currentState.name == "Feed Success") {
            _target.AdjustDoNotGetHungry(-1);
        }
    }
    #endregion

    #region Effects
    private void PreFeedSuccess() {
        //currentState.AddLogFiller(_target, _target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        _target.AdjustDoNotGetHungry(1);
        actor.AdjustFood(-20);
        currentState.SetIntelReaction(FeedSuccessReactions);
    }
    private void PerTickFeedSuccess() {
        _target.AdjustFullness(585);
    }
    private void AfterFeedSuccess() {
        _target.AdjustDoNotGetHungry(-1);
    }
    //private void PreTargetMissing() {
    //currentState.AddLogFiller(_target, _target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    #endregion

    #region Requirements
    protected bool Requirement() {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return true;
    }
    #endregion

    #region Intel Reactions
    private List<string> FeedSuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
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
                    if(targetCharacter.isAtHomeArea) {
                        reactions.Add("I am paying for my mistakes.");
                    } else {
                        reactions.Add("Please help me!");
                    }
                }
                //- Recipient Has Positive Relationship with Target
                else if (recipient.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.POSITIVE) {
                    if (targetCharacter.isAtHomeArea) {
                        reactions.Add(string.Format("{0} is paying for {1} mistakes.", targetCharacter.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.POSSESSIVE, false)));
                    } else {
                        reactions.Add(string.Format("I've got to figure out how to save {0}!", targetCharacter.name));
                        recipient.CreateSaveCharacterJob(targetCharacter);
                    }
                }
                //- Recipient Has Negative Relationship with Target
                else if (recipient.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.NEGATIVE) {
                    //if (targetCharacter.isAtHomeArea) {
                    //    reactions.Add(string.Format("I hope {0} rots in there!", targetCharacter.name));
                    //    AddTraitTo(recipient, "Cheery");
                    //} else {
                    //    reactions.Add(string.Format("I hope {0} rots in there!", targetCharacter.name));
                    //    AddTraitTo(recipient, "Cheery");
                    //}
                    reactions.Add(string.Format("I hope {0} rots in there!", targetCharacter.name));
                    AddTraitTo(recipient, "Cheery");
                }
                //- Recipient Has No Relationship with Target
                else {
                    if(recipient.faction.id == targetCharacter.faction.id) {
                        if (targetCharacter.isAtHomeArea) {
                            reactions.Add(string.Format("{0} is a criminal", targetCharacter.name));
                        } else {
                            reactions.Add(string.Format("I've got to figure out how to save {0}!", targetCharacter.name));
                            recipient.CreateSaveCharacterJob(targetCharacter);
                        }
                    } else {
                        //if (targetCharacter.isAtHomeArea) {
                        //    reactions.Add("This isn't relevant to me.");
                        //} else {
                        //    reactions.Add("This isn't relevant to me.");
                        //}
                        reactions.Add("This isn't relevant to me.");
                    }
                }
            }
        }
        return reactions;
    }
    #endregion
}

public class FeedData : GoapActionData {
    public FeedData() : base(INTERACTION_TYPE.FEED) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return true;
    }
}
