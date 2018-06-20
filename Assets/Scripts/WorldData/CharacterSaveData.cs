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
    public LOCATION_IDENTIFIER locationType;
    public int locationID;
    public int homeID;
    public int factionID;
    public PortraitSettings portraitSettings;
    public List<RelationshipSaveData> relationshipsData;

    public CharacterSaveData(ECS.Character character) {
        id = character.id;
        name = character.name;
        race = character.raceSetting.race;
        gender = character.gender;
        role = character.role.roleType;
        className = character.characterClass.className;

        if (character.specificLocation != null) {
            locationType = character.specificLocation.locIdentifier;
            locationID = character.specificLocation.id;
        } else {
            locationID = -1;
        }

        if (character.home != null) {
            homeID = character.home.id;
        } else {
            homeID = -1;
        }

        if (character.faction != null) {
            factionID = character.faction.id;
        } else {
            factionID = -1;
        }
        
        portraitSettings = character.portraitSettings;
        relationshipsData = new List<RelationshipSaveData>();
        foreach (KeyValuePair<ECS.Character, Relationship> kvp in character.relationships) {
            relationshipsData.Add(new RelationshipSaveData(kvp.Value));
        }
    }
}
