using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterTagIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    private CHARACTER_TAG _tag;
    [SerializeField] private Image icon;

    public void SetTag(CHARACTER_TAG tag) {
        _tag = tag;
        LoadIcon(tag);
    }

    private void LoadIcon(CHARACTER_TAG tag) {
        icon.sprite = CharacterManager.Instance.GetCharacterTagSprite(tag);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        UIManager.Instance.ShowSmallInfo(Utilities.NormalizeStringUpperCaseFirstLetters(_tag.ToString()));
    }

    public void OnPointerExit(PointerEventData eventData) {
        UIManager.Instance.HideSmallInfo();
    }
}
