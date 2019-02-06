using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecretScroll : SpecialToken {

    public string grantedClass { get; private set; }

    private string[] classes = new string[] { "Knight", "Marauder", "Barbarian", "Stalker", "Archer", "Hunter", "Druid", "Mage", "Shaman" };

    public SecretScroll() : base(SPECIAL_TOKEN.SECRET_SCROLL, 100) {
        //quantity = 6;
        npcAssociatedInteractionType = INTERACTION_TYPE.USE_ITEM_ON_LOCATION;
        //grantedClass = "Knight";
        grantedClass = classes[Random.Range(0, classes.Length)];
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
            Area areaLocation = sourceCharacter.specificLocation;
            if (areaLocation != null
                && areaLocation.owner != null
                && areaLocation.owner.id == sourceCharacter.faction.id
                && !areaLocation.HasClassInWeights(grantedClass)) {
                return true;
            }
        }
        return false;
    }
    #endregion

    private void ItemUsedEffect(TokenInteractionState state) {
        //state.tokenUser.LevelUp();
        state.tokenUser.ConsumeToken();

        //**Mechanics**: Faction will now be able to train the scroll's class with a weight of 20.
        state.tokenUser.faction.AddClassWeight(grantedClass, 20);

        state.descriptionLog.AddToFillers(state.tokenUser.faction, state.tokenUser.faction.name, LOG_IDENTIFIER.FACTION_1);
        state.descriptionLog.AddToFillers(null, grantedClass, LOG_IDENTIFIER.STRING_1);

        state.AddLogFiller(new LogFiller(state.tokenUser.faction, state.tokenUser.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(null, grantedClass, LOG_IDENTIFIER.STRING_1));
    }
    private void StopFailEffect(TokenInteractionState state) {
        //state.tokenUser.LevelUp();
        state.tokenUser.ConsumeToken();

        //**Mechanics**: Faction will now be able to train the scroll's class with a weight of 20.
        state.tokenUser.faction.AddClassWeight(grantedClass, 20);

        state.descriptionLog.AddToFillers(state.tokenUser.faction, state.tokenUser.faction.name, LOG_IDENTIFIER.FACTION_1);
        state.descriptionLog.AddToFillers(null, grantedClass, LOG_IDENTIFIER.STRING_1);

        state.AddLogFiller(new LogFiller(state.tokenUser.faction, state.tokenUser.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(null, grantedClass, LOG_IDENTIFIER.STRING_1));
    }
}
