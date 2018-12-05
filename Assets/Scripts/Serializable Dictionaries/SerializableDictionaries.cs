using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScheduleTemplateDictionary : SerializableDictionary<string, CharacterScheduleTemplate> { }
[System.Serializable]
public class PhaseItemDictionary : SerializableDictionary<SCHEDULE_ACTION_CATEGORY, int> { }
[System.Serializable]
public class StringIntDictionary : SerializableDictionary<string, int> { }
[System.Serializable]
public class LandmarkDefenderWeightDictionary : SerializableDictionary<DefenderSetting, int> { }
[System.Serializable]
public class InteractionWeightDictionary : SerializableDictionary<INTERACTION_TYPE, int> { }
[System.Serializable]
public class ActionCharacterTagListDictionary : SerializableDictionary<ACTION_TYPE, List<CharacterActionTagRequirement>, CharacterTagListStorage> { }
[System.Serializable]
public class BiomeLandmarkSpriteListDictionary : SerializableDictionary<BIOMES, List<LandmarkStructureSprite>, LandmarkSpriteListStorage> { }
[System.Serializable]
public class RoleInteractionsListDictionary : SerializableDictionary<CHARACTER_ROLE, List<CharacterInteractionWeight>, CharacterInteractionWeightListStorage> { }
[System.Serializable]
public class JobInteractionsListDictionary : SerializableDictionary<JOB, List<CharacterInteractionWeight>, CharacterInteractionWeightListStorage> { }
[System.Serializable]
public class TileSpriteCorruptionListDictionary : SerializableDictionary<Sprite, List<GameObject>, CorruptionObjectsListStorage> { }
[System.Serializable]
public class RaceClassListDictionary : SerializableDictionary<Race, List<RaceAreaDefenderSetting>, RaceDefenderListStorage> { }
[System.Serializable]
public class JobPortraitFramesDictionary : SerializableDictionary<JOB, PortraitFrame> { }

[System.Serializable]
public class CharacterTagListStorage : SerializableDictionary.Storage<List<CharacterActionTagRequirement>> { }
[System.Serializable]
public class LandmarkSpriteListStorage : SerializableDictionary.Storage<List<LandmarkStructureSprite>> { }
[System.Serializable]
public class CharacterInteractionWeightListStorage : SerializableDictionary.Storage<List<CharacterInteractionWeight>> { }
[System.Serializable]
public class CorruptionObjectsListStorage : SerializableDictionary.Storage<List<GameObject>> { }
[System.Serializable]
public class RaceDefenderListStorage : SerializableDictionary.Storage<List<RaceAreaDefenderSetting>> { }