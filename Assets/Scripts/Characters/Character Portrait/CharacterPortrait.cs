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
        Sprite eyeSprite = CharacterManager.Instance.eyeSprites[character.portraitSettings.eyesIndex];
        eyes.sprite = eyeSprite;

        Sprite eyeBrowSprite = CharacterManager.Instance.eyeBrowSprites[character.portraitSettings.eyeBrowIndex];
        eyebrows.sprite = eyeBrowSprite;

        nose.sprite = CharacterManager.Instance.noseSprites[character.portraitSettings.noseIndex];
        mouth.sprite = CharacterManager.Instance.mouthSprites[character.portraitSettings.mouthIndex];
        //HairSettings chosenHairSettings;
        //if (settings.gender == GENDER.FEMALE) {
        //    chosenHairSettings = CharacterManager.Instance.femaleHairSettings[settings.hairIndex];
        //} else {
        //    chosenHairSettings = CharacterManager.Instance.maleHairSettings[settings.hairIndex];
        //}
        HairSetting chosenHairSettings = CharacterManager.Instance.hairSettings[character.portraitSettings.hairIndex];
        hair.sprite = chosenHairSettings.hairSprite;
        if (chosenHairSettings.hairBackSprite == null) {
            hairBack.sprite = chosenHairSettings.hairSprite;
        } else {
            hairBack.sprite = chosenHairSettings.hairBackSprite;
        }
        //hair.transform.localPosition = chosenHairSettings.hairPosition;

        hair.color = character.portraitSettings.hairColor;
        hairBack.color = character.portraitSettings.hairColor;
    }

    public void OnPointerClick(PointerEventData eventData) {
        UIManager.Instance.ShowCharacterInfo(_character);
    }
}
