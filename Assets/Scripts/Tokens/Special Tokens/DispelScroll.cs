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
        Faction characterFaction = sourceCharacter.faction;
        Area currentLocation = sourceCharacter.specificLocation.tileLocation.areaOfTile;
        List<Character> choices = new List<Character>();
        for (int i = 0; i < currentLocation.charactersAtLocation.Count; i++) {
            Character currCharacter = currentLocation.charactersAtLocation[i];
            /*
             NPC Usage Requirement 1: Character has at least one Negative Enchantment trait and no Positive Enchantment trait. 
             Character is part of the same Faction or a Friend or Allied factions and character is not a personal Enemy. Or character is a personal Friend.
             */
            if (currCharacter.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.ENCHANTMENT) 
                && !currCharacter.HasTraitOf(TRAIT_EFFECT.POSITIVE, TRAIT_TYPE.ENCHANTMENT)) { //Character has at least one Negative Enchantment trait and no Positive Enchantment trait. 
                if (sourceCharacter.GetFriendTraitWith(currCharacter) != null) {
                    choices.Add(currCharacter); //Character is a personal Friend.
                } else if(currCharacter.faction.id == characterFaction.id) {
                    choices.Add(currCharacter); //Character is part of the same Faction
                } else {
                    Faction otherFaction = currCharacter.faction;
                    switch (otherFaction.GetRelationshipWith(characterFaction).relationshipStatus) {
                        case FACTION_RELATIONSHIP_STATUS.FRIEND:
                        case FACTION_RELATIONSHIP_STATUS.ALLY:
                            if (sourceCharacter.GetEnemyTraitWith(currCharacter) == null) {
                                choices.Add(currCharacter); //Character is part of a Friend or Allied faction and character is not a personal Enemy
                            }
                            break;
                    }
                }
            }
            /*
             NPC Usage Requirement 2: Character has at least one Positive Enchantment trait and no Negative Enchantment traits. 
             Character is part of a Disliked, Enemy or War factions and character is not a personal Friend. Or character is a personal Enemy.
             */
            else if (currCharacter.HasTraitOf(TRAIT_EFFECT.POSITIVE, TRAIT_TYPE.ENCHANTMENT)
                && !currCharacter.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.ENCHANTMENT)) {
                if (sourceCharacter.GetEnemyTraitWith(currCharacter) != null) {
                    choices.Add(currCharacter); //Character is a personal Enemy.
                } else if (currCharacter.faction.id != characterFaction.id) {
                    Faction otherFaction = currCharacter.faction;
                    switch (otherFaction.GetRelationshipWith(characterFaction).relationshipStatus) {
                        case FACTION_RELATIONSHIP_STATUS.DISLIKED:
                        case FACTION_RELATIONSHIP_STATUS.ENEMY:
                        case FACTION_RELATIONSHIP_STATUS.AT_WAR:
                            if (sourceCharacter.GetFriendTraitWith(currCharacter) == null) {
                                choices.Add(currCharacter); //Character is part of a Disliked, Enemy or War factions and character is not a personal Friend
                            }
                            break;
                    }
                }
            }
        }

        if (choices.Count > 0) {
            return choices[Random.Range(0, choices.Count)];
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
        //**Mechanics**: Remove all Enchantment type traits on the target
        RemoveAllEnchantments(state.tokenUser, targetCharacter);
    }
    private void StopFailEffect(TokenInteractionState state) {
        state.tokenUser.LevelUp(); // **Level Up**: User +1 
        state.tokenUser.ConsumeToken();
        Character targetCharacter = state.target as Character;
        if (targetCharacter == null) {
            throw new System.Exception(GameManager.Instance.TodayLogString() + "Target character of use dispel scroll by " + state.tokenUser.name + " is either null or not a character");
        }
        //**Mechanics**: Remove all Enchantment type traits on the target
        RemoveAllEnchantments(state.tokenUser, targetCharacter);

        state.descriptionLog.AddToFillers(state.interaction.investigatorCharacter, state.interaction.investigatorCharacter.name, LOG_IDENTIFIER.MINION_1);
        state.AddLogFiller(new LogFiller(state.interaction.investigatorCharacter, state.interaction.investigatorCharacter.name, LOG_IDENTIFIER.MINION_1));
    }

    //private void RemoveCharmFromTarget(Character target) {
    //    target.RemoveTrait(target.GetTrait("Charmed"));
    //}

    private void RemoveAllEnchantments(Character user, Character target) {
        List<Trait> removedTraits = target.RemoveAllTraitsByType(TRAIT_TYPE.ENCHANTMENT);
        bool removedPositiveTrait = false;
        bool removedNegativeTrait = false;
        for (int i = 0; i < removedTraits.Count; i++) {
            Trait currTrait = removedTraits[i];
            if (currTrait.effect == TRAIT_EFFECT.POSITIVE) {
                removedPositiveTrait = true;
            } else if (currTrait.effect == TRAIT_EFFECT.NEGATIVE) {
                removedNegativeTrait = true;
            }
        }

        if (removedNegativeTrait && removedPositiveTrait) {
            Debug.LogWarning("Both negative and positive traits were removed from " + target.name + " by " + user.name + ". HELP!");
        } else if (removedNegativeTrait) {
            CharacterManager.Instance.ChangePersonalRelationshipBetweenTwoCharacters(user, target, 1);
        } else if (removedPositiveTrait) {
            CharacterManager.Instance.ChangePersonalRelationshipBetweenTwoCharacters(user, target, -1);
        } else {
            Debug.LogWarning("No negative or positive traits were removed from " + target.name + " by " + user.name + ". HELP!");
        }
    }
}
