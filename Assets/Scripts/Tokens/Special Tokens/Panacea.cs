using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panacea : SpecialToken {

    public Panacea() : base(SPECIAL_TOKEN.PANACEA, 100) {
        npcAssociatedInteractionType = INTERACTION_TYPE.USE_ITEM_ON_SELF;
        interactionAttributes = new InteractionAttributes() {
            categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SAVE },
            alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            actorEffect = null,
            targetCharacterEffect = new InteractionCharacterEffect[] {
                new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TRAIT_REMOVE, effectString = "Sick" }
            },
        };
    }

    #region Overrides
    public override void CreateJointInteractionStates(Interaction interaction, Character user, object target) {
        TokenInteractionState itemUsedState = new TokenInteractionState(Item_Used, interaction, this);
        TokenInteractionState stopFailState = new TokenInteractionState(Stop_Fail, interaction, this);
        itemUsedState.SetTokenUserAndTarget(user, target);
        stopFailState.SetTokenUserAndTarget(user, target);

        //if (target != null) {
        //    //This means that the interaction is not from Use Item On Self, rather, it is from an interaction which a minion triggered
        //    itemUsedState.SetEffect(() => ItemUsedEffectMinion(itemUsedState));
        //} else {
        //    itemUsedState.SetEffect(() => ItemUsedEffectNPC(itemUsedState));
        //}

        //Since only USE_ITEM_ON_CHARACTER is being used, user and target will always have value
        itemUsedState.SetEffect(() => ItemUsedEffectNPC(itemUsedState));
        stopFailState.SetEffect(() => StopFailEffect(stopFailState));

        interaction.AddState(itemUsedState);
        interaction.AddState(stopFailState);
    }
    public override bool CanBeUsedBy(Character sourceCharacter) {
        return true;
        //return sourceCharacter.HasTraitOf(TRAIT_TYPE.ILLNESS);
    }
    public override bool CanBeUsedForTarget(Character sourceCharacter, Character targetCharacter) {
        return sourceCharacter.specificLocation.id == targetCharacter.specificLocation.id;
    }
    #endregion

    private void ItemUsedEffectMinion(TokenInteractionState state) {
        Character targetCharacter = state.target as Character;
        targetCharacter.RemoveTrait("Sick");
        state.tokenUser.ConsumeToken();

        Log stateDescriptionLog = new Log(GameManager.Instance.Today(), "Tokens", this.GetType().ToString(), state.name.ToLower() + "-minion" + "_description", state.interaction);
        stateDescriptionLog.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.MINION_1);
        stateDescriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        state.OverrideDescriptionLog(stateDescriptionLog);

        Log log = new Log(GameManager.Instance.Today(), "Tokens", GetType().ToString(), state.name.ToLower() + "_special1");
        log.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.MINION_1);
        log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        state.AddLogToInvolvedObjects(log);
    }
    private void ItemUsedEffectNPC(TokenInteractionState state) {
        Character targetCharacter = state.target as Character;
        targetCharacter.RemoveTrait("Sick");
        state.tokenUser.ConsumeToken();

        if(state.tokenUser.id == targetCharacter.id) {
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
        Character targetCharacter = state.target as Character;
        targetCharacter.RemoveTrait("Sick");
        state.tokenUser.ConsumeToken();

        state.descriptionLog.AddToFillers(state.interaction.investigatorCharacter, state.interaction.investigatorCharacter.name, LOG_IDENTIFIER.MINION_1);

        state.AddLogFiller(new LogFiller(state.interaction.investigatorCharacter, state.interaction.investigatorCharacter.name, LOG_IDENTIFIER.MINION_1));
    }
}