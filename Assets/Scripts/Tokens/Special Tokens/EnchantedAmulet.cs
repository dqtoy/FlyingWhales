using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnchantedAmulet : SpecialToken {
    private STAT _statUsed;
    public EnchantedAmulet() : base(SPECIAL_TOKEN.ENCHANTED_AMULET, 0) {
        int chance = UnityEngine.Random.Range(0, 3);
        if(chance == 0) {
            _statUsed = STAT.ATTACK;
        }else if (chance == 1) {
            _statUsed = STAT.HP;
        }else if (chance == 2) {
            _statUsed = STAT.SPEED;
        }
    }

    #region Overrides
    public override void OnObtainToken(Character character) {
        if (_statUsed == STAT.ATTACK) {
            character.AdjustAttackPercentMod(25);
        } else if (_statUsed == STAT.HP) {
            character.AdjustMaxHPPercentMod(25);
        } else if (_statUsed == STAT.SPEED) {
            character.AdjustSpeedPercentMod(25);
        }
    }
    public override void OnUnobtainToken(Character character) {
        if (_statUsed == STAT.ATTACK) {
            character.AdjustAttackPercentMod(-25);
        } else if (_statUsed == STAT.HP) {
            character.AdjustMaxHPPercentMod(-25);
        } else if (_statUsed == STAT.SPEED) {
            character.AdjustSpeedPercentMod(-25);
        }
    }
    #endregion
}
