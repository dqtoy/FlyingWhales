using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class Gregarious : Attribute {

    public Gregarious() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.GREGARIOUS) {

    }

    #region Overrides
    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
    }
    public override void OnRemoveAttribute() {
        base.OnRemoveAttribute();
    }
    #endregion
}
