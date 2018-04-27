using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCObjectComponent : ObjectComponent {
    public NPCObj npcObject;

    #region getters/setters
    public override string name {
        get { return npcObject.objectName; }
    }
    #endregion
}
