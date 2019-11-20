using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beast : CharacterRole {

    public Beast() : base(CHARACTER_ROLE.BEAST, "Beast", new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.OFFENSE, INTERACTION_CATEGORY.DEFENSE, INTERACTION_CATEGORY.INVENTORY, INTERACTION_CATEGORY.RECRUITMENT }) {
        //allowedInteractions = new INTERACTION_TYPE[] {
        //    INTERACTION_TYPE.ASSAULT,
        //};
    }
}
