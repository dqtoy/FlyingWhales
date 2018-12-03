using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class Heartbroken : CharacterAttribute {
    public Heartbroken() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.HEARTBROKEN) {
    }

    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        _character.role.AdjustConstantFunBuff(-50);
        GameDate expiryDate = GameManager.Instance.Today();
        expiryDate.AddMonths(15);
        SchedulingManager.Instance.AddEntry(expiryDate, () => _character.RemoveAttribute(this));
    }
    public override void OnRemoveAttribute() {
        base.OnRemoveAttribute();
        _character.role.AdjustConstantFunBuff(50);
    }
}
