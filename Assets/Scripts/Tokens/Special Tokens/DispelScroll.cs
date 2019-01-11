using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DispelScroll : SpecialToken {

    public DispelScroll() : base(SPECIAL_TOKEN.DISPEL_SCROLL) {
        weight = 30;
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
    public override bool CanBeUsedBy(Character sourceCharacter) {
        return GetTargetCharacterFor(sourceCharacter) != null; //check if there is a target character
    }
    public override Character GetTargetCharacterFor(Character sourceCharacter) {
        if (!sourceCharacter.isFactionless) {
            Faction characterFaction = sourceCharacter.faction;
            Area currentLocation = sourceCharacter.specificLocation.tileLocation.areaOfTile;
            List<Character> choices = new List<Character>();
            for (int i = 0; i < currentLocation.charactersAtLocation.Count; i++) {
                Character currCharacter = currentLocation.charactersAtLocation[i];
                if (currCharacter.id == sourceCharacter.id || currCharacter.GetTrait("Charmed") == null) {
                    continue; //skip (Character must have charmed trait)
                }
                if (currCharacter.GetFriendTraitWith(sourceCharacter) != null) {
                    choices.Add(currCharacter);
                } else if (!currCharacter.isFactionless && currCharacter.faction.id != characterFaction.id) {
                    Faction otherFaction = currCharacter.faction;
                    switch (otherFaction.GetRelationshipWith(characterFaction).relationshipStatus) {
                        case FACTION_RELATIONSHIP_STATUS.ENEMY:
                        case FACTION_RELATIONSHIP_STATUS.DISLIKED:
                        case FACTION_RELATIONSHIP_STATUS.NEUTRAL:
                            choices.Add(currCharacter);
                            break;
                    }
                }
            }
            if (choices.Count > 0) {
                return choices[Random.Range(0, choices.Count)];
            }
        }
        return base.GetTargetCharacterFor(sourceCharacter);
    }
    #endregion

    private void ItemUsedEffect(TokenInteractionState state) {
        state.tokenUser.LevelUp(); // **Level Up**: User +1 
        state.tokenUser.ConsumeToken();
        Character targetCharacter = state.target as Character;
        if (targetCharacter == null) {
            throw new System.Exception(GameManager.Instance.TodayLogString() + "Target character of use dispel scroll by " + state.tokenUser.name + " is either null or not a character");
        }
        //**Mechanics**: Remove Charm from target character
        RemoveCharmFromTarget(targetCharacter);
        //**Mechanics**: User and Character personal relationship +1
        CharacterManager.Instance.ChangePersonalRelationshipBetweenTwoCharacters(targetCharacter, state.tokenUser, 1);
    }
    private void StopFailEffect(TokenInteractionState state) {
        state.tokenUser.LevelUp(); // **Level Up**: User +1 
        state.tokenUser.ConsumeToken();
        Character targetCharacter = state.target as Character;
        if (targetCharacter == null) {
            throw new System.Exception(GameManager.Instance.TodayLogString() + "Target character of use dispel scroll by " + state.tokenUser.name + " is either null or not a character");
        }
        //**Mechanics**: Remove Charm from target character
        RemoveCharmFromTarget(targetCharacter);
        //**Mechanics**: User and Character personal relationship +1
        CharacterManager.Instance.ChangePersonalRelationshipBetweenTwoCharacters(targetCharacter, state.tokenUser, 1);

        state.descriptionLog.AddToFillers(state.interaction.investigatorMinion, state.interaction.investigatorMinion.name, LOG_IDENTIFIER.MINION_1);
        state.AddLogFiller(new LogFiller(state.interaction.investigatorMinion, state.interaction.investigatorMinion.name, LOG_IDENTIFIER.MINION_1));
    }

    private void RemoveCharmFromTarget(Character target) {
        target.RemoveTrait(target.GetTrait("Charmed"));
    }
}
