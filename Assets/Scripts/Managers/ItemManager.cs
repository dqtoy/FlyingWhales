using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class ItemManager : MonoBehaviour {

    public static ItemManager Instance = null;

    private Dictionary<string, ECS.Item> allItems;
    private Dictionary<WEAPON_TYPE, ECS.WeaponType> _weaponTypeData;
    private Dictionary<ARMOR_TYPE, ECS.ArmorType> _armorTypeData;

    public int crudeWeaponPowerModifier;
	public int exceptionalWeaponPowerModifier;
	public int crudeWeaponDurabilityModifier;
	public int exceptionalWeaponDurabilityModifier;

	public int crudeArmorMitigationModifier;
	public int exceptionalArmorMitigationModifier;
	public int crudeArmorDurabilityModifier;
	public int exceptionalArmorDurabilityModifier;

    private void Awake() {
        Instance = this;
    }
	internal void Initialize(){
		ConstructItemsDictionary();
        ConstructWeaponTypeData();
        ConstructArmorTypeData();
	}
    private void ConstructItemsDictionary() {
        allItems = new Dictionary<string, ECS.Item>();
        string path = "Assets/CombatPrototype/Data/Items/";
        string[] directories = Directory.GetDirectories(path);
        for (int i = 0; i < directories.Length; i++) {
            string currDirectory = directories[i];
            string itemType = new DirectoryInfo(currDirectory).Name;
            ITEM_TYPE currItemType = (ITEM_TYPE)Enum.Parse(typeof(ITEM_TYPE), itemType);
            string[] files = Directory.GetFiles(currDirectory, "*.json");
            for (int k = 0; k < files.Length; k++) {
                string currFilePath = files[k];
                string dataAsJson = File.ReadAllText(currFilePath);
                switch (currItemType) {
                    case ITEM_TYPE.WEAPON:
                        ECS.Weapon newWeapon = JsonUtility.FromJson<ECS.Weapon>(dataAsJson);
                        allItems.Add(newWeapon.itemName, newWeapon);
                        break;
                    case ITEM_TYPE.ARMOR:
                        ECS.Armor newArmor = JsonUtility.FromJson<ECS.Armor>(dataAsJson);
                        allItems.Add(newArmor.itemName, newArmor);
                        break;
                    default:
                        break;
                }
            }
        }
    }
    public ECS.Item CreateNewItemInstance(string itemName) {
        if (allItems.ContainsKey(itemName)) {
            return allItems[itemName].CreateNewCopy();
        }
        throw new System.Exception("There is no item type called " + itemName);
    }

    public ECS.Item CreateNewItemInstance(MATERIAL itemMaterial, EQUIPMENT_TYPE equipmentType) {
        string itemName = Utilities.NormalizeString(itemMaterial.ToString()) + " " + Utilities.NormalizeString(equipmentType.ToString());
        if (allItems.ContainsKey(itemName)) {
            return allItems[itemName].CreateNewCopy();
        }
        throw new System.Exception("There is no item type called " + itemName);
    }
    private void ConstructWeaponTypeData() {
        _weaponTypeData = new Dictionary<WEAPON_TYPE, ECS.WeaponType>();
        string path = "Assets/CombatPrototype/Data/WeaponTypes/";
        string[] files = Directory.GetFiles(path, "*.json");
        for (int i = 0; i < files.Length; i++) {
            string currFilePath = files[i];
            string dataAsJson = File.ReadAllText(currFilePath);
            ECS.WeaponType data = JsonUtility.FromJson<ECS.WeaponType>(dataAsJson);
            _weaponTypeData.Add(data.weaponType, data);
        }
    }
    private void ConstructArmorTypeData() {
        _armorTypeData = new Dictionary<ARMOR_TYPE, ECS.ArmorType>();
        string path = "Assets/CombatPrototype/Data/ArmorTypes/";
        string[] files = Directory.GetFiles(path, "*.json");
        for (int i = 0; i < files.Length; i++) {
            string currFilePath = files[i];
            string dataAsJson = File.ReadAllText(currFilePath);
            ECS.ArmorType data = JsonUtility.FromJson<ECS.ArmorType>(dataAsJson);
            _armorTypeData.Add(data.armorType, data);
        }
    }
    internal ECS.WeaponType GetWeaponTypeData(WEAPON_TYPE weaponType) {
        if (_weaponTypeData.ContainsKey(weaponType)) {
            return _weaponTypeData[weaponType];
        }
        return null;
    }
    internal ECS.ArmorType GetArmorTypeData(ARMOR_TYPE armorType) {
        if (_armorTypeData.ContainsKey(armorType)) {
            return _armorTypeData[armorType];
        }
        return null;
    }
    internal int GetGoldCostOfItem(ITEM_TYPE itemType, MATERIAL material) {
        if (itemType == ITEM_TYPE.ARMOR) {
            return MaterialManager.Instance.materialsLookup[material].armorData.cost;
        } else if (itemType == ITEM_TYPE.WEAPON) {
            return MaterialManager.Instance.materialsLookup[material].weaponData.cost;
        }
        return -1;
    }

    internal bool CanMaterialBeUsedForWeapon(MATERIAL material) {
        foreach (KeyValuePair<WEAPON_TYPE, ECS.WeaponType> kvp in _weaponTypeData) {
            if (kvp.Value.weaponMaterials.Contains(material)) {
                return true;
            }
        }
        return false;
    }

    internal bool CanMaterialBeUsedForArmor(MATERIAL material) {
        foreach (KeyValuePair<ARMOR_TYPE, ECS.ArmorType> kvp in _armorTypeData) {
            if (kvp.Value.armorMaterials.Contains(material)) {
                return true;
            }
        }
        return false;
    }
}
