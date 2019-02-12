using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharmSpell : SpecialToken {

    public CharmSpell() : base(SPECIAL_TOKEN.CHARM_SPELL, 50) {
        //quantity = 4;
        npcAssociatedInteractionType = INTERACTION_TYPE.USE_ITEM_ON_CHARACTER;
        interactionAttributes = new InteractionAttributes() {
            categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT },
            alignment = INTERACTION_ALIGNMENT.EVIL,
            actorEffect = null,
            targetCharacterEffect = new InteractionCharacterEffect[] {
                new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.TRAIT_GAIN, effectString = "Charmed" },
                new InteractionCharacterEffect() { effect = INTERACTION_CHARACTER_EFFECT.CHANGE_FACTION, effectString = "Actor" }
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
        //interaction.SetCurrentState(itemUsed);
    }
    public override Character GetTargetCharacterFor(Character sourceCharacter) {
        if (!sourceCharacter.isFactionless) {
            Area location = sourceCharacter.ownParty.specificLocation;
            List<Character> choices = new List<Character>();
            for (int i = 0; i < location.charactersAtLocation.Count; i++) {
                Character currCharacter = location.charactersAtLocation[i];
                if (currCharacter.id != sourceCharacter.id
                    && (currCharacter.isFactionless || currCharacter.faction.id != sourceCharacter.faction.id)
                    && !currCharacter.isLeader) {
                    choices.Add(currCharacter);
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
            if (sourceCharacter.homeArea.IsResidentsFull()) {
                return false; //resident capacity is already full, do not use charm spell
            }
            Area location = sourceCharacter.ownParty.specificLocation;
            for (int i = 0; i < location.charactersAtLocation.Count; i++) {
                Character currCharacter = location.charactersAtLocation[i];
                if (currCharacter.id != sourceCharacter.id 
                    && (currCharacter.isFactionless || currCharacter.faction.id != sourceCharacter.faction.id) 
                    && !currCharacter.isLeader) {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    private void ItemUsedEffect(TokenInteractionState state) {
        Character targetCharacter = state.target as Character;
        //**Mechanics**: Add https://trello.com/c/4xVYdGAN/1004-charmed trait to the character.
        targetCharacter.AddTrait("Charmed");
        state.tokenUser.ConsumeToken();

        //Used item on other character
        Log stateDescriptionLog = new Log(GameManager.Instance.Today(), "Tokens", this.GetType().ToString(), state.name.ToLower() + "_description", state.interaction);
        stateDescriptionLog.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        stateDescriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        stateDescriptionLog.AddToFillers(state.tokenUser.faction, state.tokenUser.faction.name, LOG_IDENTIFIER.FACTION_1);
        state.OverrideDescriptionLog(stateDescriptionLog);

        state.AddLogFiller(new LogFiller(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.ACTIVE_CHARACTER));
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(state.tokenUser.faction, state.tokenUser.faction.name, LOG_IDENTIFIER.FACTION_1));

        FactionManager.Instance.TransferCharacter(targetCharacter, state.tokenUser.faction, state.tokenUser.homeArea);
        
    }
    private void StopFailEffect(TokenInteractionState state) {
        //state.tokenUser.LevelUp();
        state.tokenUser.ConsumeToken();

        //**Mechanics**: Target character will transfer to character or player's faction
        if (state.target is Character) {
            Character target = state.target as Character;
            if (target.GetTrait("Charmed") == null) {
                Charmed charmedTrait = new Charmed(target.faction, target.homeArea);
                target.AddTrait(charmedTrait);
            }
            FactionManager.Instance.TransferCharacter(target, state.tokenUser.faction, state.tokenUser.homeArea);
        }

        state.descriptionLog.AddToFillers(state.tokenUser.faction, state.tokenUser.faction.name, LOG_IDENTIFIER.FACTION_1);
        state.descriptionLog.AddToFillers(state.interaction.investigatorCharacter, state.interaction.investigatorCharacter.name, LOG_IDENTIFIER.MINION_1);
        state.descriptionLog.AddToFillers(null, this.name, LOG_IDENTIFIER.ITEM_1);

        state.AddLogFiller(new LogFiller(state.tokenUser.faction, state.tokenUser.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(state.interaction.investigatorCharacter, state.interaction.investigatorCharacter.name, LOG_IDENTIFIER.MINION_1));
        state.AddLogFiller(new LogFiller(null, this.name, LOG_IDENTIFIER.ITEM_1));
    }
}
