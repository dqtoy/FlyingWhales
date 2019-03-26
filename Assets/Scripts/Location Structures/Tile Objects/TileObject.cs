using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObject {

    public int id { get; private set; }
    public IPointOfInterest owner { get; private set; }
    public TILE_OBJECT_TYPE tileObjectType { get; private set; }
    public Faction factionOwner { get { return null; } }
    protected List<Trait> _traits;
    public List<Trait> traits {
        get { return _traits; }
    }

    public List<string> actionHistory { get; private set; } //list of actions that was done to this object

    protected void Initialize(IPointOfInterest owner, TILE_OBJECT_TYPE tileObjectType) {
        id = Utilities.SetID(this);
        this.owner = owner;
        this.tileObjectType = tileObjectType;
        _traits = new List<Trait>();
        actionHistory = new List<string>();
    }

	public virtual void OnTargetObject(GoapAction action) {
        
    }
    public virtual void OnDoActionToObject(GoapAction action) {
        //owner.SetPOIState(POI_STATE.INACTIVE);
        AddActionToHistory(action);
    }

    #region Traits
    public bool AddTrait(string traitName) {
        return AddTrait(AttributeManager.Instance.allTraits[traitName]);
    }
    public bool AddTrait(Trait trait, Character characterResponsible = null, System.Action onRemoveAction = null) {
        if (trait.IsUnique() && GetTrait(trait.name) != null) {
            trait.SetCharacterResponsibleForTrait(characterResponsible);
            return false;
        }
        _traits.Add(trait);
        trait.SetOnRemoveAction(onRemoveAction);
        trait.SetCharacterResponsibleForTrait(characterResponsible);
        //ApplyTraitEffects(trait);
        //ApplyPOITraitInteractions(trait);
        if (trait.daysDuration > 0) {
            GameDate removeDate = GameManager.Instance.Today();
            removeDate.AddTicks(trait.daysDuration);
            SchedulingManager.Instance.AddEntry(removeDate, () => RemoveTrait(trait));
        }
        trait.OnAddTrait(owner);
        return true;
    }
    public bool RemoveTrait(Trait trait, bool triggerOnRemove = true) {
        if (_traits.Remove(trait)) {
            //UnapplyTraitEffects(trait);
            //UnapplyPOITraitInteractions(trait);
            if (triggerOnRemove) {
                trait.OnRemoveTrait(owner);
            }
            return true;
        }
        return false;
    }
    public bool RemoveTrait(string traitName, bool triggerOnRemove = true) {
        Trait trait = GetTrait(traitName);
        if (trait != null) {
            return RemoveTrait(trait, triggerOnRemove);
        }
        return false;
    }
    public void RemoveTrait(List<Trait> traits) {
        for (int i = 0; i < traits.Count; i++) {
            RemoveTrait(traits[i]);
        }
    }
    public Trait GetTrait(string traitName) {
        for (int i = 0; i < _traits.Count; i++) {
            if (_traits[i].name == traitName) {
                return _traits[i];
            }
        }
        return null;
    }
    #endregion

    #region For Testing
    protected void AddActionToHistory(GoapAction action) {
        string summary = GameManager.Instance.ConvertDayToLogString(action.executionDate) + action.actor.name + " performed " + action.goapName;
        actionHistory.Add(summary);
        if (actionHistory.Count > 50) {
            actionHistory.RemoveAt(0);
        }
    }
    public void LogActionHistory() {
        string summary = owner.ToString() + "'s action history:";
        for (int i = 0; i < actionHistory.Count; i++) {
            summary += "\n" + (i + 1).ToString() + " - " + actionHistory[i];
        }
        Debug.Log(summary);
    }
    #endregion
}
