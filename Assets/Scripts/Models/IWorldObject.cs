using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWorldObject {
    int id { get; }
	string worldObjectName { get; }
    WORLD_OBJECT_TYPE worldObjectType { get; }

    void Obtain(); //called when the player invades the region that this world object is at.
}
