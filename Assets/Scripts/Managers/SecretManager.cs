using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecretManager : MonoBehaviour {
    public static SecretManager Instance;

    //private Dictionary<int, Secret> _secretLookup;

    #region getters/setters
    //public Dictionary<int, Secret> secretLookup {
    //    get { return _secretLookup; }
    //}
    #endregion

    void Awake() {
        Instance = this;
    }

    public void Initialize() {
        //ConstructAllSecret();
    }

    //private void ConstructAllSecret() {
        //_secretLookup = new Dictionary<int, Secret>();
        //string path = Utilities.dataPath + "Secrets/";
        //string[] secrets = System.IO.Directory.GetFiles(path, "*.json");
        //for (int i = 0; i < secrets.Length; i++) {
        //    //JsonUtility.FromJsonOverwrite(System.IO.File.ReadAllText(classes[i]), monsterComponent);
        //    Secret secret = JsonUtility.FromJson<Secret>(System.IO.File.ReadAllText(secrets[i]));
        //    _secretLookup.Add(secret.id, secret);
        //}
    //}

    //public Secret CreateNewSecret(int id) {
    //    return _secretLookup[id].Clone();
    //}
}
