using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class LocalizationManager : MonoBehaviour {
	public static LocalizationManager Instance;

	public LANGUAGES language;
	public string filePath;

	private Dictionary<string, Dictionary<string, Dictionary<string, string>>> localizedText = new Dictionary<string, Dictionary<string, Dictionary<string, string>>> ();
	private bool isReady = false;
	private string missingTextString = "Localized text not found";

	void Awake(){
		if(Instance == null){
			Instance = this;
		}else if (Instance != this){
			Destroy (this.gameObject);
		}
		DontDestroyOnLoad (this.gameObject);

		this.language = Utilities.defaultLanguage;
		this.filePath = Application.streamingAssetsPath + "/" + this.language.ToString();


	}

	public void LoadLocalizedTexts()
	{
		this.localizedText.Clear ();
//		string filePath = Path.Combine (this.filePath, fileName);
		string[] directories = Directory.GetDirectories(this.filePath);
		for (int i = 0; i < directories.Length; i++) {
			string subFilePath = this.filePath + "/" + directories [i];
			string[] files = Directory.GetFiles (subFilePath);
			this.localizedText.Add (directories [i], new Dictionary<string, Dictionary<string, string>>());
			for (int j = 0; j < files.Length; j++) {
				string actualFilePath = subFilePath + "/" + files [j];
				this.localizedText [directories [i]].Add (files [j], new Dictionary<string, string> ());   
				if (File.Exists (actualFilePath)) {
					string dataAsJson = File.ReadAllText (actualFilePath);
					LocalizationData loadedData = JsonUtility.FromJson<LocalizationData> (dataAsJson);

					for (int k = 0; k < loadedData.items.Length; k++) {
						this.localizedText[directories[i]][files[j]].Add (loadedData.items [k].key, loadedData.items [k].value);   
					}

					Debug.Log ("Data loaded, dictionary contains: " + this.localizedText.Count + " entries");
				} else {
					Debug.LogError ("Cannot find file!");
				}
			}
		}


		this.isReady = true;
	}

	public string GetLocalizedValue(string category, string file, string key){
		string result = this.missingTextString;
		if (this.localizedText[category][file].ContainsKey (key)){
			result = this.localizedText[category][file][key];
		}

		return result;

	}

	public bool GetIsReady(){
		return isReady;
	}
	

}
