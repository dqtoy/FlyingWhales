using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;

public class CharacterSimButton : MonoBehaviour {
    public Text buttonText;
    [NonSerialized] public SIDES side;
    [NonSerialized] public ICharacterSim icharacterSim;

    public void SetAsCurrentlySelectedButton() {
        if(side == SIDES.A) {
            CombatSimManager.Instance.currentlySelectedSideAButton = this;
        } else {
            CombatSimManager.Instance.currentlySelectedSideBButton = this;
        }
    }
}
