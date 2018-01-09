using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ECS {
    public class CombatPrototypeManager : MonoBehaviour {

        public static CombatPrototypeManager Instance = null;

        public CharacterSetup[] baseCharacters;
		public Color[] characterColors;

		private List<Color> unusedColors = new List<Color>();
		private List<Color> usedColors = new List<Color>();

        private void Awake() {
            Instance = this;
            ConstructBaseCharacters();
			ConstructCharacterColors ();
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
        internal Character CreateNewCharacter(CharacterSetup baseCharacter) {
            return new Character(baseCharacter);
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
    }
}