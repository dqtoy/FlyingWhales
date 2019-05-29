using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpreadRumorRemoveLove : GoapAction {

    public Character rumoredCharacter { get; private set; } //This is the character whom the actor wants the poiTarget to remove love with
    public List<Log> affairMemoriesInvolvingRumoredCharacter { get; private set; }
    private Log _chosenMemory;

    public SpreadRumorRemoveLove(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.SPREAD_RUMOR_REMOVE_LOVE, INTERACTION_ALIGNMENT.EVIL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TARGET_REMOVE_RELATIONSHIP, conditionKey = "Lover", targetPOI = rumoredCharacter });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TARGET_REMOVE_RELATIONSHIP, conditionKey = "Paramour", targetPOI = rumoredCharacter });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            WeightedDictionary<string> weights = new WeightedDictionary<string>();
            weights.AddElement("Break Love Success", 10);
            weights.AddElement("Break Love Fail", 20);
            SetState(weights.PickRandomElementGivenWeights());
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 15;
    }
    public override bool InitializeOtherData(object[] otherData) {
        if (otherData.Length == 1 && otherData[0] is Character) {
            rumoredCharacter = otherData[0] as Character;
            int dayTo = GameManager.days;
            int dayFrom = dayTo - 3;
            if (dayFrom < 1) {
                dayFrom = 1;
            }
            List<Log> memories = actor.GetWitnessOrInformedMemories(dayFrom, dayTo);
            affairMemoriesInvolvingRumoredCharacter = new List<Log>();
            for (int i = 0; i < memories.Count; i++) {
                Log memory = memories[i];
                if (memory.goapAction.actor == rumoredCharacter && memory.goapAction.poiTarget != poiTarget) {
                    //if the event means Character 2 flirted, asked to make love or made love with another character other than Target, include it
                    if (memory.goapAction.goapType == INTERACTION_TYPE.CHAT_CHARACTER) {
                        ChatCharacter chatAction = memory.goapAction as ChatCharacter;
                        if (chatAction.chatResult == "flirt") {
                            affairMemoriesInvolvingRumoredCharacter.Add(memory);
                        }
                    } else if (memory.goapAction.goapType == INTERACTION_TYPE.INVITE_TO_MAKE_LOVE || memory.goapAction.goapType == INTERACTION_TYPE.MAKE_LOVE) {
                        affairMemoriesInvolvingRumoredCharacter.Add(memory);
                    }
                }
            }
            preconditions.Clear();
            expectedEffects.Clear();
            ConstructPreconditionsAndEffects();
            if (thoughtBubbleMovingLog != null) {
                thoughtBubbleMovingLog.AddToFillers(rumoredCharacter, rumoredCharacter.name, LOG_IDENTIFIER.CHARACTER_3);
            }
            return true;
        }
        return base.InitializeOtherData(otherData);
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if (rumoredCharacter != null) {
            Character target = poiTarget as Character;
            if (target.HasRelationshipOfTypeWith(rumoredCharacter, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.PARAMOUR)) {
                return actor != poiTarget && actor != rumoredCharacter && affairMemoriesInvolvingRumoredCharacter.Count > 0;
            }
            return false;
        }
        return actor != poiTarget;
    }
    #endregion

    #region State Effects
    public void PreBreakLoveSuccess() {
        Character target = poiTarget as Character;
        _chosenMemory = affairMemoriesInvolvingRumoredCharacter[UnityEngine.Random.Range(0, affairMemoriesInvolvingRumoredCharacter.Count)];
        currentState.AddLogFiller(rumoredCharacter, rumoredCharacter.name, LOG_IDENTIFIER.CHARACTER_3);
        currentState.AddLogFiller(null, Utilities.LogReplacer(_chosenMemory.goapAction.currentState.descriptionLog), LOG_IDENTIFIER.STRING_1);
    }
    public void AfterBreakLoveSuccess() {
        Character target = poiTarget as Character;
        //**Effect 1**: Target - Remove Love relationship with Character 2 
        CharacterManager.Instance.RemoveRelationshipBetween(target, rumoredCharacter, RELATIONSHIP_TRAIT.LOVER);
        CharacterManager.Instance.RemoveRelationshipBetween(target, rumoredCharacter, RELATIONSHIP_TRAIT.PARAMOUR);

        //**Effect 2**: Target - Add shared event to Target's memory
        target.CreateInformedEventLog(_chosenMemory.goapAction);
        //Log informedLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "informed_event", _chosenMemory.goapAction);
        //informedLog.AddToFillers(target, target.name, LOG_IDENTIFIER.OTHER);
        //informedLog.AddToFillers(null, Utilities.LogDontReplace(_chosenMemory), LOG_IDENTIFIER.APPEND);
        //informedLog.AddToFillers(_chosenMemory.fillers);
        //target.AddHistory(informedLog);
    }
    public void PreBreakLoveFail() {
        Character target = poiTarget as Character;
        _chosenMemory = affairMemoriesInvolvingRumoredCharacter[UnityEngine.Random.Range(0, affairMemoriesInvolvingRumoredCharacter.Count)];
        currentState.AddLogFiller(null, Utilities.LogReplacer(_chosenMemory.goapAction.currentState.descriptionLog), LOG_IDENTIFIER.STRING_1);
    }
    public void AfterBreakLoveFail() {
        Character target = poiTarget as Character;

        //**Effect 2**: Target - Add shared event to Target's memory
        target.CreateInformedEventLog(_chosenMemory.goapAction);
        //Log informedLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "informed_event", _chosenMemory.goapAction);
        //informedLog.AddToFillers(target, target.name, LOG_IDENTIFIER.OTHER);
        //informedLog.AddToFillers(null, Utilities.LogDontReplace(_chosenMemory), LOG_IDENTIFIER.APPEND);
        //informedLog.AddToFillers(_chosenMemory.fillers);
        //target.AddHistory(informedLog);
    }
    public void PreTargetMissing() {
        currentState.AddLogFiller(rumoredCharacter, rumoredCharacter.name, LOG_IDENTIFIER.CHARACTER_3);
    }
    #endregion
}
