using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ECS{
	public enum SIDES{
		A,
		B,
	}

    public class Combat {
        private List<ICharacter> _charactersSideA, _charactersSideB;
        private SIDES _winningSide, _losingSide;
        private BaseLandmark _location;
        private Action afterCombatAction;
        private List<string> resultsLog;

        #region getters/setters
        public List<ICharacter> charactersSideA {
            get { return _charactersSideA; }
        }
        public List<ICharacter> charactersSideB {
            get { return _charactersSideB; }
        }
        public SIDES winningSide {
            get { return _winningSide; }
        }
        public SIDES losingSide {
            get { return _losingSide; }
        }
        public BaseLandmark location {
            get { return _location; }
        }
        #endregion
        public Combat(Party party1, Party party2, BaseLandmark location) {
            if(party1 == null) {
                _charactersSideA = null;
            } else {
                this._charactersSideA = new List<ICharacter>(party1.icharacters);
                for (int i = 0; i < _charactersSideA.Count; i++) {
                    _charactersSideA[i].SetSide(SIDES.A); //also puts the current stat variables to combat variables
                }
            }
            if(party2 == null) {
                _charactersSideB = null;
            } else {
                this._charactersSideB = new List<ICharacter>(party2.icharacters);
                for (int i = 0; i < _charactersSideB.Count; i++) {
                    _charactersSideB[i].SetSide(SIDES.B); //also puts the current stat variables to combat variables
                }
            }
            _location = location;
            this.resultsLog = new List<string>();
        }
        public Combat(List<ICharacter> icharacters1, List<ICharacter> icharacters2, BaseLandmark location) {
            if(icharacters1 == null) {
                this._charactersSideA = null;
            } else {
                this._charactersSideA = new List<ICharacter>(icharacters1);
                for (int i = 0; i < _charactersSideA.Count; i++) {
                    _charactersSideA[i].SetSide(SIDES.A); //also puts the current stat variables to combat variables
                }
            }
            if(icharacters2 == null) {
                this._charactersSideB = null;
            } else {
                this._charactersSideB = new List<ICharacter>(icharacters2);
                for (int i = 0; i < _charactersSideB.Count; i++) {
                    _charactersSideB[i].SetSide(SIDES.B); //also puts the current stat variables to combat variables
                }
            }
            _location = location;
            this.resultsLog = new List<string>();
        }

        public void Fight(Action afterCombatAction = null) {
            AssignAfterCombatAction(afterCombatAction);

            int sideAWeight = 0;
            int sideBWeight = 0;
            CombatManager.Instance.GetCombatWeightsOfTwoLists(_charactersSideA, _charactersSideB, out sideAWeight, out sideBWeight);
            int totalWeight = sideAWeight + sideBWeight;

            float sideAChance = (sideAWeight / (float)totalWeight) * 100f;
            float sideBChance = (sideBWeight / (float)totalWeight) * 100f;

            int chance = UnityEngine.Random.Range(0, 101);
            if(chance < sideAChance) {
                _winningSide = SIDES.A;
                _losingSide = SIDES.B;
                if(_charactersSideB != null) {
                    for (int i = 0; i < _charactersSideB.Count; i++) {
                        _charactersSideB[i].Death();
                    }
                }
            } else {
                _winningSide = SIDES.B;
                _losingSide = SIDES.A;
                if (_charactersSideA != null) {
                    for (int i = 0; i < _charactersSideA.Count; i++) {
                        _charactersSideA[i].Death();
                    }
                }
            }
            if(afterCombatAction != null) {
                afterCombatAction();
            }
            Messenger.Broadcast(Signals.COMBAT_DONE, this);
        }
        public void Fight(float sideAChance, float sideBChance, Action afterCombatAction = null) {
            AssignAfterCombatAction(afterCombatAction);

            int chance = UnityEngine.Random.Range(0, 101);
            if (chance < sideAChance) {
                _winningSide = SIDES.A;
                _losingSide = SIDES.B;
                if (_charactersSideB != null) {
                    for (int i = 0; i < _charactersSideB.Count; i++) {
                        _charactersSideB[i].Death();
                    }
                }
            } else {
                _winningSide = SIDES.B;
                _losingSide = SIDES.A;
                if (_charactersSideA != null) {
                    for (int i = 0; i < _charactersSideA.Count; i++) {
                        _charactersSideA[i].Death();
                    }
                }
            }
            if (afterCombatAction != null) {
                afterCombatAction();
            }
            Messenger.Broadcast(Signals.COMBAT_DONE, this);
        }

        private List<ICharacter> GetEnemyCharacters(SIDES side) {
            if(side == SIDES.A) {
                return _charactersSideB;
            }
            return _charactersSideA;
        }
        private List<ICharacter> GetAllyCharacters(SIDES side) {
            if (side == SIDES.A) {
                return _charactersSideA;
            }
            return _charactersSideB;
        }
        public void AssignAfterCombatAction(Action action) {
            afterCombatAction = action;
        }
    }
}

