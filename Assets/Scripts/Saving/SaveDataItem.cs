using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataItem {
    public string name;
    public SPECIAL_TOKEN specialTokenType;
    public int weight;
    public bool isDisabledByPlayer;
    public List<SaveDataTrait> traits;
    public POI_STATE state;

    public void Save(SpecialToken item) {
        name = item.name;
        specialTokenType = item.specialTokenType;
        weight = item.weight;
        isDisabledByPlayer = item.isDisabledByPlayer;
        state = item.state;
        if(item.normalTraits != null) {
            traits = new List<SaveDataTrait>();
            for (int i = 0; i < item.normalTraits.Count; i++) {
                Trait trait = item.normalTraits[i];
                SaveDataTrait saveDataTrait = null;
                System.Type type = System.Type.GetType("SaveData" + trait.name);
                if (type != null) {
                    saveDataTrait = System.Activator.CreateInstance(type) as SaveDataTrait;
                } else {
                    saveDataTrait = new SaveDataTrait();
                }
                saveDataTrait.Save(trait);
                traits.Add(saveDataTrait);
            }
        }

    }
    public void Load(Area areaOwner) {
        SpecialToken item = TokenManager.Instance.CreateSpecialToken(specialTokenType, weight);
        item.SetIsDisabledByPlayer(isDisabledByPlayer);
        item.SetPOIState(state);
        for (int i = 0; i < traits.Count; i++) {
            Character responsibleCharacter = null;
            Trait trait = traits[i].Load(ref responsibleCharacter);
            item.AddTrait(trait, responsibleCharacter);
        }
        areaOwner.AddSpecialTokenToLocation(item);
    }
    public void Load(Character characterOwner) {
        SpecialToken item = TokenManager.Instance.CreateSpecialToken(specialTokenType, weight);
        item.SetIsDisabledByPlayer(isDisabledByPlayer);
        item.SetPOIState(state);
        for (int i = 0; i < traits.Count; i++) {
            Character responsibleCharacter = null;
            Trait trait = traits[i].Load(ref responsibleCharacter);
            item.AddTrait(trait, responsibleCharacter);
        }
        characterOwner.ObtainToken(item);
    }
}
