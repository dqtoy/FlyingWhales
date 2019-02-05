using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookOfTheDead : SpecialToken {

    public BookOfTheDead() : base(SPECIAL_TOKEN.BOOK_OF_THE_DEAD, 20) {
        //quantity = 1;
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
        return (sourceCharacter.race == RACE.HUMANS || sourceCharacter.race == RACE.GOBLIN || sourceCharacter.race == RACE.ELVES) 
            && sourceCharacter.gender == GENDER.MALE && sourceCharacter.specificLocation.name == "Gloomhollow Crypts" 
            && sourceCharacter.specificLocation.owner == null;
    }
    #endregion

    private void ItemUsedEffectMinion(TokenInteractionState state) {
        Character targetCharacter = state.target as Character;
        targetCharacter.ChangeClass("Necromancer");
        state.tokenUser.ConsumeToken();

        Log stateDescriptionLog = new Log(GameManager.Instance.Today(), "Tokens", this.GetType().ToString(), state.name.ToLower() + "-minion" + "_description", state.interaction);
        stateDescriptionLog.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.MINION_1);
        stateDescriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        state.OverrideDescriptionLog(stateDescriptionLog);

        Log log = new Log(GameManager.Instance.Today(), "Tokens", GetType().ToString(), state.name.ToLower() + "_special1");
        log.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.MINION_1);
        log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        state.AddLogToInvolvedObjects(log);
        //Character targetCharacter = state.target as Character;
        //targetCharacter.ChangeClass("Necromancer");
        //targetCharacter.ChangeRace(RACE.SKELETON);
        //targetCharacter.SetForcedInteraction(null);

        //Faction oldFaction = targetCharacter.faction;

        //targetCharacter.FoundFaction("Ziranna", targetCharacter.specificLocation.coreTile.landmarkOnTile);

        //targetCharacter.faction.GetRelationshipWith(oldFaction).SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.ENEMY);
        //targetCharacter.faction.GetRelationshipWith(PlayerManager.Instance.player.playerFaction).SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.ALLY);

        //Log stateDescriptionLog = new Log(GameManager.Instance.Today(), "Tokens", this.GetType().ToString(), state.name.ToLower() + "-minion" + "_description", state.interaction);
        //stateDescriptionLog.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.MINION_1);
        //stateDescriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //state.OverrideDescriptionLog(stateDescriptionLog);

        //Log log = new Log(GameManager.Instance.Today(), "Tokens", GetType().ToString(), state.name.ToLower() + "_special1");
        //log.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.MINION_1);
        //log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //state.AddLogToInvolvedObjects(log);
    }
    private void ItemUsedEffectNPC(TokenInteractionState state) {
        state.tokenUser.ChangeClass("Necromancer");
        state.tokenUser.ConsumeToken();

        Log stateDescriptionLog = new Log(GameManager.Instance.Today(), "Tokens", this.GetType().ToString(), state.name.ToLower() + "-npc" + "_description", state.interaction);
        stateDescriptionLog.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        state.OverrideDescriptionLog(stateDescriptionLog);

        Log log = new Log(GameManager.Instance.Today(), "Tokens", GetType().ToString(), state.name.ToLower() + "_special2");
        log.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        state.AddLogToInvolvedObjects(log);

        Debug.LogWarning("[Day " + GameManager.Instance.continuousDays + "] " + state.tokenUser.name + " used " + name + " on " + Utilities.GetPronounString(state.tokenUser.gender, PRONOUN_TYPE.REFLEXIVE, false) + " and became a " + state.tokenUser.characterClass.className + " at " + state.tokenUser.specificLocation.name);
        //state.tokenUser.ChangeClass("Necromancer");
        //state.tokenUser.ChangeRace(RACE.SKELETON);
        //state.tokenUser.SetForcedInteraction(null);
        //state.tokenUser.MigrateTo(state.tokenUser.specificLocation.coreTile.landmarkOnTile);

        //Faction oldFaction = state.tokenUser.faction;
        //state.tokenUser.FoundFaction("Ziranna", state.tokenUser.specificLocation.coreTile.landmarkOnTile);

        //state.tokenUser.faction.GetRelationshipWith(oldFaction).SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.DISLIKED);
        //state.tokenUser.faction.GetRelationshipWith(PlayerManager.Instance.player.playerFaction).SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.DISLIKED);

        //Log stateDescriptionLog = new Log(GameManager.Instance.Today(), "Tokens", this.GetType().ToString(), state.name.ToLower() + "-npc" + "_description", state.interaction);
        //stateDescriptionLog.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //state.OverrideDescriptionLog(stateDescriptionLog);

        //Log log = new Log(GameManager.Instance.Today(), "Tokens", GetType().ToString(), state.name.ToLower() + "_special2");
        //log.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //state.AddLogToInvolvedObjects(log);

        //Debug.LogWarning("[Day " + GameManager.Instance.continuousDays + "] " + state.tokenUser.name + " used " + name + " on " + Utilities.GetPronounString(state.tokenUser.gender, PRONOUN_TYPE.REFLEXIVE, false) + " and became a " + state.tokenUser.characterClass.className + " at " + state.tokenUser.specificLocation.name);
    }
    private void StopFailEffect(TokenInteractionState state) {
        state.tokenUser.ChangeClass("Necromancer");
        state.tokenUser.ConsumeToken();

        state.descriptionLog.AddToFillers(state.interaction.investigatorCharacter, state.interaction.investigatorCharacter.name, LOG_IDENTIFIER.MINION_1);

        state.AddLogFiller(new LogFiller(state.interaction.investigatorCharacter, state.interaction.investigatorCharacter.name, LOG_IDENTIFIER.MINION_1));

        Debug.LogWarning("[Day " + GameManager.Instance.continuousDays + "] " + state.tokenUser.name + " used " + name + " on " + Utilities.GetPronounString(state.tokenUser.gender, PRONOUN_TYPE.REFLEXIVE, false) + " and became a " + state.tokenUser.characterClass.className + " at " + state.tokenUser.specificLocation.name);
        //state.tokenUser.ChangeClass("Necromancer");
        //state.tokenUser.ChangeRace(RACE.SKELETON);
        //state.tokenUser.SetForcedInteraction(null);

        //Faction oldFaction = state.tokenUser.faction;
        //state.tokenUser.FoundFaction("Ziranna", state.tokenUser.specificLocation.coreTile.landmarkOnTile);

        //state.tokenUser.faction.GetRelationshipWith(oldFaction).SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.DISLIKED);
        //state.tokenUser.faction.GetRelationshipWith(PlayerManager.Instance.player.playerFaction).SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.DISLIKED);

        //state.descriptionLog.AddToFillers(state.interaction.investigatorMinion, state.interaction.investigatorMinion.name, LOG_IDENTIFIER.MINION_1);

        //state.AddLogFiller(new LogFiller(state.interaction.investigatorMinion, state.interaction.investigatorMinion.name, LOG_IDENTIFIER.MINION_1));

        //Debug.LogWarning("[Day " + GameManager.Instance.continuousDays + "] " + state.tokenUser.name + " used " + name + " on " + Utilities.GetPronounString(state.tokenUser.gender, PRONOUN_TYPE.REFLEXIVE, false) + " and became a " + state.tokenUser.characterClass.className + " at " + state.tokenUser.specificLocation.name);
    }
}
