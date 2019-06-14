using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intervene : PlayerJobAction {
    private Character _targetCharacter;

    private List<string> _traitNames;

    public Intervene() {
        name = "Intervene";
        SetDefaultCooldownTime(24);
        targettableTypes = new List<JOB_ACTION_TARGET>() { JOB_ACTION_TARGET.CHARACTER };
        _traitNames = new List<string>() { "Zap", "Spook", "Jolt", "Enrage"}; //, "Fumble"
    }

    public override void ActivateAction(Character assignedCharacter, IPointOfInterest targetPOI) {
        if(targetPOI is Character) {
            _targetCharacter = targetPOI as Character;
        } else {
            return;
        }
        UIManager.Instance.ShowClickableObjectPicker(_traitNames, OnClickTrait, null, CanInterveneCharacter, "Intervene " + _targetCharacter.name + " with a status.", OnHoverTrait);
    }

    protected override bool CanPerformActionTowards(Character character, Character targetCharacter) {
        if (targetCharacter.isDead || character.id == targetCharacter.id) {
            return false;
        }
        return base.CanPerformActionTowards(character, targetCharacter);
    }
    public override bool CanTarget(IPointOfInterest targetPOI) {
        if(!(targetPOI is Character)) {
            return false;
        }
        Character targetCharacter = targetPOI as Character;
        if (targetCharacter.isDead) {
            return false;
        }
        //if (targetCharacter.race != RACE.HUMANS && targetCharacter.race != RACE.ELVES) {
        //    return false;
        //}
        return base.CanTarget(targetCharacter);
    }

    #region Trait Checkers
    private void OnClickTrait(string traitName) {
        base.ActivateAction(assignedCharacter, _targetCharacter);
        UIManager.Instance.HideObjectPicker();
        Trait newTrait = null;
        if (traitName == "Zap") {
            newTrait = new Zapped();
            _targetCharacter.AddTrait(newTrait);
        }else if (traitName == "Spook") {
            newTrait = new Spooked();
            _targetCharacter.AddTrait(newTrait);
        } else if (traitName == "Jolt") {
            newTrait = new Jolted();
            _targetCharacter.AddTrait(newTrait);
        } else if (traitName == "Enrage") {
            _targetCharacter.stateComponent.SwitchToState(CHARACTER_STATE.BERSERKED, null, null, GameManager.Instance.GetTicksBasedOnMinutes(30));
        } else if (traitName == "Fumble") {
            _targetCharacter.CancelAllJobsAndPlans();
        }
        if (UIManager.Instance.characterInfoUI.isShowing) {
            UIManager.Instance.characterInfoUI.UpdateThoughtBubble();
        }
    }
    private bool CanInterveneCharacter(string traitName) {
        if (_targetCharacter.GetNormalTrait(traitName) != null) {
            return false;
        }
        return true;
    }
    public void OnHoverTrait(string traitName) {
        string tooltip = string.Empty;
        string header = traitName;
        switch (traitName) {
            case "Zap":
                tooltip = string.Format("Temporarily prevents {0} from moving for 30 minutes.", _targetCharacter.name);
                break;
            case "Spook":
                tooltip = string.Format("Temporarily forces {0} to flee from all other nearby characters.", _targetCharacter.name);
                break;
            case "Jolt":
                tooltip = string.Format("Temporarily speeds up {0}'s movement.", _targetCharacter.name);
                break;
            case "Enrage":
                tooltip = string.Format("Temporarily enrages {0}.", _targetCharacter.name);
                break;
            case "Fumble":
                tooltip = string.Format("Forces {0} to drop current action and all other existing plans.", _targetCharacter.name);
                break;
            default:
                break;
        }
        UIManager.Instance.ShowSmallInfo(tooltip, header);
    }
    #endregion
}
