
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenManager : MonoBehaviour {
    public static TokenManager Instance;

    //private Dictionary<int, Intel> _intelLookup;

    private Dictionary<string, SpecialToken> specialTokens = new Dictionary<string, SpecialToken>() {
        { "Blighted Potion", new BlightedPotion() },
        { "Book Of The Dead", new BookOfTheDead() },
        { "Charm Spell", new CharmSpell() },
        { "Fear Spell", new FearSpell() },
        { "Book Of Wizardry", new BookOfWizardry() },
        { "Brand Of The Beastmaster", new BrandOfTheBeastmaster() },
        { "Mark Of The Witch", new MarkOfTheWitch() }
    };

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
        foreach (KeyValuePair<string, SpecialToken> item in specialTokens) {
            Messenger.Broadcast(Signals.SPECIAL_TOKEN_CREATED, item.Value);
        }

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

    public SpecialToken GetSpecialToken(string name) {
        return specialTokens[name];
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
