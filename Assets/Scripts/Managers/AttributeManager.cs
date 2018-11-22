using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using ECS;

public class AttributeManager : MonoBehaviour {
    public static AttributeManager Instance;

    private List<CharacterAttribute> _allAttributes;
    private List<CharacterAttribute> _allCharacterAttributes;
    private List<CharacterAttribute> _allItemAttributes;
    private List<CharacterAttribute> _allStructureAttributes;
    private Dictionary<string, Trait> _allTraits;
    private Dictionary<string, Trait> _allPositiveTraits;

    #region getters/setters
    public List<CharacterAttribute> allAttributes {
        get { return _allAttributes; }
    }
    public List<CharacterAttribute> allCharacterAttributes {
        get { return _allCharacterAttributes; }
    }
    public List<CharacterAttribute> allItemAttributes {
        get { return _allItemAttributes; }
    }
    public List<CharacterAttribute> allStructureAttributes {
        get { return _allStructureAttributes; }
    }
    public Dictionary<string, Trait> allTraits {
        get { return _allTraits; }
    }
    public Dictionary<string, Trait> allPositiveTraits {
        get { return _allPositiveTraits; }
    }
    #endregion

    void Awake() {
        Instance = this;
    }

    public void Initialize() {
        _allTraits = new Dictionary<string, Trait>();
        _allPositiveTraits = new Dictionary<string, Trait>();
        string path = Utilities.dataPath + "CombatAttributes/";
        string[] files = Directory.GetFiles(path, "*.json");
        for (int i = 0; i < files.Length; i++) {
            Trait attribute = JsonUtility.FromJson<Trait>(System.IO.File.ReadAllText(files[i]));
            _allTraits.Add(attribute.name, attribute);
            if(attribute.type == TRAIT_TYPE.POSITIVE) {
                _allPositiveTraits.Add(attribute.name, attribute);
            }
        }
    }
    public Action<Character> GetBehavior(ATTRIBUTE_BEHAVIOR type) {
        switch (type) {
            case ATTRIBUTE_BEHAVIOR.NONE:
            return null;
        }
        return null;
    }
    public string GetRandomPositiveTrait() {
        int random = UnityEngine.Random.Range(0, _allPositiveTraits.Count);
        int count = 0;
        foreach (string traitName in _allPositiveTraits.Keys) {
            if (count == random) {
                return traitName;
            }
            count++;
        }
        return string.Empty;
    }
}
