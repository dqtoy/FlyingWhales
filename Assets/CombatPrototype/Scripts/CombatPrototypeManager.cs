using UnityEngine;
using System.Collections;

namespace ECS {
    public class CombatPrototypeManager : MonoBehaviour {

        public static CombatPrototypeManager Instance = null;

        public CharacterSetup[] baseCharacters;

        private void Awake() {
            Instance = this;
            ConstructBaseCharacters();
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
    }
}