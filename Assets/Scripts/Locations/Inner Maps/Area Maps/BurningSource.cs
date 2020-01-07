using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class BurningSource {

    public int id { get; private set; }
    public List<Character> dousers { get; private set; }
    public List<ITraitable> objectsOnFire { get; private set; }
    public DelegateTypes.OnAllBurningExtinguished onAllBurningExtinguished { get; private set; }
    public DelegateTypes.OnBurningObjectAdded onBurningObjectAdded { get; private set; }
    public DelegateTypes.OnBurningObjectRemoved onBurningObjectRemoved { get; private set; }

    private ILocation location;

    public BurningSource(ILocation location) {
        id = Utilities.SetID(this);
        dousers = new List<Character>();
        objectsOnFire = new List<ITraitable>();
        this.location = location;
        location.innerMap.AddActiveBurningSource(this);
    }

    public BurningSource(Area location, SaveDataBurningSource source) {
        id = source.id;
        dousers = new List<Character>();
        objectsOnFire = new List<ITraitable>();
        this.location = location;
        location.areaMap.AddActiveBurningSource(this);
        //LoadCharactersDousingFire(source); //This will just add the characters dousing the fires to the list.
    }

    private void LoadCharactersDousingFire(SaveDataBurningSource source) {
        for (int i = 0; i < source.characterDouserIDs.Count; i++) {
            int id = source.characterDouserIDs[i];
            Character character = CharacterManager.Instance.GetCharacterByID(id);
            AddCharactersDousingFire(character);
        }
    }
    public void AddCharactersDousingFire(Character character) {
        if (!dousers.Contains(character)) {
            dousers.Add(character);
        }
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

    public override string ToString() {
        return "Burning Source " + id.ToString() + ". Dousers: " + dousers.Count.ToString() + ". Objects: " + objectsOnFire.Count.ToString();
    }
}

[System.Serializable]
public class SaveDataBurningSource {
    public int id;
    public List<int> characterDouserIDs;

    public void Save(BurningSource bs) {
        id = bs.id;

        characterDouserIDs = new List<int>();
        for (int i = 0; i < bs.dousers.Count; i++) {
            Character character = bs.dousers[i];
            characterDouserIDs.Add(character.id);
        }
    }
}