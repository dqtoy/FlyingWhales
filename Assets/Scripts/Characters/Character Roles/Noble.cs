using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noble : CharacterRole {

    public Noble() : base(CHARACTER_ROLE.NOBLE, "Noble", new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUBTERFUGE, INTERACTION_CATEGORY.DIPLOMACY, INTERACTION_CATEGORY.EXPANSION }) {
    }
}
