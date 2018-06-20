using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RelationshipSaveData {
    public int sourceCharacterID;
    public int targetCharacterID;
    public List<CHARACTER_RELATIONSHIP> relationshipStatuses;

    public RelationshipSaveData(Relationship rel) {
        sourceCharacterID = rel.sourceCharacter.id;
        targetCharacterID = rel.targetCharacter.id;
        relationshipStatuses = new List<CHARACTER_RELATIONSHIP>(rel.relationshipStatuses);
    }
}
