using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObject {

    public int id { get; private set; }
    public IPointOfInterest owner { get; private set; }
    public TILE_OBJECT_TYPE tileObjectType { get; private set; }

    protected List<Trait> _traits;
    public List<Trait> traits {
        get { return _traits; }
    }

    protected void Initialize(IPointOfInterest owner, TILE_OBJECT_TYPE tileObjectType) {
        id = Utilities.SetID(this);
        this.owner = owner;
        this.tileObjectType = tileObjectType;
        _traits = new List<Trait>();
    }

	public virtual void OnTargetObject(GoapAction action) {
        
    }
    public virtual void OnDoActionToObject(GoapAction action) {
        owner.SetPOIState(POI_STATE.INACTIVE);
    }
    public virtual void OnDoneActionTowardsTarget(GoapAction action) { //called when the action towrds this object has been done aka. setting this object to active again
        owner.SetPOIState(POI_STATE.ACTIVE);
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
}
