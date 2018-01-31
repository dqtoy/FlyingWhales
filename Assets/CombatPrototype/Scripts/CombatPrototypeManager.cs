using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ECS {
    public class CombatPrototypeManager : MonoBehaviour {

        public static CombatPrototypeManager Instance = null;

        public CharacterSetup[] baseCharacters;
		public Color[] characterColors;
		public AttributeSkill[] attributeSkills;
		public Dictionary<WEAPON_TYPE, List<Skill>> weaponTypeSkills = new Dictionary<WEAPON_TYPE, List<Skill>> ();

		private List<Color> unusedColors = new List<Color>();
		private List<Color> usedColors = new List<Color>();
		public CombatPrototype combat;

        private void Awake() {
            Instance = this;
        }
		internal void Initialize(){
			ConstructBaseCharacters();
			ConstructCharacterColors ();
			ConstructAttributeSkills ();
			ConstructWeaponTypeSkills ();
			NewCombat ();
		}
        private void ConstructBaseCharacters() {
            string path = "Assets/CombatPrototype/Data/CharacterSetups/";
            string[] baseCharacterJsons = System.IO.Directory.GetFiles(path, "*.json");
            baseCharacters = new CharacterSetup[baseCharacterJsons.Length];
            for (int i = 0; i < baseCharacterJsons.Length; i++) {
                string file = baseCharacterJsons[i];
                string dataAsJson = System.IO.File.ReadAllText(file);
                CharacterSetup charSetup = JsonUtility.FromJson<CharacterSetup>(dataAsJson);
                baseCharacters[i] = charSetup;
            }
        }

        /*
         * Create a new character given a base character setup.
         * */
        internal ECS.Character CreateNewCharacter(CharacterSetup baseCharacter) {
            return new ECS.Character(baseCharacter);
        }

		private void ConstructCharacterColors(){
			unusedColors = characterColors.ToList (); 
		}

		internal Color UseRandomCharacterColor(){
			Color chosenColor = Color.black;
			if(unusedColors.Count > 0){
				chosenColor = unusedColors [UnityEngine.Random.Range (0, unusedColors.Count)];
				unusedColors.Remove (chosenColor);
				usedColors.Add (chosenColor);
			}else{
				chosenColor = characterColors [UnityEngine.Random.Range (0, characterColors.Length)];
			}
			return chosenColor;
		}

		internal void ReturnCharacterColorToPool(Color color){
			if(usedColors.Remove(color)){
				unusedColors.Add (color);
			}
		}

		private void ConstructAttributeSkills(){
			string path = "Assets/CombatPrototype/Data/AttributeSkills/";
			string[] attributeSkillsJson = System.IO.Directory.GetFiles(path, "*.json");
			attributeSkills = new AttributeSkill[attributeSkillsJson.Length];
			for (int i = 0; i < attributeSkillsJson.Length; i++) {
				string file = attributeSkillsJson[i];
				string dataAsJson = System.IO.File.ReadAllText(file);
				AttributeSkill attSkill = JsonUtility.FromJson<AttributeSkill>(dataAsJson);
				attSkill.ConstructAttributeSkillsList ();
				attributeSkills[i] = attSkill;
			}
		}

		private void ConstructWeaponTypeSkills(){
			string path = "Assets/CombatPrototype/Data/WeaponTypeSkills/";
			string[] weaponSkillsJson = System.IO.Directory.GetFiles(path, "*.json");
			for (int i = 0; i < weaponSkillsJson.Length; i++) {
				string file = weaponSkillsJson[i];
				string dataAsJson = System.IO.File.ReadAllText(file);
				WeaponSkill weapSkill = JsonUtility.FromJson<WeaponSkill>(dataAsJson);
				weapSkill.ConstructWeaponSkillsList ();
				weaponTypeSkills.Add (weapSkill.weaponType, new List<Skill> (weapSkill.skills));
			}
		}

		internal CharacterSetup GetBaseCharacterSetupBasedOnClass(string className){
			for (int i = 0; i < this.baseCharacters.Length; i++) {
                CharacterSetup currBase = this.baseCharacters[i];
                if (currBase.characterClassName.ToLower() == className.ToLower()){
					return currBase;
				}
			}
			return null;
		}

        internal CharacterSetup GetBaseCharacterSetup(string className, RACE race) {
            for (int i = 0; i < this.baseCharacters.Length; i++) {
                CharacterSetup currBase = this.baseCharacters[i];
                if (currBase.characterClassName.ToLower() == className.ToLower() && currBase.raceSetting.race == race) {
                    return currBase;
                }
            }
            return null;
        }

		internal CharacterSetup GetBaseCharacterSetup(string fileName) {
			for (int i = 0; i < this.baseCharacters.Length; i++) {
				CharacterSetup currBase = this.baseCharacters[i];
				if (currBase.fileName == fileName) {
					return currBase;
				}
			}
			return null;
		}

		internal void NewCombat(){
			this.combat = new CombatPrototype (null, null);
		}
		public void StartCombat(){
			this.combat.CombatSimulation ();
		}
		public void CombatResults(CombatPrototype combat){
			for (int i = 0; i < combat.deadCharacters.Count; i++) {
				combat.deadCharacters [i].Death ();
			}

			//Prisoner or Leave to Die
			List<ECS.Character> winningCharacters = null;
			int leaveToDieWeight = 100;
			if(combat.winningSide == SIDES.A){
				if(combat.charactersSideA[0].faction == null){
					leaveToDieWeight += 200;
				}
				winningCharacters = combat.charactersSideA;
			}else{
				if(combat.charactersSideB[0].faction == null){
					leaveToDieWeight += 200;
				}
				winningCharacters = combat.charactersSideB;
			}

			if(combat.faintedCharacters.Count > 0){
				WeightedDictionary<string> prisonWeights = new WeightedDictionary<string>();
				int prisonerWeight = 50;
				prisonWeights.AddElement ("prison", prisonerWeight);
				prisonWeights.AddElement ("leave", leaveToDieWeight);
				string pickedWeight = prisonWeights.PickRandomElementGivenWeights ();

				if(pickedWeight == "prison"){
					for (int i = 0; i < combat.faintedCharacters.Count; i++) {
						if(combat.faintedCharacters[i].currentSide != combat.winningSide){
							combat.faintedCharacters [i].Faint ();
							winningCharacters[0].AddPrisoner(combat.faintedCharacters[i]);
						}else{
							combat.faintedCharacters [i].SetHP (1);
						}
					}
				}else{
					for (int i = 0; i < combat.faintedCharacters.Count; i++) {
						if(combat.faintedCharacters[i].currentSide != combat.winningSide){
							combat.faintedCharacters [i].Death ();
						}else{
							combat.faintedCharacters [i].SetHP (1);
						}
					}
				}
			}

			//Check prisoners of defeated party or character
			if(combat.losingSide == SIDES.A && combat.sideAPrisoners != null){
				CheckDefeatedPartyPrisoners (winningCharacters, combat.sideAPrisoners);
			}else if(combat.losingSide == SIDES.B && combat.sideBPrisoners != null){
				CheckDefeatedPartyPrisoners (winningCharacters, combat.sideBPrisoners);
			}
		}

		private void CheckDefeatedPartyPrisoners(List<ECS.Character> winningCharacters, List<ECS.Character> prisoners){
			WeightedDictionary<string> weights = new WeightedDictionary<string> ();
			string pickedWeight = string.Empty;
			int takePrisonerWeight = 50;
			int releaseWeight = 100;
			int killWeight = 10;
			if(winningCharacters[0].party != null){
				if(winningCharacters[0].party.partyLeader.HasTrait(TRAIT.RUTHLESS)){
					killWeight += 500;
				}
				if(winningCharacters[0].party.partyLeader.HasTrait(TRAIT.BENEVOLENT)){
					releaseWeight += 500;
				}
				if(winningCharacters[0].party.partyLeader.HasTrait(TRAIT.PACIFIST)){
					killWeight -= 100;
					if(killWeight < 0){
						killWeight = 0;
					}
				}
			}else{
				if(winningCharacters[0].HasTrait(TRAIT.RUTHLESS)){
					killWeight += 500;
				}
				if(winningCharacters[0].HasTrait(TRAIT.BENEVOLENT)){
					releaseWeight += 500;
				}
				if(winningCharacters[0].HasTrait(TRAIT.PACIFIST)){
					killWeight -= 100;
					if(killWeight < 0){
						killWeight = 0;
					}
				}
			}

			weights.AddElement ("prisoner", takePrisonerWeight);
			weights.AddElement ("release", releaseWeight);

			while(prisoners.Count > 0) {
				if(prisoners[0].faction != null){
					if(winningCharacters[0].faction != null){
						if (prisoners [0].faction.id == winningCharacters [0].faction.id) {
							prisoners [0].ReleasePrisoner ();
							continue;
						} else {
							FactionRelationship fr = prisoners [0].faction.GetRelationshipWith (winningCharacters [0].faction);
							if(fr != null && fr.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE){
								killWeight += 200;
							}
						}
					}else{
						killWeight += 200;
					}
				}else{
					killWeight += 200;
				}

				if (winningCharacters [0].party != null){
					if (winningCharacters [0].party.partyLeader.raceSetting.race != prisoners [0].raceSetting.race && winningCharacters [0].party.partyLeader.HasTrait (TRAIT.RACIST)) {
						killWeight += 100;
					}
				}else{
					if (winningCharacters [0].raceSetting.race != prisoners [0].raceSetting.race && winningCharacters [0].HasTrait (TRAIT.RACIST)) {
						killWeight += 100;
					}
				}

				weights.ChangeElement ("kill", killWeight);
				pickedWeight = weights.PickRandomElementGivenWeights ();
				if(pickedWeight == "prisoner"){
					if (winningCharacters [0].party != null) {
						prisoners [0].TransferPrisoner (winningCharacters [0].party);
					}else{
						prisoners [0].TransferPrisoner (winningCharacters [0]);
					}
				}else if(pickedWeight == "kill"){
					prisoners [0].Death ();
				}else if(pickedWeight == "release"){
					prisoners [0].ReleasePrisoner ();
				}
			}
		}
    }
}