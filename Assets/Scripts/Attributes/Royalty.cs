using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Royalty : Attribute {

    public Royalty() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.ROYALTY) {

    }

    #region Overrides
    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        _character.AddAttribute(ATTRIBUTE.SINGER);
    }
    #endregion
}
