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
using ECS;

public class MonsterPanelUI : MonoBehaviour {
    public static MonsterPanelUI Instance;

    public InputField nameInput;
    public InputField levelInput;
    public InputField expInput;
    public InputField hpInput;
    public InputField spInput;
    public InputField powerInput;
    public InputField speedInput;
    public InputField defInput;
    public InputField dodgeInput;
    public InputField hitInput;
    public InputField critInput;

    public Dropdown typeOptions;
    public Dropdown skillOptions;

    public Transform skillContentTransform;

    public GameObject monsterSkillBtnGO;

    [NonSerialized] public MonsterSkillButton currentSelectedButton;

    private List<string> _allSkills;

    #region getters/setters
    public List<string> allSkills {
        get { return _allSkills; }
    }
    #endregion

    void Awake() {
        Instance = this;
    }
    void Start() {
        _allSkills = new List<string>();
        LoadAllData();
    }

    #region Utilities
    public void UpdateSkillList() {
        skillOptions.ClearOptions();
        skillOptions.AddOptions(SkillPanelUI.Instance.allSkills);
    }
    private void LoadAllData() {
        typeOptions.ClearOptions();

        string[] monsterTypes = System.Enum.GetNames(typeof(MONSTER_TYPE));

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
        defInput.text = "0";
        dodgeInput.text = "0";
        hitInput.text = "0";
        critInput.text = "0";

        typeOptions.value = 0;
        skillOptions.value = 0;

        _allSkills.Clear();
        foreach (Transform child in skillContentTransform) {
            GameObject.Destroy(child.gameObject);
        }
    }
    private void SaveMonster() {
#if UNITY_EDITOR
        if (nameInput.text == string.Empty) {
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
        hpInput.text = monster.maxHP.ToString();
        spInput.text = monster.maxSP.ToString();
        powerInput.text = monster.attackPower.ToString();
        speedInput.text = monster.agility.ToString();
        defInput.text = monster.def.ToString();
        dodgeInput.text = monster.dodgeChance.ToString();
        hitInput.text = monster.hitChance.ToString();
        critInput.text = monster.critChance.ToString();

        for (int i = 0; i < monster.skillNames.Count; i++) {
            string skillName = monster.skillNames[i];
            _allSkills.Add(skillName);
            GameObject go = GameObject.Instantiate(monsterSkillBtnGO, skillContentTransform);
            go.GetComponent<MonsterSkillButton>().buttonText.text = skillName;
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

    #endregion
}
