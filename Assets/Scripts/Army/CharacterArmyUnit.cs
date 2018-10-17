using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterArmyUnit : Character {

	public int armyCount { get; private set; }
    private int armyCap;

    protected new Army _ownParty;
    protected new Army _currentParty;

    #region getters/setters
    public override string name {
        get { return armyCount + " " + Utilities.GetNormalizedSingularRace(_raceSetting.race) + " " + characterClass.className; }
    }
    public override Party ownParty {
        get { return _ownParty; }
    }
    public override Party currentParty {
        get { return _currentParty; }
    }
    #endregion

    public CharacterArmyUnit(string className, RACE race, int armyCount) : base(className, race, GENDER.MALE) {
        this.armyCap = GetArmyCap();
        this.armyCount = armyCount;
    }

    #region Army Management
    public void AdjustArmyCount(int adjustment) {
        armyCount += adjustment;
        if (armyCap == -1) {
            armyCount = Mathf.Max(0, armyCount);
        } else {
            armyCount = Mathf.Clamp(armyCount, 0, armyCap);
        }
    }
    private int GetArmyCap() {
        if (this.characterClass.className.Equals("Knight")) {
            return 25;
        } else if (this.characterClass.className.Equals("Archer")) {
            return 20;
        } else if (this.characterClass.className.Contains("Mage")) {
            return 15;
        } else if (this.characterClass.className.Equals("Cleric")) {
            return 15;
        }
        return -1;
    }
    public bool isCapped() {
        if (armyCap == -1) {
            return false; //no cap
        } else {
            return armyCount >= armyCap;
        }
    }
    public int GetProductionCost() {
        if (this.characterClass.className.Equals("Knight")) {
            return 20;
        } else if (this.characterClass.className.Contains("Mage")) {
            return 30;
        } else if (this.characterClass.className.Equals("Archer")) {
            return 40;
        } else if (this.characterClass.className.Equals("Cleric")) {
            return 50;
        }
        return 0;
    }
    #endregion

    #region overrides
    /*
    Create a new Party with this character as the leader.
     */
    public override Party CreateOwnParty() {
        if (_ownParty != null) {
            _ownParty.RemoveCharacter(this);
        }
        Army newParty = new Army(this);
        SetOwnedParty(newParty);
        newParty.AddCharacter(this);
        //newParty.CreateCharacterObject();
        return newParty;
    }
    public override void SetOwnedParty(Party party) {
        _ownParty = party as Army;
    }
    public override void SetCurrentParty(Party party) {
        _currentParty = party as Army;
    }
    #endregion

}
