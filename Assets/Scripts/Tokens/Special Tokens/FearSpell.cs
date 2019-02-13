using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FearSpell : SpecialToken {

    public FearSpell() : base(SPECIAL_TOKEN.FEAR_SPELL, 100) {
        //quantity = 6;
        npcAssociatedInteractionType = INTERACTION_TYPE.USE_ITEM_ON_CHARACTER;
        interactionAttributes = new InteractionAttributes() {
            categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUBTERFUGE },
            alignment = INTERACTION_ALIGNMENT.NEUTRAL,
            actorEffect = null,
            targetCharacterEffect = null,
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
        //interaction.SetCurrentState(itemUsed);
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
        Character targetCharacter = state.target as Character;
        state.tokenUser.ConsumeToken();
        //**Mechanics**: Target character will trigger https://trello.com/c/vDKl0cyy/859-character-flees on the next tick (overriding any other action).
        if (state.target is Character) {
            Character target = state.target as Character;
            Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.CHARACTER_FLEES, target.specificLocation);
            target.SetForcedInteraction(interaction);
        }


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
        //state.tokenUser.LevelUp();
        state.tokenUser.ConsumeToken();

        //**Mechanics**: Target character will trigger https://trello.com/c/vDKl0cyy/859-character-flees on the next tick (overriding any other action).
        if (state.target is Character) {
            Character target = state.target as Character;
            Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.CHARACTER_FLEES, target.specificLocation);
            target.SetForcedInteraction(interaction);
        }

        state.descriptionLog.AddToFillers(state.interaction.investigatorCharacter, state.interaction.investigatorCharacter.name, LOG_IDENTIFIER.MINION_1);
        state.descriptionLog.AddToFillers(null, this.name, LOG_IDENTIFIER.ITEM_1);

        state.AddLogFiller(new LogFiller(state.interaction.investigatorCharacter, state.interaction.investigatorCharacter.name, LOG_IDENTIFIER.MINION_1));
        state.AddLogFiller(new LogFiller(null, this.name, LOG_IDENTIFIER.ITEM_1));
    }
}
