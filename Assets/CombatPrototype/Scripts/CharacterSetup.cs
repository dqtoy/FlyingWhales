using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS {
    [System.Serializable]
    public class CharacterSetup {
        public string fileName;

		public string characterClassName;
		public string raceSettingName;
		public CHARACTER_ROLE optionalRole;
        public List<CHARACTER_TAG> tags;

		public List<ItemAndType> preEquippedItems;


        private CharacterClass _charClass;
		private RaceSetting _raceSetting;


        #region getters/setters
        public CharacterClass characterClass {
            get {
                if (_charClass == null) {
					_charClass = JsonUtility.FromJson<CharacterClass>(System.IO.File.ReadAllText(Application.dataPath + "/StreamingAssets/Data/CharacterClasses/" + characterClassName + ".json"));
                }
                return _charClass;
            }
        }
		public RaceSetting raceSetting {
            get {
                if (_raceSetting == null) {
					_raceSetting = JsonUtility.FromJson<RaceSetting>(System.IO.File.ReadAllText(Application.dataPath + "/StreamingAssets/Data/RaceSettings/" + raceSettingName + ".json"));
                }
                return _raceSetting;
            }
        }
        #endregion

    }
}