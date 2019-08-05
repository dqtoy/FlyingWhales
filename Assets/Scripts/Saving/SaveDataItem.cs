﻿using System.Collections;
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
                SaveDataTrait saveDataTrait = new SaveDataTrait();
                saveDataTrait.Save(item.normalTraits[i]);
                traits.Add(saveDataTrait);
            }
        }

    }
    public void Load(Area areaOwner) {
        SpecialToken item = TokenManager.Instance.CreateSpecialToken(specialTokenType, weight);
        item.SetIsDisabledByPlayer(isDisabledByPlayer);
        item.SetPOIState(state);
        for (int i = 0; i < traits.Count; i++) {
            traits[i].Load(item);
        }
        areaOwner.AddSpecialTokenToLocation(item);
    }
    public void Load(Character characterOwner) {
        SpecialToken item = TokenManager.Instance.CreateSpecialToken(specialTokenType, weight);
        item.SetIsDisabledByPlayer(isDisabledByPlayer);
        item.SetPOIState(state);
        for (int i = 0; i < traits.Count; i++) {
            traits[i].Load(item);
        }
        characterOwner.ObtainToken(item);
    }
}