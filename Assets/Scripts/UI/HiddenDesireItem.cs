
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HiddenDesireItem : MonoBehaviour {

    //private HiddenDesire hiddenDesire;
    private Character owner;

    [SerializeField] private Image desireImage;

    //public void SetHiddenDesire(HiddenDesire hiddenDesire, Character owner) {
    //    this.hiddenDesire = hiddenDesire;
    //    this.owner = owner;
    //    UpdateVisuals();
    //}
    //private void UpdateVisuals() {
    //    if (hiddenDesire.isAwakened) {
    //        desireImage.color = Color.white;
    //    } else {
    //        desireImage.color = Color.gray;
    //    }
    //}

    //public void ShowHiddenDesireInfo() {
    //    if (hiddenDesire.isAwakened) {
    //        UIManager.Instance.ShowSmallInfo(hiddenDesire.description);
    //    } else {
    //        PlayerAbility pa = PlayerManager.Instance.player.GetAbility("Awaken Hidden Desire");
    //        UIManager.Instance.ShowSmallInfo("Awaken desire? Cost is: " + pa.GetMagicCostString(UIManager.Instance.characterInfoUI.currentlyShowingCharacter));
    //    }
        
    //}
    public void HideHiddenDesireInfo() {
        UIManager.Instance.HideSmallInfo();
    }

    //public void OnPointerClick(PointerEventData eventData) {
    //    if (hiddenDesire.isAwakened) {
    //        return; //desire is already awakened
    //    }
    //    if (!owner.isBeingInspected && !GameManager.Instance.inspectAll) {
    //        return;
    //    }
    //    AwakenDesire ad = PlayerManager.Instance.player.GetAbility("Awaken Hidden Desire") as AwakenDesire;
    //    if (!ad.CanBeActivated(owner)) {
    //        Messenger.Broadcast<string, bool>(Signals.SHOW_POPUP_MESSAGE, "You do not have enough magic!", true);
    //        return;
    //    }
    //    ad.Activate(owner, owner);
    //    UpdateVisuals();
    //}
}
