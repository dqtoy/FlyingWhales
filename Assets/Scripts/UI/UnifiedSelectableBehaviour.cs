using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnifiedSelectableBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    private Selectable selectable;
    private TextMeshProUGUI text;

    private bool isHovering;

    private Toggle toggle {
        get {
            if (isToggle) {
                return selectable as Toggle;
            }
            return null;
        }
    }
    private bool isToggle {
        get { return selectable is Toggle; }
    }

    public void Initialize() {
        selectable = this.GetComponent<Selectable>();
        text = this.GetComponentInChildren<TextMeshProUGUI>();
        //selectable.transition = Selectable.Transition.SpriteSwap;
        //SpriteState ss = new SpriteState();
        //ss.highlightedSprite = UIManager.Instance.settings.hoverOverSprite;
        //ss.pressedSprite = UIManager.Instance.settings.hoverOverSprite;
        //ss.disabledSprite = UIManager.Instance.settings.hoverOutSprite;
        //selectable.spriteState = ss;
        if (isToggle) {
            toggle.onValueChanged.AddListener(OnValueChange);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        isHovering = true;
        if (isToggle) {
            if (toggle.isOn) {
                return;
            }
        }
        selectable.image.sprite = UIManager.Instance.settings.hoverOverSprite;
        text.color = UIManager.Instance.settings.hoverOverTextColor;
    }

    public void OnPointerExit(PointerEventData eventData) {
        isHovering = false;
        if (isToggle) {
            if (toggle.isOn) {
                return;
            }
        }
        selectable.image.sprite = UIManager.Instance.settings.hoverOutSprite;
        text.color = UIManager.Instance.settings.hoverOutTextColor;
    }

    #region Toggle Specific
    private void OnValueChange(bool isOn) {
        if (isHovering) {
            return;
        }
        if (isOn) {
            selectable.image.sprite = UIManager.Instance.settings.hoverOverSprite;
            text.color = UIManager.Instance.settings.toggleOnTextColor;
        } else {
            selectable.image.sprite = UIManager.Instance.settings.hoverOutSprite;
            text.color = UIManager.Instance.settings.toggleOffTextColor;
        }
    }
    #endregion

}
