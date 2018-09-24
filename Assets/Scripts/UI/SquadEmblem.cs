using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIHoverHandler))]
public class SquadEmblem : MonoBehaviour {

    private Squad squad;

    [SerializeField] private Image frame, tint, outline, sigil;

    public void SetSquad(Squad squad) {
        this.squad = squad;
        if (squad == null) {
            this.gameObject.SetActive(false);
        } else {
            frame.sprite = squad.emblemBG.frame;
            tint.sprite = squad.emblemBG.tint;
            outline.sprite = squad.emblemBG.outline;
            sigil.sprite = squad.emblem;
            tint.color = squad.squadColor;
            this.gameObject.SetActive(true);
        }
    }

    public void ShowSquadInfo() {
        if (squad == null) {
            return;
        }
        UIManager.Instance.ShowSmallInfo(this.squad.name);
    }
    public void HideSquadInfo() {
        if (squad == null) {
            return;
        }
        UIManager.Instance.HideSmallInfo();
    }
}
