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

    private PointerEventData.InputButton interactionBtn = PointerEventData.InputButton.Right;

    [Header("BG")]
    [SerializeField] private Image baseBG;
    [SerializeField] private Image lockedFrame;
    [SerializeField] private TextMeshProUGUI lvlTxt;
    [SerializeField] private GameObject lvlGO;

    [Header("Face")]
    [SerializeField] private GameObject faceParentGO;

    [Header("Skin")]
    [SerializeField] private Image skin;

    [Header("Hair")]
    [SerializeField] private Image hair;

    [Header("Body")]
    [SerializeField] private Image body;

    [Header("Top")]
    [SerializeField] private Image top;

    [Header("Under")]
    [SerializeField] private Image under;

    [Header("Other")]
    [SerializeField] private Image wholeImage;
    [SerializeField] private FactionEmblem factionEmblem;
    [SerializeField] private GameObject hoverObj;

    private Vector2 defaultPos = new Vector2(11.7f, -3f);
    private Vector2 defaultSize = new Vector2(97f, 97f);

    #region getters/setters
    public Character thisCharacter {
        get { return _character; }
    }
    public PortraitSettings portraitSettings {
        get { return _portraitSettings; }
    }
    #endregion

    void Start() {
        //if (skin != null) {
        //    Material mat = Instantiate(CharacterManager.Instance.hsvMaterial);
        //    skin.material = mat;
        //    //top.material = mat;
        //}
        //if (hair != null) {
        //    Material mat = Instantiate(CharacterManager.Instance.hsvMaterial);
        //    hair.material = mat;
        //}
    }

    private void OnEnable() {
        Messenger.AddListener<Character>(Signals.CHARACTER_LEVEL_CHANGED, OnCharacterLevelChanged);
        Messenger.AddListener<Character>(Signals.FACTION_SET, OnFactionSet);
        Messenger.AddListener<Character>(Signals.CHARACTER_CHANGED_RACE, OnCharacterChangedRace);
    }
    private void OnDisable() {
        RemoveListeners();
    }

    public void GeneratePortrait(Character character, CHARACTER_ROLE role = CHARACTER_ROLE.NONE) {
        _character = character;
        if(character == null) {
            SetBodyPartsState(false);
            SetWholeImageSprite(null);
            //wholeImage.gameObject.SetActive(true);
            return;
        }
        _portraitSettings = character.portraitSettings;

        Sprite classPortrait = CharacterManager.Instance.GetClassPortraitSprite(character.characterClass.className);
        if (classPortrait != null) {
            SetWholeImageSprite(classPortrait);
        } else {
            SetBody(character.portraitSettings.bodyIndex);
            SetSkin(character.portraitSettings.skinIndex);
            SetHair(character.portraitSettings.hairIndex);
            SetUnder(character.portraitSettings.underIndex);
            SetTop(character.portraitSettings.topIndex);
            SetWholeImageSprite(null);
        }
        UpdateLvl();
        UpdateFrame();
        UpdateFactionEmblem();

        under.rectTransform.SetSiblingIndex(0);
        skin.rectTransform.SetSiblingIndex(1);
        body.rectTransform.SetSiblingIndex(2);
        hair.rectTransform.SetSiblingIndex(3);
        top.rectTransform.SetSiblingIndex(4);
        UpdateShader();
    }
    public void GeneratePortrait(PortraitSettings portraitSettings) {
        _portraitSettings = portraitSettings;
        if (portraitSettings == null) {
            SetBodyPartsState(false);
            SetWholeImageSprite(null);
            return;
        }
        SetBody(portraitSettings.bodyIndex);
        SetSkin(portraitSettings.skinIndex);
        SetHair(portraitSettings.hairIndex);
        //SetHairColor(portraitSettings.hairColor);
        SetWholeImageSprite(null);
        UpdateFactionEmblem();
    }

    #region Utilities
    public void UpdateLvl() {
        lvlTxt.text = _character.level.ToString();
    }
    private void SetBodyPartsState(bool state) {
        faceParentGO.SetActive(state);
    }
    private void SetWholeImageSprite(Sprite sprite) {
        wholeImage.sprite = sprite;
        SetBodyPartsState(sprite == null);
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
    #endregion

    #region Pointer Actions
    public void SetClickButton(PointerEventData.InputButton btn) {
        interactionBtn = btn;
    }
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
            UIManager.Instance.ShowCharacterInfo(_character);
        }
    }
    #endregion

    #region Body Parts
    public void SetHair(int index) {
        Sprite chosenHairSettings = CharacterManager.Instance.GetHairSprite(index, _portraitSettings.race, _portraitSettings.gender);
        //Sprite hairSprite = CharacterManager.Instance.GetHairSprite(index, _imgSize, _character.);
        hair.sprite = chosenHairSettings;

        hair.gameObject.SetActive(chosenHairSettings != null);
    }
    public void SetSkin(int index) {
        Sprite headSprite = CharacterManager.Instance.GetSkinSprite(index, _portraitSettings.race, _portraitSettings.gender);
        skin.sprite = headSprite;
        skin.gameObject.SetActive(headSprite != null);
    }
    public void SetBody(int index) {
        body.sprite = CharacterManager.Instance.GetBodySprite(index, _portraitSettings.race, _portraitSettings.gender);
        body.gameObject.SetActive(body.sprite != null);
    }
    public void SetUnder(int index) {
        under.sprite = CharacterManager.Instance.GetUnderSprite(index, _portraitSettings.race, _portraitSettings.gender);
        under.gameObject.SetActive(under.sprite != null);
    }
    public void SetTop(int index) {
        top.sprite = CharacterManager.Instance.GetTopSprite(index, _portraitSettings.race, _portraitSettings.gender);
        top.gameObject.SetActive(top.sprite != null);
    }
    public void SetHairColor(Color hairColor) {
        //hair.color = hairColor;
        //hairBack.color = hairColor;
        Color newColor = new Color(hairColor.r, hairColor.g, hairColor.b, 115f/255f);
        //hairOverlay.color = newColor;
        //hairBackOverlay.color = newColor;
        //facialHairOverlay.color = newColor;
    }
    #endregion

    #region Listeners
    private void OnCharacterLevelChanged(Character character) {
        if (_character != null && _character.id == character.id) {
            UpdateLvl();
        }
    }
    private void RemoveListeners() {
        if (Messenger.eventTable.ContainsKey(Signals.CHARACTER_LEVEL_CHANGED)) {
            Messenger.RemoveListener<Character>(Signals.CHARACTER_LEVEL_CHANGED, OnCharacterLevelChanged);
        }
        if (Messenger.eventTable.ContainsKey(Signals.FACTION_SET)) {
            Messenger.RemoveListener<Character>(Signals.FACTION_SET, OnFactionSet);
        }
        if (Messenger.eventTable.ContainsKey(Signals.CHARACTER_CHANGED_RACE)) {
            Messenger.RemoveListener<Character>(Signals.CHARACTER_CHANGED_RACE, OnCharacterChangedRace);
        }
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
    private void UpdateShader() {
        /*
         Elves: Skin Color can change
        Humans: Hair Color can change
        Faeries: Skin Color and Hair Color can change
        Goblins: Skin Color and Hair Color can change
        Wolves: Skin Color can change
        Dragons: Skin Color can change
         */
        switch (_character.race) {
            case RACE.HUMANS:
                skin.material = null;
                hair.material = Instantiate(CharacterManager.Instance.hsvMaterial);
                wholeImage.material = null;
                hair.material.SetVector("_HSVAAdjust", new Vector4(_character.hHairColor/360f, 0f, 0f, 0f));
                break;
            case RACE.ELVES:
                skin.material = Instantiate(CharacterManager.Instance.hsvMaterial);
                hair.material = null;
                wholeImage.material = null;
                skin.material.SetVector("_HSVAAdjust", new Vector4(_character.hSkinColor/360f, 0f, 0f, 0f));
                break;
            case RACE.GOBLIN:
                skin.material = Instantiate(CharacterManager.Instance.hsvMaterial);
                hair.material = Instantiate(CharacterManager.Instance.hsvMaterial);
                wholeImage.material = null;
                skin.material.SetVector("_HSVAAdjust", new Vector4(_character.hSkinColor/360f, 0f, 0f, 0f));
                hair.material.SetVector("_HSVAAdjust", new Vector4(_character.hHairColor/360f, 0f, 0f, 0f));
                break;
            case RACE.DRAGON:
                skin.material = Instantiate(CharacterManager.Instance.hsvMaterial);
                hair.material = null;
                wholeImage.material = null;
                skin.material.SetVector("_HSVAAdjust", new Vector4(_character.hSkinColor/360f, 0f, 0f, 0f));
                break;
            case RACE.WOLF:
                skin.material = Instantiate(CharacterManager.Instance.hsvMaterial);
                hair.material = null;
                wholeImage.material = null;
                skin.material.SetVector("_HSVAAdjust", new Vector4(_character.hSkinColor/360f, 0f, 0f, 0f));
                break;
            case RACE.FAERY:
                skin.material = Instantiate(CharacterManager.Instance.hsvMaterial);
                hair.material = Instantiate(CharacterManager.Instance.hsvMaterial);
                wholeImage.material = null;
                skin.material.SetVector("_HSVAAdjust", new Vector4(_character.hSkinColor/360f, 0f, 0f, 0f));
                hair.material.SetVector("_HSVAAdjust", new Vector4(_character.hHairColor/360f, 0f, 0f, 0f));
                break;
            case RACE.SPIDER:
                skin.material = Instantiate(CharacterManager.Instance.hsvMaterial);
                hair.material = null;
                wholeImage.material = null;
                skin.material.SetVector("_HSVAAdjust", new Vector4(_character.hSkinColor/360f, 0f, 0f, 0f));
                break;
            case RACE.DEMON:
                wholeImage.material = Instantiate(CharacterManager.Instance.hsvMaterial);
                wholeImage.material.SetVector("_HSVAAdjust", new Vector4(_character.demonColor/360f, 0f, 0f, 0f));
                break;
            default:
                skin.material = null;
                hair.material = null;
                wholeImage.material = null;
                break;
        }
    }
    public void RandomizeHSV() {
        //Color origRGBCcolor = wholeImage.color;
        //float H, S, V;
        //Color.RGBToHSV(origRGBCcolor, out H, out S, out V);
        //Debug.Log("H: " + H + " S: " + S + " V: " + V);

        //H = Random.Range(140f, 220f) / 360f;
        //S = 50f/100f;
        //wholeImage.color = Color.HSVToRGB(H, S, V);
        wholeImage.material.SetVector("_HSVAAdjust", new Vector4(Random.Range(-0.4f, 0.4f), 0f, 0f, 0f));

    }
    #endregion

    public void OnCharacterChangedRace(Character character) {
        if (_character != null && _character.id == character.id) {
            GeneratePortrait(character);
        }
    }

    
}
