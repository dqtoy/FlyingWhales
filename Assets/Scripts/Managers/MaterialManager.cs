using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MaterialManager : MonoBehaviour {
	public static MaterialManager Instance;
	private Dictionary<MATERIAL, Materials> _materialsLookup;
	private List<MATERIAL> _edibleMaterials;

	#region getters/setters
	public Dictionary<MATERIAL, Materials> materialsLookup{
		get { return _materialsLookup; }
	}
	public List<MATERIAL> edibleMaterials{
		get { return _edibleMaterials; }
	}
	#endregion

	void Awake(){
		Instance = this;
	}
	internal void Initialize(){
		ConstructMaterials ();
	}
	private void ConstructMaterials(){
		_materialsLookup = new Dictionary<MATERIAL, Materials> ();
		_edibleMaterials = new List<MATERIAL> ();
		string path = "Assets/CombatPrototype/Data/Materials/";
		string[] materials = System.IO.Directory.GetFiles(path, "*.json");
		for (int i = 0; i < materials.Length; i++) {
			string dataAsJson = System.IO.File.ReadAllText(materials[i]);
			Materials material = JsonUtility.FromJson<Materials>(dataAsJson);
			_materialsLookup.Add(material.material, material);
			if(material.isEdible){
				_edibleMaterials.Add (material.material);
			}
		}
	}
}
