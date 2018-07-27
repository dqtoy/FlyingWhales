using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterTagIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    private CHARACTER_TAG _tag;
    [SerializeField] private Image icon;

    private bool isHovering = false;

    public void SetTag(CHARACTER_TAG tag) {
        _tag = tag;
        LoadIcon(tag);
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
            UIManager.Instance.ShowSmallInfo(Utilities.NormalizeStringUpperCaseFirstLetters(_tag.ToString()));
        }
    }
}
