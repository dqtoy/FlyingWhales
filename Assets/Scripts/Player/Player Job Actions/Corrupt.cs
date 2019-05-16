using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corrupt : PlayerJobAction {
    private Character _targetCharacter;

    private List<string> _traitNames;

    public Corrupt() {
        name = "Corrupt";
        SetDefaultCooldownTime(24);
        targettableTypes = new List<JOB_ACTION_TARGET>() { JOB_ACTION_TARGET.CHARACTER };
        //"Unconscious", "Restrained", "Cursed", "Sick", "Injured"
        _traitNames = new List<string>() { "Lycanthropy", "Kleptomaniac", "Violent", "Vampiric", "Unfaithful"}; //, "Unconscious", "Injured", "Sick", "Cursed", "Death"
    }

    public override void ActivateAction(Character assignedCharacter, Character targetCharacter) {
        _targetCharacter = targetCharacter;
        UIManager.Instance.ShowClickableObjectPicker(_traitNames, OnClickTrait, null, CanCorruptCharacter, "Corrupt " + _targetCharacter.name + " with an affliction.", OnHoverTrait);
    }

    protected override bool ShouldButtonBeInteractable(Character character, Character targetCharacter) {
        if (targetCharacter.isDead || character.id == targetCharacter.id) { //|| (!targetCharacter.isTracked && !GameManager.Instance.inspectAll)
            return false;
        }
        //if (targetCharacter.role.roleType == CHARACTER_ROLE.BEAST || targetCharacter.race == RACE.SKELETON) {
        //    return false;
        //}
        return base.ShouldButtonBeInteractable(character, targetCharacter);
    }
    public override bool CanTarget(Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (targetCharacter.race != RACE.HUMANS && targetCharacter.race != RACE.ELVES) {
            return false;
        }
        return base.CanTarget(targetCharacter);
    }

    #region Trait Checkers
    private void OnClickTrait(string traitName) {
        base.ActivateAction(assignedCharacter, _targetCharacter);
        UIManager.Instance.HideObjectPicker();
        if (traitName == "Lycanthropy") {
            Lycanthropy newTrait = new Lycanthropy();
            _targetCharacter.AddTrait(newTrait);
        }else if (traitName == "Unconscious") {
            Unconscious newTrait = new Unconscious();
            _targetCharacter.AddTrait(newTrait);
        } else if (traitName == "Restrained") {
            Restrained newTrait = new Restrained();
            _targetCharacter.AddTrait(newTrait);
        } else if (traitName == "Cursed") {
            Cursed newTrait = new Cursed();
            _targetCharacter.AddTrait(newTrait);
        } else if (traitName == "Sick") {
            Sick newTrait = new Sick();
            _targetCharacter.AddTrait(newTrait);
        } else if (traitName == "Injured") {
            Injured newTrait = new Injured();
            _targetCharacter.AddTrait(newTrait);
        } else if (traitName == "Kleptomaniac") {
            Kleptomaniac newTrait = new Kleptomaniac();
            _targetCharacter.AddTrait(newTrait);
        } else if (traitName == "Unfaithful") {
            Unfaithful newTrait = new Unfaithful();
            _targetCharacter.AddTrait(newTrait);
        } else if (traitName == "Death") {
            _targetCharacter.Death();
        }
    }
    private bool CanCorruptCharacter(string traitName) {
        if (_targetCharacter.GetTrait(traitName) != null) {
            return false;
        }
        if (traitName == "Violent" || traitName == "Vampiric") {
            return false; //disable these for now.
        }
        if(traitName == "Lycanthropy" && _targetCharacter.race == RACE.WOLF) {
            return false;
        }
        return true;
    }
    public void OnHoverTrait(string traitName) {
        string tooltip = string.Empty;
        string header = traitName;
        switch (traitName) {
            case "Lycanthropy":
                tooltip = "Will sometimes transform into a wild wolf whenever it sleeps.";
                break;
            case "Kleptomaniac":
                tooltip = "Enjoys stealing other people's items.";
                break;
            case "Violent":
                tooltip = "Prone to bouts of violence. Not yet available for this prototype.";
                break;
            case "Vampiric":
                tooltip = "Needs blood for sustenance. Not yet available for this prototype.";
                break;
            case "Unfaithful":
                tooltip = "Prone to affairs. Not yet available for this prototype.";
                break;
            default:
                break;
        }
        UIManager.Instance.ShowSmallInfo(tooltip, header);
    }
    #endregion
}
