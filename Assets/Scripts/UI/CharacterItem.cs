
using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterItem : PooledObject {

    public Character character { get; private set; }

    public CharacterPortrait portrait;
    [SerializeField] private RectTransform thisTrans;
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private TextMeshProUGUI subLbl;
    [SerializeField] private GameObject coverGO;

    public virtual void SetCharacter(Character character) {
        this.character = character;
        UpdateInfo();
       
    }
    public void ShowCharacterInfo() {
        UIManager.Instance.ShowCharacterInfo(character);
    }
    protected virtual void UpdateInfo() {
        portrait.GeneratePortrait(character);
        nameLbl.text = character.name;
        subLbl.text = character.raceClassName;       
    }

    public void ShowItemInfo() {
        if (character == null) {
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
