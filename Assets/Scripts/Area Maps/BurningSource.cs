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

    private Area location;

    public BurningSource(Area location) {
        id = Utilities.SetID(this);
        dousers = new List<Character>();
        objectsOnFire = new List<IPointOfInterest>();
        this.location = location;
        location.areaMap.AddActiveBurningSource(this);
    }

    public BurningSource(Area location, SaveDataBurningSource source) {
        id = source.id;
        dousers = new List<Character>();
        objectsOnFire = new List<IPointOfInterest>();
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
    //TODO:
    /// <summary>
    /// Activate all characters that are in the dousing fire list.
    /// NOTE: This is only for loading.
    /// </summary>
    //public void ActivateCharactersDousingFire() {
    //    for (int i = 0; i < dousers.Count; i++) {
    //        Character currDouser = dousers[i];
    //        CharacterStateJob existingJob = currDouser.jobQueue.GetJob(JOB_TYPE.REMOVE_FIRE) as CharacterStateJob;
    //        if (existingJob == null) {
    //            CharacterStateJob job = new CharacterStateJob(JOB_TYPE.REMOVE_FIRE, CHARACTER_STATE.DOUSE_FIRE);
    //            existingJob = job;
    //            currDouser.jobQueue.AddJobInQueue(job);
    //        }
    //        existingJob.AddOnUnassignAction(this.RemoveCharactersDousingFire); //This is the action responsible for reducing the number of characters dousing the fire when a character decides to quit the job.
    //        currDouser.CancelAllPlans(); //cancel all other plans except douse fire.
    //        currDouser.jobQueue.ProcessFirstJobInQueue(currDouser);

    //        if (currDouser.stateComponent.currentState is DouseFireState) {
    //            DouseFireState state = currDouser.stateComponent.currentState as DouseFireState;
    //            for (int j = 0; j < objectsOnFire.Count; j++) {
    //                IPointOfInterest poi = objectsOnFire[j];
    //                state.OnTraitableGainedTrait(poi, poi.traitContainer.GetNormalTrait("Burning"));
    //            }
    //            state.DetermineAction();
    //        }
    //    }
    //}

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
    public void AddObjectOnFire(IPointOfInterest poi) {
        objectsOnFire.Add(poi);
        onBurningObjectAdded?.Invoke(poi);
    }
    public void RemoveObjectOnFire(IPointOfInterest poi) {
        if (objectsOnFire.Remove(poi)) {
            onBurningObjectRemoved?.Invoke(poi);
            if (objectsOnFire.Count == 0) {
                onAllBurningExtinguished?.Invoke(this);
                location.areaMap.RemoveActiveBurningSources(this);
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