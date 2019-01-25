using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlightedPotion : SpecialToken {

    public BlightedPotion() : base(SPECIAL_TOKEN.BLIGHTED_POTION) {
        //quantity = 4;
        weight = 100;
        npcAssociatedInteractionType = INTERACTION_TYPE.USE_ITEM_ON_CHARACTER;
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

        //interaction.SetCurrentState(inflictIllnessState);
    }
    public override Character GetTargetCharacterFor(Character sourceCharacter) {
        //NPC Usage Requirement: Character must be part of a Disliked or Enemy Faction
        if (!sourceCharacter.isFactionless) {
            Area location = sourceCharacter.ownParty.specificLocation;
            List<Character> choices = new List<Character>();
            for (int i = 0; i < location.charactersAtLocation.Count; i++) {
                Character currCharacter = location.charactersAtLocation[i];
                if (currCharacter.id != sourceCharacter.id && !currCharacter.isFactionless && currCharacter.faction.id != sourceCharacter.faction.id) {
                    FactionRelationship rel = sourceCharacter.faction.GetRelationshipWith(currCharacter.faction);
                    if (rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.DISLIKED || rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY) {
                        choices.Add(currCharacter);
                    }
                }
            }
            if (choices.Count > 0) {
                return choices[Random.Range(0, choices.Count)];
            }
        }
        return base.GetTargetCharacterFor(sourceCharacter);
    }
    public override bool CanBeUsedBy(Character sourceCharacter) {
        if (!sourceCharacter.isFactionless) {
            Area location = sourceCharacter.ownParty.specificLocation;
            List<Character> choices = new List<Character>();
            for (int i = 0; i < location.charactersAtLocation.Count; i++) {
                Character currCharacter = location.charactersAtLocation[i];
                if (currCharacter.id != sourceCharacter.id && !currCharacter.isFactionless && currCharacter.faction.id != sourceCharacter.faction.id) {
                    FactionRelationship rel = sourceCharacter.faction.GetRelationshipWith(currCharacter.faction);
                    if (rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.DISLIKED || rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    #endregion

    private void ItemUsedEffect(TokenInteractionState state) {
        string chosenIllnessName = AttributeManager.Instance.GetRandomIllness();
        (state.target as Character).AddTrait(AttributeManager.Instance.allIllnesses[chosenIllnessName]);
        state.tokenUser.LevelUp();
        state.tokenUser.ConsumeToken();

        state.descriptionLog.AddToFillers(null, chosenIllnessName, LOG_IDENTIFIER.STRING_1);
        state.AddLogFiller(new LogFiller(null, chosenIllnessName, LOG_IDENTIFIER.STRING_1));
    }
    private void StopFailEffect(TokenInteractionState state) {
        string chosenIllnessName = AttributeManager.Instance.GetRandomIllness();
        (state.target as Character).AddTrait(AttributeManager.Instance.allIllnesses[chosenIllnessName]);
        state.tokenUser.LevelUp();
        state.tokenUser.ConsumeToken();

        state.descriptionLog.AddToFillers(state.interaction.investigatorCharacter, state.interaction.investigatorCharacter.name, LOG_IDENTIFIER.MINION_1);
        state.descriptionLog.AddToFillers(null, chosenIllnessName, LOG_IDENTIFIER.STRING_1);
        state.descriptionLog.AddToFillers(null, this.name, LOG_IDENTIFIER.ITEM_1);

        state.AddLogFiller(new LogFiller(state.interaction.investigatorCharacter, state.interaction.investigatorCharacter.name, LOG_IDENTIFIER.MINION_1));
        state.AddLogFiller(new LogFiller(null, chosenIllnessName, LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(null, this.name, LOG_IDENTIFIER.ITEM_1));
    }
}
