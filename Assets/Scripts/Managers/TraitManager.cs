using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Linq;
using Traits;

public class TraitManager : MonoBehaviour {
    public static TraitManager Instance;

    private Dictionary<string, Trait> _allTraits;
    private Trait[] instancedTraits { get; set; }
    [SerializeField] private StringSpriteDictionary traitIconDictionary;

    //Trait Processors
    public static TraitProcessor characterTraitProcessor;
    public static TraitProcessor tileObjectTraitProcessor;
    public static TraitProcessor specialTokenTraitProcessor;
    public static TraitProcessor defaultTraitProcessor;
    
    //list of traits that a character can gain on their own
    public readonly string[] traitPool = new string[] { "Vigilant", "Diplomatic",
        "Fireproof", "Accident Prone", "Unfaithful", "Drunkard", "Music Lover", "Music Hater", "Ugly", "Blessed", "Nocturnal",
        "Herbalist", "Optimist", "Pessimist", "Fast", "Chaste", "Lustful", "Coward", "Lazy", "Hardworking", "Glutton", "Robust", "Suspicious" , "Inspiring", "Pyrophobic",
        "Narcoleptic", "Hothead", "Evil", "Treacherous", "Disillusioned", "Ambitious", "Authoritative", "Healer"
    };
    //"Kleptomaniac","Curious", "Craftsman"
    public List<string> buffTraitPool { get; private set; }
    public List<string> flawTraitPool { get; private set; }
    public List<string> neutralTraitPool { get; private set; }

    #region getters/setters
    public Dictionary<string, Trait> allTraits {
        get { return _allTraits; }
    }
    #endregion

    void Awake() {
        Instance = this;
        CreateTraitProcessors();
    }

    public void Initialize() {
        _allTraits = new Dictionary<string, Trait>();
        string path = Utilities.dataPath + "Traits/";
        string[] files = Directory.GetFiles(path, "*.json");
        for (int i = 0; i < files.Length; i++) {
            Trait attribute = JsonUtility.FromJson<Trait>(System.IO.File.ReadAllText(files[i]));
            _allTraits.Add(attribute.name, attribute);
        }
        
        AddInstancedTraits(); //Traits with their own classes
        
        buffTraitPool = new List<string>();
        flawTraitPool = new List<string>();
        neutralTraitPool = new List<string>();
        
        //Categorize traits from trait pool
        for (int i = 0; i < traitPool.Length; i++) {
            string currTraitName = traitPool[i];
            if (TraitManager.Instance.allTraits.ContainsKey(currTraitName)) {
                Trait trait = TraitManager.Instance.allTraits[currTraitName];
                if (trait.type == TRAIT_TYPE.BUFF) {
                    buffTraitPool.Add(currTraitName);
                } else if (trait.type == TRAIT_TYPE.FLAW) {
                    flawTraitPool.Add(currTraitName);
                } else {
                    neutralTraitPool.Add(currTraitName);
                }
            } else {
                throw new Exception("There is no trait named: " + currTraitName);
            }
        }
    }

    #region Utilities
    private void AddInstancedTraits() {
        instancedTraits = new Trait[] {
            //new Builder(),
            new Grudge(),
            new PatrollingCharacter(),
            new Reanimated(),
            new Restrained(),
            new Assaulter(),
            new AttemptedMurderer(),
            new Cursed(),
            new Injured(),
            new Kleptomaniac(),
            new Lycanthrope(),
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
            new Agoraphobic(),
            new Infected(),
            new MusicLover(),
            new MusicHater(),
            new SerialKiller(),
            new Plagued(),
            new Vigilant(),
            new Curious(),
            //new Healer(),
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
            new Drunkard(),
            new Pessimist(),
            new Lazy(),
            new Coward(),
            new Berserked(),
            new Catatonic(),
            new Griefstricken(),
            new Heartbroken(),
            new Cultist(),
            new Disillusioned(),
            new Chaste(),
            new Lustful(),
            new Edible(),
            new ElementalMaster(),
            new Paralyzed(),
        };
        for (int i = 0; i < instancedTraits.Length; i++) {
            Trait trait = instancedTraits[i];
            _allTraits.Add(trait.name, trait);
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
            if (string.Equals(currTrait.name, traitName, StringComparison.OrdinalIgnoreCase)) { //|| string.Equals(currTrait.GetType().ToString(), traitName, StringComparison.OrdinalIgnoreCase)
                return true;
            }
        }
        return false;
    }
    public Trait CreateNewInstancedTraitClass(string traitName) {
        string noSpacesTraitName = Utilities.RemoveAllWhiteSpace(traitName);
        string typeName = $"Traits.{ noSpacesTraitName }";
        Type type = System.Type.GetType(typeName);
        return System.Activator.CreateInstance(type) as Trait;
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
        List<string> allBuffs = new List<string>(buffTraitPool);
        for (int i = 0; i < character.traitContainer.allTraits.Count; i++) {
            Trait trait = character.traitContainer.allTraits[i];
            if (trait.mutuallyExclusive != null) {
                allBuffs = Utilities.RemoveElements(allBuffs, trait.mutuallyExclusive);
            }
        }
        return allBuffs;
    }
    /// <summary>
    /// Utility function to determine if this character's flaws can still be activated
    /// </summary>
    /// <returns></returns>
    public bool CanStillTriggerFlaws(Character character) {
        if (character.isDead) {
            return false;
        }
        if (character.faction.isPlayerFaction) {
            return false;
        }
        if (character.role.roleType == CHARACTER_ROLE.BEAST) {
            return false;
        }
        if (character is Summon) {
            return false;
        }
        if (character.returnedToLife) {
            return false;
        }
        //if(doNotDisturb > 0) {
        //    return false;
        //}
        return true;
    }
    #endregion

    #region Trait Processors
    private void CreateTraitProcessors() {
        characterTraitProcessor = new CharacterTraitProcessor();
        tileObjectTraitProcessor = new TileObjectTraitProcessor();
        specialTokenTraitProcessor = new SpecialTokenTraitProcessor();
        defaultTraitProcessor = new DefaultTraitProcessor();
    }
    #endregion
}
