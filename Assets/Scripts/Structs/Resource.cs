using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Resource {
    public RESOURCE resource;
    public int amount;

    public Resource(RESOURCE resource, int amount) {
        this.resource = resource;
        this.amount = amount;
    }
}
