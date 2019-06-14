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
        _traitNames = new List<string>() { "Lycanthropy", "Kleptomaniac", "Vampiric", "Unfaithful", "Violent"}; //, "Unconscious", "Injured", "Sick", "Cursed", "Death", "Restrained" 
    }

    public override void ActivateAction(Character assignedCharacter, IPointOfInterest targetPOI) {
        if(targetPOI is Character) {
            _targetCharacter = targetPOI as Character;
        } else {
            return;
        }
        UIManager.Instance.ShowClickableObjectPicker(_traitNames, OnClickTrait, null, CanCorruptCharacter, "Corrupt " + _targetCharacter.name + " with an affliction.", OnHoverTrait);
    }

    protected override bool CanPerformActionTowards(Character character, Character targetCharacter) {
        if (targetCharacter.isDead || character.id == targetCharacter.id) { //|| (!targetCharacter.isTracked && !GameManager.Instance.inspectAll)
            return false;
        }
        //if (targetCharacter.role.roleType == CHARACTER_ROLE.BEAST || targetCharacter.race == RACE.SKELETON) {
        //    return false;
        //}
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
        if (targetCharacter.race != RACE.HUMANS && targetCharacter.race != RACE.ELVES) {
            return false;
        }
        return base.CanTarget(targetCharacter);
    }

    #region Trait Checkers
    private void OnClickTrait(string traitName) {
        base.ActivateAction(assignedCharacter, _targetCharacter);
        UIManager.Instance.HideObjectPicker();
        Trait newTrait = null;
        if (traitName == "Lycanthropy") {
            newTrait = new Lycanthropy();
            _targetCharacter.AddTrait(newTrait);
        }else if (traitName == "Unconscious") {
            newTrait = new Unconscious();
            _targetCharacter.AddTrait(newTrait);
        } else if (traitName == "Restrained") {
            newTrait = new Restrained();
            _targetCharacter.AddTrait(newTrait);
        } else if (traitName == "Cursed") {
            newTrait = new Cursed();
            _targetCharacter.AddTrait(newTrait);
        } else if (traitName == "Sick") {
            newTrait = new Sick();
            _targetCharacter.AddTrait(newTrait);
        } else if (traitName == "Injured") {
            newTrait = new Injured();
            _targetCharacter.AddTrait(newTrait);
        } else if (traitName == "Kleptomaniac") {
            newTrait = new Kleptomaniac();
            _targetCharacter.AddTrait(newTrait);
        } else if (traitName == "Unfaithful") {
            newTrait = new Unfaithful();
            _targetCharacter.AddTrait(newTrait);
        } else if (traitName == "Death") {
            _targetCharacter.Death();
        } else if (traitName == "Vampiric") {
            newTrait = new Vampiric();
            _targetCharacter.AddTrait(newTrait);
        }
        if (newTrait != null) {
            _targetCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "afflicted", null, newTrait.name);
        }
    }
    private bool CanCorruptCharacter(string traitName) {
        if (_targetCharacter.GetNormalTrait(traitName) != null) {
            return false;
        }
        if (traitName == "Violent") {
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
                tooltip = "Needs blood for sustenance.";
                break;
            case "Unfaithful":
                tooltip = "Prone to affairs.";
                break;
            default:
                break;
        }
        UIManager.Instance.ShowSmallInfo(tooltip, header);
    }
    #endregion
}
