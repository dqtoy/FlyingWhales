using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SquadEmblem : MonoBehaviour {

    private Squad squad;

    [SerializeField] private Image frame, tint, outline, sigil;

    public void SetSquad(Squad squad) {
        this.squad = squad;
        frame.sprite = squad.emblemBG.frame;
        tint.sprite = squad.emblemBG.tint;
        outline.sprite = squad.emblemBG.outline;
        sigil.sprite = squad.emblem;
    }
}
