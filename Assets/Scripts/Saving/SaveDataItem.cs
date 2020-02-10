using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree.Types;
using Inner_Maps;
using Traits;

[System.Serializable]
public class SaveDataItem {
    public int id;
    public string name;
    public SPECIAL_TOKEN specialTokenType;
    public int weight;
    public bool isDisabledByPlayer;
    public List<SaveDataTrait> traits;
    public POI_STATE state;

    public int structureID;
    public STRUCTURE_TYPE structureType;
    public Vector3Save gridTile;

    public void Save(SpecialToken item) {
        id = item.id;
        name = item.name;
        specialTokenType = item.specialTokenType;
        weight = item.weight;
        isDisabledByPlayer = item.isDisabledByPlayer;
        state = item.state;
        if(item.traitContainer.allTraits != null) {
            traits = new List<SaveDataTrait>();
            for (int i = 0; i < item.traitContainer.allTraits.Count; i++) {
                Trait trait = item.traitContainer.allTraits[i];
                SaveDataTrait saveDataTrait = SaveManager.ConvertTraitToSaveDataTrait(trait);
                if(saveDataTrait != null) {
                    saveDataTrait.Save(trait);
                    traits.Add(saveDataTrait);
                }
            }
        }

        if(item.gridTileLocation != null) {
            structureID = item.gridTileLocation.structure.id;
            structureType = item.gridTileLocation.structure.structureType;
            gridTile = new Vector3Save(item.gridTileLocation.localPlace.x, item.gridTileLocation.localPlace.y, 0);
        } else {
            gridTile = new Vector3Save(0f, 0f, -1f);
        }
    }
    public void Load(Settlement settlementOwner) {
        SpecialToken item = TokenManager.Instance.CreateSpecialToken(specialTokenType, weight);
        item.SetID(id);
        item.SetIsDisabledByPlayer(isDisabledByPlayer);
        item.SetPOIState(state);
        for (int i = 0; i < traits.Count; i++) {
            Character responsibleCharacter = null;
            Trait trait = traits[i].Load(ref responsibleCharacter);
            item.traitContainer.AddTrait(item, trait, responsibleCharacter);
        }
        LocationStructure structure = null;
        LocationGridTile tile = null;
        if(gridTile.z != -1f) {
            structure = settlementOwner.GetStructureByID(structureType, structureID);
            tile = settlementOwner.innerMap.map[(int) gridTile.x, (int) gridTile.y];
        }
        structure.AddItem(item, tile);
    }
    public void Load(Character characterOwner) {
        SpecialToken item = TokenManager.Instance.CreateSpecialToken(specialTokenType, weight);
        item.SetID(id);
        item.SetIsDisabledByPlayer(isDisabledByPlayer);
        item.SetPOIState(state);
        for (int i = 0; i < traits.Count; i++) {
            Character responsibleCharacter = null;
            Trait trait = traits[i].Load(ref responsibleCharacter);
            item.traitContainer.AddTrait(item, trait, responsibleCharacter);
        }
        characterOwner.ObtainItem(item);
    }
}