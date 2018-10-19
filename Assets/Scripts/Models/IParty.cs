using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;
using System;

public interface IParty {
    int id { get; }
    int numOfAttackers { get; set; }
    string name { get; }
    string coloredUrlName { get; }
    string urlName { get; }
    float computedPower { get; }
    MODE currentMode { get; }
    Faction attackedByFaction { get; set; }
    Faction faction { get; }
    CharacterAvatar icon { get; }
    Region currentRegion { get; }
    //Area home { get; }
    Combat currentCombat { get; set; }
    BaseLandmark landmarkLocation { get; }
    BaseLandmark homeLandmark { get; }
    CharacterAction currentAction { get; }
    ICharacter mainCharacter { get; }
    ICharacterObject icharacterObject { get; }
    ILocation specificLocation { get; }
    IActionData iactionData { get; }
    List<ICharacter> icharacters { get; }

    void EndAction();
    void GoHome(Action action = null, Action actionOnStartOfMovement = null);
    void AddCharacter(ICharacter icharacter);
    void RemoveCharacter(ICharacter icharacter);
    void AdvertiseSelf(ActionThread actionThread);
    void SetSpecificLocation(ILocation location);
}
