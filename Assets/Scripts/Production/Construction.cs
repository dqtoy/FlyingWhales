using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Construction : MonoBehaviour {
	public Structure structure;
	public TECHNOLOGY technology;
	public Production production;
	public List<StructureMaterial> structureMaterials;

	private Dictionary<MATERIAL, StructureMaterial> _structureMaterialsLookup;

	void Awake(){
		if(structureMaterials != null && structureMaterials.Count > 0){
			_structureMaterialsLookup = new Dictionary<MATERIAL, StructureMaterial> ();
			for (int i = 0; i < structureMaterials.Count; i++) {
				_structureMaterialsLookup.Add (structureMaterials [i].material, structureMaterials [i]);
			}
		}
	}
}
