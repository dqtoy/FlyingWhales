using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropCharacter : GoapAction {
    private LocationStructure _workAreaStructure;

    public override LocationStructure targetStructure {
        get { return _workAreaStructure; }
    }

    public DropCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.DROP_CHARACTER, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        _workAreaStructure = actor.homeArea.GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE);
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
        SetState("Drop Success");
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 1;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Target Missing");
    //}
    public override void DoAction(GoapPlan plan) {
        SetTargetStructure();
        base.DoAction(plan);
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
        currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        currentState.AddLogFiller(_workAreaStructure.location, _workAreaStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        currentState.SetIntelReaction(DropSuccessIntelReaction);
    }
    public void AfterDropSuccess() {
        Character target = poiTarget as Character;
        actor.ownParty.RemoveCharacter(target);
        //target.MoveToAnotherStructure(_workAreaStructure);
        AddActualEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = actor.homeArea, targetPOI = poiTarget });
        if(target.gridTileLocation.structure.structureType == STRUCTURE_TYPE.WAREHOUSE) {
            //Create judgement job
        }
    }
    #endregion

    #region Intel Reactions
    private List<string> DropSuccessIntelReaction(Character recipient) {
        List<string> reactions = new List<string>();
        //Recipient and Target have at least one non-negative relationship and Actor is not from the same faction:
        Character targetCharacter = poiTarget as Character;
        if (recipient.HasRelationshipOfEffectWith(targetCharacter, TRAIT_EFFECT.POSITIVE) && actor.faction != recipient.faction) {
            //- **Recipient Response Text**: "Thank you for letting me know about this. I've got to find a way to free [Target Name]!
            reactions.Add(string.Format("Thank you for letting me know about this. I've got to find a way to free {0}!", targetCharacter.name));
            //-**Recipient Effect**: If Adventurer or Soldier or Unaligned Non-Beast, create a Save Target plan.If Civilian, Noble or Faction Leader, create an Ask for Save Help plan.
            if (recipient.role.roleType == CHARACTER_ROLE.ADVENTURER || recipient.role.roleType == CHARACTER_ROLE.SOLDIER || (recipient.isFactionless && recipient.role.roleType != CHARACTER_ROLE.BEAST)) {
                recipient.StartGOAP(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Abducted" }, targetCharacter, GOAP_CATEGORY.REACTION, true, new List<Character>() { targetCharacter }, false);
            }
        }
        //Recipient and Target have no relationship but from the same faction and Actor is not from the same faction:
        else if (recipient.relationships.ContainsKey(targetCharacter) && recipient.faction == targetCharacter.faction && recipient.faction != actor.faction) {
            //- **Recipient Response Text**: "Thank you for letting me know about this. I've got to find a way to free [Target Name]!
            reactions.Add(string.Format("Thank you for letting me know about this. I've got to find a way to free {0}!", targetCharacter.name));
            //-**Recipient Effect**: If Adventurer or Soldier or Unaligned Non-Beast, create a Save Target plan.If Civilian, Noble or Faction Leader, create an Ask for Save Help plan.
            if (recipient.role.roleType == CHARACTER_ROLE.ADVENTURER || recipient.role.roleType == CHARACTER_ROLE.SOLDIER || (recipient.isFactionless && recipient.role.roleType != CHARACTER_ROLE.BEAST)) {
                recipient.StartGOAP(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Abducted" }, targetCharacter, GOAP_CATEGORY.REACTION, true, new List<Character>() { targetCharacter }, false);
            }
        }
        //Recipient and Target are enemies:
        else if (recipient.GetRelationshipTraitWith(targetCharacter, RELATIONSHIP_TRAIT.ENEMY) != null) {
            //- **Recipient Response Text**: "[Target Name] got abducted? That's what [he/she] deserves."
            reactions.Add(string.Format("{0} got abducted? That's what {1} deserves.", targetCharacter.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
            //-**Recipient Effect**: no effect
        }
        //Recipient, Actor and Target are from the same faction and Target has Criminal Trait:
        if (recipient.faction == actor.faction && recipient.faction == targetCharacter.faction && targetCharacter.GetTrait("Criminal") != null) {
            //- **Recipient Response Text**: "[Target Name] is a criminal. I cannot do anything for [him/her]."
            reactions.Add(string.Format("{0} is a criminal. I cannot do anything for {1}.", targetCharacter.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.OBJECTIVE, false)));
            //-**Recipient Effect**: no effect
        }
        return reactions;
    }
    #endregion
}
