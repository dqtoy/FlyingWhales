using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class ItemChest : IEncounterable {

    private int _tier;
    private ITEM_TYPE _chestType;
    private int _chanceToGet;
    private WeightedDictionary<MATERIAL> _materialWeights;

    public ItemChest(int tier, ITEM_TYPE chestType, int chanceToGet = 100) {
        _tier = tier;
        _chestType = chestType;
        _chanceToGet = chanceToGet;
        ConstructMaterialWeightsDictionary();
    }

    public void StartEncounter(ECS.Character encounteredBy) {
        Debug.Log(encounteredBy.name + " has encountered a tier " + _tier.ToString() + " " + _chestType.ToString() + " chest!");
        if(encounteredBy.party == null) {
            ECS.Item gainedItem = RandomizeItemForCharacter(encounteredBy);
            if(gainedItem != null) {
                encounteredBy.EquipItem(gainedItem); //TODO: Change this to put item in inventory, if the character cannot equip the gained item
            }
        } else {
            for (int i = 0; i < encounteredBy.party.partyMembers.Count; i++) {
                ECS.Character currMember = encounteredBy.party.partyMembers[i];
                ECS.Item gainedItem = RandomizeItemForCharacter(currMember);
                if (gainedItem != null) {
                    currMember.EquipItem(gainedItem); //TODO: Change this to put item in inventory, if the character cannot equip the gained item
                }
            }
        }
    }

    public ECS.Item RandomizeItemForCharacter(ECS.Character character) {
        if(UnityEngine.Random.Range(0, 100) < _chanceToGet) {
            MATERIAL equipmentMaterial = GetEquipmentMaterial();
            EQUIPMENT_TYPE equipmentType = GetRandomEquipmentType(character);
            string itemName = Utilities.NormalizeString(equipmentMaterial.ToString()) + " " + Utilities.NormalizeString(equipmentType.ToString());
            return ItemManager.Instance.CreateNewItemInstance(itemName);
        }
        return null; //did not get anything
    }

    private EQUIPMENT_TYPE GetRandomEquipmentType(ECS.Character character) {
        switch (_chestType) {
            case ITEM_TYPE.WEAPON:
                return EQUIPMENT_TYPE.NONE;
            case ITEM_TYPE.ARMOR:
                return (EQUIPMENT_TYPE)GetArmorTypeForCharacter(character);
            default:
                return EQUIPMENT_TYPE.NONE;
        }
    }
    /*
     Get the armor type the character will get from this chest.
     NOTE: This will only be triggered if the chest type is ARMOR.
         */
    private ARMOR_TYPE GetArmorTypeForCharacter(ECS.Character character) {
        //This is the list of armor, set by priority, change if needed
        List<ARMOR_TYPE> orderedArmorTypes = new List<ARMOR_TYPE>() {
            ARMOR_TYPE.SHIRT,
            ARMOR_TYPE.LEGGINGS,
            ARMOR_TYPE.HELMET,
            ARMOR_TYPE.BRACER,
            ARMOR_TYPE.BOOT
        };

        for (int i = 0; i < orderedArmorTypes.Count; i++) {
            ARMOR_TYPE currArmorType = orderedArmorTypes[i];
            //Get the armor type that the character doesn't currently have
            if (!character.HasEquipmentOfType((EQUIPMENT_TYPE)currArmorType)) {
                return currArmorType; //character has not yet equipped an armor of this type
            }
        }

        //if the code has reached this, it means that the character currently has all armor types, randomize armor type by weight
        WeightedDictionary<ARMOR_TYPE> weightedArmorTypes = Utilities.GetWeightedArmorTypes();
        return weightedArmorTypes.PickRandomElementGivenWeights();
    }
    /*
     Construct the material weights dictionary.
     This is called upon initialization of this chest.
         */
    private void ConstructMaterialWeightsDictionary() {
        _materialWeights = new WeightedDictionary<MATERIAL>();
        if (_chestType == ITEM_TYPE.ARMOR) {
            switch (_tier) {
                case 1:
                    _materialWeights.AddElement(MATERIAL.COTTON, 100);
                    _materialWeights.AddElement(MATERIAL.SILK, 50);
                    break;
                case 2:
                    _materialWeights.AddElement(MATERIAL.COTTON, 50);
                    _materialWeights.AddElement(MATERIAL.SILK, 50);
                    _materialWeights.AddElement(MATERIAL.LEATHER, 50);
                    break;
                case 3:
                    _materialWeights.AddElement(MATERIAL.SILK, 50);
                    _materialWeights.AddElement(MATERIAL.LEATHER, 100);
                    _materialWeights.AddElement(MATERIAL.IRON, 50);
                    break;
                case 4:
                    _materialWeights.AddElement(MATERIAL.LEATHER, 50);
                    _materialWeights.AddElement(MATERIAL.IRON, 100);
                    _materialWeights.AddElement(MATERIAL.COBALT, 50);
                    break;
                case 5:
                    _materialWeights.AddElement(MATERIAL.COBALT, 100);
                    _materialWeights.AddElement(MATERIAL.MITHRIL, 50);
                    break;
                default:
                    break;
            }
        } else if (_chestType == ITEM_TYPE.WEAPON) {
            switch (_tier) {
                case 1:
                    _materialWeights.AddElement(MATERIAL.OAK, 100);
                    _materialWeights.AddElement(MATERIAL.IRON, 50);
                    break;
                case 2:
                    _materialWeights.AddElement(MATERIAL.IRON, 100);
                    _materialWeights.AddElement(MATERIAL.OAK, 50);
                    break;
                case 3:
                    _materialWeights.AddElement(MATERIAL.IRON, 100);
                    _materialWeights.AddElement(MATERIAL.YEW, 50);
                    _materialWeights.AddElement(MATERIAL.COBALT, 50);
                    break;
                case 4:
                    _materialWeights.AddElement(MATERIAL.YEW, 100);
                    _materialWeights.AddElement(MATERIAL.COBALT, 100);
                    _materialWeights.AddElement(MATERIAL.EBONY, 50);
                    break;
                case 5:
                    _materialWeights.AddElement(MATERIAL.EBONY, 100);
                    _materialWeights.AddElement(MATERIAL.MITHRIL, 50);
                    break;
                default:
                    break;
            }
        }
    }
    /*
     This will return a random item material for this chest.
     NOTE: Material weights depend on the chest tier and the chest type
         */
    private MATERIAL GetEquipmentMaterial() {
        return _materialWeights.PickRandomElementGivenWeights();
    }
}
