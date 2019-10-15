using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for objects that can have traits.
/// </summary>
public interface ITraitable {
    string name { get; }
    List<Trait> normalTraits { get; }
    LocationGridTile gridTileLocation { get; }

    #region Traits
    bool AddTrait(string traitName, Character characterResponsible = null, System.Action onRemoveAction = null, GoapAction gainedFromDoing = null, bool triggerOnAdd = true);
    bool AddTrait(Trait trait, Character characterResponsible = null, System.Action onRemoveAction = null, GoapAction gainedFromDoing = null, bool triggerOnAdd = true);
    bool RemoveTrait(Trait trait, bool triggerOnRemove = true, Character removedBy = null, bool includeAlterEgo = true);
    bool RemoveTrait(string traitName, bool triggerOnRemove = true, Character removedBy = null);
    void RemoveTrait(List<Trait> traits);
    List<Trait> RemoveAllTraitsByType(TRAIT_TYPE traitType);
    Trait GetNormalTrait(params string[] traitName);
    #endregion
}
