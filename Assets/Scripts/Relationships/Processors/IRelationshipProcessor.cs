using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRelationshipProcessor {

    void OnRelationshipAdded(Relatable rel1, Relatable rel2, RELATIONSHIP_TRAIT relType);
    void OnRelationshipRemoved(Relatable rel1, Relatable rel2, RELATIONSHIP_TRAIT relType);
}
