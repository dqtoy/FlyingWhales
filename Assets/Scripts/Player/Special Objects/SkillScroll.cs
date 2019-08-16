using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillScroll : SpecialObject {

    public SkillScroll() : base(SPECIAL_OBJECT_TYPE.SKILL_SCROLL) { }

    #region Overrides
    public override void Obtain() {
        base.Obtain();
        COMBAT_ABILITY[] skills = Utilities.GetEnumValues<COMBAT_ABILITY>();
        PlayerManager.Instance.CreateNewCombatAbility(skills[Random.Range(1, skills.Length)]);
    }
    #endregion
}
