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
        internal List<ICharacter> charactersSideA;
        internal List<ICharacter> charactersSideB;
        private Action afterCombatAction;

        internal SIDES winningSide;

        internal List<string> resultsLog;

        public Combat(Party party1, Party party2) {
            this.charactersSideA = new List<ICharacter>(party1.icharacters);
            this.charactersSideB = new List<ICharacter>(party2.icharacters);
            for (int i = 0; i < charactersSideA.Count; i++) {
                charactersSideA[i].SetSide(SIDES.A); //also puts the current stat variables to combat variables
            }
            for (int i = 0; i < charactersSideB.Count; i++) {
                charactersSideB[i].SetSide(SIDES.B); //also puts the current stat variables to combat variables
            }
            this.resultsLog = new List<string>();
        }
        public Combat(List<ICharacter> icharacters1, List<ICharacter> icharacters2, Action afterCombatAction = null) {
            this.charactersSideA = new List<ICharacter>(icharacters1);
            this.charactersSideB = new List<ICharacter>(icharacters2);
            for (int i = 0; i < charactersSideA.Count; i++) {
                charactersSideA[i].SetSide(SIDES.A); //also puts the current stat variables to combat variables
            }
            for (int i = 0; i < charactersSideB.Count; i++) {
                charactersSideB[i].SetSide(SIDES.B); //also puts the current stat variables to combat variables
            }
            this.afterCombatAction = afterCombatAction;
            this.resultsLog = new List<string>();
        }

        public void Fight(Action afterCombatAction = null) {
            AssignAfterCombatAction(afterCombatAction);

            int sideAWeight = 0;
            int sideBWeight = 0;
            CombatManager.Instance.GetCombatWeightsOfTwoLists(charactersSideA, charactersSideB, out sideAWeight, out sideBWeight);
            int totalWeight = sideAWeight + sideBWeight;

            float sideAChance = (sideAWeight / (float)totalWeight) * 100f;
            float sideBChance = (sideBWeight / (float)totalWeight) * 100f;

            int chance = UnityEngine.Random.Range(0, 101);
            if(chance < sideAChance) {
                winningSide = SIDES.A;
                for (int i = 0; i < charactersSideB.Count; i++) {
                    charactersSideB[i].Death();
                }
            } else {
                winningSide = SIDES.B;
                for (int i = 0; i < charactersSideA.Count; i++) {
                    charactersSideA[i].Death();
                }
            }
            if(afterCombatAction != null) {
                afterCombatAction();
            }
            //for (int i = 0; i < afterCombatActions.Count; i++) {
            //    afterCombatActions[i]();
            //}
        }
        public void Fight(float sideAChance, float sideBChance, Action afterCombatAction = null) {
            AssignAfterCombatAction(afterCombatAction);

            int chance = UnityEngine.Random.Range(0, 101);
            if (chance < sideAChance) {
                winningSide = SIDES.A;
                for (int i = 0; i < charactersSideB.Count; i++) {
                    charactersSideB[i].Death();
                }
            } else {
                winningSide = SIDES.B;
                for (int i = 0; i < charactersSideA.Count; i++) {
                    charactersSideA[i].Death();
                }
            }
            if (afterCombatAction != null) {
                afterCombatAction();
            }
            //for (int i = 0; i < afterCombatActions.Count; i++) {
            //    afterCombatActions[i]();
            //}
        }

        private List<ICharacter> GetEnemyCharacters(SIDES side) {
            if(side == SIDES.A) {
                return charactersSideB;
            }
            return charactersSideA;
        }
        private List<ICharacter> GetAllyCharacters(SIDES side) {
            if (side == SIDES.A) {
                return charactersSideA;
            }
            return charactersSideB;
        }

        //public void AddAfterCombatAction(Action action) {
        //    afterCombatActions.Add(action);
        //}

        public void AssignAfterCombatAction(Action action) {
            afterCombatAction = action;
        }
    }
}

