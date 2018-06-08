using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPortrait : MonoBehaviour {

    [Header("Eyes")]
    [SerializeField] private Image leftEye;
    [SerializeField] private Image rightEye;
    [SerializeField] private Image leftEyeBrow;
    [SerializeField] private Image rightEyeBrow;
    
    [Header("Nose")]
    [SerializeField] private Image nose;

    [Header("Mouth")]
    [SerializeField] private Image mouth;

    [Header("Hair")]
    [SerializeField] private Image hair;

    public void GeneratePortrait(PortraitSettings settings) {
        Sprite eyeSprite = CharacterManager.Instance.eyeSprites[settings.eyesIndex];
        leftEye.sprite = eyeSprite;
        rightEye.sprite = eyeSprite;

        Sprite eyeBrowSprite = CharacterManager.Instance.eyeBrowSprites[settings.eyeBrowIndex];
        leftEyeBrow.sprite = eyeBrowSprite;
        rightEyeBrow.sprite = eyeBrowSprite;

        nose.sprite = CharacterManager.Instance.noseSprites[settings.noseIndex];
        mouth.sprite = CharacterManager.Instance.mouthSprites[settings.mouthIndex];
        HairSettings chosenHairSettings;
        if (settings.gender == GENDER.FEMALE) {
            chosenHairSettings = CharacterManager.Instance.femaleHairSettings[settings.hairIndex];
        } else {
            chosenHairSettings = CharacterManager.Instance.maleHairSettings[settings.hairIndex];
        }
        hair.sprite = chosenHairSettings.hairVisual;
        hair.transform.localPosition = chosenHairSettings.hairPosition;

        hair.color = settings.hairColor;
    }
}
