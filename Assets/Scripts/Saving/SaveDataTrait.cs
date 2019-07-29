using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataTrait {
    public string name;
    public int level;
    public int daysDuration;
    public bool isDisabled;
    public int responsibleCharacterID;
    public List<int> responsibleCharacterIDs;
    public List<TraitEffect> effects;

    //Date Established
    public int month;
    public int day;
    public int year;
    public int tick;

    public void Save(Trait trait) {
        name = trait.name;
        level = trait.level;
        daysDuration = trait.daysDuration;
        isDisabled = trait.isDisabled;
        
        if(trait.responsibleCharacter == null) {
            responsibleCharacterID = -1;
        } else {
            responsibleCharacterID = trait.responsibleCharacter.id;
        }

        responsibleCharacterIDs = new List<int>();
        if (trait.responsibleCharacters != null) {
            for (int i = 0; i < trait.responsibleCharacters.Count; i++) {
                responsibleCharacterIDs.Add(trait.responsibleCharacters[i].id);
            }
        }

        effects = trait.effects;

        month = trait.dateEstablished.month;
        day = trait.dateEstablished.day;
        year = trait.dateEstablished.year;
        tick = trait.dateEstablished.tick;
    }

    public void Load(IPointOfInterest poi) {
        Character responsibleCharacter = null;
        if(responsibleCharacterID != -1) {
            responsibleCharacter = CharacterManager.Instance.GetCharacterByID(responsibleCharacterID);
        }
        Trait trait = null;
        if (AttributeManager.Instance.IsInstancedTrait(name)) {
            trait = AttributeManager.Instance.CreateNewInstancedTraitClass(name);
        } else {
            trait = AttributeManager.Instance.allTraits[name];
        }
        trait.SetLevel(level);
        trait.OverrideDuration(daysDuration);
        trait.SetIsDisabled(isDisabled);
        trait.SetTraitEffects(effects);
        trait.SetDateEstablished(new GameDate(month, day, year, tick));
        for (int i = 0; i < responsibleCharacterIDs.Count; i++) {
            Character currChar = CharacterManager.Instance.GetCharacterByID(responsibleCharacterIDs[i]);
            trait.AddCharacterResponsibleForTrait(currChar);
        }
        poi.AddTrait(trait, responsibleCharacter);
    }
    public void Load(AlterEgoData alterEgo) {
        Character responsibleCharacter = null;
        if (responsibleCharacterID != -1) {
            responsibleCharacter = CharacterManager.Instance.GetCharacterByID(responsibleCharacterID);
        }
        Trait trait = null;
        if (AttributeManager.Instance.IsInstancedTrait(name)) {
            trait = AttributeManager.Instance.CreateNewInstancedTraitClass(name);
        } else {
            trait = AttributeManager.Instance.allTraits[name];
        }
        trait.SetLevel(level);
        trait.OverrideDuration(daysDuration);
        trait.SetIsDisabled(isDisabled);
        trait.SetTraitEffects(effects);
        trait.SetDateEstablished(new GameDate(month, day, year, tick));
        for (int i = 0; i < responsibleCharacterIDs.Count; i++) {
            Character currChar = CharacterManager.Instance.GetCharacterByID(responsibleCharacterIDs[i]);
            trait.AddCharacterResponsibleForTrait(currChar);
        }
        trait.SetCharacterResponsibleForTrait(responsibleCharacter);
        alterEgo.AddTrait(trait);
    }
}
