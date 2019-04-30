using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShareIntel : PlayerJobAction {

    public Character targetCharacter { get; private set; }

    public ShareIntel() {
        name = "Share Intel";
        SetDefaultCooldownTime(24);
        targettableTypes = new List<JOB_ACTION_TARGET>() { JOB_ACTION_TARGET.CHARACTER };
    }

    public override void ActivateAction(Character assignedCharacter, Character targetCharacter) {
        //base.ActivateAction(assignedCharacter, targetCharacter);
        SetSubText("Pick intel to share with " +  targetCharacter.name);
        UIManager.Instance.OpenShareIntelMenu(targetCharacter, assignedCharacter);
        
        //PlayerUI.Instance.SetIntelMenuState(true);
        //PlayerUI.Instance.SetIntelItemClickActions(targetCharacter.ShareIntel);
        //PlayerUI.Instance.AddIntelItemOtherClickActions(() => base.ActivateAction(assignedCharacter, targetCharacter));
        //PlayerUI.Instance.AddIntelItemOtherClickActions(() => SetTargetCharacter(targetCharacter));
        //PlayerUI.Instance.AddIntelItemOtherClickActions(() => SetSubText(string.Empty));
        //PlayerUI.Instance.AddIntelItemOtherClickActions(() => PlayerUI.Instance.SetIntelItemClickActions(null));
    }
    public void BaseActivate(Character targetCharacter) {
        base.ActivateAction(assignedCharacter, targetCharacter);
        SetSubText(string.Empty);
        SetTargetCharacter(targetCharacter);
    }
    public override void DeactivateAction() {
        this.assignedCharacter = null;
        isActive = false;
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.RemoveListener<JOB, Character>(Signals.CHARACTER_UNASSIGNED_FROM_JOB, OnCharacterUnassignedFromJob);
        targetCharacter = null;
        SetSubText(string.Empty);
    }
    protected override bool ShouldButtonBeInteractable(Character character, Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (character.id == targetCharacter.id) {
            return false;
        }
        if (this.targetCharacter != null && targetCharacter.id == this.targetCharacter.id) {
            return false;
        }
        if (PlayerManager.Instance.player.allIntel.Count == 0) {
            return false;
        }
        if (UIManager.Instance.IsShareIntelMenuOpen()) {
            return false;
        }
        return base.ShouldButtonBeInteractable(character, targetCharacter);
    }
    public override bool CanTarget(Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (assignedCharacter == targetCharacter) {
            return false;
        }
        if (this.targetCharacter != null && targetCharacter.id == this.targetCharacter.id) {
            return false;
        }
        if (PlayerManager.Instance.player.allIntel.Count == 0) {
            return false;
        }
        return base.CanTarget(targetCharacter);
    }
    protected override void OnCharacterDied(Character characterThatDied) {
        base.OnCharacterDied(characterThatDied);
        if (!this.isActive) {
            return; //if this action is no longer active, do not check if the character that died was the target
        }
        if (characterThatDied.id == targetCharacter.id) {
            DeactivateAction();
        }
    }
    private void SetTargetCharacter(Character targetCharacter) {
        this.targetCharacter = targetCharacter;
    }
}
