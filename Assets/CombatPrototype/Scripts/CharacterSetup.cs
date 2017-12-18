using UnityEngine;
using System.Collections;

namespace ECS {
    [System.Serializable]
    public class CharacterSetup {
        public string fileName;
        public string characterClassName;
        public string raceSettingName;

        private CharacterClass _charClass;
		private RaceComponent _raceSetting;

        #region getters/setters
        public CharacterClass characterClass {
            get {
                if (_charClass == null) {
                    _charClass = JsonUtility.FromJson<CharacterClass>(System.IO.File.ReadAllText("Assets/CombatPrototype/Data/CharacterClasses/" + characterClassName + ".json"));
                }
                return _charClass;
            }
        }
		public RaceComponent raceSetting {
            get {
                if (_raceSetting == null) {
					_raceSetting = JsonUtility.FromJson<RaceComponent>(System.IO.File.ReadAllText("Assets/CombatPrototype/Data/RaceSettings/" + raceSettingName + ".json"));
                }
                return _raceSetting;
            }
        }
        #endregion

    }
}