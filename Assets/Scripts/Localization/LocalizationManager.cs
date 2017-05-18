using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class LocalizationManager : MonoBehaviour {
	public static LocalizationManager Instance;

	public LANGUAGES language;
	public string filePath;

	protected Dictionary<string, Dictionary<string, Dictionary<string, string>>> _localizedText = new Dictionary<string, Dictionary<string, Dictionary<string, string>>> ();
	private bool isReady = false;
	private string missingTextString = "Localized text not found";


	//getters and setters
	public Dictionary<string, Dictionary<string, Dictionary<string, string>>> localizedText{
		get {return this._localizedText;}
	}

	void Awake(){
		if(Instance == null){
			Instance = this;
		}else if (Instance != this){
			Destroy (this.gameObject);
		}
		DontDestroyOnLoad (this.gameObject);

		this.language = Utilities.defaultLanguage;
		this.filePath = Application.streamingAssetsPath + "/" + this.language.ToString();
		LoadLocalizedTexts ();
	}

	/*
	 * Load Localized Text From the StreamingAssets
	 * Directory, and put them in the localizedText Dictionary
	 * by category then by file name.
	 * e.g. localizedText["Events"]["BorderConflict"]
	 * */
	public void LoadLocalizedTexts(){
		this._localizedText.Clear ();
		string[] directories = Directory.GetDirectories(this.filePath);
		for (int i = 0; i < directories.Length; i++) {
			string categoryName = Path.GetFileName(directories [i]);
			if (!this._localizedText.ContainsKey(categoryName)) {
				this._localizedText.Add (categoryName, new Dictionary<string, Dictionary<string, string>> ());
			}
			string[] files = Directory.GetFiles(directories[i], "*.json");
			for (int j = 0; j < files.Length; j++) {
				string fileName = Path.GetFileNameWithoutExtension(files[j]);
				if (!this._localizedText.ContainsKey(fileName)) {
					this._localizedText [categoryName].Add (fileName, new Dictionary<string, string> ());
				}
				if (File.Exists(files[j])) {
					string dataAsJson = File.ReadAllText(files[j]);
					LocalizationData loadedData = JsonUtility.FromJson<LocalizationData> (dataAsJson);

					for (int k = 0; k < loadedData.items.Length; k++) {
						this._localizedText[categoryName][fileName].Add(loadedData.items [k].key, loadedData.items [k].value);   
					}
					Debug.Log ("Data loaded, dictionary contains: " + this._localizedText.Count + " entries");
				} else {
					Debug.LogError ("Cannot find file!");
				}
			}
		}
		this.isReady = true;
	}

	/*
	 * Get value of a key in a category in the
	 * localizedText Dictionary.
	 * */
	public string GetLocalizedValue(string category, string file, string key){
		string result = string.Empty;
		if (this._localizedText[category][file].ContainsKey (key)){
			result = this._localizedText[category][file][key];
		}

		return result;

	}

	/*
	 * Get if the LocalizationManager has loaded
	 * all the necessary files into the localizedText
	 * Dictionary.
	 * */
	public bool GetIsReady(){
		return isReady;
	}
	

}
