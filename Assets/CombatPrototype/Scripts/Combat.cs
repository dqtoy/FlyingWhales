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
        internal List<Action> afterCombatActions;

        internal SIDES winningSide;

        internal List<string> resultsLog;

        public Combat(Party party1, Party party2) {
            this.charactersSideA = party1.icharacters;
            this.charactersSideB = party2.icharacters;
            for (int i = 0; i < party1.icharacters.Count; i++) {
                party1.icharacters[i].SetSide(SIDES.A); //also puts the current stat variables to combat variables
            }
            for (int i = 0; i < party2.icharacters.Count; i++) {
                party2.icharacters[i].SetSide(SIDES.B); //also puts the current stat variables to combat variables
            }
            this.afterCombatActions = new List<Action>();
            this.resultsLog = new List<string>();
        }

        public void Fight() {
            int sideAWeight = 0;
            int sideBWeight = 0;
            CombatManager.Instance.GetCombatWeightsOfTwoLists(charactersSideA, charactersSideB, out sideAWeight, out sideBWeight);
            int totalWeight = sideAWeight + sideBWeight;

            float sideAChance = sideAWeight / (float)totalWeight;
            float sideBChance = sideBWeight / (float)totalWeight;

            int chance = UnityEngine.Random.Range(0, 101);
            if(chance < sideAChance) {

            } else {

            }
        }
        public void Fight(float sideAChance, float sideBChance) {
            int chance = UnityEngine.Random.Range(0, 101);
            if (chance < sideAChance) {

            } else {

            }
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

        public void AddAfterCombatAction(Action action) {
            afterCombatActions.Add(action);
        }
    }
}

