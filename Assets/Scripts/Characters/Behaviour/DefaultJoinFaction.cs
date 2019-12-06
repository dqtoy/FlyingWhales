using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultJoinFaction : CharacterBehaviourComponent {
    public DefaultJoinFaction() {
        attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.DO_NOT_SKIP_PROCESSING, BEHAVIOUR_COMPONENT_ATTRIBUTE.ONCE_PER_DAY };
    }

    public override bool TryDoBehaviour(Character character, ref string log) {
        if (character.currentRegion != null && character.currentRegion.owner != null && character.isFactionless) {
            log += "\n-" + character.name + " is factionless and in a non settlement region: " + character.currentRegion.name + " that has faction owner: " + character.currentRegion.owner.name;
            log += "\n-15% chance to join faction";
            character.currentRegion.owner.JoinFaction(character);
            return true;
        }
        return false;
    }
}
