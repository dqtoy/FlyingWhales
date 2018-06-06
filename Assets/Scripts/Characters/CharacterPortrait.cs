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
        if (settings.gender == GENDER.FEMALE) {
            hair.sprite = CharacterManager.Instance.femaleHairSprites[settings.hairIndex];
        } else {
            hair.sprite = CharacterManager.Instance.maleHairSprites[settings.hairIndex];
        }
        hair.color = settings.hairColor;
    }
}
