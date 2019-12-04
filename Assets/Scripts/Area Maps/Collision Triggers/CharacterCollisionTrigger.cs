using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCollisionTrigger : BaseCollisionTrigger<Character>, IVisibleCollider {
    public IPointOfInterest poi { get; private set; }

    public override void Initialize(Character character) {
        base.Initialize(character);
        this.poi = character;
    }
    public void SetMainColliderState(bool state) {
        mainCollider.enabled = state;
    }
}
