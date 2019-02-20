using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventPopup : MonoBehaviour {

    private Interaction interaction;

    public void SetEvent(Interaction interaction) {
        this.interaction = interaction;
    }
}
