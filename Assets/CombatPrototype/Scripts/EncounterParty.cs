using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EncounterParty : MonoBehaviour {
	public TextAsset[] partyMembers;

	private List<ECS.CharacterSetup> _characterSetups;

	internal void Initialize(){
		_characterSetups = new List<ECS.CharacterSetup> ();
		ConstructAllCharacterSetups ();
	}
	private void ConstructAllCharacterSetups(){
		for (int i = 0; i < partyMembers.Length; i++) {
			ECS.CharacterSetup charSetup = ECS.CombatManager.Instance.GetBaseCharacterSetup(partyMembers[i].name);
			_characterSetups.Add(charSetup);
		}	
	}

	internal List<ECS.Character> GetAllCharacters(DungeonLandmark originLandmark = null){
		List<ECS.Character> characters = new List<ECS.Character> ();
		for (int i = 0; i < _characterSetups.Count; i++) {
			ECS.Character newCharacter = CharacterManager.Instance.CreateNewCharacter(_characterSetups[i].optionalRole, _characterSetups[i]);
			newCharacter.SetCharacterColor (Color.red);
			if(originLandmark != null){
                if (newCharacter.raceSetting.tags.Contains(CHARACTER_TAG.NESTING)) {
                    newCharacter.SetLair(originLandmark);
                }
				originLandmark.AddCharacterToLocation(newCharacter);
			}
			characters.Add (newCharacter);
		}
		return characters;
	}
}
