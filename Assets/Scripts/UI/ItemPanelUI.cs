using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ECS;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class ItemPanelUI : MonoBehaviour {
    public static ItemPanelUI Instance;

    public Dropdown itemTypeOptions;
    public Dropdown weaponTypeOptions;
    public Dropdown armorTypeOptions;
    public Dropdown weaponPrefixOptions;
    public Dropdown weaponSuffixOptions;
    public Dropdown armorPrefixOptions;
    public Dropdown armorSuffixOptions;
    public Dropdown elementOptions;

    public InputField nameInput;
    public InputField descriptionInput;
    public InputField interactionInput;
    public InputField goldCostInput;
    public InputField powerInput;
    public InputField defInput;

    public Toggle stackableToggle;

    public GameObject weaponFieldsGO;
    public GameObject armorFieldsGO;

    void Awake() {
        Instance = this;
    }
    void Start() {
        LoadAllData();
    }

    #region Utilities
    private void LoadAllData() {
        itemTypeOptions.ClearOptions();
        weaponTypeOptions.ClearOptions();
        armorTypeOptions.ClearOptions();
        weaponPrefixOptions.ClearOptions();
        weaponSuffixOptions.ClearOptions();
        armorPrefixOptions.ClearOptions();
        armorSuffixOptions.ClearOptions();
        elementOptions.ClearOptions();

        string[] itemTypes = System.Enum.GetNames(typeof(ITEM_TYPE));
        string[] weaponTypes = System.Enum.GetNames(typeof(WEAPON_TYPE));
        string[] armorTypes = System.Enum.GetNames(typeof(ARMOR_TYPE));
        string[] weaponPrefixes = System.Enum.GetNames(typeof(WEAPON_PREFIX));
        string[] weaponSuffixes = System.Enum.GetNames(typeof(WEAPON_SUFFIX));
        string[] armorPrefixes = System.Enum.GetNames(typeof(ARMOR_PREFIX));
        string[] armorSuffixes = System.Enum.GetNames(typeof(ARMOR_SUFFIX));
        string[] elements = System.Enum.GetNames(typeof(ELEMENT));


        itemTypeOptions.AddOptions(itemTypes.ToList());
        weaponTypeOptions.AddOptions(weaponTypes.ToList());
        armorTypeOptions.AddOptions(armorTypes.ToList());
        weaponPrefixOptions.AddOptions(weaponPrefixes.ToList());
        weaponSuffixOptions.AddOptions(weaponSuffixes.ToList());
        armorPrefixOptions.AddOptions(armorPrefixes.ToList());
        armorSuffixOptions.AddOptions(armorSuffixes.ToList());
        elementOptions.AddOptions(elements.ToList());
    }
    private void ClearData() {
        itemTypeOptions.value = 0;
        weaponTypeOptions.value = 0;
        armorTypeOptions.value = 0;
        weaponPrefixOptions.value = 0;
        weaponSuffixOptions.value = 0;
        armorPrefixOptions.value = 0;
        armorSuffixOptions.value = 0;
        elementOptions.value = 0;

        nameInput.text = string.Empty;
        descriptionInput.text = string.Empty;
        interactionInput.text = string.Empty;
        goldCostInput.text = "0";
        powerInput.text = "0";
        defInput.text = "0";

        stackableToggle.isOn = false;

        armorFieldsGO.SetActive(false);
    }
    private void SaveItem() {
#if UNITY_EDITOR
        if (nameInput.text == string.Empty) {
            EditorUtility.DisplayDialog("Error", "Please specify an Item Name", "OK");
            return;
        }
        string path = Utilities.dataPath + "Items/" + itemTypeOptions.options[itemTypeOptions.value].text + "/" + nameInput.text + ".json";
        if (Utilities.DoesFileExist(path)) {
            if (EditorUtility.DisplayDialog("Overwrite Item", "An item with name " + nameInput.text + " already exists. Replace with this item?", "Yes", "No")) {
                File.Delete(path);
                SaveItemJson(path);
            }
        } else {
            SaveItemJson(path);
        }
#endif
    }
    private void SaveItemJson(string path) {
        ITEM_TYPE itemType = (ITEM_TYPE) System.Enum.Parse(typeof(ITEM_TYPE), itemTypeOptions.options[itemTypeOptions.value].text);
        if (itemType == ITEM_TYPE.WEAPON) {
            SaveWeapon(path);
        } else if (itemType == ITEM_TYPE.ARMOR) {
            SaveArmor(path);
        } else {
            Save(path);
        }
    }
    private void SaveWeapon(string path) {
        Weapon weapon = new Weapon();

        SetCommonData(weapon);

        weapon.weaponType = (WEAPON_TYPE) System.Enum.Parse(typeof(WEAPON_TYPE), weaponTypeOptions.options[weaponTypeOptions.value].text);
        weapon.SetPrefix((WEAPON_PREFIX) System.Enum.Parse(typeof(WEAPON_PREFIX), weaponPrefixOptions.options[weaponPrefixOptions.value].text));
        weapon.SetSuffix((WEAPON_SUFFIX) System.Enum.Parse(typeof(WEAPON_SUFFIX), weaponSuffixOptions.options[weaponSuffixOptions.value].text));
        weapon.element = (ELEMENT) System.Enum.Parse(typeof(ELEMENT), elementOptions.options[elementOptions.value].text);
        weapon.weaponPower = float.Parse(powerInput.text);

        SaveJson(weapon, path);
    }
    private void SaveArmor(string path) {
        Armor armor = new Armor();

        SetCommonData(armor);

        armor.armorType = (ARMOR_TYPE) System.Enum.Parse(typeof(ARMOR_TYPE), armorTypeOptions.options[armorTypeOptions.value].text);
        armor.SetPrefix((ARMOR_PREFIX) System.Enum.Parse(typeof(ARMOR_PREFIX), armorPrefixOptions.options[armorPrefixOptions.value].text));
        armor.SetSuffix((ARMOR_SUFFIX) System.Enum.Parse(typeof(ARMOR_SUFFIX), armorSuffixOptions.options[armorSuffixOptions.value].text));
        armor.def = int.Parse(defInput.text);

        SaveJson(armor, path);
    }
    private void Save(string path) {
        Item item = new Item();
        SetCommonData(item);
        SaveJson(item, path);
    }
    private void SetCommonData(Item newItem) {
        newItem.itemType = (ITEM_TYPE) System.Enum.Parse(typeof(ITEM_TYPE), itemTypeOptions.options[itemTypeOptions.value].text);
        newItem.itemName = nameInput.text;
        newItem.description = descriptionInput.text;
        newItem.interactString = interactionInput.text;
        newItem.goldCost = int.Parse(goldCostInput.text);
        newItem.isStackable = stackableToggle.isOn;
    }
    private void SaveJson(Item item, string path) {
        string jsonString = JsonUtility.ToJson(item);

        System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
        writer.WriteLine(jsonString);
        writer.Close();

#if UNITY_EDITOR
        //Re-import the file to update the reference in the editor
        UnityEditor.AssetDatabase.ImportAsset(path);
#endif

        Debug.Log("Successfully saved item at " + path);
    }

    private void LoadItem() {
#if UNITY_EDITOR
        string filePath = EditorUtility.OpenFilePanel("Select Item", Utilities.dataPath + "Items/", "json");

        if (!string.IsNullOrEmpty(filePath)) {
            ClearData();
            string dataAsJson = File.ReadAllText(filePath);

            Item item = JsonUtility.FromJson<Item>(dataAsJson);
            if(item.itemType == ITEM_TYPE.WEAPON) {
                Weapon weapon = JsonUtility.FromJson<Weapon>(dataAsJson);
                LoadItemDataToUI(weapon);
            } else if (item.itemType == ITEM_TYPE.ARMOR) {
                Armor armor = JsonUtility.FromJson<Armor>(dataAsJson);
                LoadItemDataToUI(armor);
            } else {
                LoadItemDataToUI(item);
            }
        }
#endif
    }

    private void LoadItemDataToUI(Item item) {
        itemTypeOptions.value = GetItemTypeIndex(item.itemType.ToString());
        nameInput.text = item.itemName;
        descriptionInput.text = item.description;
        interactionInput.text = item.interactString;
        goldCostInput.text = item.goldCost.ToString();
        stackableToggle.isOn = item.isStackable;

        if(item.itemType == ITEM_TYPE.WEAPON) {
            Weapon weapon = item as Weapon;
            weaponTypeOptions.value = GetWeaponTypeIndex(weapon.weaponType.ToString());
            weaponPrefixOptions.value = GetWeaponPrefixIndex(weapon.prefixType.ToString());
            weaponSuffixOptions.value = GetWeaponSuffixIndex(weapon.suffixType.ToString());
            elementOptions.value = GetElementIndex(weapon.element.ToString());
            powerInput.text = weapon.weaponPower.ToString();
        }else if (item.itemType == ITEM_TYPE.ARMOR) {
            Armor armor = item as Armor;
            armorTypeOptions.value = GetArmorTypeIndex(armor.armorType.ToString());
            armorPrefixOptions.value = GetArmorPrefixIndex(armor.prefixType.ToString());
            armorSuffixOptions.value = GetArmorSuffixIndex(armor.suffixType.ToString());
            defInput.text = armor.def.ToString();
        }
    }
    private int GetItemTypeIndex(string name) {
        for (int i = 0; i < itemTypeOptions.options.Count; i++) {
            if (itemTypeOptions.options[i].text == name) {
                return i;
            }
        }
        return 0;
    }
    private int GetWeaponTypeIndex(string name) {
        for (int i = 0; i < weaponTypeOptions.options.Count; i++) {
            if (weaponTypeOptions.options[i].text == name) {
                return i;
            }
        }
        return 0;
    }
    private int GetWeaponPrefixIndex(string name) {
        for (int i = 0; i < weaponPrefixOptions.options.Count; i++) {
            if (weaponPrefixOptions.options[i].text == name) {
                return i;
            }
        }
        return 0;
    }
    private int GetWeaponSuffixIndex(string name) {
        for (int i = 0; i < weaponSuffixOptions.options.Count; i++) {
            if (weaponSuffixOptions.options[i].text == name) {
                return i;
            }
        }
        return 0;
    }
    private int GetElementIndex(string name) {
        for (int i = 0; i < elementOptions.options.Count; i++) {
            if (elementOptions.options[i].text == name) {
                return i;
            }
        }
        return 0;
    }
    private int GetArmorTypeIndex(string name) {
        for (int i = 0; i < armorTypeOptions.options.Count; i++) {
            if (armorTypeOptions.options[i].text == name) {
                return i;
            }
        }
        return 0;
    }
    private int GetArmorPrefixIndex(string name) {
        for (int i = 0; i < armorPrefixOptions.options.Count; i++) {
            if (armorPrefixOptions.options[i].text == name) {
                return i;
            }
        }
        return 0;
    }
    private int GetArmorSuffixIndex(string name) {
        for (int i = 0; i < armorSuffixOptions.options.Count; i++) {
            if (armorSuffixOptions.options[i].text == name) {
                return i;
            }
        }
        return 0;
    }
    #endregion

    #region OnValueChanged
    public void OnItemTypeChange(int index) {
        weaponFieldsGO.SetActive(false);
        armorFieldsGO.SetActive(false);

        ITEM_TYPE itemType = (ITEM_TYPE) System.Enum.Parse(typeof(ITEM_TYPE), itemTypeOptions.options[index].text);
        if(itemType == ITEM_TYPE.WEAPON) {
            weaponFieldsGO.SetActive(true);
        }else if (itemType == ITEM_TYPE.ARMOR) {
            armorFieldsGO.SetActive(true);
        }
    }
    #endregion

    #region Button Clicks
    public void OnClickAddNewItem() {
        ClearData();
    }
    public void OnClickEditItem() {
        LoadItem();
    }
    public void OnClickSaveItem() {
        SaveItem();
    }
    #endregion
}
