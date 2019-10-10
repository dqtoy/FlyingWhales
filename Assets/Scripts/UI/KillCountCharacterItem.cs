using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KillCountCharacterItem : CharacterItem {

    [SerializeField] private TextMeshProUGUI deathLbl;
    [SerializeField] private RectTransform deathLblRT;
    [SerializeField] private RectTransform deathReasonContainer;

    public override void SetCharacter(Character character) {
        base.SetCharacter(character);
        Messenger.AddListener<Character>(Signals.CHARACTER_CHANGED_RACE, OnCharacterChangedRace);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<Character, Trait>(Signals.TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.AddListener<Character, Trait>(Signals.TRAIT_REMOVED, OnCharacterRemovedTrait);
        Messenger.AddListener<Character>(Signals.CHARACTER_RETURNED_TO_LIFE, OnCharacterReturnedToLife);
        Messenger.AddListener<Character>(Signals.CHARACTER_SWITCHED_ALTER_EGO, OnCharacterSwitchedAlterEgo);
    }
    public override void Reset() {
        base.Reset();
        Messenger.RemoveListener<Character>(Signals.CHARACTER_CHANGED_RACE, OnCharacterChangedRace);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.RemoveListener<Character, Trait>(Signals.TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.RemoveListener<Character, Trait>(Signals.TRAIT_REMOVED, OnCharacterRemovedTrait);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_RETURNED_TO_LIFE, OnCharacterReturnedToLife);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_SWITCHED_ALTER_EGO, OnCharacterSwitchedAlterEgo);
        StopScroll();
    }
    protected override void UpdateInfo() {
        base.UpdateInfo();
        deathReasonContainer.gameObject.SetActive(!character.IsAble() || !LandmarkManager.Instance.enemyOfPlayerArea.region.IsFactionHere(character.faction));
        if (character.isDead) {
            deathLbl.text = "\"" + character.deathStr + "\"";
        } else {
            string text = string.Empty;
            Trait negDisTrait = character.GetTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER);
            if (negDisTrait != null) {
                if (negDisTrait is Unconscious) {
                    text = "\"" + character.name + " was knocked out by " + negDisTrait.responsibleCharacter.name + ".\"";
                } else if (negDisTrait is Restrained) {
                    text = "\"" + character.name + " was restrained by " + negDisTrait.responsibleCharacter.name + ".\"";
                } else if (negDisTrait is Paralyzed) {
                    text = "\"" + character.name + " became paralyzed.\"";
                }
            } else {
                if (character.returnedToLife) {
                    if (character.characterClass.className == "Zombie") {
                        text = "\"" + character.name + " turned into a zombie.\"";
                    } else {
                        text = "\"" + character.name + " was resurrected into a mindless skeleton.\"";
                    }
                } else if (character.currentAlterEgo.name != CharacterManager.Original_Alter_Ego) {
                    text = "\"" + character.name + " turned into a " + character.currentAlterEgo.name + ".\"";
                } else if (character.role.roleType == CHARACTER_ROLE.MINION) {
                    text = "\"" + character.name + " became a minion.\"";
                }
            }
            deathLbl.text = text;
        }
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
    private void OnCharacterRemovedTrait(Character character, Trait trait) {
        if (character.id == this.character.id && trait.type == TRAIT_TYPE.DISABLER && trait.effect == TRAIT_EFFECT.NEGATIVE) {
            UpdateInfo();
        }
    }
    private void OnCharacterReturnedToLife(Character character) {
        if (character.id == this.character.id) {
            UpdateInfo();
        }
    }
    private void OnCharacterSwitchedAlterEgo(Character character) {
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
