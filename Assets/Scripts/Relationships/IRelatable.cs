using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRelatable {

	string name { get; }
    IRelationshipContainer relationshipContainer { get; }

}
