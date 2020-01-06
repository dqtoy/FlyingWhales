using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all map generation actions.
/// </summary>
public abstract class MapGenerationComponent {

	public abstract IEnumerator Execute(MapGenerationData data);
}
