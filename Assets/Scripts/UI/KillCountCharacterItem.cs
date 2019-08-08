
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
    [SerializeField] private RectTransform deathLblRT;
    [SerializeField] private RectTransform deathReasonContainer;
    [SerializeField] private GameObject coverGO;

    public void SetCharacter(Character character) {
        this.character = character;
        UpdateInfo();
        Messenger.AddListener<Character>(Signals.CHARACTER_CHANGED_RACE, OnCharacterChangedRace);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<Character, Trait>(Signals.TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.AddListener<Character>(Signals.CHARACTER_RETURNED_TO_LIFE, OnCharacterReturnedToLife);
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
            deathLbl.text = "\"" + character.deathStr + "\"";
        } else {
            string text = string.Empty;
            Trait negDisTrait = character.GetTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER);
            if (negDisTrait is Unconscious) {
                text = "\"" + character.name + " was knocked out by " + negDisTrait.responsibleCharacter.name + ".\"";
            } else if (negDisTrait is Restrained) {
                text = "\"" + character.name + " was restrained by " + negDisTrait.responsibleCharacter.name + ".\"";
            } else if (character.returnedToLife) {
                if (character.characterClass.className == "Zombie") {
                    text = "\"" + character.name + " turned into a zombie.\"";
                } else {
                    text = "\"" + character.name + " was resurrected into a mindless skeleton.\"";
                }
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
        StopScroll();
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
    private void OnCharacterReturnedToLife(Character character) {
        if (character.id == this.character.id) {
            UpdateInfo();
        }
    }
    #endregion

    #region Utilities
    Coroutine scrollRoutine;
    public void ScrollText() {
        if (deathLblRT.sizeDelta.x < deathReasonContainer.sizeDelta.x || scrollRoutine != null) {
            return;
        }
        scrollRoutine = StartCoroutine(Scroll());
    }
    public void StopScroll() {
        if (scrollRoutine != null) {
            StopCoroutine(scrollRoutine);
            scrollRoutine = null;
        }
        deathLblRT.anchoredPosition = new Vector3(0f, deathLblRT.anchoredPosition.y);
    }
    private IEnumerator Scroll() {
        float width = deathLbl.preferredWidth;
        Vector3 startPosition = deathLblRT.anchoredPosition;

        float difference = deathReasonContainer.sizeDelta.x - deathLblRT.sizeDelta.x;

        float scrollDirection = -1f;

        while (true) {
            float newX = deathLblRT.anchoredPosition.x + (0.5f * scrollDirection);
            deathLblRT.anchoredPosition = new Vector3(newX, startPosition.y, startPosition.z);
            if (deathLblRT.anchoredPosition.x < difference) {
                scrollDirection = 1f;
            } else if (deathLblRT.anchoredPosition.x > 0) {
                scrollDirection = -1f;
            }
            yield return null;
        }
    }
    #endregion
}
