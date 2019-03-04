using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterMarker : PooledObject {

    public delegate void HoverMarkerAction(Character character, LocationGridTile location);
    public HoverMarkerAction hoverEnterAction;

    public System.Action hoverExitAction;

    public Character character { get; private set; }
    public LocationGridTile location { get; private set; }

    [SerializeField] private RectTransform rt;
    [SerializeField] private Toggle toggle;
    [SerializeField] private Image mainImg;
    [SerializeField] private Image clickedImg;

    [Header("Male")]
    [SerializeField] private Sprite defaultMaleSprite;
    [SerializeField] private Sprite hoveredMaleSprite;
    [SerializeField] private Sprite clickedMaleSprite;

    [Header("Female")]
    [SerializeField] private Sprite defaultFemaleSprite;
    [SerializeField] private Sprite hoveredFemaleSprite;
    [SerializeField] private Sprite clickedFemaleSprite;

    public void SetCharacter(Character character, LocationGridTile location) {
        this.character = character;
        this.location = location;
        if (UIManager.Instance.characterInfoUI.isShowing) {
            toggle.isOn = UIManager.Instance.characterInfoUI.activeCharacter.id == character.id;
        }
        SpriteState ss = new SpriteState();
        switch (character.gender) {
            case GENDER.MALE:
                mainImg.sprite = defaultMaleSprite;
                clickedImg.sprite = clickedMaleSprite;
                ss.highlightedSprite = hoveredMaleSprite;
                ss.pressedSprite = clickedMaleSprite;
                break;
            case GENDER.FEMALE:
                mainImg.sprite = defaultFemaleSprite;
                clickedImg.sprite = clickedFemaleSprite;
                ss.highlightedSprite = hoveredFemaleSprite;
                ss.pressedSprite = clickedFemaleSprite;
                break;
            default:
                mainImg.sprite = defaultMaleSprite;
                clickedImg.sprite = clickedMaleSprite;
                ss.highlightedSprite = hoveredMaleSprite;
                ss.pressedSprite = clickedMaleSprite;
                break;
        }
        toggle.spriteState = ss;

        Vector3 randomRotation = new Vector3(0f, 0f, 90f);
        randomRotation.z *= Random.Range(1f, 4f);
        rt.localRotation = Quaternion.Euler(randomRotation);

        Messenger.AddListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
    }

    public void SetHoverAction(HoverMarkerAction hoverEnterAction, System.Action hoverExitAction) {
        this.hoverEnterAction = hoverEnterAction;
        this.hoverExitAction = hoverExitAction;
    }

    public void HoverAction() {
        if (hoverEnterAction != null) {
            hoverEnterAction.Invoke(character, location);
        }
    }
    public void HoverExitAction() {
        if (hoverExitAction != null) {
            hoverExitAction();
        }
    }

    public override void Reset() {
        base.Reset();
        character = null;
        location = null;
        hoverEnterAction = null;
        hoverExitAction = null;
        Messenger.RemoveListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.RemoveListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
    }

    public void OnPointerClick(bool state) {
        if (state) {
            UIManager.Instance.ShowCharacterInfo(character);
        }
    }

    private void OnMenuOpened(UIMenu menu) {
        if (menu is CharacterInfoUI) {
            if ((menu as CharacterInfoUI).activeCharacter.id == character.id) {
                toggle.isOn = true;
            } else {
                toggle.isOn = false;
            }
             
        }
    }
    private void OnMenuClosed(UIMenu menu) {
        if (menu is CharacterInfoUI) {
            toggle.isOn = false;
        }
    }
}
