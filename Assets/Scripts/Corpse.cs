using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corpse {

    public Character character { get; private set; }

	public Corpse(Character character) {
        this.character = character;
    }
}
