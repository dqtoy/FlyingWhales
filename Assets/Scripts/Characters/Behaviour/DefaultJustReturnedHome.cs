using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultJustReturnedHome : CharacterBehaviourComponent {
    public DefaultJustReturnedHome() {
        attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.INSIDE_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log) {
        if (character.previousCurrentActionNode != null && character.previousCurrentActionNode.action.goapType == INTERACTION_TYPE.RETURN_HOME && character.currentStructure == character.homeStructure) {
            log += "\n-" + character.name + " is in home structure and just returned home";
            TileObject deskOrTable = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
            log += "\n-Sit if there is still an unoccupied Table or Desk in the current location";
            if (deskOrTable != null) {
                log += "\n  -" + character.name + " will do action Sit on " + deskOrTable.ToString();
                character.PlanIdle(INTERACTION_TYPE.SIT, deskOrTable);
            } else {
                log += "\n-Otherwise, stand idle";
                log += "\n  -" + character.name + " will do action Stand";
                character.PlanIdle(INTERACTION_TYPE.STAND, character);
            }
            return true;
        }
        return false;
    }
}
