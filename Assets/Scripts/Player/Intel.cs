using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Intel : IPlayerPicker {
    public int id;
    public string name;
    public string description;

    #region getters/setters
    public string thisName {
        get { return name; }
    }
    #endregion
    public void SetData(IntelComponent intelComponent) {
        id = intelComponent.id;
        name = intelComponent.thisName;
        description = intelComponent.description;
    }
}