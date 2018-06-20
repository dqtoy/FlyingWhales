using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterSaveData {
    public int id;
    public string name;
    public RACE race;
    public GENDER gender;
    public CHARACTER_ROLE role;
    public string className;
    public PortraitSettings portraitSettings;
    public List<RelationshipSaveData> relationshipsData;

    public CharacterSaveData(ECS.Character character) {
        id = character.id;
        name = character.name;
        race = character.raceSetting.race;
        gender = character.gender;
        role = character.role.roleType;
        className = character.characterClass.className;
        portraitSettings = character.portraitSettings;
        relationshipsData = new List<RelationshipSaveData>();
        foreach (KeyValuePair<ECS.Character, Relationship> kvp in character.relationships) {
            relationshipsData.Add(new RelationshipSaveData(kvp.Value));
        }
    }
}
