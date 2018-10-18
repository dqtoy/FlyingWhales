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
    public Button button;
    public InteractionItem interactionItem;

    public void Initialize() {
        Messenger.AddListener(Signals.UPDATED_CURRENCIES, UpdateButton);
    }
    private void OnDestroy() {
        Messenger.RemoveListener(Signals.UPDATED_CURRENCIES, UpdateButton);
    }
    private void UpdateButton() {
        if(_actionOption != null) {
            button.interactable = _actionOption.CanBeDone();
        }
    }
    public void SetOption(ActionOption actionOption) {
        _actionOption = actionOption;
        buttonText.text = _actionOption.description;
        costText.text = _actionOption.cost.amount.ToString();
        costImage.sprite = PlayerManager.Instance.GetSpriteByCurrency(_actionOption.cost.currency);
        UpdateButton();
    }
    public void OnClickThis() {
        interactionItem.SetCurrentSelectedActionOption(_actionOption);
    }
}
