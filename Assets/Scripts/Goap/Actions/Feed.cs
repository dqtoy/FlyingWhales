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
        actor.AdjustSupply(-10);
        currentState.SetIntelReaction(State1Reactions);
    }
    private void PerTickFeedSuccess() {
        _target.AdjustFullness(12);
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
    private List<string> State1Reactions(Character recipient, Intel sharedIntel) {
        List<string> reactions = new List<string>();
        Character target = poiTarget as Character;

        if (recipient == actor) {
            return reactions; //return empty list if same actor
        }

        RELATIONSHIP_EFFECT relWithTarget = recipient.GetRelationshipEffectWith(poiTargetAlterEgo);

        //Recipient and Target have at least one non-negative relationship and Actor is not from the same faction:
        if (relWithTarget == RELATIONSHIP_EFFECT.POSITIVE && actorAlterEgo.faction != recipient.faction) {
            //- **Recipient Response Text**: "Thank you for letting me know where [Target Name] is! I've got to find a way to free [him/her]!
            reactions.Add(string.Format("Thank you for letting me know where {0} is! I've got to find a way to free {1}", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.OBJECTIVE, false)));
            //-**Recipient Effect * *: If Adventurer or Soldier or Unaligned Non - Beast, create a https://trello.com/c/aQPY0gej/1657-save-troubled-character-job. 
            if (recipient.role.roleType == CHARACTER_ROLE.ADVENTURER || recipient.role.roleType == CHARACTER_ROLE.SOLDIER 
                || (recipient.role.roleType != CHARACTER_ROLE.BEAST && recipient.isFactionless)) {
                recipient.CreateSaveCharacterJob(target);
            }
            //If Civilian, Noble or Faction Leader, create a https://trello.com/c/zuF4ongh/1658-ask-for-help-save-character-job.
            else if (recipient.role.roleType == CHARACTER_ROLE.CIVILIAN || recipient.role.roleType == CHARACTER_ROLE.NOBLE 
                || recipient.role.roleType == CHARACTER_ROLE.LEADER) {
                recipient.CreateAskForHelpSaveCharacterJob(target);
            }
        }

        //Recipient and Target have no relationship but from the same faction and Actor is not from the same faction:
        else if (relWithTarget == RELATIONSHIP_EFFECT.NONE && recipient.faction == poiTargetAlterEgo.faction && recipient.faction != actorAlterEgo.faction) {
            //- **Recipient Response Text**: "Thank you for letting me know where [Target Name] is! I've got to find a way to free [him/her]!
            reactions.Add(string.Format("Thank you for letting me know where {0} is! I've got to find a way to free {1}", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.OBJECTIVE, false)));
            //-**Recipient Effect * *: If Adventurer or Soldier or Unaligned Non - Beast, create a https://trello.com/c/aQPY0gej/1657-save-troubled-character-job. 
            if (recipient.role.roleType == CHARACTER_ROLE.ADVENTURER || recipient.role.roleType == CHARACTER_ROLE.SOLDIER
                || (recipient.role.roleType != CHARACTER_ROLE.BEAST && recipient.isFactionless)) {
                recipient.CreateSaveCharacterJob(target);
            }
            //If Civilian, Noble or Faction Leader, create a https://trello.com/c/zuF4ongh/1658-ask-for-help-save-character-job.
            else if (recipient.role.roleType == CHARACTER_ROLE.CIVILIAN || recipient.role.roleType == CHARACTER_ROLE.NOBLE
                || recipient.role.roleType == CHARACTER_ROLE.LEADER) {
                recipient.CreateAskForHelpSaveCharacterJob(target);
            }
        }

        //Recipient and Target are enemies:
        else if (recipient.HasRelationshipOfTypeWith(poiTargetAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
            //- **Recipient Response Text**: "I don't really care what happens to [Target Name]. I hope they stop feeding [him/her]."
            reactions.Add(string.Format("I don't really care what happens to {0}. I hope they stop feeding {1}", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.OBJECTIVE, false)));
            //-**Recipient Effect * *: no effect
        }

        //Recipient, Actor and Target are from the same faction and Target has Criminal Trait:
        else if (recipient.faction == actorAlterEgo.faction && recipient.faction == poiTargetAlterEgo.faction && target.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
            //- **Recipient Response Text**: "Though [Target Name] is a criminal, it's only fair that [he/she] gets to eat sometimes."
            reactions.Add(string.Format("Though {0} is a criminal, it's only fair that {1} gets to eat sometimes.", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
            //-**Recipient Effect * *: no effect
        }

        return reactions;
    }
    #endregion
}
