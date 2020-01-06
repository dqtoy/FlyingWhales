using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuppotingSettlementMapGeneration : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
		yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.CreateTwoNewSettlementsAtTheStartOfGame());
	}
}
