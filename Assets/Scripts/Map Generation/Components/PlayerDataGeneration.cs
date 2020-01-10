using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataGeneration : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
		PlayerManager.Instance.InitializePlayer(data.portal);
		yield return null;
	}
}
