using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollOfFrenzy : SpecialToken {

    public ScrollOfFrenzy() : base(SPECIAL_TOKEN.SCROLL_OF_FRENZY, 100) {
        npcAssociatedInteractionType = INTERACTION_TYPE.USE_ITEM_ON_CHARACTER;
        interactionAttributes = new InteractionAttributes() {
            categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUBTERFUGE },
            alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            actorEffect = null,
            targetCharacterEffect = new InteractionCharacterEffect[] {
                new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TRAIT_GAIN, effectString = "Berserker" }
            },
        };
    }

    #region Overrides
    public override void CreateJointInteractionStates(Interaction interaction, Character user, object target) {
        TokenInteractionState itemUsedState = new TokenInteractionState(Item_Used, interaction, this);
        TokenInteractionState stopFailState = new TokenInteractionState(Stop_Fail, interaction, this);
        itemUsedState.SetTokenUserAndTarget(user, target);
        stopFailState.SetTokenUserAndTarget(user, target);

        itemUsedState.SetEffect(() => ItemUsedEffect(itemUsedState));
        stopFailState.SetEffect(() => StopFailEffect(stopFailState));

        interaction.AddState(itemUsedState);
        interaction.AddState(stopFailState);
    }
    public override bool CanBeUsedBy(Character sourceCharacter) {
        return true;
    }
    public override bool CanBeUsedForTarget(Character sourceCharacter, Character targetCharacter) {
        return sourceCharacter.specificLocation.id == targetCharacter.specificLocation.id;
    }
    public override void StartTokenInteractionState(Character user, Character target) {
        //User will stay in current structure
    }
    #endregion

    private void ItemUsedEffect(TokenInteractionState state) {
        Character targetCharacter = state.target as Character;
        Trait berserkerTrait = AttributeManager.Instance.allTraits["Berserker"];
        targetCharacter.AddTrait(berserkerTrait);
        //state.tokenUser.LevelUp();
        state.tokenUser.ConsumeToken(this);

        if (state.tokenUser.id == targetCharacter.id) {
            //Used item on self
            Log stateDescriptionLog = new Log(GameManager.Instance.Today(), "Tokens", this.GetType().ToString(), state.name.ToLower() + "-npc" + "_description", state.interaction);
            stateDescriptionLog.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            state.OverrideDescriptionLog(stateDescriptionLog);

            Log log = new Log(GameManager.Instance.Today(), "Tokens", GetType().ToString(), state.name.ToLower() + "_special2");
            log.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            state.AddLogToInvolvedObjects(log);
        } else {
            //Used item on other character
            Log stateDescriptionLog = new Log(GameManager.Instance.Today(), "Tokens", this.GetType().ToString(), state.name.ToLower() + "-othernpc" + "_description", state.interaction);
            stateDescriptionLog.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            stateDescriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.OverrideDescriptionLog(stateDescriptionLog);

            Log log = new Log(GameManager.Instance.Today(), "Tokens", GetType().ToString(), state.name.ToLower() + "_special3");
            log.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.AddLogToInvolvedObjects(log);
        }
    }
    private void StopFailEffect(TokenInteractionState state) {
        Trait berserkerTrait = AttributeManager.Instance.allTraits["Berserker"];
        (state.target as Character).AddTrait(berserkerTrait);
        //state.tokenUser.LevelUp();
        state.tokenUser.ConsumeToken(this);

        state.descriptionLog.AddToFillers(state.interaction.investigatorCharacter, state.interaction.investigatorCharacter.name, LOG_IDENTIFIER.MINION_1);
        state.descriptionLog.AddToFillers(null, berserkerTrait.name, LOG_IDENTIFIER.STRING_1);
        state.descriptionLog.AddToFillers(null, this.name, LOG_IDENTIFIER.ITEM_1);

        state.AddLogFiller(new LogFiller(state.interaction.investigatorCharacter, state.interaction.investigatorCharacter.name, LOG_IDENTIFIER.MINION_1));
        state.AddLogFiller(new LogFiller(null, berserkerTrait.name, LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(null, this.name, LOG_IDENTIFIER.ITEM_1));
    }
}
