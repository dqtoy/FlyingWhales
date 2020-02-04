using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public enum SIDES{
	A,
	B,
}

public class Combat {
    private List<Character> _charactersSideA, _charactersSideB;
    private SIDES _winningSide, _losingSide;
    private Settlement _location;
    //private Action afterCombatAction;
    //private List<string> resultsLog;

    #region getters/setters
    public List<Character> charactersSideA {
        get { return _charactersSideA; }
    }
    public List<Character> charactersSideB {
        get { return _charactersSideB; }
    }
    public SIDES winningSide {
        get { return _winningSide; }
    }
    public SIDES losingSide {
        get { return _losingSide; }
    }
    public Settlement location {
        get { return _location; }
    }
    #endregion
    public Combat(Party party1, Party party2, Settlement location) {
        if(party1 == null) {
            _charactersSideA = null;
        } else {
            //this._charactersSideA = new List<Character>(party1.characters);
            //for (int i = 0; i < _charactersSideA.Count; i++) {
            //    _charactersSideA[i].SetSide(SIDES.A); //also puts the current stat variables to combat variables
            //}
        }
        if(party2 == null) {
            _charactersSideB = null;
        } else {
            //this._charactersSideB = new List<Character>(party2.characters);
            //for (int i = 0; i < _charactersSideB.Count; i++) {
            //    _charactersSideB[i].SetSide(SIDES.B); //also puts the current stat variables to combat variables
            //}
        }
        _location = location;
        //this.resultsLog = new List<string>();
    }
    public Combat(List<Character> icharacters1, List<Character> icharacters2, Settlement location) {
        if(icharacters1 == null) {
            this._charactersSideA = null;
        } else {
            this._charactersSideA = new List<Character>(icharacters1);
            for (int i = 0; i < _charactersSideA.Count; i++) {
                _charactersSideA[i].SetSide(SIDES.A); //also puts the current stat variables to combat variables
            }
        }
        if(icharacters2 == null) {
            this._charactersSideB = null;
        } else {
            this._charactersSideB = new List<Character>(icharacters2);
            for (int i = 0; i < _charactersSideB.Count; i++) {
                _charactersSideB[i].SetSide(SIDES.B); //also puts the current stat variables to combat variables
            }
        }
        _location = location;
        //this.resultsLog = new List<string>();
    }

    public void Fight(Action afterCombatAction = null) {
        //AssignAfterCombatAction(afterCombatAction);

        int sideAWeight = 0;
        int sideBWeight = 0;
        CombatManager.Instance.GetCombatWeightsOfTwoLists(_charactersSideA, _charactersSideB, out sideAWeight, out sideBWeight);
        int totalWeight = sideAWeight + sideBWeight;

        float sideAChance = (sideAWeight / (float)totalWeight) * 100f;
        //float sideBChance = (sideBWeight / (float)totalWeight) * 100f;

        int chance = UnityEngine.Random.Range(0, 101);
        if(chance < sideAChance) {
            _winningSide = SIDES.A;
            _losingSide = SIDES.B;

            //Winners level up
            if (_charactersSideA != null) {
                for (int i = 0; i < _charactersSideA.Count; i++) {
                    _charactersSideA[i].LevelUp();
                }
            }
            //Losers die
            if (_charactersSideB != null) {
                for (int i = 0; i < _charactersSideB.Count; i++) {
                    _charactersSideB[i].Death();
                }
            }
        } else {
            _winningSide = SIDES.B;
            _losingSide = SIDES.A;

            //Winners level up
            if (_charactersSideB != null) {
                for (int i = 0; i < _charactersSideB.Count; i++) {
                    _charactersSideB[i].LevelUp();
                }
            }
            //Losers die
            if (_charactersSideA != null) {
                for (int i = 0; i < _charactersSideA.Count; i++) {
                    _charactersSideA[i].Death();
                }
            }
        }
        afterCombatAction?.Invoke();
        Messenger.Broadcast(Signals.COMBAT_DONE, this);
    }
    public void Fight(float sideAChance, float sideBChance, Action afterCombatAction = null) {
        //AssignAfterCombatAction(afterCombatAction);

        int chance = UnityEngine.Random.Range(0, 101);
        if (chance < sideAChance) {
            _winningSide = SIDES.A;
            _losingSide = SIDES.B;

            //Winners level up
            if (_charactersSideA != null) {
                for (int i = 0; i < _charactersSideA.Count; i++) {
                    _charactersSideA[i].LevelUp();
                }
            }
            //Losers die
            if (_charactersSideB != null) {
                for (int i = 0; i < _charactersSideB.Count; i++) {
                    _charactersSideB[i].Death();
                }
            }
        } else {
            _winningSide = SIDES.B;
            _losingSide = SIDES.A;

            //Winners level up
            if (_charactersSideB != null) {
                for (int i = 0; i < _charactersSideB.Count; i++) {
                    _charactersSideB[i].LevelUp();
                }
            }
            //Losers die
            if (_charactersSideA != null) {
                for (int i = 0; i < _charactersSideA.Count; i++) {
                    _charactersSideA[i].Death();
                }
            }
        }
        afterCombatAction?.Invoke();
        Messenger.Broadcast(Signals.COMBAT_DONE, this);
    }

    private List<Character> GetEnemyCharacters(SIDES side) {
        if(side == SIDES.A) {
            return _charactersSideB;
        }
        return _charactersSideA;
    }
    private List<Character> GetAllyCharacters(SIDES side) {
        if (side == SIDES.A) {
            return _charactersSideA;
        }
        return _charactersSideB;
    }
    //public void AssignAfterCombatAction(Action action) {
    //    afterCombatAction = action;
    //}
}