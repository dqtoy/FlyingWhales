using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellScroll : SpecialObject {

    public SpellScroll() : base(SPECIAL_OBJECT_TYPE.SPELL_SCROLL) { }

    #region Overrides
    public override void Obtain() {
        base.Obtain();
        INTERVENTION_ABILITY[] spells = Utilities.GetEnumValues<INTERVENTION_ABILITY>();
        PlayerManager.Instance.player.GainNewInterventionAbility(spells[Random.Range(1, spells.Length)]);
    }
    #endregion
}
