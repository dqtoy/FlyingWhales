using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterTagIcon : PooledObject, IPointerEnterHandler, IPointerExitHandler {

    public CharacterTag tag { get; private set; }
    [SerializeField] private Image icon;

    private bool isHovering = false;

    public void SetTag(CharacterTag tag) {
        this.tag = tag;
        LoadIcon(tag.tagType);
    }

    private void LoadIcon(CHARACTER_TAG tag) {
        icon.sprite = CharacterManager.Instance.GetCharacterTagSprite(tag);
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
            UIManager.Instance.ShowSmallInfo(Utilities.NormalizeStringUpperCaseFirstLetters(tag.tagType.ToString()));
        }
    }

    public override void Reset() {
        base.Reset();
        tag = null;
    }
}
