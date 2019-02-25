
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenManager : MonoBehaviour {
    public static TokenManager Instance;

    //private Dictionary<int, Intel> _intelLookup;

    //private Dictionary<string, int> specialTokens = new Dictionary<string, int>() {
    //    { "Blighted Potion", 4 },
    //    { "Book Of The Dead", 1 },
    //    { "Charm Spell", 4 },
    //    { "Fear Spell", 4 },
    //    { "Book Of Wizardry", 1 },
    //    { "Brand Of The Beastmaster", new BrandOfTheBeastmaster() },
    //    { "Mark Of The Witch", new MarkOfTheWitch() },
    //    { "Secret Scroll", new SecretScroll() },
    //};

    public List<SpecialTokenSettings> specialTokenSettings;

    #region getters/setters
    //public Dictionary<int, Intel> intelLookup {
    //    get { return _intelLookup; }
    //}
    #endregion

    void Awake() {
        Instance = this;
    }

    public void Initialize() {
        LoadSpecialTokens();
    }

    private void LoadSpecialTokens() {
        for (int i = 0; i < specialTokenSettings.Count; i++) {
            SpecialTokenSettings currSetting = specialTokenSettings[i];
            List<Area> areas = GetPossibleAreaSpawns(currSetting);
            if (areas.Count <= 0) {
                continue; //skip
            }
            for (int j = 0; j < currSetting.quantity; j++) {
                if (Random.Range(0, 100) < currSetting.appearanceWeight) {
                    Area chosenArea = areas[Random.Range(0, areas.Count)];
                    SpecialToken createdToken = CreateSpecialToken(currSetting.tokenType, currSetting.appearanceWeight);
                    if (createdToken != null) {
                        chosenArea.AddSpecialTokenToLocation(createdToken);
                        createdToken.SetOwner(chosenArea.owner);
                        Messenger.Broadcast<SpecialToken>(Signals.SPECIAL_TOKEN_CREATED, createdToken);
                    }
                }
            }
        }
    }
    public SpecialToken CreateSpecialToken(SPECIAL_TOKEN tokenType, int appearanceWeight = 0) {
        switch (tokenType) {
            case SPECIAL_TOKEN.BLIGHTED_POTION:
                return new BlightedPotion();
            case SPECIAL_TOKEN.BOOK_OF_THE_DEAD:
                return new BookOfTheDead();
            case SPECIAL_TOKEN.CHARM_SPELL:
                return new CharmSpell();
            case SPECIAL_TOKEN.FEAR_SPELL:
                return new FearSpell();
            case SPECIAL_TOKEN.MARK_OF_THE_WITCH:
                return new MarkOfTheWitch();
            case SPECIAL_TOKEN.BRAND_OF_THE_BEASTMASTER:
                return new BrandOfTheBeastmaster();
            case SPECIAL_TOKEN.BOOK_OF_WIZARDRY:
                return new BookOfWizardry();
            case SPECIAL_TOKEN.SECRET_SCROLL:
                return new SecretScroll();
            case SPECIAL_TOKEN.MUTAGENIC_GOO:
                return new MutagenicGoo();
            case SPECIAL_TOKEN.DISPEL_SCROLL:
                return new DispelScroll();
            case SPECIAL_TOKEN.PANACEA:
                return new Panacea();
            case SPECIAL_TOKEN.ENCHANTED_AMULET:
                return new EnchantedAmulet();
            case SPECIAL_TOKEN.GOLDEN_NECTAR:
                return new GoldenNectar();
            case SPECIAL_TOKEN.SCROLL_OF_POWER:
                return new ScrollOfPower();
            case SPECIAL_TOKEN.ACID_FLASK:
                return new AcidFlask();
            default:
                return new SpecialToken(tokenType, appearanceWeight);
        }
    }
    public SpecialTokenSettings GetTokenSettings(SPECIAL_TOKEN tokenType) {
        for (int i = 0; i < specialTokenSettings.Count; i++) {
            if(specialTokenSettings[i].tokenType == tokenType) {
                return specialTokenSettings[i];
            }
        }
        return null;
    }
    public List<Area> GetPossibleAreaSpawns(SpecialTokenSettings setting) {
        List<Area> areas = new List<Area>();
        for (int i = 0; i < setting.areaLocations.Count; i++) {
            string areaName = setting.areaLocations[i];
            Area area = LandmarkManager.Instance.GetAreaByName(areaName);
            if (area == null) {
                //throw new System.Exception("There is no area named " + areaName);
            } else {
                areas.Add(area);
            }
        }
        return areas;
    }

    //public SpecialToken GetSpecialToken(string name) {
    //    return null;
    //}

    //public Intel GetIntel(int id) {
    //    return _intelLookup[id];
    //}

    //public List<Intel> GetIntelConcerning(Character character) {
    //    List<Intel> intel = new List<Intel>();
    //    foreach (KeyValuePair<int, Intel> kvp in _intelLookup) {
    //        Intel currIntel = kvp.Value;
    //        if (currIntel.description.Contains(character.name) || currIntel.name.Contains(character.name)) {
    //            if (!intel.Contains(currIntel)) {
    //                intel.Add(currIntel);
    //            }
    //        }
    //    }
    //    return intel;
    //}

    public List<Token> GetIntelConcerning(List<Character> character) {
        List<Token> intel = new List<Token>();
        List<Token> intelForCharacter = GetIntelConcerning(character);
        for (int i = 0; i < intelForCharacter.Count; i++) {
            Token currIntelForCharacter = intelForCharacter[i];
            if (!intel.Contains(currIntelForCharacter)) {
                intel.Add(currIntelForCharacter);
            }
        }
        return intel;
    }

    //public List<Intel> GetIntelConcerning(List<Party> parties) {
    //    List<Intel> intel = new List<Intel>();
    //    for (int i = 0; i < parties.Count; i++) {
    //        Party currParty = parties[i];
    //        if (!(currParty is CharacterParty)) {
    //            continue; //skip non character parties
    //        }
    //        for (int j = 0; j < currParty.icharacters.Count; j++) {
    //            ICharacter currCharacter = currParty.icharacters[j];
    //            if (currCharacter is Character) {
    //                List<Intel> intelForCharacter = GetIntelConcerning(currCharacter as Character);
    //                for (int k = 0; k < intelForCharacter.Count; k++) {
    //                    Intel currIntelForCharacter = intelForCharacter[k];
    //                    if (!intel.Contains(currIntelForCharacter)) {
    //                        intel.Add(currIntelForCharacter);
    //                    }
    //                }
    //            }
    //        }
    //    }
        
    //    return intel;
    //}
}

[System.Serializable]
public class SpecialTokenSettings {
    public string tokenName;
    public SPECIAL_TOKEN tokenType;
    public int quantity;
    public int appearanceWeight;
    public List<string> areaLocations;
}
