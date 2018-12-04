using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;


public class MonsterPanelUI : MonoBehaviour {
    public static MonsterPanelUI Instance;

    public InputField nameInput;
    public InputField levelInput;
    public InputField expInput;
    public InputField hpInput;
    public InputField spInput;
    public InputField powerInput;
    public InputField speedInput;
    public InputField dodgeInput;
    public InputField hitInput;
    public InputField critInput;
    public InputField itemDropRateInput;
    public InputField armyCountInput;

    public Dropdown typeOptions;
    public Dropdown skillOptions;
    public Dropdown itemDropOptions;

    public Toggle isSleepingOnSpawnToggle;

    public Transform skillContentTransform;
    public Transform itemDropContentTransform;

    public GameObject skillsGO;
    public GameObject itemDropGO;
    public GameObject monsterSkillBtnGO;
    public GameObject itemDropBtnPrefab;

    [NonSerialized] public MonsterSkillButton currentSelectedButton;

    private ItemDropBtn _currentSelectedItemDropBtn;
    private List<string> _allSkills;
    private List<ItemDrop> _itemDrops;

    #region getters/setters
    public List<string> allSkills {
        get { return _allSkills; }
    }
    public List<ItemDrop> itemDrops {
        get { return _itemDrops; }
    }
    #endregion

    void Awake() {
        Instance = this;
    }

    #region Utilities
    public void UpdateSkillList() {
        skillOptions.ClearOptions();
        skillOptions.AddOptions(SkillPanelUI.Instance.allSkills);
    }
    public void UpdateItemDropOptions() {
        itemDropOptions.ClearOptions();
        itemDropOptions.AddOptions(ItemPanelUI.Instance.allItems);
    }
    public void LoadAllData() {
        _allSkills = new List<string>();
        _itemDrops = new List<ItemDrop>();
        typeOptions.ClearOptions();
        //itemDropOptions.ClearOptions();

        string[] monsterTypes = System.Enum.GetNames(typeof(MONSTER_TYPE));

        //List<string> allItems = new List<string>();
        //string path = Utilities.dataPath + "Items/";
        //string[] directories = Directory.GetDirectories(path);
        //for (int i = 0; i < directories.Length; i++) {
        //    string[] files = Directory.GetFiles(directories[i], "*.json");
        //    for (int j = 0; j < files.Length; j++) {
        //        allItems.Add(Path.GetFileNameWithoutExtension(files[j]));
        //    }
        //}

        //itemDropOptions.AddOptions(allItems);
        typeOptions.AddOptions(monsterTypes.ToList());
    }
    private void ClearData() {
        currentSelectedButton = null;
        nameInput.text = string.Empty;

        levelInput.text = "1";
        expInput.text = "0";
        hpInput.text = "0";
        spInput.text = "0";
        powerInput.text = "0";
        speedInput.text = "0";
        dodgeInput.text = "0";
        hitInput.text = "0";
        critInput.text = "0";
        itemDropRateInput.text = "0";
        armyCountInput.text = "1";

        typeOptions.value = 0;
        skillOptions.value = 0;
        itemDropOptions.value = 0;

        isSleepingOnSpawnToggle.isOn = false;

        _allSkills.Clear();
        _itemDrops.Clear();
        foreach (Transform child in skillContentTransform) {
            GameObject.Destroy(child.gameObject);
        }
        foreach (Transform child in itemDropContentTransform) {
            GameObject.Destroy(child.gameObject);
        }
    }
    private void SaveMonster() {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(nameInput.text)) {
            EditorUtility.DisplayDialog("Error", "Please specify a Monster Name", "OK");
            return;
        }
        string path = Utilities.dataPath + "Monsters/" + nameInput.text + ".json";
        if (Utilities.DoesFileExist(path)) {
            if (EditorUtility.DisplayDialog("Overwrite Monster", "A monster with name " + nameInput.text + " already exists. Replace with this monster?", "Yes", "No")) {
                File.Delete(path);
                SaveMonsterJson(path);
            }
        } else {
            SaveMonsterJson(path);
        }
#endif
    }
    private void SaveMonsterJson(string path) {
        Monster newMonster = new Monster();

        newMonster.SetDataFromMonsterPanelUI();

        string jsonString = JsonUtility.ToJson(newMonster);

        System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
        writer.WriteLine(jsonString);
        writer.Close();

#if UNITY_EDITOR
        //Re-import the file to update the reference in the editor
        UnityEditor.AssetDatabase.ImportAsset(path);
#endif
        Debug.Log("Successfully saved monster at " + path);

        CombatSimManager.Instance.UpdateAllMonsters();
    }
    private void LoadMonster() {
#if UNITY_EDITOR
        string filePath = EditorUtility.OpenFilePanel("Select Monster", Utilities.dataPath + "Monsters/", "json");

        if (!string.IsNullOrEmpty(filePath)) {
            string dataAsJson = File.ReadAllText(filePath);

            Monster monster = JsonUtility.FromJson<Monster>(dataAsJson);
            ClearData();
            LoadMonsterDataToUI(monster);
        }
#endif
    }
    private void LoadMonsterDataToUI(Monster monster) {
        nameInput.text = monster.name;
        typeOptions.value = GetMonsterTypeIndex(monster.type);
        levelInput.text = monster.level.ToString();
        expInput.text = monster.experienceDrop.ToString();
        hpInput.text = monster.hp.ToString();
        spInput.text = monster.maxSP.ToString();
        powerInput.text = monster.attackPower.ToString();
        speedInput.text = monster.speed.ToString();
        dodgeInput.text = monster.dodgeChance.ToString();
        hitInput.text = monster.hitChance.ToString();
        critInput.text = monster.critChance.ToString();
        armyCountInput.text = monster.startingArmyCount.ToString();
        isSleepingOnSpawnToggle.isOn = monster.isSleepingOnSpawn;

        for (int i = 0; i < monster.skillNames.Count; i++) {
            string skillName = monster.skillNames[i];
            _allSkills.Add(skillName);
            GameObject go = GameObject.Instantiate(monsterSkillBtnGO, skillContentTransform);
            go.GetComponent<MonsterSkillButton>().buttonText.text = skillName;
        }
        for (int i = 0; i < monster.itemDrops.Count; i++) {
            ItemDrop itemDrop = monster.itemDrops[i];
            _itemDrops.Add(itemDrop);
            GameObject go = GameObject.Instantiate(itemDropBtnPrefab, itemDropContentTransform);
            go.GetComponent<ItemDropBtn>().Set(itemDrop.itemName, itemDrop.dropRate);
        }
    }
    private int GetMonsterTypeIndex(MONSTER_TYPE monsterType) {
        for (int i = 0; i < typeOptions.options.Count; i++) {
            if (typeOptions.options[i].text == monsterType.ToString()) {
                return i;
            }
        }
        return 0;
    }
    public void SetItemDropBn(ItemDropBtn btn) {
        _currentSelectedItemDropBtn = btn;
    }
    private bool HasItemDrop(string itemName) {
        for (int i = 0; i < _itemDrops.Count; i++) {
            if(_itemDrops[i].itemName == itemName) {
                return true;
            }
        }
        return false;
    }
    private bool RemoveItemDrop(string itemName) {
        for (int i = 0; i < _itemDrops.Count; i++) {
            if (_itemDrops[i].itemName == itemName) {
                _itemDrops.RemoveAt(i);
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Button Clicks
    public void OnClickAddNewMonster() {
        ClearData();
    }
    public void OnClickEditMonster() {
        LoadMonster();
    }
    public void OnClickSaveMonster() {
        SaveMonster();
    }
    public void OnClickAddSkill() {
        string skillToAdd = skillOptions.options[skillOptions.value].text;
        if (!_allSkills.Contains(skillToAdd)) {
            _allSkills.Add(skillToAdd);
            GameObject go = GameObject.Instantiate(monsterSkillBtnGO, skillContentTransform);
            go.GetComponent<MonsterSkillButton>().buttonText.text = skillToAdd;
        }
    }
    public void OnClickRemoveSkill() {
        if (currentSelectedButton != null) {
            string skillToRemove = currentSelectedButton.buttonText.text;
            if (_allSkills.Remove(skillToRemove)) {
                GameObject.Destroy(currentSelectedButton.gameObject);
                currentSelectedButton = null;
            }
        }
    }
    public void OnClickSkills() {
        skillsGO.SetActive(true);
        itemDropGO.SetActive(false);
    }
    public void OnClickItemDrops() {
        skillsGO.SetActive(false);
        itemDropGO.SetActive(true);
    }
    public void OnClickAddItemDrop() {
        string itemToAdd = itemDropOptions.options[itemDropOptions.value].text;
        if (!HasItemDrop(itemToAdd)) {
            float rate = float.Parse(itemDropRateInput.text);
            ItemDrop itemDrop = new ItemDrop() {
                itemName = itemToAdd,
                dropRate = rate
            };
            _itemDrops.Add(itemDrop);
            GameObject go = GameObject.Instantiate(itemDropBtnPrefab, itemDropContentTransform);
            go.GetComponent<ItemDropBtn>().Set(itemDrop.itemName, itemDrop.dropRate);
        }
    }
    public void OnClickRemoveItemDrop() {
        if (_currentSelectedItemDropBtn != null) {
            string itemToRemove = _currentSelectedItemDropBtn.name;
            if (RemoveItemDrop(itemToRemove)) {
                GameObject.Destroy(_currentSelectedItemDropBtn.gameObject);
                _currentSelectedItemDropBtn = null;
            }
        }
    }
    #endregion
}
