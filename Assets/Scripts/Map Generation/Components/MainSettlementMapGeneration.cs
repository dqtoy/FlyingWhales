using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSettlementMapGeneration : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
		yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.GenerateAreaMap(
			data.settlement.tileLocation.areaOfTile));
	}
}
