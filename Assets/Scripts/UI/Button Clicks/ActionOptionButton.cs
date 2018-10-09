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

    public void SetOption(ActionOption actionOption) {
        _actionOption = actionOption;
        buttonText.text = _actionOption.description;
        costText.text = _actionOption.cost.amount.ToString();
        costImage.sprite = PlayerManager.Instance.GetSpriteByCurrency(_actionOption.cost.currency);
    }
}
