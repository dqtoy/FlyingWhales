
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterArmyUnit : Character {

	public int armyCount { get; private set; }
    private int armyCap;

    //protected new Army _ownParty;
    //protected new Army _currentParty;

    #region getters/setters
    public override string name {
        get { return armyCount + " " + Utilities.GetNormalizedSingularRace(_raceSetting.race) + " " + characterClass.className; }
    }
    //public override int attackPower {
    //    get { return _attackPower * armyCount; }
    //}
    //public override int speed {
    //    get { return _speed * armyCount; }
    //}
    //public override int maxHP {
    //    get { return _maxHP * armyCount; }
    //}
    //public override Party ownParty {
    //    get { return _ownParty; }
    //}
    //public override Party currentParty {
    //    get { return _currentParty; }
    //}
    public new Party party {
        get { return base.party; }
    }
    #endregion

    public CharacterArmyUnit(string className, RACE race) : base(className, race, GENDER.MALE) {
        this.armyCap = GetArmyCap();
        this.armyCount = 0; // _characterClass.armyCount;
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

    #region Overrides
    public void AdjustHP(int amount, Character killer = null) {
        int previous = this._currentHP;
        this._currentHP += amount;
        this._currentHP = Mathf.Clamp(this._currentHP, 0, hp);
        int diff = hp - _currentHP;
        if(diff > 0) {
            int armyLoss = diff / _maxHP;
            AdjustArmyCount(-armyLoss);
        }
        if (previous != this._currentHP) {
            if (this._currentHP == 0) {
                Death();
            }
        }
    }
    #endregion

}

public interface IUnit {
    string name { get; }
}
