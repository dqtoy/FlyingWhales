using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntelManager : MonoBehaviour {
    public static IntelManager Instance;

    private Dictionary<int, Intel> _intelLookup;

    #region getters/setters
    public Dictionary<int, Intel> intelLookup {
        get { return _intelLookup; }
    }
    #endregion

    void Awake() {
        Instance = this;
    }

    public void Initialize() {
        ConstructAllIntel();
    }

    private void ConstructAllIntel() {
        _intelLookup = new Dictionary<int, Intel> ();
        string path = Utilities.dataPath + "Intels/";
        string[] intels = System.IO.Directory.GetFiles(path, "*.json");
        for (int i = 0; i < intels.Length; i++) {
            //JsonUtility.FromJsonOverwrite(System.IO.File.ReadAllText(classes[i]), monsterComponent);
            Intel intel = JsonUtility.FromJson<Intel>(System.IO.File.ReadAllText(intels[i]));
            _intelLookup.Add(intel.id, intel);
        }
    }
}
