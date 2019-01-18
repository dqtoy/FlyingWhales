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

    [Header("BG")]
    [SerializeField] private Image baseBG;
    [SerializeField] private Image lockedFrame;
    [SerializeField] private TextMeshProUGUI lvlTxt;
    [SerializeField] private GameObject lvlGO;

    [Header("Face")]
    [SerializeField] private GameObject faceParentGO;

    [Header("Head")]
    [SerializeField] private Image head;

    [Header("Eyes")]
    [SerializeField] private Image eyes;
    [SerializeField] private Image eyebrows;
    
    [Header("Nose")]
    [SerializeField] private Image nose;

    [Header("Mouth")]
    [SerializeField] private Image mouth;

    [Header("Hair")]
    [SerializeField] private Image hair;
    [SerializeField] private Image hairBack;
    [SerializeField] private Image hairOverlay;
    [SerializeField] private Image hairBackOverlay;
    [SerializeField] private Image facialHair;
    [SerializeField] private Image facialHairOverlay;

    [Header("Body")]
    [SerializeField] private Image body;

    [Header("Monster")]
    [SerializeField] private Image wholeImage;

    [Header("Name")]
    [SerializeField] private TextMeshProUGUI nameLbl;

    [Header("Other")]
    [SerializeField] private GameObject unknownGO;
    [SerializeField] private FactionEmblem factionEmblem;

    private Vector2 defaultPos = new Vector2(11.7f, -3f);
    private Vector2 defaultSize = new Vector2(97f, 97f);

    #region getters/setters
    public Character thisCharacter {
        get { return _character; }
    }
    public PortraitSettings portraitSettings {
        get { return _portraitSettings; }
    }
    public bool isLocked {
        get { return unknownGO.activeSelf; }
    }
    #endregion

    void Awake() {
        //Material mat = Instantiate(wholeImage.material);
        //wholeImage.material = mat;
    }

    private void OnEnable() {
        Messenger.AddListener<Character>(Signals.CHARACTER_LEVEL_CHANGED, OnCharacterLevelChanged);
        Messenger.AddListener<Character>(Signals.FACTION_SET, OnFactionSet);
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
        if (character.race == RACE.DEMON) {
            SetWholeImageSprite(CharacterManager.Instance.GetDemonPortraitSprite(character.characterClass.className));
        } else {
            SetBody(character.portraitSettings.bodyIndex);
            SetHead(character.portraitSettings.headIndex);
            SetEyes(character.portraitSettings.eyesIndex);
            SetEyebrows(character.portraitSettings.eyesIndex);
            SetNose(character.portraitSettings.noseIndex);
            SetMouth(character.portraitSettings.mouthIndex);
            SetHair(character.portraitSettings.hairIndex);
            SetFacialHair(character.portraitSettings.facialHairIndex);
            SetHairColor(character.portraitSettings.hairColor);
            SetWholeImageSprite(null);
        }
        nameLbl.text = character.urlName;
        UpdateLvl();
        UpdateFrame();
        UpdateFactionEmblem();

        RectTransform faceRT = faceParentGO.GetComponent<RectTransform>();
        if (character.gender == GENDER.MALE && character.race != RACE.HUMANS && character.race != RACE.DEMON) {
            faceRT.sizeDelta = new Vector2(108f, 108f);
            faceRT.anchoredPosition = Vector2.zero;
        } else {
            faceRT.sizeDelta = defaultSize;
            faceRT.anchoredPosition = defaultPos;
        }
        //UpdateUnknownVisual();
    }
    public void GeneratePortrait(PortraitSettings portraitSettings) {
        _portraitSettings = portraitSettings;
        if (portraitSettings == null) {
            SetBodyPartsState(false);
            SetWholeImageSprite(null);
            return;
        }
        SetBody(portraitSettings.bodyIndex);
        SetHead(portraitSettings.headIndex);
        SetEyes(portraitSettings.eyesIndex);
        SetEyebrows(portraitSettings.eyesIndex);
        SetNose(portraitSettings.noseIndex);
        SetMouth(portraitSettings.mouthIndex);
        SetHair(portraitSettings.hairIndex);
        SetFacialHair(portraitSettings.facialHairIndex);
        SetHairColor(portraitSettings.hairColor);
        SetWholeImageSprite(null);
        UpdateFactionEmblem();
    }

    #region Utilities
    private void UpdateLvl() {
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
    public void ToggleNameLabel(bool state) {
        if (nameLbl.gameObject.activeSelf != state) {
            nameLbl.gameObject.SetActive(state);
        }
    }
    public void SwitchBGToLocked() {
        lockedFrame.gameObject.SetActive(true);
        //draggableFrameGO.SetActive(false);
    }
    public void SwitchBGToDraggable() {
        lockedFrame.gameObject.SetActive(false);
        //draggableFrameGO.SetActive(true);
    }
    //public void UpdateUnknownVisual() {
    //    if (_character != null) {
    //        if (_character.isDefender) {
    //            if (forceShowPortrait || GameManager.Instance.inspectAll) {
    //                lvlGO.SetActive(true);
    //                unknownGO.SetActive(false);
    //                SetBodyPartsState(true);
    //            } else {
    //                lvlGO.SetActive(_character.defendingArea.areaInvestigation.isActivelyCollectingToken);
    //                unknownGO.SetActive(!_character.defendingArea.areaInvestigation.isActivelyCollectingToken);
    //                SetBodyPartsState(_character.defendingArea.areaInvestigation.isActivelyCollectingToken);
    //            }
    //        } else {
    //            CharacterToken characterToken = _character.characterToken;
    //            if (forceShowPortrait || GameManager.Instance.inspectAll) {
    //                lvlGO.SetActive(true);
    //                unknownGO.SetActive(false);
    //                SetBodyPartsState(true);
    //            } else {
    //                lvlGO.SetActive(characterToken.isObtainedByPlayer);
    //                unknownGO.SetActive(!characterToken.isObtainedByPlayer);
    //                SetBodyPartsState(characterToken.isObtainedByPlayer);
    //            }
    //        }
            
    //    }
    //}
    private void UpdateFrame() {
        if (_character != null && _character.job.jobType != JOB.NONE) {
            PortraitFrame frame = CharacterManager.Instance.GetPortraitFrame(_character.job.jobType);
            baseBG.sprite = frame.baseBG;
            lockedFrame.sprite = frame.frameOutline;
        }
    }
    public void ShowCharacterInfo() {
        UIManager.Instance.ShowSmallInfo(_character.name);
    }
    public void HideCharacterInfo() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region Pointer Actions
    public void OnPointerClick(PointerEventData eventData) {
#if !WORLD_CREATION_TOOL
        if (ignoreInteractions || isLocked) {
            return;
        }
        if (eventData.button == PointerEventData.InputButton.Right) {
            OnRightClick();
        }
        
#endif
    }
    public void OnClick(BaseEventData eventData) {
        if (ignoreInteractions) {
            return;
        }
        OnPointerClick(eventData as PointerEventData);
    }
    public void OnRightClick() {
        if (_character != null) {
            UIManager.Instance.ShowCharacterInfo(_character);
        }
    }
    #endregion

    #region Body Parts
    public void SetHair(int index) {
        HairSetting chosenHairSettings = CharacterManager.Instance.GetHairSprite(index, _portraitSettings.race, _portraitSettings.gender);
        //Sprite hairSprite = CharacterManager.Instance.GetHairSprite(index, _imgSize, _character.);
        hair.sprite = chosenHairSettings.hairSprite;
        hairOverlay.sprite = chosenHairSettings.hairSprite;
        hairBack.sprite = chosenHairSettings.hairBackSprite;
        hairBackOverlay.sprite = chosenHairSettings.hairBackSprite;
        if (chosenHairSettings.hairBackSprite == null) {
            hairBack.gameObject.SetActive(false);
            hairBackOverlay.gameObject.SetActive(false);
        } else {
            hairBack.gameObject.SetActive(true);
            hairBackOverlay.gameObject.SetActive(true);
        }

        //if (!_ignoreSize) {
        //    hair.SetNativeSize();
        //    hairBack.SetNativeSize();
        //    hairOverlay.SetNativeSize();
        //    hairBackOverlay.SetNativeSize();
        //}
        hair.gameObject.SetActive(true);
        hairOverlay.gameObject.SetActive(true);
        //if (chosenHairSettings.hairBackSprite == null) {
        //    hairBack.sprite = chosenHairSettings.hairSprite;
        //} else {
        //    hairBack.sprite = chosenHairSettings.hairBackSprite;
        //}
    }
    public void SetHead(int index) {
        Sprite headSprite = CharacterManager.Instance.GetHeadSprite(index, _portraitSettings.race, _portraitSettings.gender);
        head.sprite = headSprite;
        //if (!_ignoreSize) {
        //    head.SetNativeSize();
        //}
        head.gameObject.SetActive(true);
    }
    public void SetEyes(int index) {
        Sprite eyeSprite = CharacterManager.Instance.GetEyeSprite(index, _portraitSettings.race, _portraitSettings.gender);
        eyes.sprite = eyeSprite;
        //if (!_ignoreSize) {
        //    eyes.SetNativeSize();
        //}
        eyes.gameObject.SetActive(eyeSprite != null);
    }
    public void SetEyebrows(int index) {
        Sprite eyeBrowSprite = CharacterManager.Instance.GetEyebrowSprite(index, _portraitSettings.race, _portraitSettings.gender);
        eyebrows.sprite = eyeBrowSprite;
        //if (!_ignoreSize) {
        //    eyebrows.SetNativeSize();
        //}
        eyebrows.gameObject.SetActive(eyeBrowSprite != null);
    }
    public void SetNose(int index) {
        nose.sprite = CharacterManager.Instance.GetNoseSprite(index, _portraitSettings.race, _portraitSettings.gender);
        //if (!_ignoreSize) {
        //    nose.SetNativeSize();
        //}
        nose.gameObject.SetActive(nose.sprite != null);
    }
    public void SetMouth(int index) {
        mouth.sprite = CharacterManager.Instance.GetMouthSprite(index, _portraitSettings.race, _portraitSettings.gender);
        //if (!_ignoreSize) {
        //    mouth.SetNativeSize();
        //}
        mouth.gameObject.SetActive(mouth.sprite != null);
    }
    public void SetFacialHair(int index) {
        facialHair.sprite = CharacterManager.Instance.GetFacialHairSprite(index, _portraitSettings.race, _portraitSettings.gender);
        facialHairOverlay.sprite = facialHair.sprite;
        if (facialHair.sprite == null) {
            facialHair.gameObject.SetActive(false);
            facialHairOverlay.gameObject.SetActive(false);
        } else {
            facialHair.gameObject.SetActive(true);
            facialHairOverlay.gameObject.SetActive(true);
        }
        //if (!_ignoreSize) {
        //    body.SetNativeSize();
        //}
    }
    public void SetBody(int index) {
        body.sprite = CharacterManager.Instance.GetBodySprite(index, _portraitSettings.race, _portraitSettings.gender);
        //if (!_ignoreSize) {
        //    body.SetNativeSize();
        //}
        body.gameObject.SetActive(body.sprite != null);
    }
    public void SetHairColor(Color hairColor) {
        //hair.color = hairColor;
        //hairBack.color = hairColor;
        Color newColor = new Color(hairColor.r, hairColor.g, hairColor.b, 115f/255f);
        hairOverlay.color = newColor;
        hairBackOverlay.color = newColor;
        facialHairOverlay.color = newColor;
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
}
