﻿
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
                    SpecialToken createdToken = null;
                    switch (currSetting.tokenType) {
                        case SPECIAL_TOKEN.BLIGHTED_POTION:
                            createdToken = new BlightedPotion();
                            break;
                        case SPECIAL_TOKEN.BOOK_OF_THE_DEAD:
                            createdToken = new BookOfTheDead();
                            break;
                        case SPECIAL_TOKEN.CHARM_SPELL:
                            createdToken = new CharmSpell();
                            break;
                        case SPECIAL_TOKEN.FEAR_SPELL:
                            createdToken = new FearSpell();
                            break;
                        case SPECIAL_TOKEN.MARK_OF_THE_WITCH:
                            createdToken = new MarkOfTheWitch();
                            break;
                        case SPECIAL_TOKEN.BRAND_OF_THE_BEASTMASTER:
                            createdToken = new BrandOfTheBeastmaster();
                            break;
                        case SPECIAL_TOKEN.BOOK_OF_WIZARDRY:
                            createdToken = new BookOfWizardry();
                            break;
                        case SPECIAL_TOKEN.SECRET_SCROLL:
                            createdToken = new SecretScroll();
                            break;
                        case SPECIAL_TOKEN.MUTAGENIC_GOO:
                            createdToken = new MutagenicGoo();
                            break;
                        case SPECIAL_TOKEN.DISPEL_SCROLL:
                            createdToken = new DispelScroll();
                            break;
                        case SPECIAL_TOKEN.PANACEA:
                            createdToken = new Panacea();
                            break;
                        default:
                            createdToken = new SpecialToken(currSetting.tokenType);
                            break;
                    }
                    if (createdToken != null) {
                        chosenArea.AddSpecialTokenToLocation(createdToken);
                        createdToken.SetOwner(chosenArea.owner);
                        Messenger.Broadcast<SpecialToken>(Signals.SPECIAL_TOKEN_CREATED, createdToken);
                    }
                }
            }
        }
        //foreach (KeyValuePair<string, SpecialToken> item in specialTokens) {
        //    Messenger.Broadcast(Signals.SPECIAL_TOKEN_CREATED, item.Value);
        //}

        //specialTokens = new Dictionary<string, SpecialToken>();
        //string path = Utilities.dataPath + "Tokens/";
        //string[] tokens = System.IO.Directory.GetFiles(path, "*.json");
        //for (int i = 0; i < tokens.Length; i++) {
        //    //JsonUtility.FromJsonOverwrite(System.IO.File.ReadAllText(classes[i]), monsterComponent);
        //    SpecialToken token = JsonUtility.FromJson<SpecialToken>(System.IO.File.ReadAllText(tokens[i]));
        //    switch (token.specialTokenType) {
        //        case SPECIAL_TOKEN.BLIGHTED_POTION:
        //            token = new BlightedPotion();
        //            break;
        //        case SPECIAL_TOKEN.BOOK_OF_THE_DEAD:
        //            token = new BookOfTheDead();
        //            break;
        //        default:
        //            break;
        //    }
        //    specialTokens.Add(token.name, token);
        //    Messenger.Broadcast(Signals.SPECIAL_TOKEN_CREATED, token);
        //}
    }

    public List<Area> GetPossibleAreaSpawns(SpecialTokenSettings setting) {
        List<Area> areas = new List<Area>();
        for (int i = 0; i < setting.areaLocations.Count; i++) {
            string areaName = setting.areaLocations[i];
            Area area = LandmarkManager.Instance.GetAreaByName(areaName);
            if (area == null) {
                throw new System.Exception("There is no area named " + areaName);
            } else {
                areas.Add(area);
            }
        }
        return areas;
    }

    public SpecialToken GetSpecialToken(string name) {
        return null;
    }

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
