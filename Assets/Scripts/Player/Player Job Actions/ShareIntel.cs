using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class ShareIntel : PlayerSpell {
    //TODO: REDO SHARE INTEL TO BE A PLAYER ABILITY!
    public Character targetCharacter { get; private set; }

    public ShareIntel() : base(SPELL_TYPE.ABDUCT) {
        //description = "The Diplomat will reach out to a character and share a piece of information with them.";
        SetDefaultCooldownTime(24);
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    public override void ActivateAction(IPointOfInterest targetPOI) {
        //base.ActivateAction(assignedCharacter, targetCharacter);
        if (!(targetPOI is Character)) {
            return;
        }
        Character targetCharacter = targetPOI as Character;
        UIManager.Instance.OpenShareIntelMenu(targetCharacter);
        
        //PlayerUI.Instance.SetIntelMenuState(true);
        //PlayerUI.Instance.SetIntelItemClickActions(targetCharacter.ShareIntel);
        //PlayerUI.Instance.AddIntelItemOtherClickActions(() => base.ActivateAction(assignedCharacter, targetCharacter));
        //PlayerUI.Instance.AddIntelItemOtherClickActions(() => SetTargetCharacter(targetCharacter));
        //PlayerUI.Instance.AddIntelItemOtherClickActions(() => SetSubText(string.Empty));
        //PlayerUI.Instance.AddIntelItemOtherClickActions(() => PlayerUI.Instance.SetIntelItemClickActions(null));
    }
    public void BaseActivate(Character targetCharacter) {
        base.ActivateAction(targetCharacter);
        SetTargetCharacter(targetCharacter);
    }
    public override void DeactivateAction() {
        isActive = false;
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.RemoveListener<JOB, Character>(Signals.MINION_UNASSIGNED_FROM_JOB, OnCharacterUnassignedFromJob);
        targetCharacter = null;
    }
    protected override bool CanPerformActionTowards(Character targetCharacter) {
        if (targetCharacter.race != RACE.HUMANS && targetCharacter.race != RACE.ELVES) {
            return false;
        }
        if (targetCharacter.isDead) {
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
        if (targetCharacter.traitContainer.GetNormalTrait<Trait>("Unconscious", "Resting") != null) {
            return false;
        }
        return base.CanPerformActionTowards(targetCharacter);
    }
    public override bool CanTarget(IPointOfInterest targetPOI, ref string hoverText) {
        if (!(targetPOI is Character)) {
            return false;
        }
        Character targetCharacter = targetPOI as Character;
        if(targetCharacter.race != RACE.HUMANS && targetCharacter.race != RACE.ELVES) {
            return false;
        }
        if (targetCharacter.isDead) {
            return false;
        }
        if (this.targetCharacter != null && targetCharacter.id == this.targetCharacter.id) {
            return false;
        }
        if (PlayerManager.Instance.player.allIntel.Count == 0) {
            return false;
        }
        if(targetCharacter.traitContainer.GetNormalTrait<Trait>("Unconscious", "Resting") != null) {
            return false;
        }
        return base.CanTarget(targetCharacter, ref hoverText);
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
