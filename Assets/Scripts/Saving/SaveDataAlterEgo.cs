using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataAlterEgo {
    public string name;
    public int factionID;
    public RACE race;
    public CHARACTER_ROLE roleType;
    public string characterClassName;
    public int level;

    //Relationships
    public List<SaveDataRelationship> relationships;

    //Traits
    public List<SaveDataTrait> traits;

    public void Save(AlterEgoData alterEgo) {
        name = alterEgo.name;
        factionID = alterEgo.faction.id;
        race = alterEgo.race;
        roleType = alterEgo.role.roleType;
        characterClassName = alterEgo.characterClass.className;
        level = alterEgo.level;

        traits = new List<SaveDataTrait>();
        if (alterEgo.traits != null) {
            for (int i = 0; i < alterEgo.traits.Count; i++) {
                SaveDataTrait saveDataTrait = new SaveDataTrait();
                saveDataTrait.Save(alterEgo.traits[i]);
                traits.Add(saveDataTrait);
            }
        }

        relationships = new List<SaveDataRelationship>();
        foreach (CharacterRelationshipData relData in alterEgo.relationships.Values) {
            SaveDataRelationship saveDataRelationship = new SaveDataRelationship();
            saveDataRelationship.Save(relData);
            relationships.Add(saveDataRelationship);
        }
    }

    public void Load(Character character) {
        AlterEgoData alterEgoData = character.CreateNewAlterEgo(name);
        alterEgoData.SetFaction(FactionManager.Instance.GetFactionBasedOnID(factionID));
        alterEgoData.SetRace(race);
        alterEgoData.SetRole(CharacterManager.Instance.GetRoleByRoleType(roleType));
        alterEgoData.SetCharacterClass(CharacterManager.Instance.CreateNewCharacterClass(characterClassName));
        alterEgoData.SetLevel(level);
        for (int i = 0; i < traits.Count; i++) {
            traits[i].Load(alterEgoData);
        }
    }
    public void LoadRelationships(Character character) {
        AlterEgoData alterEgoData = character.GetAlterEgoData(name);
        for (int i = 0; i < relationships.Count; i++) {
            relationships[i].Load(alterEgoData);
        }
    }
}
