using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class RelationshipSaveData {
    public int sourceCharacterID;
    public int targetCharacterID;
    public List<RELATIONSHIP_TRAIT> rels;

    public RelationshipSaveData(CharacterRelationshipData rel) {
        sourceCharacterID = rel.owner.id;
        targetCharacterID = rel.targetCharacter.id;
        rels = new List<RELATIONSHIP_TRAIT>(rel.rels.Select(x => x.relType).ToList());
    }
}
