using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurningSource {

    public int id { get; private set; }
    public List<Character> dousers { get; private set; }
    public List<IPointOfInterest> objectsOnFire { get; private set; }
    public DelegateTypes.OnAllBurningExtinguished onAllBurningExtinguished { get; private set; }
    public DelegateTypes.OnBurningObjectAdded onBurningObjectAdded { get; private set; }
    public DelegateTypes.OnBurningObjectRemoved onBurningObjectRemoved { get; private set; }


    public BurningSource() {
        id = Utilities.SetID(this);
        dousers = new List<Character>();
        objectsOnFire = new List<IPointOfInterest>();
    }

    public void AddCharactersDousingFire(Character character) {
        dousers.Add(character);
    }
    public void RemoveCharactersDousingFire(Character character) {
        dousers.Remove(character);
    }
    public Character GetNearestDouserFrom(Character otherCharacter) {
        Character nearest = null;
        float nearestDist = 9999f;
        for (int i = 0; i < dousers.Count; i++) {
            Character currDouser = dousers[i];
            float dist = Vector2.Distance(currDouser.gridTileLocation.localLocation, otherCharacter.gridTileLocation.localLocation);
            if (dist < nearestDist) {
                nearest = currDouser;
                nearestDist = dist;
            }
        }
        return nearest;
    }
    public void AddObjectOnFire(IPointOfInterest poi) {
        objectsOnFire.Add(poi);
        onBurningObjectAdded?.Invoke(poi);
    }
    public void RemoveObjectOnFire(IPointOfInterest poi) {
        if (objectsOnFire.Remove(poi)) {
            onBurningObjectRemoved?.Invoke(poi);
            if (objectsOnFire.Count == 0) {
                onAllBurningExtinguished?.Invoke(this);
            }
        }
    }
    public void AddOnBurningExtinguishedAction(DelegateTypes.OnAllBurningExtinguished action) {
        onAllBurningExtinguished += action;
    }
    public void RemoveOnBurningExtinguishedAction(DelegateTypes.OnAllBurningExtinguished action) {
        onAllBurningExtinguished -= action;
    }
    public void AddOnBurningObjectAddedAction(DelegateTypes.OnBurningObjectAdded action) {
        onBurningObjectAdded += action;
    }
    public void RemoveOnBurningObjectAddedAction(DelegateTypes.OnBurningObjectAdded action) {
        onBurningObjectAdded -= action;
    }
    public void AddOnBurningObjectRemovedAction(DelegateTypes.OnBurningObjectRemoved action) {
        onBurningObjectRemoved += action;
    }
    public void RemoveOnBurningObjectRemovedAction(DelegateTypes.OnBurningObjectRemoved action) {
        onBurningObjectRemoved -= action;
    }
}
