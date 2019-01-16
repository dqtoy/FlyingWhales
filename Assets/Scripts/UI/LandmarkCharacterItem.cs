
using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LandmarkCharacterItem : PooledObject {

    public Character character { get; private set; }

    public CharacterPortrait portrait;

    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private TextMeshProUGUI subLbl;

    public void SetCharacter(Character character) {
        this.character = character;
        portrait.GeneratePortrait(character);
        nameLbl.text = character.name;
        subLbl.text = Utilities.GetNormalizedSingularRace(character.race) + " " + character.characterClass.className;
    }

    public void ShowCharacterInfo() {
        UIManager.Instance.ShowCharacterInfo(character);
    }


    public void ShowItemInfo() {
        if (character == null) {
            return;
        }
        if (portrait.isLocked) {
            return;
        }
        if (character.currentParty.characters.Count > 1) {
            UIManager.Instance.ShowSmallInfo(character.currentParty.name);
        } else {
            UIManager.Instance.ShowSmallInfo(character.name);
        }
    }
    public void HideItemInfo() {
        UIManager.Instance.HideSmallInfo();
    }
}
