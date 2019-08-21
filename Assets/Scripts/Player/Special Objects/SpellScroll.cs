using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellScroll : SpecialObject {

    public SpellScroll() : base(SPECIAL_OBJECT_TYPE.SPELL_SCROLL) { }

    #region Overrides
    public override void Obtain() {
        base.Obtain();
        INTERVENTION_ABILITY[] spells = PlayerManager.Instance.allInterventionAbilities;
        PlayerManager.Instance.player.GainNewInterventionAbility(spells[Random.Range(0, spells.Length)], true);
    }
    #endregion
}
