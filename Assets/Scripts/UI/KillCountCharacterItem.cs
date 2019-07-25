
using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KillCountCharacterItem : PooledObject {

    public Character character { get; private set; }

    public CharacterPortrait portrait;
    [SerializeField] private RectTransform thisTrans;
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private TextMeshProUGUI subLbl;
    [SerializeField] private TextMeshProUGUI deathLbl;
    [SerializeField] private GameObject coverGO;

    public void SetCharacter(Character character) {
        this.character = character;
        UpdateInfo();
        Messenger.AddListener<Character>(Signals.CHARACTER_CHANGED_RACE, OnCharacterChangedRace);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<Character, Trait>(Signals.TRAIT_ADDED, OnCharacterGainedTrait);
    }
    public void ShowCharacterInfo() {
        UIManager.Instance.ShowCharacterInfo(character);
    }
    private void UpdateInfo() {
        portrait.GeneratePortrait(character);
        nameLbl.text = character.name;
        subLbl.text = character.raceClassName;
        deathLbl.gameObject.SetActive(!character.IsAble());
        if (character.isDead) {
            deathLbl.text = character.deathStr;
        } else {
            string text = string.Empty;
            Trait negDisTrait = character.GetTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER);
            if (negDisTrait is Unconscious) {
                text = character.name + " was knocked out by " + negDisTrait.responsibleCharacter.name;
            } else if (negDisTrait is Restrained) {
                text = character.name + " was restrained by " + negDisTrait.responsibleCharacter.name;
            }
            deathLbl.text = text;
        }
       
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

    public override void Reset() {
        base.Reset();
        Messenger.RemoveListener<Character>(Signals.CHARACTER_CHANGED_RACE, OnCharacterChangedRace);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.RemoveListener<Character, Trait>(Signals.TRAIT_ADDED, OnCharacterGainedTrait);
    }

    #region Listeners
    private void OnCharacterChangedRace(Character character) {
        if (character.id == this.character.id) {
            UpdateInfo();
        }
    }
    private void OnCharacterDied(Character character) {
        if (character.id == this.character.id) {
            UpdateInfo();
        }
    }
    private void OnCharacterGainedTrait(Character character, Trait trait) {
        if (character.id == this.character.id && trait.type == TRAIT_TYPE.DISABLER && trait.effect == TRAIT_EFFECT.NEGATIVE) {
            UpdateInfo();
        }
    }
    #endregion
}
