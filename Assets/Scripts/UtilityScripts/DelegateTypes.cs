using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DelegateTypes {

    //Burning Source
    public delegate void OnAllBurningExtinguished(BurningSource source);
    public delegate void OnBurningObjectAdded(IPointOfInterest poi);
    public delegate void OnBurningObjectRemoved(IPointOfInterest poi);
}
