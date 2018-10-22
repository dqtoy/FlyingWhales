using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using ECS;

public class AttributeManager : MonoBehaviour {
    public static AttributeManager Instance;

    private List<Attribute> _allAttributes;
    private List<Attribute> _allCharacterAttributes;
    private List<Attribute> _allItemAttributes;
    private List<Attribute> _allStructureAttributes;
    private Dictionary<string, CombatAttribute> _allCombatAttributes;

    #region getters/setters
    public List<Attribute> allAttributes {
        get { return _allAttributes; }
    }
    public List<Attribute> allCharacterAttributes {
        get { return _allCharacterAttributes; }
    }
    public List<Attribute> allItemAttributes {
        get { return _allItemAttributes; }
    }
    public List<Attribute> allStructureAttributes {
        get { return _allStructureAttributes; }
    }
    public Dictionary<string, CombatAttribute> allCombatAttributes {
        get { return _allCombatAttributes; }
    }
    #endregion

    void Awake() {
        Instance = this;
    }

    public void Initialize() {
        _allCombatAttributes = new Dictionary<string, CombatAttribute>();
        string path = Utilities.dataPath + "CombatAttributes/";
        string[] files = Directory.GetFiles(path, "*.json");
        for (int i = 0; i < files.Length; i++) {
            CombatAttribute attribute = JsonUtility.FromJson<CombatAttribute>(System.IO.File.ReadAllText(files[i]));
            _allCombatAttributes.Add(attribute.name, attribute);
        }
        //_allCharacterAttributes = new List<Attribute>();
        //_allItemAttributes = new List<Attribute>();
        //_allStructureAttributes = new List<Attribute>();

        //string path = Utilities.dataPath + "Attributes/";
        //string[] directories = Directory.GetDirectories(path);
        //for (int i = 0; i < directories.Length; i++) {
        //    string folderName = new DirectoryInfo(directories[i]).Name;
        //    string[] files = Directory.GetFiles(directories[i], "*.json");
        //    if (folderName == "CHARACTER") {
        //        for (int j = 0; j < files.Length; j++) {
        //            Attribute attribute = JsonUtility.FromJson<Attribute>(System.IO.File.ReadAllText(files[j]));
        //            attribute.Initialize();
        //            _allCharacterAttributes.Add(attribute);
        //            _allAttributes.Add(attribute);
        //        }
        //    } else if (folderName == "ITEM") {
        //        for (int j = 0; j < files.Length; j++) {
        //            Attribute attribute = JsonUtility.FromJson<Attribute>(System.IO.File.ReadAllText(files[j]));
        //            attribute.Initialize();
        //            _allItemAttributes.Add(attribute);
        //            _allAttributes.Add(attribute);
        //        }
        //    } else if (folderName == "STRUCTURE") {
        //        for (int j = 0; j < files.Length; j++) {
        //            Attribute attribute = JsonUtility.FromJson<Attribute>(System.IO.File.ReadAllText(files[j]));
        //            attribute.Initialize();
        //            _allStructureAttributes.Add(attribute);
        //            _allAttributes.Add(attribute);
        //        }
        //    }
        //}
        }

    public Action<Character> GetBehavior(ATTRIBUTE_BEHAVIOR type) {
        switch (type) {
            case ATTRIBUTE_BEHAVIOR.NONE:
            return null;
        }
        return null;
    }
}
