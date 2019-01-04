using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecretScroll : SpecialToken {

    private string grantedClass;

    public SecretScroll() : base(SPECIAL_TOKEN.SECRET_SCROLL) {
        quantity = 6;
        weight = 100;
        npcAssociatedInteractionType = INTERACTION_TYPE.USE_ITEM_ON_LOCATION;
        grantedClass = "Knight";
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
    }
    public override bool CanBeUsedBy(Character sourceCharacter) {
        if (!sourceCharacter.isFactionless) {
            if (sourceCharacter.homeLandmark.tileLocation.areaOfTile.IsResidentsFull()) {
                return false; //resident capacity is already full, do not use charm spell
            }
            Area location = sourceCharacter.ownParty.specificLocation.tileLocation.areaOfTile;
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
        state.tokenUser.LevelUp();
        state.tokenUser.ConsumeToken();

        state.descriptionLog.AddToFillers(state.tokenUser.faction, state.tokenUser.faction.name, LOG_IDENTIFIER.FACTION_1);
        state.descriptionLog.AddToFillers(null, grantedClass, LOG_IDENTIFIER.STRING_1);

        state.AddLogFiller(new LogFiller(state.tokenUser.faction, state.tokenUser.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(null, grantedClass, LOG_IDENTIFIER.STRING_1));
    }
    private void StopFailEffect(TokenInteractionState state) {
        state.tokenUser.LevelUp();
        state.tokenUser.ConsumeToken();

        state.descriptionLog.AddToFillers(state.tokenUser.faction, state.tokenUser.faction.name, LOG_IDENTIFIER.FACTION_1);
        state.descriptionLog.AddToFillers(null, grantedClass, LOG_IDENTIFIER.STRING_1);

        state.AddLogFiller(new LogFiller(state.tokenUser.faction, state.tokenUser.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(null, grantedClass, LOG_IDENTIFIER.STRING_1));
    }
}
