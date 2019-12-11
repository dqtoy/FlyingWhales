using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionFeature  {

	public string name { get; protected set; }
    public string description { get; protected set; }
    public REGION_FEATURE_TYPE type { get; protected set; } //whether this feature is active or passive. Passive means that this trait does not require activation to proveide it's effects, Active is just the opposite
    public bool isRemovedOnActivation { get; protected set; }
    public bool isRemovedOnInvade { get; protected set; }

    #region Virtuals
    /// <summary>
    /// If this feature is an Active type, This executes any effects it may have.
    /// </summary>
    public virtual void Activate(Region region) { }
    #endregion

}
