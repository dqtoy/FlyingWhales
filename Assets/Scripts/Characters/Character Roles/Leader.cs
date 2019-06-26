using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leader : CharacterRole {
    public override int reservedSupply { get { return 50; } }

    public Leader() : base(CHARACTER_ROLE.LEADER, "Unique", new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.DIPLOMACY, INTERACTION_CATEGORY.DEFENSE }) {
        allowedInteractions = new INTERACTION_TYPE[] {
            INTERACTION_TYPE.GET_SUPPLY,
            INTERACTION_TYPE.ASSAULT_ACTION_NPC,
        };
        requiredItems = new SPECIAL_TOKEN[] {
            SPECIAL_TOKEN.HEALING_POTION,
            SPECIAL_TOKEN.HEALING_POTION
        };
    }
}
