using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellScroll : SpecialObject {

    public SpellScroll() : base(SPECIAL_OBJECT_TYPE.SPELL_SCROLL) { }
    public SpellScroll(SaveDataSpecialObject data) : base(data) { }

    #region Overrides
    public override void Obtain() {
        base.Obtain();
        INTERVENTION_ABILITY[] spells = PlayerManager.Instance.allInterventionAbilities;
        INTERVENTION_ABILITY chosenSpell = spells[Random.Range(0, spells.Length)];
        
        UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained Intervention Ability: " + Utilities.NormalizeStringUpperCaseFirstLetters(chosenSpell.ToString()), () => PlayerManager.Instance.player.GainNewInterventionAbility(chosenSpell, true));
    }
    #endregion
}

public class SaveDataSpellScroll : SaveDataSpecialObject {
    public override SpecialObject Load() {
        return new SpellScroll(this);
    }
}
