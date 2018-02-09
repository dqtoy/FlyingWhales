using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MaterialPreference {
	public List<MATERIAL> prioritizedMaterials;
	public MATERIAL neededMaterial;

	public MaterialPreference(List<MATERIAL> materials){
		prioritizedMaterials = materials;
		neededMaterial = materials [0];
	}
}
