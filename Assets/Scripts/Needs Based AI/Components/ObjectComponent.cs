using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 Base Class for object components
     */
public class ObjectComponent : MonoBehaviour {

    public List<ObjectState> states;

    #region getters/setters
    public virtual new string name {
        get { return this.gameObject.name; }
    }
    #endregion
}
