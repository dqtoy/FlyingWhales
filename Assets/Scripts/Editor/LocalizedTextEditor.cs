#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LocalizedTextEditor : EditorWindow{
	public LocalizationData localizationData;

	LANGUAGES language = LANGUAGES.NONE;
	Vector2 scrollPos = Vector2.zero;

	[MenuItem ("Window/Localized Text Editor")]
	static void Init()
	{
		EditorWindow.GetWindow (typeof(LocalizedTextEditor)).Show ();
	}

	private void OnGUI(){
		this.scrollPos = EditorGUILayout.BeginScrollView (this.scrollPos, GUILayout.Width(this.position.width), GUILayout.Height(this.position.height));
		this.language = (LANGUAGES)EditorGUILayout.EnumPopup ("Select Language: ", this.language);
		if(this.language != LANGUAGES.NONE){
			if (localizationData != null) 
			{
				SerializedObject serializedObject = new SerializedObject (this);
				SerializedProperty serializedProperty = serializedObject.FindProperty ("localizationData");
				EditorGUILayout.PropertyField (serializedProperty, true);
				serializedObject.ApplyModifiedProperties ();

				if (GUILayout.Button ("Save data")) 
				{
					SaveGameData ();
				}
			}

			if (GUILayout.Button ("Load data")) 
			{
				LoadGameData ();
			}

			if (GUILayout.Button ("Create new data")) 
			{
				CreateNewData ();
			}
		}
		EditorGUILayout.EndScrollView ();
	}


	private void LoadGameData(){
		string filePath = EditorUtility.OpenFilePanel ("Select localization data file", Application.streamingAssetsPath + "/" + this.language, "json");

		if (!string.IsNullOrEmpty (filePath)) 
		{
			string dataAsJson = File.ReadAllText (filePath);

			localizationData = JsonUtility.FromJson<LocalizationData> (dataAsJson);
		}
	}

	private void SaveGameData(){
		string filePath = EditorUtility.SaveFilePanel ("Save localization data file", Application.streamingAssetsPath + "/" + this.language, "", "json");

		if (!string.IsNullOrEmpty(filePath))
		{
			string dataAsJson = JsonUtility.ToJson(localizationData);
			File.WriteAllText (filePath, dataAsJson);
		}
	}

	private void CreateNewData(){
		localizationData = new LocalizationData ();
	}
}