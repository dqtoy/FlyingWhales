using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterPortrait : MonoBehaviour, IPointerClickHandler {

    private ECS.Character _character;

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

    public void GeneratePortrait(ECS.Character character) {
        _character = character;
        SetHead(character.portraitSettings.headIndex);
        SetEyes(character.portraitSettings.eyesIndex);
        SetEyebrows(character.portraitSettings.eyesIndex);
        SetNose(character.portraitSettings.noseIndex);
        SetMouth(character.portraitSettings.mouthIndex);
        SetHair(character.portraitSettings.hairIndex);
        SetHairColor(character.portraitSettings.hairColor);
    }
    public void GeneratePortrait(PortraitSettings portraitSettings) {
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
                CharacterAction attackAction = _character.characterObject.currentState.GetAction(ACTION_TYPE.ATTACK);
                if (attackAction.CanBeDone() && attackAction.CanBeDoneBy(UIManager.Instance.characterInfoUI.currentlyShowingCharacter)) { //TODO: Change this checker to relationship status checking instead of just faction
                    UIManager.Instance.characterInfoUI.currentlyShowingCharacter.actionData.AssignAction(attackAction);
                    UIManager.Instance.characterInfoUI.SetAttackButtonState(false);
                    return;
                }
            } else if (UIManager.Instance.characterInfoUI.isWaitingForJoinBattleTarget) {
                CharacterAction joinBattleAction = _character.characterObject.currentState.GetAction(ACTION_TYPE.JOIN_BATTLE);
                if (joinBattleAction.CanBeDone() && joinBattleAction.CanBeDoneBy(UIManager.Instance.characterInfoUI.currentlyShowingCharacter)) { //TODO: Change this checker to relationship status checking instead of just faction
                    UIManager.Instance.characterInfoUI.currentlyShowingCharacter.actionData.AssignAction(joinBattleAction);
                    UIManager.Instance.characterInfoUI.SetJoinBattleButtonState(false);
                    return;
                }
            }
            UIManager.Instance.ShowCharacterInfo(_character);
        }
#endif
    }


    public void SetHair(int index) {
        HairSetting chosenHairSettings = CharacterManager.Instance.hairSettings[index];
        hair.sprite = chosenHairSettings.hairSprite;
        if (chosenHairSettings.hairBackSprite == null) {
            hairBack.sprite = chosenHairSettings.hairSprite;
        } else {
            hairBack.sprite = chosenHairSettings.hairBackSprite;
        }
    }
    public void SetHead(int index) {
        Sprite headSprite = CharacterManager.Instance.headSprites[index];
        head.sprite = headSprite;
    }
    public void SetEyes(int index) {
        Sprite eyeSprite = CharacterManager.Instance.eyeSprites[index];
        eyes.sprite = eyeSprite;
    }
    public void SetEyebrows(int index) {
        Sprite eyeBrowSprite = CharacterManager.Instance.eyeBrowSprites[index];
        eyebrows.sprite = eyeBrowSprite;
    }
    public void SetNose(int index) {
        nose.sprite = CharacterManager.Instance.noseSprites[index];
    }
    public void SetMouth(int index) {
        mouth.sprite = CharacterManager.Instance.mouthSprites[index];
    }
    public void SetHairColor(Color hairColor) {
        hair.color = hairColor;
        hairBack.color = hairColor;
    }
    public Color GetHairColor() {
        return hair.color;
    }
}
