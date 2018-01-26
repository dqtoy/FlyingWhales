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
			this.combat = new CombatPrototype (null);
		}
		public void StartCombat(){
			this.combat.CombatSimulation ();
		}
		public void CombatResults(CombatPrototype combat){
			for (int i = 0; i < combat.deadCharacters.Count; i++) {
				combat.deadCharacters [i].Death ();
			}
		}
    }
}