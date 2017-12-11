using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public enum SIDES{
		A,
		B,
	}

	public class CombatPrototype : MonoBehaviour {
		public CombatPrototype Instance;

		public Dictionary<SIDES, List<Character>> allCharactersAndSides;

		void Awake(){
			Instance = this;
		}

		void Start(){
			this.allCharactersAndSides = new Dictionary<SIDES, List<Character>> ();
		}

		//Add a character to a side
		internal void AddCharacter(SIDES side, Character character){
			if(this.allCharactersAndSides.ContainsKey(side)){
				this.allCharactersAndSides [side].Add (character);
			}else{
				this.allCharactersAndSides.Add (side, new List<Character>(){character});
			}
		}

		//Remove a character from a side
		internal void RemoveCharacter(SIDES side, Character character){
			if(this.allCharactersAndSides.ContainsKey(side)){
				this.allCharactersAndSides [side].Remove (character);
			}
		}

		//This simulates the whole combat system
		internal void CombatSimulation(){

		}
	}
}

