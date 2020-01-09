using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultExtraCatcher : CharacterBehaviourComponent {
    public DefaultExtraCatcher() {
        attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.OUTSIDE_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log) {
        if (!character.isAtHomeRegion && character.trapStructure.structure == null) {
            log += "\n-" + character.name + " is in another area and Base Structure is empty";
            log += "\n-100% chance to return home";
            character.PlanIdleReturnHome();
            return true;
        }
        return false;
    }
}
