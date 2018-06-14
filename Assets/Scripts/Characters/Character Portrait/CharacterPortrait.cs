using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPortrait : MonoBehaviour {

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

    public void GeneratePortrait(PortraitSettings settings) {
        Sprite eyeSprite = CharacterManager.Instance.eyeSprites[settings.eyesIndex];
        eyes.sprite = eyeSprite;

        Sprite eyeBrowSprite = CharacterManager.Instance.eyeBrowSprites[settings.eyeBrowIndex];
        eyebrows.sprite = eyeBrowSprite;

        nose.sprite = CharacterManager.Instance.noseSprites[settings.noseIndex];
        mouth.sprite = CharacterManager.Instance.mouthSprites[settings.mouthIndex];
        //HairSettings chosenHairSettings;
        //if (settings.gender == GENDER.FEMALE) {
        //    chosenHairSettings = CharacterManager.Instance.femaleHairSettings[settings.hairIndex];
        //} else {
        //    chosenHairSettings = CharacterManager.Instance.maleHairSettings[settings.hairIndex];
        //}
        HairSetting chosenHairSettings = CharacterManager.Instance.hairSettings[settings.hairIndex];
        hair.sprite = chosenHairSettings.hairSprite;
        if (chosenHairSettings.hairBackSprite == null) {
            hairBack.sprite = chosenHairSettings.hairSprite;
        } else {
            hairBack.sprite = chosenHairSettings.hairBackSprite;
        }
        //hair.transform.localPosition = chosenHairSettings.hairPosition;

        hair.color = settings.hairColor;
        hairBack.color = settings.hairColor;
    }
}
