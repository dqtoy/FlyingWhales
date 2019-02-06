using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShareIntel : PlayerJobAction {

    public Character target { get; private set; }

    public ShareIntel() {
        actionName = "Share Intel";
        SetDefaultCooldownTime(10);
        targettableTypes = new List<JOB_ACTION_TARGET>() { JOB_ACTION_TARGET.CHARACTER };
    }

    public override void ActivateAction(Character assignedCharacter, Character targetCharacter) {
        //base.ActivateAction(assignedCharacter, targetCharacter);
        SetSubText("Pick intel to share with " +  targetCharacter.name);
        PlayerUI.Instance.SetIntelMenuState(true);
        PlayerUI.Instance.SetIntelItemClickActions(targetCharacter.ShareIntel);
        PlayerUI.Instance.AddIntelItemOtherClickActions(() => base.ActivateAction(assignedCharacter, targetCharacter));
        PlayerUI.Instance.AddIntelItemOtherClickActions(() => SetTargetCharacter(targetCharacter));
        PlayerUI.Instance.AddIntelItemOtherClickActions(() => SetSubText(string.Empty));
        PlayerUI.Instance.AddIntelItemOtherClickActions(() => PlayerUI.Instance.SetIntelItemClickActions(null));
    }
    public override void DeactivateAction() {
        base.DeactivateAction();
        target = null;
        SetSubText(string.Empty);
    }
    protected override bool ShouldButtonBeInteractable(Character character, Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (character.id == targetCharacter.id) {
            return false;
        }
        if (target != null && targetCharacter.id == target.id) {
            return false;
        }
        return base.ShouldButtonBeInteractable(character, targetCharacter);
    }
    protected override void OnCharacterDied(Character characterThatDied) {
        base.OnCharacterDied(characterThatDied);
        if (!this.isActive) {
            return; //if this action is no longer active, do not check if the character that died was the target
        }
        if (characterThatDied.id == target.id) {
            DeactivateAction();
        }
    }
    private void SetTargetCharacter(Character targetCharacter) {
        target = targetCharacter;
    }
}
