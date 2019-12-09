using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerserkBehaviour : CharacterBehaviourComponent {

    public override bool TryDoBehaviour(Character character, ref string log) {
        log += "\n-" + character.name + " is berserked, will only stroll";
        character.PlanIdleStrollOutside();
        return true;
    }
}
