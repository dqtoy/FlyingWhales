using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterSaveData {
    public int id;
    public string name;
    public RACE race;
    public GENDER gender;
    public SEXUALITY sexuality;
    public CharacterRole role;
    public string className;
    public LOCATION_IDENTIFIER locationType;
    public int locationID;
    public int homeID;
    public int homeRegionID;
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
    public int level;

    public CharacterSaveData(Character character) {
        id = character.id;
        name = character.name;
        race = character.raceSetting.race;
        gender = character.gender;
        sexuality = character.sexuality;
        role = character.role;
        className = character.characterClass.className;

        if (character.party.specificLocation != null) {
            //locationType = character.party.specificLocation.locIdentifier;
            locationID = character.party.specificLocation.id;
        } else {
            locationID = -1;
        }

        if (character.homeRegion != null) {
            homeRegionID = character.homeRegion.id;
        } else {
            homeRegionID = -1;
        }

        if (character.faction != null) {
            factionID = character.faction.id;
        } else {
            factionID = -1;
        }
        
        portraitSettings = character.portraitSettings;

        //equipmentData = new List<string>();
        //for (int i = 0; i < character.equippedItems.Count; i++) {
        //    Item item = character.equippedItems[i];
        //    equipmentData.Add(item.itemName);
        //}

        //inventoryData = new List<string>();
        //for (int i = 0; i < character.inventory.Count; i++) {
        //    Item item = character.inventory[i];
        //    inventoryData.Add(item.itemName);
        //}

        relationshipsData = new List<RelationshipSaveData>();
        foreach (KeyValuePair<AlterEgoData, CharacterRelationshipData> kvp in character.relationships) {
            relationshipsData.Add(new RelationshipSaveData(kvp.Value));
        }

        //attributes = new List<ATTRIBUTE>();
        //for (int i = 0; i < character.attributes.Count; i++) {
        //    attributes.Add(character.attributes[i].attribute);
        //}

        morality = character.morality;
        level = character.level;
    }
}
