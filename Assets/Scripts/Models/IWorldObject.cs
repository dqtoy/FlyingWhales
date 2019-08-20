using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWorldObject {
	string worldObjectName { get; }

    void Obtain(); //called when the player invades the region that this world object is at.
}
