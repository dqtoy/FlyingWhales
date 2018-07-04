using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterPortrait : MonoBehaviour, IPointerClickHandler {

    private ICharacter _character;
    private IMAGE_SIZE _imgSize;
    private bool _ignoreSize;


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

    [Header("Body")]
    [SerializeField] private Image body;

    public void GeneratePortrait(ICharacter character, IMAGE_SIZE imgSize, bool ignoreSize = false) {
        _character = character;
        _ignoreSize = ignoreSize;
        SetImageSize(imgSize, ignoreSize);
        SetBody(character.portraitSettings.bodyIndex);
        SetHead(character.portraitSettings.headIndex);
        SetEyes(character.portraitSettings.eyesIndex);
        SetEyebrows(character.portraitSettings.eyesIndex);
        SetNose(character.portraitSettings.noseIndex);
        SetMouth(character.portraitSettings.mouthIndex);
        SetHair(character.portraitSettings.hairIndex);
        SetHairColor(character.portraitSettings.hairColor);
    }
    public void GeneratePortrait(PortraitSettings portraitSettings, IMAGE_SIZE imgSize, bool ignoreSize = false) {
        _ignoreSize = ignoreSize;
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

    public void OnPointerClick(PointerEventData eventData) {
#if !WORLD_CREATION_TOOL
        if (_character != null) {
            if (UIManager.Instance.characterInfoUI.isWaitingForAttackTarget) {
                CharacterAction attackAction = _character.iparty.icharacterObject.currentState.GetAction(ACTION_TYPE.ATTACK);
                if (attackAction.CanBeDone() && attackAction.CanBeDoneBy(UIManager.Instance.characterInfoUI.currentlyShowingCharacter.party)) { //TODO: Change this checker to relationship status checking instead of just faction
                    UIManager.Instance.characterInfoUI.currentlyShowingCharacter.party.actionData.AssignAction(attackAction);
                    UIManager.Instance.characterInfoUI.SetAttackButtonState(false);
                    return;
                }
            } else if (UIManager.Instance.characterInfoUI.isWaitingForJoinBattleTarget) {
                CharacterAction joinBattleAction = _character.iparty.icharacterObject.currentState.GetAction(ACTION_TYPE.JOIN_BATTLE);
                if (joinBattleAction.CanBeDone() && joinBattleAction.CanBeDoneBy(UIManager.Instance.characterInfoUI.currentlyShowingCharacter.party)) { //TODO: Change this checker to relationship status checking instead of just faction
                    UIManager.Instance.characterInfoUI.currentlyShowingCharacter.party.actionData.AssignAction(joinBattleAction);
                    UIManager.Instance.characterInfoUI.SetJoinBattleButtonState(false);
                    return;
                }
            }
            if (_character is ECS.Character) {
                UIManager.Instance.ShowCharacterInfo(_character as ECS.Character);
            }
        }
#endif
    }


    public void SetHair(int index) {
        //HairSetting chosenHairSettings = CharacterManager.Instance.hairSettings[index];
        Sprite hairSprite = CharacterManager.Instance.GetHairSprite(index, _imgSize);
        hair.sprite = hairSprite;
        hairBack.sprite = null;
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
        Sprite headSprite = CharacterManager.Instance.GetHeadSprite(index, _imgSize);
        head.sprite = headSprite;
        if (!_ignoreSize) {
            head.SetNativeSize();
        }
    }
    public void SetEyes(int index) {
        Sprite eyeSprite = CharacterManager.Instance.GetEyeSprite(index, _imgSize);
        eyes.sprite = eyeSprite;
        if (!_ignoreSize) {
            eyes.SetNativeSize();
        }
    }
    public void SetEyebrows(int index) {
        Sprite eyeBrowSprite = CharacterManager.Instance.GetEyebrowSprite(index, _imgSize);
        eyebrows.sprite = eyeBrowSprite;
        if (!_ignoreSize) {
            eyebrows.SetNativeSize();
        }
    }
    public void SetNose(int index) {
        nose.sprite = CharacterManager.Instance.GetNoseSprite(index, _imgSize);
        if (!_ignoreSize) {
            nose.SetNativeSize();
        }
    }
    public void SetMouth(int index) {
        mouth.sprite = CharacterManager.Instance.GetMouthSprite(index, _imgSize);
        if (!_ignoreSize) {
            mouth.SetNativeSize();
        }
    }
    public void SetBody(int index) {
        body.sprite = CharacterManager.Instance.GetBodySprite(index, _imgSize);
        if (!_ignoreSize) {
            body.SetNativeSize();
        }
    }
    public void SetHairColor(Color hairColor) {
        hair.color = hairColor;
        hairBack.color = hairColor;
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
