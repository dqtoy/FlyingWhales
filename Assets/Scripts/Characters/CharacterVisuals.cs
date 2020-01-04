using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterVisuals {

    public PortraitSettings portraitSettings { get; private set; }
    public Material hairMaterial { get; private set; }
    public Material wholeImageMaterial { get; private set; }

    public Dictionary<string, Sprite> markerAnimations { get; private set; }


    public CharacterVisuals(Character character) {
        portraitSettings = CharacterManager.Instance.GenerateRandomPortrait(character.race, character.gender, character.characterClass.className);
        CreateHairMaterial();
        UpdateMarkerAnimations(character);
    }
    public CharacterVisuals(SaveDataCharacter data) {
        portraitSettings = data.portraitSettings;
        CreateHairMaterial();
    }

    private void UpdatePortraitSettings(Character character) {
        portraitSettings = CharacterManager.Instance.GenerateRandomPortrait(character.race, character.gender, character.characterClass.className);
    }
    private void CreateHairMaterial() {
        hairMaterial = GameManager.Instantiate(CharacterManager.Instance.hsvMaterial);
        hairMaterial.SetVector("_HSVAAdjust", new Vector4(portraitSettings.hairColor / 360f, 0f, 0f, 0f));
    }
    public void CreateWholeImageMaterial() {
        hairMaterial = GameManager.Instantiate(CharacterManager.Instance.hsvMaterial);
        hairMaterial.SetVector("_HSVAAdjust", new Vector4(portraitSettings.wholeImageColor / 360f, 0f, 0f, 0f));
    }

    public void UpdateAllVisuals(Character character) {
        if (character.isSwitchingAlterEgo) {
            return;
        }
        UpdateMarkerAnimations(character);
        UpdatePortraitSettings(character);
        if (character.marker != null) {
            character.marker.UpdateMarkerVisuals();
        }
    }

    private void UpdateMarkerAnimations(Character character) {
        CharacterClassAsset assets = CharacterManager.Instance.GetMarkerAsset(character.race, character.gender, character.characterClass.className);
        if (assets != null) {
            markerAnimations = new Dictionary<string, Sprite>();
            for (int i = 0; i < assets.animationSprites.Count; i++) {
                Sprite currSprite = assets.animationSprites[i];
                markerAnimations.Add(currSprite.name, currSprite);
            }
        }
    }
}
