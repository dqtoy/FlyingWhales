using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Intel {
    public int id;
    public string name;
    public string description;

    public void SetData(IntelComponent intelComponent) {
        id = intelComponent.id;
        name = intelComponent.thisName;
        description = intelComponent.description;
    }
}