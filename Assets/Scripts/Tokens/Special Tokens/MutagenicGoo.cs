using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutagenicGoo : SpecialToken {

    public MutagenicGoo() : base(SPECIAL_TOKEN.MUTAGENIC_GOO) {
        weight = 80;
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
        return true;
    }
    #endregion
    
    private void ItemUsedEffectNPC(TokenInteractionState state) {
        ChangeRaceRandomly(state.tokenUser);
        state.tokenUser.ConsumeToken();
        Log stateDescriptionLog = new Log(GameManager.Instance.Today(), "Tokens", this.GetType().ToString(), state.name.ToLower() + "-npc" + "_description");
        stateDescriptionLog.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        stateDescriptionLog.AddToFillers(null, Utilities.GetNormalizedSingularRace(state.tokenUser.race), LOG_IDENTIFIER.STRING_1);
        state.OverrideDescriptionLog(stateDescriptionLog);

        Log log = new Log(GameManager.Instance.Today(), "Tokens", GetType().ToString(), state.name.ToLower() + "_special2");
        log.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, Utilities.GetNormalizedSingularRace(state.tokenUser.race), LOG_IDENTIFIER.STRING_1);
        state.AddLogToInvolvedObjects(log);
    }
    private void ItemUsedEffectMinion(TokenInteractionState state) {
        ChangeRaceRandomly(state.tokenUser);
        state.tokenUser.ConsumeToken();
        Log stateDescriptionLog = new Log(GameManager.Instance.Today(), "Tokens", this.GetType().ToString(), state.name.ToLower() + "-minion" + "_description");
        stateDescriptionLog.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        stateDescriptionLog.AddToFillers(null, Utilities.GetNormalizedSingularRace(state.tokenUser.race), LOG_IDENTIFIER.STRING_1);
        state.OverrideDescriptionLog(stateDescriptionLog);

        Log log = new Log(GameManager.Instance.Today(), "Tokens", GetType().ToString(), state.name.ToLower() + "_special1");
        log.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, Utilities.GetNormalizedSingularRace(state.tokenUser.race), LOG_IDENTIFIER.STRING_1);
        state.AddLogToInvolvedObjects(log);
    }
    private void StopFailEffect(TokenInteractionState state) {
        ChangeRaceRandomly(state.tokenUser);
        state.tokenUser.ConsumeToken();

        state.descriptionLog.AddToFillers(state.interaction.investigatorCharacter, state.interaction.investigatorCharacter.name, LOG_IDENTIFIER.MINION_1);
        state.descriptionLog.AddToFillers(null, Utilities.GetNormalizedSingularRace(state.tokenUser.race), LOG_IDENTIFIER.STRING_1);

        state.AddLogFiller(new LogFiller(state.interaction.investigatorCharacter, state.interaction.investigatorCharacter.name, LOG_IDENTIFIER.MINION_1));
        state.AddLogFiller(new LogFiller(null, Utilities.GetNormalizedSingularRace(state.tokenUser.race), LOG_IDENTIFIER.STRING_1));
    }

    private void ChangeRaceRandomly(Character character) {
        //**Mechanics**: Change character's race randomly (beast to beast only, non-beast to non-beast only)
        List<RACE> choices;
        bool isBeast = Utilities.IsRaceBeast(character.race);
        if (isBeast) {
            choices = new List<RACE>(Utilities.beastRaces);
        } else {
            choices = new List<RACE>(Utilities.nonBeastRaces);
        }
        choices.Remove(character.race);
        RACE chosenRace = choices[Random.Range(0, choices.Count)];
        character.ChangeRace(chosenRace);
        if (isBeast) { //change class to the appropriate class
            WeightedDictionary<AreaCharacterClass> classWeights = LandmarkManager.Instance.GetDefaultClassWeights(chosenRace);
            character.ChangeClass(classWeights.PickRandomElementGivenWeights().className);
        }
    }
}
