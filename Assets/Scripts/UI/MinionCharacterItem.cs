using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinionCharacterItem : CharacterItem {

    [SerializeField] private Image combatAbilityImg;
    [SerializeField] private TextMeshProUGUI subTextLbl;
    [SerializeField] private RectTransform subTextRT;
    [SerializeField] private RectTransform subTextContainer;

    public override void SetCharacter(Character character) {
        base.SetCharacter(character);
        UpdateCombatAbility();
        UpdateSubText();
        UpdateCover();
        Messenger.AddListener<Minion, Region>(Signals.MINION_CHANGED_ASSIGNED_REGION, OnMinionChangedAssignedRegion);
    }

    private void OnMinionChangedAssignedRegion(Minion minion, Region region) {
        if (minion.character == this.character) {
            UpdateSubText();
            UpdateCover();
        }
    }

    private void UpdateCombatAbility() {
        combatAbilityImg.sprite = PlayerManager.Instance.GetCombatAbilitySprite(character.minion.combatAbility.name);
    }

    public void ShowCombatAbilityTooltip() {
        string header = character.minion.combatAbility.name;
        string message = character.minion.combatAbility.description;
        UIManager.Instance.ShowSmallInfo(message, header);
    }

    private void UpdateSubText() {
        if (character.minion.busyReasonLog != null) {
            subTextLbl.text = Utilities.LogReplacer(character.minion.busyReasonLog);
            subTextContainer.gameObject.SetActive(true);
        } else {
            subTextLbl.text = "Actions: ";
            for (int i = 0; i < character.minion.deadlySin.assignments.Length; i++) {
                subTextLbl.text += Utilities.NormalizeStringUpperCaseFirstLetters(character.minion.deadlySin.assignments[i].ToString());
                if (i + 1 != character.minion.deadlySin.assignments.Length) {
                    subTextLbl.text += ", ";
                }
            }
            subTextContainer.gameObject.SetActive(true);
        }
    }
    private void UpdateCover() {
        if (character.minion.assignedRegion != null) {
            SetCoverState(true);
        } else {
            SetCoverState(false);
        }
    }

    #region Utilities
    Coroutine scrollRoutine;
    public void ScrollText() {
        if (subTextRT.sizeDelta.x < subTextContainer.sizeDelta.x || scrollRoutine != null) {
            return;
        }
        scrollRoutine = StartCoroutine(Scroll());
    }
    public void StopScroll() {
        if (scrollRoutine != null) {
            StopCoroutine(scrollRoutine);
            scrollRoutine = null;
        }
        subTextRT.anchoredPosition = new Vector3(0f, subTextRT.anchoredPosition.y);
    }
    private IEnumerator Scroll() {
        float width = subTextLbl.preferredWidth;
        Vector3 startPosition = subTextRT.anchoredPosition;

        float difference = subTextContainer.sizeDelta.x - subTextRT.sizeDelta.x;

        float scrollDirection = -1f;

        while (true) {
            float newX = subTextRT.anchoredPosition.x + (0.5f * scrollDirection);
            subTextRT.anchoredPosition = new Vector3(newX, startPosition.y, startPosition.z);
            if (subTextRT.anchoredPosition.x < difference) {
                scrollDirection = 1f;
            } else if (subTextRT.anchoredPosition.x > 0) {
                scrollDirection = -1f;
            }
            yield return null;
        }
    }
    #endregion

    public override void Reset() {
        base.Reset();
        Messenger.RemoveListener<Minion, Region>(Signals.MINION_CHANGED_ASSIGNED_REGION, OnMinionChangedAssignedRegion);
    }
}
