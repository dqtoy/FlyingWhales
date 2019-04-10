using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noble : CharacterRole {
    public override int reservedSupply { get { return 30; } }

    public Noble() : base(CHARACTER_ROLE.NOBLE, "Noble", new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUBTERFUGE, INTERACTION_CATEGORY.DIPLOMACY, INTERACTION_CATEGORY.EXPANSION }) {
        allowedInteractions = new INTERACTION_TYPE[] {
            INTERACTION_TYPE.GET_SUPPLY,
        };
    }
}
