using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public struct LandmarkPartyData {
    public List<ICharacter> partyMembers;
    public CharacterAction action;
    public int currentDuration;
}
