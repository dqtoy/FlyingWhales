using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionOptionButton : MonoBehaviour {
    private ActionOption _actionOption;

    public TextMeshProUGUI buttonText;
    public TextMeshProUGUI costText;
    public Image costImage;
    public Toggle toggle;
    public InteractionItem interactionItem;

    #region getters/setters
    public ActionOption actionOption {
        get { return _actionOption; }
    }
    #endregion
    public void Initialize() {
        Messenger.AddListener(Signals.UPDATED_CURRENCIES, UpdateButton);
    }
    private void OnDestroy() {
        Messenger.RemoveListener(Signals.UPDATED_CURRENCIES, UpdateButton);
    }
    private void UpdateButton() {
        if(_actionOption != null) {
            toggle.interactable = _actionOption.CanBeDone();
        }
    }
    public void SetOption(ActionOption actionOption) {
        _actionOption = actionOption;
        if(_actionOption != null) {
            buttonText.text = _actionOption.name;
            if (actionOption.cost.amount > 0) {
                costText.gameObject.SetActive(true);
                costImage.gameObject.SetActive(true);
                costText.text = _actionOption.cost.amount.ToString();
                costImage.sprite = PlayerManager.Instance.GetSpriteByCurrency(_actionOption.cost.currency);
            } else {
                costText.gameObject.SetActive(false);
                costImage.gameObject.SetActive(false);
                //costText.text = _actionOption.cost.amount.ToString();
                //costImage.sprite = PlayerManager.Instance.GetSpriteByCurrency(_actionOption.cost.currency);
            }
            
            UpdateButton();
        }
    }
    public void OnClickThis(bool state) {
        if (state) {
            interactionItem.SetCurrentSelectedActionOption(_actionOption);
        } else {
            if(!toggle.group.AnyTogglesOn()){
                interactionItem.assignmentGO.SetActive(false);
                interactionItem.ClearNeededObjectSlots ();
                interactionItem.SetCurrentSelectedActionOption(null);
                //if (interactionItem.confirmMinionGO.activeSelf) {
                //    interactionItem.confirmMinionGO.SetActive(false);
                //    interactionItem.portrait.GeneratePortrait(null, 95, true);
                //}
            }
        }
    }
}
