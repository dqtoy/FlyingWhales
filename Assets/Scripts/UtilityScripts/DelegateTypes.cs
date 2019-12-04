using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public static class DelegateTypes {

    //Burning Source
    public delegate void OnAllBurningExtinguished(BurningSource source);
    public delegate void OnBurningObjectAdded(ITraitable poi);
    public delegate void OnBurningObjectRemoved(ITraitable poi);
}
