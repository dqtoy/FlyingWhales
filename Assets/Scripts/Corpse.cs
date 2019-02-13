using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corpse {

    public Character character { get; private set; }
    public LocationStructure location { get; private set; }

	public Corpse(Character character, LocationStructure structure) {
        this.character = character;
        location = structure;
    }
}
