using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterPortrait : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

    private ICharacter _character;
    private IMAGE_SIZE _imgSize;
    private bool _ignoreSize;
    private PortraitSettings _portraitSettings;

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

    [Header("Body")]
    [SerializeField] private Image body;

    [Header("Monster")]
    [SerializeField] private Image wholeImage;

    #region getters/setters
    public ECS.Character thisCharacter {
        get { return _character as ECS.Character; }
    }
    #endregion

    public void GeneratePortrait(ICharacter character, IMAGE_SIZE imgSize, bool ignoreSize = false) {
        _character = character;
        _ignoreSize = ignoreSize;
        SetImageSize(imgSize, ignoreSize);
        _portraitSettings = character.portraitSettings;
        if (character is ECS.Character) {
            SetBody(character.portraitSettings.bodyIndex);
            SetHead(character.portraitSettings.headIndex);
            SetEyes(character.portraitSettings.eyesIndex);
            SetEyebrows(character.portraitSettings.eyesIndex);
            SetNose(character.portraitSettings.noseIndex);
            SetMouth(character.portraitSettings.mouthIndex);
            SetHair(character.portraitSettings.hairIndex);
            SetHairColor(character.portraitSettings.hairColor);
            wholeImage.gameObject.SetActive(false);
        } else if (character is Monster) {
            body.gameObject.SetActive(false);
            head.gameObject.SetActive(false);
            eyes.gameObject.SetActive(false);
            eyebrows.gameObject.SetActive(false);
            nose.gameObject.SetActive(false);
            mouth.gameObject.SetActive(false);
            hair.gameObject.SetActive(false);
            hairBack.gameObject.SetActive(false);
            hairOverlay.gameObject.SetActive(false);
            hairBackOverlay.gameObject.SetActive(false);
            wholeImage.sprite = MonsterManager.Instance.GetMonsterSprite(character.name);
            wholeImage.gameObject.SetActive(true);
        }
        
    }
    public void GeneratePortrait(PortraitSettings portraitSettings, IMAGE_SIZE imgSize, bool ignoreSize = false) {
        _ignoreSize = ignoreSize;
        _portraitSettings = portraitSettings;
        SetImageSize(imgSize, ignoreSize);
        SetBody(portraitSettings.bodyIndex);
        SetHead(portraitSettings.headIndex);
        SetEyes(portraitSettings.eyesIndex);
        SetEyebrows(portraitSettings.eyesIndex);
        SetNose(portraitSettings.noseIndex);
        SetMouth(portraitSettings.mouthIndex);
        SetHair(portraitSettings.hairIndex);
        SetHairColor(portraitSettings.hairColor);
    }

    #region Pointer Actions
    public void OnPointerEnter(PointerEventData eventData) {
#if !WORLD_CREATION_TOOL
        this.transform.localScale = new Vector3(2f, 2f, 1f);
#endif
    }
    public void OnPointerExit(PointerEventData eventData) {
#if !WORLD_CREATION_TOOL
        this.transform.localScale = Vector3.one;
#endif
    }
    public void OnPointerClick(PointerEventData eventData) {
#if !WORLD_CREATION_TOOL
        if (_character != null) {
            if (UIManager.Instance.characterInfoUI.isWaitingForAttackTarget) {
                CharacterAction attackAction = _character.iparty.icharacterObject.currentState.GetAction(ACTION_TYPE.ATTACK);
                if (attackAction.CanBeDone(_character.iparty.icharacterObject) && attackAction.CanBeDoneBy(UIManager.Instance.characterInfoUI.currentlyShowingCharacter.party, _character.iparty.icharacterObject)) { //TODO: Change this checker to relationship status checking instead of just faction
                    UIManager.Instance.characterInfoUI.currentlyShowingCharacter.party.actionData.AssignAction(attackAction, _character.iparty.icharacterObject);
                    UIManager.Instance.characterInfoUI.SetAttackButtonState(false);
                    return;
                }
            } else if (UIManager.Instance.characterInfoUI.isWaitingForJoinBattleTarget) {
                CharacterAction joinBattleAction = _character.iparty.icharacterObject.currentState.GetAction(ACTION_TYPE.JOIN_BATTLE);
                if (joinBattleAction.CanBeDone(_character.iparty.icharacterObject) && joinBattleAction.CanBeDoneBy(UIManager.Instance.characterInfoUI.currentlyShowingCharacter.party, _character.iparty.icharacterObject)) { //TODO: Change this checker to relationship status checking instead of just faction
                    UIManager.Instance.characterInfoUI.currentlyShowingCharacter.party.actionData.AssignAction(joinBattleAction, _character.iparty.icharacterObject);
                    UIManager.Instance.characterInfoUI.SetJoinBattleButtonState(false);
                    return;
                }
            }
            NewParty iparty = _character.iparty;
            if (iparty.icharacters.Count > 1) {
                UIManager.Instance.ShowPartyInfo(iparty);
            } else if (iparty.icharacters.Count == 1) {
                if (iparty.icharacters[0] is ECS.Character) {
                    UIManager.Instance.ShowCharacterInfo(iparty.icharacters[0] as ECS.Character);
                } else if (iparty.icharacters[0] is Monster) {
                    UIManager.Instance.ShowMonsterInfo(iparty.icharacters[0] as Monster);
                }
            }
        }
#endif
    }
    #endregion

    public void SetHair(int index) {
        HairSetting chosenHairSettings = CharacterManager.Instance.GetHairSprite(index, _imgSize, _portraitSettings.race, _portraitSettings.gender);
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
           
        if (!_ignoreSize) {
            hair.SetNativeSize();
            hairBack.SetNativeSize();
        }
        
        //if (chosenHairSettings.hairBackSprite == null) {
        //    hairBack.sprite = chosenHairSettings.hairSprite;
        //} else {
        //    hairBack.sprite = chosenHairSettings.hairBackSprite;
        //}
    }
    public void SetHead(int index) {
        Sprite headSprite = CharacterManager.Instance.GetHeadSprite(index, _imgSize, _portraitSettings.race, _portraitSettings.gender);
        head.sprite = headSprite;
        if (!_ignoreSize) {
            head.SetNativeSize();
        }
    }
    public void SetEyes(int index) {
        Sprite eyeSprite = CharacterManager.Instance.GetEyeSprite(index, _imgSize, _portraitSettings.race, _portraitSettings.gender);
        eyes.sprite = eyeSprite;
        if (!_ignoreSize) {
            eyes.SetNativeSize();
        }
    }
    public void SetEyebrows(int index) {
        Sprite eyeBrowSprite = CharacterManager.Instance.GetEyebrowSprite(index, _imgSize, _portraitSettings.race, _portraitSettings.gender);
        eyebrows.sprite = eyeBrowSprite;
        if (!_ignoreSize) {
            eyebrows.SetNativeSize();
        }
    }
    public void SetNose(int index) {
        nose.sprite = CharacterManager.Instance.GetNoseSprite(index, _imgSize, _portraitSettings.race, _portraitSettings.gender);
        if (!_ignoreSize) {
            nose.SetNativeSize();
        }
    }
    public void SetMouth(int index) {
        mouth.sprite = CharacterManager.Instance.GetMouthSprite(index, _imgSize, _portraitSettings.race, _portraitSettings.gender);
        if (!_ignoreSize) {
            mouth.SetNativeSize();
        }
    }
    public void SetBody(int index) {
        body.sprite = CharacterManager.Instance.GetBodySprite(index, _imgSize, _portraitSettings.race, _portraitSettings.gender);
        if (!_ignoreSize) {
            body.SetNativeSize();
        }
    }
    public void SetHairColor(Color hairColor) {
        //hair.color = hairColor;
        //hairBack.color = hairColor;
        Color newColor = new Color(hairColor.r, hairColor.g, hairColor.b, 115f/255f);
        hairOverlay.color = newColor;
        hairBackOverlay.color = newColor;
    }
    public Color GetHairColor() {
        return hair.color;
    }

    private void SetImageSize(IMAGE_SIZE imgSize, bool ignoreSize) {
        _imgSize = imgSize;
        if (!ignoreSize) {
            switch (imgSize) {
                case IMAGE_SIZE.X64:
                    (this.transform as RectTransform).sizeDelta = new Vector2(64f, 64f);
                    break;
                case IMAGE_SIZE.X256:
                    (this.transform as RectTransform).sizeDelta = new Vector2(256f, 256f);
                    break;
                default:
                    break;
            }
        }
    }
}
