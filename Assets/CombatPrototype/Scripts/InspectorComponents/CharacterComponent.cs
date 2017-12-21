using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS {
	public class CharacterComponent : MonoBehaviour {
		public string fileName;

		public string characterClassName;
		public string raceSettingName;

		public int currCharacterSelectedIndex;
		public int currRaceSelectedIndex;

		public List<string> raceChoices;
		public List<string> characterClassChoices;
	}
}
