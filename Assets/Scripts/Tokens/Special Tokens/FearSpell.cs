using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FearSpell : SpecialToken {

    public FearSpell() : base(SPECIAL_TOKEN.FEAR_SPELL) {
        quantity = 6;
        weight = 100;
    }

    #region Overrides
    public override void CreateJointInteractionStates(Interaction interaction, Character user, object target) {
        TokenInteractionState itemUsed = new TokenInteractionState(Item_Used, interaction, this);
        itemUsed.SetTokenUserAndTarget(user, target);

        itemUsed.SetEffect(() => ItemUsedEffect(itemUsed));

        interaction.AddState(itemUsed);
        //interaction.SetCurrentState(itemUsed);
    }
    public override Character GetTargetCharacterFor(Character sourceCharacter) {
        //NPC Usage Requirement: Character must be part of a Disliked or Enemy Faction
        if (!sourceCharacter.isFactionless) {
            Area location = sourceCharacter.ownParty.specificLocation.tileLocation.areaOfTile;
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
            Area location = sourceCharacter.ownParty.specificLocation.tileLocation.areaOfTile;
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
        state.tokenUser.LevelUp();
        //**Mechanics**: Target character will trigger https://trello.com/c/vDKl0cyy/859-character-flees on the next tick (overriding any other action).
        if (state.target is Character) {
            Character target = state.target as Character;
            Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.CHARACTER_FLEES, target.specificLocation.tileLocation.landmarkOnTile);
            target.SetForcedInteraction(interaction);
        }
        //state.descriptionLog.AddToFillers(null, chosenIllnessName, LOG_IDENTIFIER.STRING_1);
        //state.AddLogFiller(new LogFiller(null, chosenIllnessName, LOG_IDENTIFIER.STRING_1));
    }
}
