using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterVisuals {

    public PortraitSettings portraitSettings { get; private set; }
    public Material hairMaterial { get; private set; }
    public Material wholeImageMaterial { get; private set; }

    public Dictionary<string, Sprite> markerVisuals { get; private set; }


    public CharacterVisuals(Character character) {
        portraitSettings = CharacterManager.Instance.GenerateRandomPortrait(character.race, character.gender, character.characterClass.className);
        CreateHairMaterial();
        UpdateMarkerVisuals(character);
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
        UpdateMarkerVisuals(character);
        UpdatePortraitSettings(character);
    }

    private void UpdateMarkerVisuals(Character character) {
        CharacterClassAsset assets = CharacterManager.Instance.GetMarkerAsset(character.race, character.gender, character.characterClass.className);
        markerVisuals = new Dictionary<string, Sprite>();
        for (int i = 0; i < assets.animationSprites.Count; i++) {
            Sprite currSprite = assets.animationSprites[i];
            markerVisuals.Add(currSprite.name, currSprite);
        }
    }
}
