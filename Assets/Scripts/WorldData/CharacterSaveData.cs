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
    public int homeLandmarkID;
    public int factionID;
    public PortraitSettings portraitSettings;
    public List<string> equipmentData;
    public List<string> inventoryData;
    public List<RelationshipSaveData> relationshipsData;
    public List<ATTRIBUTE> attributes;
    public HIDDEN_DESIRE hiddenDesire;
    public List<int> secrets;
    //public List<IntelReaction> intelReactions;
    public MORALITY morality;

    public CharacterSaveData(ECS.Character character) {
        id = character.id;
        name = character.name;
        race = character.raceSetting.race;
        gender = character.gender;
        role = character.role.roleType;
        className = character.characterClass.className;

        if (character.party.specificLocation != null) {
            locationType = character.party.specificLocation.locIdentifier;
            locationID = character.party.specificLocation.id;
        } else {
            locationID = -1;
        }
        homeID = -1;
        //if (character.home != null) {
        //    homeID = character.home.id;
        //} else {
            
        //}

        if (character.homeLandmark != null) {
            homeLandmarkID = character.homeLandmark.id;
        } else {
            homeLandmarkID = -1;
        }

        if (character.faction != null) {
            factionID = character.faction.id;
        } else {
            factionID = -1;
        }
        
        portraitSettings = character.portraitSettings;

        //equipmentData = new List<string>();
        //for (int i = 0; i < character.equippedItems.Count; i++) {
        //    ECS.Item item = character.equippedItems[i];
        //    equipmentData.Add(item.itemName);
        //}

        inventoryData = new List<string>();
        for (int i = 0; i < character.inventory.Count; i++) {
            ECS.Item item = character.inventory[i];
            inventoryData.Add(item.itemName);
        }

        relationshipsData = new List<RelationshipSaveData>();
        foreach (KeyValuePair<ECS.Character, Relationship> kvp in character.relationships) {
            relationshipsData.Add(new RelationshipSaveData(kvp.Value));
        }

        attributes = new List<ATTRIBUTE>();
        for (int i = 0; i < character.attributes.Count; i++) {
            attributes.Add(character.attributes[i].attribute);
        }

        if (character.hiddenDesire == null) {
            hiddenDesire = HIDDEN_DESIRE.NONE;
        } else {
            hiddenDesire = character.hiddenDesire.type;
        }

        secrets = new List<int>();
        for (int i = 0; i < character.secrets.Count; i++) {
            Secret secret = character.secrets[i];
            secrets.Add(secret.id);
        }

        //intelReactions = new List<IntelReaction>();
        //foreach (KeyValuePair<int, GAME_EVENT> kvp in character.intelReactions) {
        //    intelReactions.Add(new IntelReaction(kvp.Key, kvp.Value));
        //}

        morality = character.morality;
    }
}
