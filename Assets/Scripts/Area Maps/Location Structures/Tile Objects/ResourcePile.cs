using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ResourcePile : TileObject {

	public RESOURCE providedResource { get; protected set; }
    public int resourceInPile { get; protected set; }

    public ResourcePile(RESOURCE providedResource) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.OBTAIN_RESOURCE, INTERACTION_TYPE.DROP_RESOURCE, INTERACTION_TYPE.DESTROY_RESOURCE };
        this.providedResource = providedResource;
    }

    #region Virtuals
    public virtual void SetResourceInPile(int amount) {
        resourceInPile = amount;
        resourceInPile = Mathf.Max(0, resourceInPile);
    }
    public virtual void AdjustResourceInPile(int adjustment) {
        resourceInPile += adjustment;
        resourceInPile = Mathf.Max(0, resourceInPile);
    }
    public virtual bool HasResource() {
        return resourceInPile > 0;
    }
    #endregion
}
