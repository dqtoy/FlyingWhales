using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterPortrait : MonoBehaviour, IPointerClickHandler {

    private ECS.Character _character;

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
        SetEyes(character.portraitSettings.eyesIndex);
        SetEyebrows(character.portraitSettings.eyesIndex);
        SetNose(character.portraitSettings.noseIndex);
        SetMouth(character.portraitSettings.mouthIndex);
        SetHair(character.portraitSettings.hairIndex);
        SetHairColor(character.portraitSettings.hairColor);
    }

    public void OnPointerClick(PointerEventData eventData) {
        UIManager.Instance.ShowCharacterInfo(_character);
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
}
