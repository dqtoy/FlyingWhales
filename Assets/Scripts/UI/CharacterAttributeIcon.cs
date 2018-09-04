using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterAttributeIcon : PooledObject, IPointerEnterHandler, IPointerExitHandler {

    public CharacterTag attribute { get; private set; }
    [SerializeField] private Image icon;

    private bool isHovering = false;

    public void SetTag(CharacterTag attribute) {
        this.attribute = attribute;
        LoadIcon(attribute.attribute);
    }

    private void LoadIcon(CHARACTER_TAG attribute) {
        icon.sprite = CharacterManager.Instance.GetCharacterAttributeSprite(attribute);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        isHovering = false;
        UIManager.Instance.HideSmallInfo();
    }

    private void Update() {
        if (isHovering) {
            UIManager.Instance.ShowSmallInfo(Utilities.NormalizeStringUpperCaseFirstLetters(attribute.attribute.ToString()));
        }
    }

    public override void Reset() {
        base.Reset();
        attribute = null;
    }
}
