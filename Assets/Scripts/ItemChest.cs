using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class ItemChest : IEncounterable {

    private int _tier;
    private ITEM_TYPE _chestType;
    private int _chanceToGet;
    private WeightedDictionary<MATERIAL> _materialWeights;
    private WeightedDictionary<QUALITY> _qualityWeights;

    public string encounterName {
        get { return "Tier " + _tier.ToString() + " " + Utilities.NormalizeString(_chestType.ToString()) + " Chest"; }
    }

    public ItemChest(int tier, ITEM_TYPE chestType, int chanceToGet = 100) {
        _tier = tier;
        _chestType = chestType;
        _chanceToGet = chanceToGet;
        ConstructMaterialWeightsDictionary();
        ConstructQualityWeightsDictionary();
    }

    public bool StartEncounter(ECS.Character encounteredBy) {
        Debug.Log(encounteredBy.name + " has encountered a tier " + _tier.ToString() + " " + _chestType.ToString() + " chest!");
        ECS.Item gainedItem = RandomizeItemForCharacter(encounteredBy);
        if(gainedItem != null) {
            string quality = string.Empty;
            if (gainedItem is ECS.Weapon) {
                quality = ((ECS.Weapon)gainedItem).quality.ToString();
            } else if (gainedItem is ECS.Armor) {
                quality = ((ECS.Armor)gainedItem).quality.ToString();
            }
            Debug.Log(encounteredBy.name + " obtains " + quality + " " + gainedItem.itemName + " from the chest.");
            encounteredBy.PickupItem(gainedItem); //put item in inventory
            encounteredBy.EquipItem(gainedItem); //if the character can equip the item, equip it, otherwise, keep in inventory
        } else {
            Debug.Log(encounteredBy.name + " got nothing from the chest");
        }
        return true;
    }

    public bool StartEncounter(Party party) {
        for (int i = 0; i < party.partyMembers.Count; i++) {
            ECS.Character currMember = party.partyMembers[i];
            ECS.Item gainedItem = RandomizeItemForCharacter(currMember);
            if (gainedItem != null) {
                string quality = string.Empty;
                if (gainedItem is ECS.Weapon) {
                    QUALITY itemQuality = ((ECS.Weapon)gainedItem).quality;
                    if (itemQuality != QUALITY.NORMAL) {
                        quality = Utilities.NormalizeString(itemQuality.ToString()) + " ";
                    }
                } else if (gainedItem is ECS.Armor) {
                    QUALITY itemQuality = ((ECS.Armor)gainedItem).quality;
                    if (itemQuality != QUALITY.NORMAL) {
                        quality = Utilities.NormalizeString(itemQuality.ToString()) + " ";
                    }
                }

                if(party.currentQuest != null) {
                    //Add Logs
                    party.currentQuest.AddNewLog(currMember.name + " obtains " + quality + gainedItem.itemName + " from the chest.");
                }
                Debug.Log(currMember.name + " obtains " + quality + " " + gainedItem.itemName + " from the chest.");
                currMember.PickupItem(gainedItem); //put item in inventory
                if (currMember.EquipItem(gainedItem)) {//if the character can equip the item, equip it, otherwise, keep in inventory
                    string log = currMember.name + " equips the " + gainedItem.itemName + " on ";
                    if(currMember.gender == GENDER.FEMALE) {
                        log += "her ";
                    } else {
                        log += "his ";
                    }
                    if (gainedItem is ECS.Weapon) {
                        List<ECS.IBodyPart> bodyParts = ((ECS.Weapon)gainedItem).bodyPartsAttached;
                        for (int j = 0; j < bodyParts.Count; j++) {
                            ECS.IBodyPart currBodyPart = bodyParts[j];
                            log += currBodyPart.name + " ";
                        }
                    } else if (gainedItem is ECS.Armor) {
                        ECS.IBodyPart bodyPart = ((ECS.Armor)gainedItem).bodyPartAttached;
                        log += bodyPart.name;
                    }
                    party.currentQuest.AddNewLog(log);
                }
            } else {
                Debug.Log(currMember.name + " got nothing from the chest");
            }
        }
        return true;
    }

    public ECS.Item RandomizeItemForCharacter(ECS.Character character) {
        if(UnityEngine.Random.Range(0, 100) < _chanceToGet) {
            MATERIAL equipmentMaterial = GetEquipmentMaterial();
            EQUIPMENT_TYPE equipmentType = GetRandomEquipmentType(character);
            QUALITY equipmentQuality = GetEquipmentQuality();
            string itemName = Utilities.NormalizeString(equipmentMaterial.ToString()) + " " + Utilities.NormalizeString(equipmentType.ToString());
            ECS.Item item = ItemManager.Instance.CreateNewItemInstance(itemName);
            if(_chestType == ITEM_TYPE.ARMOR) {
                ((ECS.Armor)item).SetQuality(equipmentQuality);
            } else if (_chestType == ITEM_TYPE.WEAPON) {
                ((ECS.Weapon)item).SetQuality(equipmentQuality);
            }
            return item;
        }
        return null; //did not get anything
    }

    private EQUIPMENT_TYPE GetRandomEquipmentType(ECS.Character character) {
        switch (_chestType) {
            case ITEM_TYPE.WEAPON:
                return (EQUIPMENT_TYPE)GetWeaponTypeForCharacter(character);
            case ITEM_TYPE.ARMOR:
                return (EQUIPMENT_TYPE)GetArmorTypeForCharacter(character);
            default:
                return EQUIPMENT_TYPE.NONE;
        }
    }
    /*
     Get a random weapon type for a character
     given its allowed weapon types.
         */
    private WEAPON_TYPE GetWeaponTypeForCharacter(ECS.Character character) {
        return character.characterClass.allowedWeaponTypes[UnityEngine.Random.Range(0, character.characterClass.allowedWeaponTypes.Count)];
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
                    _materialWeights.AddElement(MATERIAL.LINEN, 50);
                    break;
                case 3:
                    _materialWeights.AddElement(MATERIAL.SILK, 50);
					_materialWeights.AddElement(MATERIAL.LINEN, 100);
                    _materialWeights.AddElement(MATERIAL.IRON, 50);
                    break;
                case 4:
					_materialWeights.AddElement(MATERIAL.LINEN, 50);
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
     Construct the quality weights dictionary.
     This is called upon initialization of this chest.
         */
    private void ConstructQualityWeightsDictionary() {
        _qualityWeights = new WeightedDictionary<QUALITY>();
        int crudeChance = 0;
        int normalChance = 0;
        int exceptionalChance = 0;
        if (_chestType == ITEM_TYPE.ARMOR) {
            switch (_tier) {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                    crudeChance = 30;
                    normalChance = 50;
                    exceptionalChance = 20;
                    break;
                default:
                    break;
            }
        } else if (_chestType == ITEM_TYPE.WEAPON) {
            switch (_tier) {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                    crudeChance = 30;
                    normalChance = 50;
                    exceptionalChance = 20;
                    break;
                default:
                    break;
            }
        }
        _qualityWeights.AddElement(QUALITY.CRUDE, crudeChance);
        _qualityWeights.AddElement(QUALITY.NORMAL, normalChance);
        _qualityWeights.AddElement(QUALITY.EXCEPTIONAL, exceptionalChance);
    }
    /*
     This will return a random item material for this chest.
     NOTE: Material weights depend on the chest tier and the chest type
         */
    private MATERIAL GetEquipmentMaterial() {
        return _materialWeights.PickRandomElementGivenWeights();
    }
    /*
     This will return a random item quality for this chest.
     NOTE: quality weights depend on the chest tier and the chest type
         */
    private QUALITY GetEquipmentQuality() {
        return _qualityWeights.PickRandomElementGivenWeights();
    }

	public void ReturnResults(object result){
		
	}
}
