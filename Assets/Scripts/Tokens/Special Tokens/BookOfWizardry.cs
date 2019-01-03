using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookOfWizardry : SpecialToken {

    public BookOfWizardry() : base(SPECIAL_TOKEN.BOOK_OF_WIZARDRY) {
        quantity = 1;
        weight = 20;
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
    }
    public override bool CanBeUsedBy(Character sourceCharacter) {
        //return true;
        return sourceCharacter.gender == GENDER.MALE && sourceCharacter.characterClass.attackType == ATTACK_TYPE.MAGICAL_RANGED && sourceCharacter.role.roleType != CHARACTER_ROLE.BEAST;
    }
    #endregion

    private void ItemUsedEffectMinion(TokenInteractionState state) {
        Character targetCharacter = state.target as Character;
        targetCharacter.ChangeClass("Archmage");

        Log stateDescriptionLog = new Log(GameManager.Instance.Today(), "Tokens", this.GetType().ToString(), state.name.ToLower() + "-minion" + "_description");
        stateDescriptionLog.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.MINION_1);
        stateDescriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        state.OverrideDescriptionLog(stateDescriptionLog);

        Log log = new Log(GameManager.Instance.Today(), "Tokens", GetType().ToString(), state.name.ToLower() + "_special1");
        stateDescriptionLog.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.MINION_1);
        stateDescriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        state.AddLogToInvolvedObjects(log);
    }
    private void ItemUsedEffectNPC(TokenInteractionState state) {
        state.tokenUser.ChangeClass("Archmage");
        state.tokenUser.ConsumeToken();

        Log stateDescriptionLog = new Log(GameManager.Instance.Today(), "Tokens", this.GetType().ToString(), state.name.ToLower() + "-npc" + "_description");
        stateDescriptionLog.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        state.OverrideDescriptionLog(stateDescriptionLog);

        Log log = new Log(GameManager.Instance.Today(), "Tokens", GetType().ToString(), state.name.ToLower() + "_special2");
        stateDescriptionLog.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        state.AddLogToInvolvedObjects(log);

        Debug.LogWarning("[Day " + GameManager.Instance.continuousDays + "] " + state.tokenUser.name + " used " + name + " on " + Utilities.GetPronounString(state.tokenUser.gender, PRONOUN_TYPE.REFLEXIVE, false) + " and became a " + state.tokenUser.characterClass.className + " at " + state.tokenUser.specificLocation.tileLocation.areaOfTile.name);
    }
    private void StopFailEffect(TokenInteractionState state) {
        state.tokenUser.ChangeClass("Archmage");
        state.tokenUser.ConsumeToken();

        state.descriptionLog.AddToFillers(state.interaction.investigatorMinion, state.interaction.investigatorMinion.name, LOG_IDENTIFIER.MINION_1);

        state.AddLogFiller(new LogFiller(state.interaction.investigatorMinion, state.interaction.investigatorMinion.name, LOG_IDENTIFIER.MINION_1));

        Debug.LogWarning("[Day " + GameManager.Instance.continuousDays + "] " + state.tokenUser.name + " used " + name + " on " + Utilities.GetPronounString(state.tokenUser.gender, PRONOUN_TYPE.REFLEXIVE, false) + " and became a " + state.tokenUser.characterClass.className + " at " + state.tokenUser.specificLocation.tileLocation.areaOfTile.name);
    }
}
