using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leader : CharacterRole {

    public Leader() : base(CHARACTER_ROLE.LEADER, "Unique", new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.DIPLOMACY, INTERACTION_CATEGORY.DEFENSE }) {
    }
}
