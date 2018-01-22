using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EncounterParty : MonoBehaviour {
	public TextAsset[] partyMembers;

	private List<ECS.CharacterSetup> _characterSetups;

	void Start(){
		_characterSetups = new List<ECS.CharacterSetup> ();
	}

	private void ConstructAllCharacters(){
		string path = "Assets/CombatPrototype/Data/CharacterSetups/";
		for (int i = 0; i < partyMembers.Length; i++) {
			string file = path + partyMembers[i].name + ".json";
			string dataAsJson = System.IO.File.ReadAllText(file);
			ECS.CharacterSetup charSetup = JsonUtility.FromJson<ECS.CharacterSetup>(dataAsJson);
			_characterSetups.Add(charSetup);
		}	
	}

	internal List<ECS.CharacterSetup> GetAllCharacters(){
		return new List<ECS.CharacterSetup> (_characterSetups);
	}
}
