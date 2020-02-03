using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinionCharacterItem : CharacterNameplateItem {


    void OnEnable() {
        if (this.character != null) {
            UpdateSubText();
        }
    }

    public override void SetObject(Character character) {
        base.SetObject(character);
        UpdateSubText();
        Messenger.AddListener<Minion, Region>(Signals.MINION_CHANGED_ASSIGNED_REGION, OnMinionChangedAssignedRegion);
    }

    private void OnMinionChangedAssignedRegion(Minion minion, Region region) {
        if (minion.character == this.character) {
            UpdateSubText();
        }
    }

    public void ShowCombatAbilityTooltip() {
        string header = character.minion.combatAbility.name;
        string message = character.minion.combatAbility.description;
        UIManager.Instance.ShowSmallInfo(message, header);
    }

    private void UpdateSubText() {
        if (character.minion.busyReasonLog != null) {
            supportingLbl.text = Ruinarch.Utilities.LogReplacer(character.minion.busyReasonLog);
            SetSupportingLabelState(true);
        } else {
            SetSupportingLabelState(false);
        }
    }

    public override void Reset() {
        base.Reset();
        Messenger.RemoveListener<Minion, Region>(Signals.MINION_CHANGED_ASSIGNED_REGION, OnMinionChangedAssignedRegion);
    }
}
