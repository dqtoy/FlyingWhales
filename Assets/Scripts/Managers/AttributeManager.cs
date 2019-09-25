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

    public INTERACTION_TYPE[] excludedActionsFromAccidentProneTrait = new INTERACTION_TYPE[] {
        INTERACTION_TYPE.STUMBLE, INTERACTION_TYPE.PUKE, INTERACTION_TYPE.SEPTIC_SHOCK, INTERACTION_TYPE.ACCIDENT
    };

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
            new Burning(),
            new Burnt(),
            new Agoraphobia(),
            new Zombie_Virus(),
            new MusicLover(),
            new MusicHater(),
            new SerialKiller(),
            new Plagued(),
            new Vigilant(),
            new Curious(),
            new Doctor(),
            new Diplomatic(),
            new AccidentProne(),
            new Wet(),
            new CharacterTrait(),
            new Nocturnal(),
            new Herbalist(),
            new Hardworking(),
            new Glutton(),
            new Suspicious(),
            new Narcoleptic(),
            new Hothead(),
            new Inspiring(),
            new Pyrophobic(),
            new Angry(),
            new Alcoholic(),
            new Pessimist(),
            new Lazy(),
            new Coward(),
            new Berserked(),
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
            if (string.Equals(currTrait.name, traitName, StringComparison.OrdinalIgnoreCase) || string.Equals(currTrait.GetType().ToString(), traitName, StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
        }
        return false;
    }
    public Trait CreateNewInstancedTraitClass(string traitName) {
        string noSpacesTraitName = Utilities.RemoveAllWhiteSpace(traitName);
        return System.Activator.CreateInstance(System.Type.GetType(noSpacesTraitName)) as Trait;
    }
    public List<Trait> GetAllTraitsOfType(TRAIT_TYPE type) {
        List<Trait> traits = new List<Trait>();
        foreach (Trait trait in _allTraits.Values) {
            if(trait.type == type) {
                traits.Add(trait);
            }
        }
        return traits;
    }
    public List<string> GetAllBuffTraits() {
        List<string> buffTraits = new List<string>();
        foreach (KeyValuePair<string, Trait> kvp in allTraits) {
            if (kvp.Value.type == TRAIT_TYPE.BUFF) {
                buffTraits.Add(kvp.Key);
            }
        }
        return buffTraits;
    }
    public List<string> GetAllFlawTraits() {
        List<string> flawTraits = new List<string>();
        foreach (KeyValuePair<string, Trait> kvp in allTraits) {
            if (kvp.Value.type == TRAIT_TYPE.FLAW) {
                flawTraits.Add(kvp.Key);
            }
        }
        return flawTraits;
    }
    public List<string> GetAllBuffTraitsThatCharacterCanHave(Character character) {
        List<string> allBuffs = GetAllBuffTraits();
        for (int i = 0; i < character.normalTraits.Count; i++) {
            Trait trait = character.normalTraits[i];
            if (trait.mutuallyExclusive != null) {
                allBuffs = Utilities.RemoveElements(allBuffs, trait.mutuallyExclusive);
            }
        }
        return allBuffs;
    }
    public List<string> GetAllFlawTraitsThatCharacterCanHave(Character character) {
        List<string> allFlaws = GetAllFlawTraits();
        for (int i = 0; i < character.normalTraits.Count; i++) {
            Trait trait = character.normalTraits[i];
            if (trait.mutuallyExclusive != null) {
                allFlaws = Utilities.RemoveElements(allFlaws, trait.mutuallyExclusive);
            }
        }
        return allFlaws;
    }
    #endregion
}
