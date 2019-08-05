using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Linq;

public class AttributeManager : MonoBehaviour {
    public static AttributeManager Instance;

    private Dictionary<string, Trait> _allTraits;
    private Dictionary<string, Trait> _allPositiveTraits;
    private Dictionary<string, Trait> _allIlnesses;
    private Trait[] instancedTraits;

    [SerializeField] private StringSpriteDictionary traitIconDictionary;

    #region getters/setters
    public Dictionary<string, Trait> allTraits {
        get { return _allTraits; }
    }
    public Dictionary<string, Trait> allPositiveTraits {
        get { return _allPositiveTraits; }
    }
    public Dictionary<string, Trait> allIllnesses {
        get { return _allIlnesses; }
    }
    #endregion

    void Awake() {
        Instance = this;
    }

    public void Initialize() {
        _allTraits = new Dictionary<string, Trait>();
        _allPositiveTraits = new Dictionary<string, Trait>();
        _allIlnesses = new Dictionary<string, Trait>();
        string path = Utilities.dataPath + "CombatAttributes/";
        string[] files = Directory.GetFiles(path, "*.json");
        for (int i = 0; i < files.Length; i++) {
            Trait attribute = JsonUtility.FromJson<Trait>(System.IO.File.ReadAllText(files[i]));
            CategorizeTrait(attribute);
        }
#if !WORLD_CREATION_TOOL
        AddInstancedTraits(); //Traits with their own classes
#endif
    }

    #region Utilities
    private void AddInstancedTraits() {
        instancedTraits = new Trait[] {
            new Craftsman(),
            new Criminal(),
            new Grudge(),
            new PatrollingCharacter(),
            new Reanimated(),
            new Restrained(),
            new Assaulter(),
            new AttemptedMurderer(),
            new Cursed(),
            new Injured(),
            new Kleptomaniac(),
            new Lycanthropy(),
            new Vampiric(),
            new Murderer(),
            new Poisoned(),
            new Resting(),
            new Sick(),
            new Thief(),
            new Unconscious(),
            new Betrayed(),
            new Zapped(),
            new Spooked(),
            new Jolted(),
            new Taunted(),
            new Cannibal(),
            new Lethargic(),
            new BerserkBuff(),
            new Aberration(),
            new Dead(),
            new Disabled(),
            new Invisible(),
            new Unfaithful(),
            new Drunk(),
        };
        for (int i = 0; i < instancedTraits.Length; i++) {
            CategorizeTrait(instancedTraits[i]);
        }
    }
    private void CategorizeTrait(Trait attribute) {
        _allTraits.Add(attribute.name, attribute);
        if (attribute.effect == TRAIT_EFFECT.POSITIVE) {
            _allPositiveTraits.Add(attribute.name, attribute);
        }
        if (attribute.type == TRAIT_TYPE.ILLNESS) {
            _allIlnesses.Add(attribute.name, attribute);
        }
    }
    public Sprite GetTraitIcon(string traitName) {
        if (traitIconDictionary.ContainsKey(traitName)) {
            return traitIconDictionary[traitName];
        }
        return traitIconDictionary.Values.First();
    }
    public bool HasTraitIcon(string traitName) {
        return traitIconDictionary.ContainsKey(traitName);
    }
    public bool IsInstancedTrait(string traitName) {
        for (int i = 0; i < instancedTraits.Length; i++) {
            Trait currTrait = instancedTraits[i];
            if (string.Equals(currTrait.name, traitName, StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
        }
        return false;
    }
    public Trait CreateNewInstancedTraitClass(string traitName) {
        return System.Activator.CreateInstance(System.Type.GetType(traitName)) as Trait;
    }
    #endregion
}
