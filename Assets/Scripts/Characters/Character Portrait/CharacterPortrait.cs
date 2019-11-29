using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using EZObjectPools;


public class CharacterPortrait : PooledObject, IPointerClickHandler {

    private Character _character;
    private PortraitSettings _portraitSettings;

    public bool ignoreInteractions = false;

    private PointerEventData.InputButton interactionBtn = PointerEventData.InputButton.Left;

    [Header("BG")]
    [SerializeField] private Image baseBG;
    [SerializeField] private Image lockedFrame;
    [SerializeField] private TextMeshProUGUI lvlTxt;
    [SerializeField] private GameObject lvlGO;

    [Header("Face")]
    [SerializeField] private Image head;
    [SerializeField] private Image brows;
    [SerializeField] private Image eyes;
    [SerializeField] private Image mouth;
    [SerializeField] private Image nose;
    [SerializeField] private Image hair;
    [SerializeField] private Image mustache;
    [SerializeField] private Image beard;
    [SerializeField] private Image wholeImage;

    [Header("Other")]
    [SerializeField] private FactionEmblem factionEmblem;
    [SerializeField] private GameObject hoverObj;

    private System.Action onClickAction;

    #region getters/setters
    public Character thisCharacter {
        get { return _character; }
    }
    public PortraitSettings portraitSettings {
        get { return _portraitSettings; }
    }
    #endregion

    private bool isPixelPerfect;

    private void OnEnable() {
        Messenger.AddListener<Character>(Signals.CHARACTER_LEVEL_CHANGED, OnCharacterLevelChanged);
        Messenger.AddListener<Character>(Signals.FACTION_SET, OnFactionSet);
        Messenger.AddListener<Character>(Signals.CHARACTER_CHANGED_RACE, OnCharacterChangedRace);
        Messenger.AddListener<Character>(Signals.ROLE_CHANGED, OnCharacterChangedRole);
    }

    public void GeneratePortrait(Character character, bool makePixelPerfect = true) {
        _character = character;
        _portraitSettings = character.portraitSettings;

        SetPortraitAsset("head", character.portraitSettings.head, _portraitSettings.race, _portraitSettings.gender, head);
        SetPortraitAsset("brows", character.portraitSettings.brows, _portraitSettings.race, _portraitSettings.gender, brows);
        SetPortraitAsset("eyes", character.portraitSettings.eyes, _portraitSettings.race, _portraitSettings.gender, eyes);
        SetPortraitAsset("mouth", character.portraitSettings.mouth, _portraitSettings.race, _portraitSettings.gender, mouth);
        SetPortraitAsset("nose", character.portraitSettings.nose, _portraitSettings.race, _portraitSettings.gender, nose);
        SetPortraitAsset("hair", character.portraitSettings.hair, _portraitSettings.race, _portraitSettings.gender, hair);
        SetPortraitAsset("mustache", character.portraitSettings.mustache, _portraitSettings.race, _portraitSettings.gender, mustache);
        SetPortraitAsset("beard", character.portraitSettings.beard, _portraitSettings.race, _portraitSettings.gender, beard);

        isPixelPerfect = makePixelPerfect;

        if (makePixelPerfect) {
            head.SetNativeSize();
            brows.SetNativeSize();
            eyes.SetNativeSize();
            mouth.SetNativeSize();
            nose.SetNativeSize();
            hair.SetNativeSize();
            mustache.SetNativeSize();
            beard.SetNativeSize();

            (head.transform as RectTransform).anchoredPosition = new Vector2(55f, 55f);
            (brows.transform as RectTransform).anchoredPosition = new Vector2(55f, 55f);
            (eyes.transform as RectTransform).anchoredPosition = new Vector2(55f, 55f);
            (mouth.transform as RectTransform).anchoredPosition = new Vector2(55f, 55f);
            (nose.transform as RectTransform).anchoredPosition = new Vector2(55f, 55f);
            (hair.transform as RectTransform).anchoredPosition = new Vector2(55f, 55f);
            (mustache.transform as RectTransform).anchoredPosition = new Vector2(55f, 55f);
            (beard.transform as RectTransform).anchoredPosition = new Vector2(55f, 55f);
        }

        if (string.IsNullOrEmpty(_portraitSettings.wholeImage) == false) {
            //use whole image
            SetWholeImageSprite(CharacterManager.Instance.GetWholeImagePortraitSprite(_portraitSettings.wholeImage));
            SetWholeImageColor(_portraitSettings.wholeImageColor);
        } else {
            SetWholeImageSprite(null);
            SetHairColor(_portraitSettings.hairColor);
        }
        UpdateLvl();
        UpdateFrame();
        UpdateFactionEmblem();

        wholeImage.rectTransform.SetSiblingIndex(0);
        head.rectTransform.SetSiblingIndex(1);
        brows.rectTransform.SetSiblingIndex(2);
        eyes.rectTransform.SetSiblingIndex(3);
        nose.rectTransform.SetSiblingIndex(4);
        hair.rectTransform.SetSiblingIndex(5);
        mustache.rectTransform.SetSiblingIndex(6);
        beard.rectTransform.SetSiblingIndex(7);
        mouth.rectTransform.SetSiblingIndex(8);
        lvlGO.SetActive(false);
    }

    #region Utilities
    public void UpdateLvl() {
        lvlTxt.text = _character.level.ToString();
    }
    private void SetWholeImageSprite(Sprite sprite) {
        wholeImage.sprite = sprite;
        SetWholeImageState(sprite != null);
    }
    private void SetWholeImageState(bool state) {
        wholeImage.gameObject.SetActive(state);
    }
    public Color GetHairColor() {
        return hair.color;
    }
    private void UpdateFrame() {
        if (_character != null && _character.role.roleType != CHARACTER_ROLE.NONE) {
            PortraitFrame frame = CharacterManager.Instance.GetPortraitFrame(_character.role.roleType);
            baseBG.sprite = frame.baseBG;
            lockedFrame.sprite = frame.frameOutline;
            SetBaseBGState(true);
        }
    }
    public void SetBaseBGState(bool state) {
        baseBG.gameObject.SetActive(state);
    }
    public void ShowCharacterInfo() {
        UIManager.Instance.ShowSmallInfo(_character.name);
    }
    public void HideCharacterInfo() {
        UIManager.Instance.HideSmallInfo();
    }
    public void SetImageRaycastTargetState(bool state) {
        Image[] targets = this.GetComponentsInChildren<Image>();
        for (int i = 0; i < targets.Length; i++) {
            Image currImage = targets[i];
            currImage.raycastTarget = state;
        }
    }
    public void SetSize(float size) {
        Vector2 newSize = new Vector2(size, size);
        head.rectTransform.sizeDelta = newSize;
        brows.rectTransform.sizeDelta = newSize;
        eyes.rectTransform.sizeDelta = newSize;
        mouth.rectTransform.sizeDelta = newSize;
        nose.rectTransform.sizeDelta = newSize;
        hair.rectTransform.sizeDelta = newSize;
        mustache.rectTransform.sizeDelta = newSize;
        beard.rectTransform.sizeDelta = newSize;

        Vector2 newPos = new Vector2(size / 2f, size / 2f);

        head.rectTransform.anchoredPosition = newPos;
        head.rectTransform.anchorMin = Vector2.zero;

        brows.rectTransform.anchoredPosition = newPos;
        head.rectTransform.anchorMin = Vector2.zero;

        eyes.rectTransform.anchoredPosition = newPos;
        head.rectTransform.anchorMin = Vector2.zero;

        mouth.rectTransform.anchoredPosition = newPos;
        head.rectTransform.anchorMin = Vector2.zero;

        nose.rectTransform.anchoredPosition = newPos;
        head.rectTransform.anchorMin = Vector2.zero;

        hair.rectTransform.anchoredPosition = newPos;
        mustache.rectTransform.anchoredPosition = newPos;
        beard.rectTransform.anchoredPosition = newPos;
    }
    #endregion

    #region Pointer Actions
    //public void SetClickButton(PointerEventData.InputButton btn) {
    //    interactionBtn = btn;
    //}
    public void OnPointerClick(PointerEventData eventData) {
#if !WORLD_CREATION_TOOL
        if (ignoreInteractions) {
            return;
        }
        if (eventData.button == interactionBtn) {
            OnClick();
        }
        
#endif
    }
    public void OnClick(BaseEventData eventData) {
        if (ignoreInteractions || !gameObject.activeSelf) {
            return;
        }
        OnPointerClick(eventData as PointerEventData);
    }
    public void OnClick() {
        ShowCharacterMenu();
    }
    public void SetHoverHighlightState(bool state) {
        hoverObj.SetActive(state);
    }
    public void ShowCharacterMenu() {
        if (_character != null) {
            UIManager.Instance.ShowCharacterInfo(_character, true);
        }
    }
    #endregion

    #region Body Parts
    public void SetPortraitAsset(string identifier, int index, RACE race, GENDER gender, Image renderer) {
        Sprite sprite = CharacterManager.Instance.GetPortraitSprite(identifier, index, race, gender);
        renderer.sprite = sprite;
        renderer.gameObject.SetActive(renderer.sprite != null);
    }
    #endregion

    #region Listeners
    private void OnCharacterLevelChanged(Character character) {
        if (_character != null && _character.id == character.id) {
            UpdateLvl();
        }
    }
    private void RemoveListeners() {
        Messenger.RemoveListener<Character>(Signals.CHARACTER_LEVEL_CHANGED, OnCharacterLevelChanged);
        Messenger.RemoveListener<Character>(Signals.FACTION_SET, OnFactionSet);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_CHANGED_RACE, OnCharacterChangedRace);
        Messenger.RemoveListener<Character>(Signals.ROLE_CHANGED, OnCharacterChangedRole);
    }
    #endregion

    #region Pooled Object
    public override void Reset() {
        base.Reset();
        _character = null;
        ignoreInteractions = false;
        RemoveListeners();
    }
    #endregion

    #region Faction
    public void OnFactionSet(Character character) {
        if (_character != null && _character.id == character.id) {
            UpdateFactionEmblem();
        }
    }
    private void UpdateFactionEmblem() {
        factionEmblem.SetFaction(_character.faction);
    }
    #endregion

    #region Shader
    private void SetHairColor(float hairColor) {
        Material mat = Instantiate(CharacterManager.Instance.hsvMaterial);
        mat.SetVector("_HSVAAdjust", new Vector4(hairColor / 360f, 0f, 0f, 0f));
        hair.material = mat;
        mustache.material = mat;
        beard.material = mat;
    }
    private void SetWholeImageColor(float wholeImageColor) {
        wholeImage.material = Instantiate(CharacterManager.Instance.hsvMaterial);
        wholeImage.material.SetVector("_HSVAAdjust", new Vector4(wholeImageColor / 360f, 0f, 0f, 0f));
    }
    #endregion

    public void OnCharacterChangedRace(Character character) {
        if (_character != null && _character.id == character.id) {
            GeneratePortrait(character, isPixelPerfect);
        }
    }
    private void OnCharacterChangedRole(Character character) {
        if (_character != null && _character.id == character.id) {
            GeneratePortrait(character, isPixelPerfect);
        }
    }



}
