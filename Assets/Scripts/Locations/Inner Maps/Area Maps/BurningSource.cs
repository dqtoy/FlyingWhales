using System.Collections;
using System.Collections.Generic;
using Traits;

public class BurningSource {

    public int id { get; private set; }
    public List<ITraitable> objectsOnFire { get; private set; }
    public DelegateTypes.OnAllBurningExtinguished onAllBurningExtinguished { get; private set; }
    public DelegateTypes.OnBurningObjectAdded onBurningObjectAdded { get; private set; }
    public DelegateTypes.OnBurningObjectRemoved onBurningObjectRemoved { get; private set; }

    private ILocation location;

    public BurningSource(ILocation location) {
        id = Utilities.SetID(this);
        new List<Character>();
        objectsOnFire = new List<ITraitable>();
        this.location = location;
        location.innerMap.AddActiveBurningSource(this);
    }
    public void AddObjectOnFire(ITraitable poi) {
        objectsOnFire.Add(poi);
        onBurningObjectAdded?.Invoke(poi);
    }
    public void RemoveObjectOnFire(ITraitable poi) {
        if (objectsOnFire.Remove(poi)) {
            onBurningObjectRemoved?.Invoke(poi);
            if (objectsOnFire.Count == 0) {
                onAllBurningExtinguished?.Invoke(this);
                location.innerMap.RemoveActiveBurningSources(this);
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

    public bool HasFireInSettlement(Settlement settlement) {
        for (int i = 0; i < objectsOnFire.Count; i++) {
            ITraitable traitable = objectsOnFire[i];
            if (traitable.gridTileLocation.IsPartOfSettlement(settlement)) {
                return true;
            }
        }
        return false;
    }
    
    public override string ToString() {
        return $"Burning Source {id.ToString()}. Objects: {objectsOnFire.Count.ToString()}";
    }
}

[System.Serializable]
public class SaveDataBurningSource {
    public int id;

    public void Save(BurningSource bs) {
        id = bs.id;
    }
}