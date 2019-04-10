using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leader : CharacterRole {
    public override int reservedSupply { get { return 50; } }

    public Leader() : base(CHARACTER_ROLE.LEADER, "Unique", new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.DIPLOMACY, INTERACTION_CATEGORY.DEFENSE }) {
        allowedInteractions = new INTERACTION_TYPE[] {
            INTERACTION_TYPE.GET_SUPPLY,
        };
    }
}
