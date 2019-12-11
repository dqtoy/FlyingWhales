using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkBehaviour : CharacterBehaviourComponent {

    public override bool TryDoBehaviour(Character character, ref string log) {
        log += "\n-" + character.name + " is going to work or will do needs recovery...";
        if (!character.PlanJobQueueFirst()) {
            if (!character.PlanFullnessRecoveryActions()) {
                if (!character.PlanTirednessRecoveryActions()) {
                    if (!character.PlanHappinessRecoveryActions()) {
                        if (!character.PlanWorkActions()) {
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }
}
