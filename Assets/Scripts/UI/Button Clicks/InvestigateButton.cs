using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InvestigateButton : MonoBehaviour {
    public Toggle toggle;
    public string actionName;

    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite hoverOverSprite;
    [SerializeField] private Sprite hoverOutSprite;

    public void OnClickThis(bool state) {
        if (state) {
            UIManager.Instance.landmarkInfoUI.SetCurentSelectedInvestigateButton(this);
        } else {
            if (!toggle.group.AnyTogglesOn()) {
                UIManager.Instance.landmarkInfoUI.SetCurentSelectedInvestigateButton(null);
            }
        }
    }

    public void OnHoverOver() {
        if (toggle.interactable) {
            buttonImage.sprite = hoverOverSprite;
        }
    }

    public void OnHoverOut() {
        if (toggle.interactable) {
            buttonImage.sprite = hoverOutSprite;
        }
    }
}
