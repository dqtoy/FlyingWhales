using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EncounterParty : MonoBehaviour {
	public TextAsset[] partyMembers;

	private List<CharacterSetup> _characterSetups;

	//internal void Initialize(){
	//	_characterSetups = new List<CharacterSetup> ();
	//	//ConstructAllCharacterSetups ();
	//}
	//private void ConstructAllCharacterSetups(){
	//	for (int i = 0; i < partyMembers.Length; i++) {
	//		CharacterSetup charSetup = CombatManager.Instance.GetBaseCharacterSetup(partyMembers[i].name);
	//		_characterSetups.Add(charSetup);
	//	}	
	//}

	//internal List<Character> GetAllCharacters(DungeonLandmark originLandmark = null){
	//	List<Character> characters = new List<Character> ();
	//	for (int i = 0; i < _characterSetups.Count; i++) {
	//		Character newCharacter = CharacterManager.Instance.CreateNewCharacter(_characterSetups[i].optionalRole, Utilities.GetRandomGender(), _characterSetups[i]);
	//		newCharacter.SetCharacterColor (Color.red);
	//		if(originLandmark != null){
 //               if (newCharacter.raceSetting.tags.Contains(ATTRIBUTE.NESTING)) {
 //                   newCharacter.SetLair(originLandmark);
 //               }
	//			originLandmark.AddCharacterToLocation(newCharacter);
	//		}
	//		characters.Add (newCharacter);
	//	}
	//	return characters;
	//}
}
