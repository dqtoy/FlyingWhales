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
    private Trait[] instancedTraits;

    [SerializeField] private StringSpriteDictionary traitIconDictionary;

    //Trait Processors
    public static TraitProcessor characterTraitProcessor;
    public static TraitProcessor tileObjectTraitProcessor;
    public static TraitProcessor specialTokenTraitProcessor;

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
        string path = Utilities.dataPath + "CombatAttributes/";
        string[] files = Directory.GetFiles(path, "*.json");
        for (int i = 0; i < files.Length; i++) {
            Trait attribute = JsonUtility.FromJson<Trait>(System.IO.File.ReadAllText(files[i]));
            _allTraits.Add(attribute.name, attribute);
        }
        AddInstancedTraits(); //Traits with their own classes
    }

    #region Utilities
    private void AddInstancedTraits() {
        instancedTraits = new Trait[] {
            new Craftsman(),
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
        List<string> allBuffs = GetAllBuffTraits();
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
        if (character.faction == PlayerManager.Instance.player.playerFaction) {
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
    }
    #endregion
}
