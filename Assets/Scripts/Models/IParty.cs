using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

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
    Area home { get; }
    StructureObj homeStructure { get; }
    Combat currentCombat { get; set; }
    BaseLandmark landmarkLocation { get; }
    ICharacter mainCharacter { get; }
    ICharacterObject icharacterObject { get; }
    ILocation specificLocation { get; }
    List<ICharacter> icharacters { get; }

    void GoHome();
    void AddCharacter(ICharacter icharacter);
    void RemoveCharacter(ICharacter icharacter);
    void AdvertiseSelf(ActionThread actionThread);
    void SetSpecificLocation(ILocation location);
}
