using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(HoverHandler))]
public class ButtonHighlight : MonoBehaviour {

    public Selectable selectable;

    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite hoverOverSprite;
    [SerializeField] private Sprite hoverOutSprite;

    public void OnHoverOver() {
        if (selectable.interactable) {
            buttonImage.sprite = hoverOverSprite;
        }
    }

    public void OnHoverOut() {
        if (selectable.interactable) {
            buttonImage.sprite = hoverOutSprite;
        }
    }
}
