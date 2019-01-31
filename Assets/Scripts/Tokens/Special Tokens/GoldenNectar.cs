using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldenNectar : SpecialToken {
    public GoldenNectar() : base(SPECIAL_TOKEN.GOLDEN_NECTAR, 0) {
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
        return true;
    }
    #endregion

    private void ItemUsedEffectMinion(TokenInteractionState state) {
        STAT stat = GetRandomStat();
        Character targetCharacter = state.target as Character;
        if(stat == STAT.ATTACK) {
            targetCharacter.AdjustAttackMod(5);
        }else if (stat == STAT.HP) {
            targetCharacter.AdjustMaxHPMod(20);
        }else if (stat == STAT.ATTACK) {
            targetCharacter.AdjustSpeedMod(1);
        }
        state.tokenUser.ConsumeToken();

        Log stateDescriptionLog = new Log(GameManager.Instance.Today(), "Tokens", this.GetType().ToString(), state.name.ToLower() + "-minion" + "_description");
        stateDescriptionLog.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.MINION_1);
        stateDescriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        stateDescriptionLog.AddToFillers(null, GetStringEquivalentOfStat(stat), LOG_IDENTIFIER.STRING_1);
        state.OverrideDescriptionLog(stateDescriptionLog);

        Log log = new Log(GameManager.Instance.Today(), "Tokens", GetType().ToString(), state.name.ToLower() + "_special1");
        log.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.MINION_1);
        log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, GetStringEquivalentOfStat(stat), LOG_IDENTIFIER.STRING_1);
        state.AddLogToInvolvedObjects(log);
    }
    private void ItemUsedEffectNPC(TokenInteractionState state) {
        STAT stat = GetRandomStat();
        if (stat == STAT.ATTACK) {
            state.tokenUser.AdjustAttackMod(5);
        } else if (stat == STAT.HP) {
            state.tokenUser.AdjustMaxHPMod(20);
        } else if (stat == STAT.ATTACK) {
            state.tokenUser.AdjustSpeedMod(1);
        }
        state.tokenUser.ConsumeToken();

        Log stateDescriptionLog = new Log(GameManager.Instance.Today(), "Tokens", this.GetType().ToString(), state.name.ToLower() + "-npc" + "_description");
        stateDescriptionLog.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        stateDescriptionLog.AddToFillers(null, GetStringEquivalentOfStat(stat), LOG_IDENTIFIER.STRING_1);
        state.OverrideDescriptionLog(stateDescriptionLog);

        Log log = new Log(GameManager.Instance.Today(), "Tokens", GetType().ToString(), state.name.ToLower() + "_special2");
        log.AddToFillers(state.tokenUser, state.tokenUser.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, GetStringEquivalentOfStat(stat), LOG_IDENTIFIER.STRING_1);
        state.AddLogToInvolvedObjects(log);
    }
    private void StopFailEffect(TokenInteractionState state) {
        STAT stat = GetRandomStat();
        if (stat == STAT.ATTACK) {
            state.tokenUser.AdjustAttackMod(5);
        } else if (stat == STAT.HP) {
            state.tokenUser.AdjustMaxHPMod(20);
        } else if (stat == STAT.ATTACK) {
            state.tokenUser.AdjustSpeedMod(1);
        }
        state.tokenUser.ConsumeToken();

        state.descriptionLog.AddToFillers(state.interaction.investigatorCharacter, state.interaction.investigatorCharacter.name, LOG_IDENTIFIER.MINION_1);
        state.descriptionLog.AddToFillers(null, GetStringEquivalentOfStat(stat), LOG_IDENTIFIER.STRING_1);

        state.AddLogFiller(new LogFiller(state.interaction.investigatorCharacter, state.interaction.investigatorCharacter.name, LOG_IDENTIFIER.MINION_1));
        state.AddLogFiller(new LogFiller(null, GetStringEquivalentOfStat(stat), LOG_IDENTIFIER.STRING_1));
    }
    private string GetStringEquivalentOfStat(STAT stat) {
        if (stat == STAT.ATTACK) {
            return "Strength";
        } else if (stat == STAT.HP) {
            return "Vitality";
        } else if (stat == STAT.SPEED) {
            return "Speed";
        }
        return string.Empty;
    }
    private STAT GetRandomStat() {
        int chance = UnityEngine.Random.Range(0, 3);
        if(chance == 0) {
            return STAT.ATTACK;
        }else if (chance == 1) {
            return STAT.HP;
        }
        return STAT.SPEED;
    }
}
