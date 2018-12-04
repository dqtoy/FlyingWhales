using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RaceManager : MonoBehaviour {
    public static RaceManager Instance;

    private Dictionary<string, RaceSetting> _racesDictionary;

    #region getters/setters
    public Dictionary<string, RaceSetting> racesDictionary {
        get { return _racesDictionary; }
    }
    #endregion

    void Awake() {
        Instance = this;    
    }

    public void Initialize() {
        ConstructAllRaces();
    }

    private void ConstructAllRaces() {
        _racesDictionary = new Dictionary<string, RaceSetting>();
        string path = Utilities.dataPath + "RaceSettings/";
        string[] races = System.IO.Directory.GetFiles(path, "*.json");
        for (int i = 0; i < races.Length; i++) {
            RaceSetting currentRace = JsonUtility.FromJson<RaceSetting>(System.IO.File.ReadAllText(races[i]));
            _racesDictionary.Add(currentRace.race.ToString(), currentRace);
        }
    }
}
