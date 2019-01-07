
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
                    switch (currSetting.tokenName) {
                        case "Mark of the Witch":
                            createdToken = new MarkOfTheWitch();
                            break;
                        case "Brand of the Beastmaster":
                            createdToken = new BrandOfTheBeastmaster();
                            break;
                        case "Book of Wizardry":
                            createdToken = new BookOfWizardry();
                            break;
                        case "Blighted Potion":
                            createdToken = new BlightedPotion();
                            break;
                        case "Fear Spell":
                            createdToken = new FearSpell();
                            break;
                        case "Charm Spell":
                            createdToken = new CharmSpell();
                            break;
                        //case "Scroll of Power":
                        //    chosenArea.AddSpecialTokenToLocation(new ScrollOf());
                        //    break;
                        case "Book of the Dead":
                            createdToken = new BookOfTheDead();
                            break;
                        case "Secret Scroll":
                            createdToken = new SecretScroll();
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
            areas.Add(LandmarkManager.Instance.GetAreaByName(areaName));
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
    public int quantity;
    public int appearanceWeight;
    public List<string> areaLocations;
}
