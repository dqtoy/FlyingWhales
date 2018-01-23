using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class ItemManager : MonoBehaviour {

    public static ItemManager Instance = null;

    private Dictionary<string, ECS.Item> allItems;

    private void Awake() {
        Instance = this;
        ConstructItemsDictionary();
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
}
