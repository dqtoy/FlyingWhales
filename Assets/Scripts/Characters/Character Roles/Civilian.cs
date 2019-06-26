using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Civilian : CharacterRole {

    public override int reservedSupply { get { return 50; } }

    public Civilian() : base(CHARACTER_ROLE.CIVILIAN, "Civilian", new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUPPLY, INTERACTION_CATEGORY.INVENTORY }) {
        allowedInteractions = new INTERACTION_TYPE[] {
            INTERACTION_TYPE.MINE_ACTION,
            INTERACTION_TYPE.CHOP_WOOD,
            INTERACTION_TYPE.SCRAP,
            INTERACTION_TYPE.ASSAULT_ACTION_NPC,
        };
        requiredItems = new SPECIAL_TOKEN[] {
            SPECIAL_TOKEN.TOOL,
            SPECIAL_TOKEN.HEALING_POTION
        };
    }
}
