using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharmSpell : SpecialToken {

    public CharmSpell() : base(SPECIAL_TOKEN.CHARM_SPELL) {
        quantity = 4;
    }

    #region Overrides
    public override void CreateJointInteractionStates(Interaction interaction, Character user, object target) {
        TokenInteractionState itemUsed = new TokenInteractionState("Item Used", interaction, this);
        itemUsed.SetTokenUserAndTarget(user, target);

        itemUsed.SetEffect(() => ItemUsedEffect(itemUsed));

        interaction.AddState(itemUsed);
        interaction.SetCurrentState(itemUsed);
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
        string chosenIllnessName = AttributeManager.Instance.GetRandomIllness();
        (state.target as Character).AddTrait(AttributeManager.Instance.allIllnesses[chosenIllnessName]);
        state.tokenUser.LevelUp();

        state.descriptionLog.AddToFillers(null, chosenIllnessName, LOG_IDENTIFIER.STRING_1);
        state.AddLogFiller(new LogFiller(null, chosenIllnessName, LOG_IDENTIFIER.STRING_1));
    }
}
