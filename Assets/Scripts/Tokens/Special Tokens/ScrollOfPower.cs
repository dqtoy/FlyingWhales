using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollOfPower : SpecialToken {

    public ScrollOfPower() : base(SPECIAL_TOKEN.SCROLL_OF_POWER, 80) {
        npcAssociatedInteractionType = INTERACTION_TYPE.USE_ITEM_ON_SELF;
    }

    #region Overrides
    public override void CreateJointInteractionStates(Interaction interaction, Character user, object target) {
        TokenInteractionState itemUsedState = new TokenInteractionState(Item_Used, interaction, this);
        TokenInteractionState stopFailState = new TokenInteractionState(Stop_Fail, interaction, this);
        itemUsedState.SetTokenUserAndTarget(user, target);
        stopFailState.SetTokenUserAndTarget(user, target);

        if (target != null) {
            //This means that the interaction is not from Use Item On Self, rather, it is from an interaction which a minion triggered
            itemUsedState.SetEffect(() => ItemUsedEffectMinion(itemUsedState));
        } else {
            itemUsedState.SetEffect(() => ItemUsedEffectNPC(itemUsedState));
        }
        stopFailState.SetEffect(() => StopFailEffect(stopFailState));

        interaction.AddState(itemUsedState);
        interaction.AddState(stopFailState);

        //interaction.SetCurrentState(inflictIllnessState);
    }
    public override bool CanBeUsedBy(Character sourceCharacter) {
        //Requirement: Character is not max level.
        return sourceCharacter.level < CharacterManager.Instance.maxLevel;
    }
    #endregion

    private void ItemUsedEffectNPC(TokenInteractionState state) {
        state.tokenUser.ConsumeToken();

        //**Mechanics**: Character Level +2
        state.tokenUser.LevelUp(2);
        Log stateDescriptionLog = new Log(GameManager.Instance.Today(), "Tokens", this.GetType().ToString(), state.name.ToLower() + "-npc" + "_description");
        stateDescriptionLog.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        state.OverrideDescriptionLog(stateDescriptionLog);

        Log log = new Log(GameManager.Instance.Today(), "Tokens", GetType().ToString(), state.name.ToLower() + "_special2");
        log.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        state.AddLogToInvolvedObjects(log);
    }
    private void ItemUsedEffectMinion(TokenInteractionState state) {
        state.tokenUser.ConsumeToken();

        //**Mechanics**: Character Level +2
        Character targetCharacter = state.target as Character;
        if (targetCharacter != null) {
            targetCharacter.LevelUp(2);

            Log stateDescriptionLog = new Log(GameManager.Instance.Today(), "Tokens", this.GetType().ToString(), state.name.ToLower() + "-minion" + "_description");
            stateDescriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.OverrideDescriptionLog(stateDescriptionLog);

            Log log = new Log(GameManager.Instance.Today(), "Tokens", GetType().ToString(), state.name.ToLower() + "_special1");
            log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.AddLogToInvolvedObjects(log);
        }
    }
    private void StopFailEffect(TokenInteractionState state) {
        state.tokenUser.ConsumeToken();

        //**Mechanics**: Character Level +2
        Character targetCharacter = state.target as Character;
        if (targetCharacter != null) {
            targetCharacter.LevelUp(2);
        }
    }
}
