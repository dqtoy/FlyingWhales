using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellScroll : SpecialObject {

    public SpellScroll() : base(SPECIAL_OBJECT_TYPE.SPELL_SCROLL) { }
    public SpellScroll(SaveDataSpecialObject data) : base(data) { }

    #region Overrides
    public override void Obtain() {
        base.Obtain();
        SPELL_TYPE[] spells = PlayerManager.Instance.allSpellTypes;
        SPELL_TYPE chosenSpell = spells[Random.Range(0, spells.Length)];
        if (PlayerManager.Instance.player.HasEmptyInterventionSlot()) {
            PlayerManager.Instance.player.GainNewInterventionAbility(chosenSpell);
            UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained Spell: " + UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(chosenSpell.ToString()), null);
        } else {
            UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained Spell: " + UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(chosenSpell.ToString()), () => PlayerManager.Instance.player.GainNewInterventionAbility(chosenSpell, true));
        }
        
        
    }
    #endregion
}

public class SaveDataSpellScroll : SaveDataSpecialObject {
    public override SpecialObject Load() {
        return new SpellScroll(this);
    }
}
