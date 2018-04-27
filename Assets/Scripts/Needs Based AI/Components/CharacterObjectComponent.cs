using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterObjectComponent : ObjectComponent {
    public CharacterObj characterObject;

    #region getters/setters
    public override string name {
        get { return characterObject.objectName; }
    }
    #endregion
}
