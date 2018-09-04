using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class Heartbroken : CharacterTag {
    public Heartbroken(Character character) : base(character, CHARACTER_TAG.HEARTBROKEN) {
    }

    public override void Initialize() {
        base.Initialize();
        //_character.role.AdjustConstantSanityBuff(-25);
        _character.role.AdjustConstantFunBuff(-50);
        GameDate expiryDate = GameManager.Instance.Today();
        expiryDate.AddDays(15);
        SchedulingManager.Instance.AddEntry(expiryDate, () => _character.RemoveCharacterAttribute(this));
    }

    public override void OnRemoveTag() {
        base.OnRemoveTag();
        //_character.role.AdjustConstantSanityBuff(25);
        _character.role.AdjustConstantFunBuff(50);
    }
}
