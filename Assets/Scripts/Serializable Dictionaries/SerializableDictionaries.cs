using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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
public class RaceClassListDictionary : SerializableDictionary<RACE, List<RaceAreaDefenderSetting>, RaceDefenderListStorage> { }
[System.Serializable]
public class RolePortraitFramesDictionary : SerializableDictionary<CHARACTER_ROLE, PortraitFrame> { }
[System.Serializable]
public class BiomeSpriteAnimationDictionary : SerializableDictionary<Sprite, RuntimeAnimatorController> { }
[System.Serializable]
public class LogReplacerDictionary : SerializableDictionary<string, LOG_IDENTIFIER> { }
[System.Serializable]
public class StringSpriteDictionary : SerializableDictionary<string, Sprite> { }
[System.Serializable]
public class FactionEmblemDictionary : SerializableDictionary<int, Sprite> { }
[System.Serializable]
public class JobIconsDictionary : SerializableDictionary<JOB, Sprite> { }
[System.Serializable]
public class WallSpritesDictionary : SerializableDictionary<TwoTileDirections, Sprite> { }
[System.Serializable]
public class ItemTileBaseDictionary : SerializableDictionary<SPECIAL_TOKEN, TileBase> { }
[System.Serializable]
public class FoodTileBaseDictionary : SerializableDictionary<FOOD, TileBase> { }
[System.Serializable]
public class TileObjectTileBaseDictionary : SerializableDictionary<TILE_OBJECT_TYPE, TileObjectTileSetting> { }
[System.Serializable]
public class ItemSpriteDictionary : SerializableDictionary<SPECIAL_TOKEN, Sprite> { }
[System.Serializable]
public class TileObjectBiomeAssetDictionary : SerializableDictionary<BIOMES, BiomeTileObjectTileSetting> { }
[System.Serializable]
public class TileObjectSlotDictionary : SerializableDictionary<TileBase, List<TileObjectSlotSetting>, TileObjectSlotListStorage> { }
[System.Serializable]
public class CursorTextureDictionary : SerializableDictionary<CursorManager.Cursor_Type, Texture2D> { }
[System.Serializable]
public class AreaTypeSpriteDictionary : SerializableDictionary<AREA_TYPE, Sprite> { }
[System.Serializable]
public class SummonSettingDictionary : SerializableDictionary<SUMMON_TYPE, SummonSettings> { }
[System.Serializable]
public class ArtifactSettingDictionary : SerializableDictionary<ARTIFACT_TYPE, ArtifactSettings> { }
[System.Serializable]
public class SeamlessEdgeAssetsDictionary : SerializableDictionary<LocationGridTile.Ground_Type, List<TileBase>, TileBaseListStorage> { }


//List storage
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
[System.Serializable]
public class TileObjectSlotListStorage : SerializableDictionary.Storage<List<TileObjectSlotSetting>> { }
[System.Serializable]
public class TileBaseListStorage : SerializableDictionary.Storage<List<TileBase>> { }