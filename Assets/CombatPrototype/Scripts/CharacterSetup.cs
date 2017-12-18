using UnityEngine;
using System.Collections;

namespace ECS {
    [System.Serializable]
    public class CharacterSetup {
        public string fileName;

		public TextAsset characterClassJson;
		public TextAsset raceSetupJson;

        private CharacterClass _charClass;
		private RaceSetting _raceSetting;


        #region getters/setters
        public CharacterClass characterClass {
            get {
                if (_charClass == null) {
					_charClass = JsonUtility.FromJson<CharacterClass>(characterClassJson.text);
                }
                return _charClass;
            }
        }
		public RaceSetting raceSetting {
            get {
                if (_raceSetting == null) {
					_raceSetting = JsonUtility.FromJson<RaceSetting>(raceSetupJson.text);
                }
                return _raceSetting;
            }
        }
        #endregion

    }
}