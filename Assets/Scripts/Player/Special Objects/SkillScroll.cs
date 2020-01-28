using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class SkillScroll : SpecialObject {

    public SkillScroll() : base(SPECIAL_OBJECT_TYPE.SKILL_SCROLL) { }
    public SkillScroll(SaveDataSpecialObject data) : base(data) { }

    #region Overrides
    public override void Obtain() {
        base.Obtain();
        COMBAT_ABILITY[] skills = CollectionUtilities.GetEnumValues<COMBAT_ABILITY>();
        CombatAbility newAbility = PlayerManager.Instance.CreateNewCombatAbility(skills[Random.Range(1, skills.Length)]);
        UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained Combat Ability: " + newAbility.name, () => PlayerUI.Instance.newMinionAbilityUI.ShowNewMinionAbilityUI(newAbility));
    }
    #endregion
}

public class SaveDataSkillScroll : SaveDataSpecialObject {
    public override SpecialObject Load() {
        return new SkillScroll(this);
    }
}
