using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : CharacterRole {

    public Soldier() : base(CHARACTER_ROLE.SOLDIER, "Normal", new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUBTERFUGE, INTERACTION_CATEGORY.OFFENSE, INTERACTION_CATEGORY.DEFENSE }) {
    }
}
