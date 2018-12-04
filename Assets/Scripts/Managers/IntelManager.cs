
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntelManager : MonoBehaviour {
    public static IntelManager Instance;

    //private Dictionary<int, Intel> _intelLookup;

    #region getters/setters
    //public Dictionary<int, Intel> intelLookup {
    //    get { return _intelLookup; }
    //}
    #endregion

    void Awake() {
        Instance = this;
    }

    public void Initialize() {
        //ConstructAllIntel();
    }

    //private void ConstructAllIntel() {
    //    _intelLookup = new Dictionary<int, Intel> ();
    //    string path = Utilities.dataPath + "Intels/";
    //    string[] intels = System.IO.Directory.GetFiles(path, "*.json");
    //    for (int i = 0; i < intels.Length; i++) {
    //        //JsonUtility.FromJsonOverwrite(System.IO.File.ReadAllText(classes[i]), monsterComponent);
    //        Intel intel = JsonUtility.FromJson<Intel>(System.IO.File.ReadAllText(intels[i]));
    //        _intelLookup.Add(intel.id, intel);
    //    }
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

    public List<Intel> GetIntelConcerning(List<Character> character) {
        List<Intel> intel = new List<Intel>();
        List<Intel> intelForCharacter = GetIntelConcerning(character);
        for (int i = 0; i < intelForCharacter.Count; i++) {
            Intel currIntelForCharacter = intelForCharacter[i];
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
