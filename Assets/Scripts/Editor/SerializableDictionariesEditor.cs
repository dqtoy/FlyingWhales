using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CustomPropertyDrawer(typeof(PhaseItemDictionary))]
public class PhaseItemDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(StringIntDictionary))]
public class StringIntDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(LandmarkDefenderWeightDictionary))]
public class LandmarkDefenderWeightDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(InteractionWeightDictionary))]
public class InteractionWeightDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(ActionCharacterTagListDictionary))]
public class ActionCharacterTagDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(BiomeLandmarkSpriteListDictionary))]
public class BiomeLandmarkSpriteListDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(RoleInteractionsListDictionary))]
public class RoleInteractionsListDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(JobInteractionsListDictionary))]
public class JobInteractionsListDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(TileSpriteCorruptionListDictionary))]
public class TileSpriteCorruptionListDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(RaceClassListDictionary))]
public class RaceDefenderListDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(RolePortraitFramesDictionary))]
public class JobPortraitFramesDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(BiomeSpriteAnimationDictionary))]
public class BiomeSpriteAnimationDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(LogReplacerDictionary))]
public class LogReplacerDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(StringSpriteDictionary))]
public class LocationPortraitDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(FactionEmblemDictionary))]
public class FactionEmblemDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(JobIconsDictionary))]
public class JobIconsDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(WallSpritesDictionary))]
public class WallSpritesDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(ItemTileBaseDictionary))]
public class ItemTileBaseDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(FoodTileBaseDictionary))]
public class FoodTileBaseDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(TileObjectTileBaseDictionary))]
public class TileObjectTileBaseDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(ItemSpriteDictionary))]
public class ItemSpriteDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(TileObjectBiomeAssetDictionary))]
public class TileObjectBiomeAssetDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(TileObjectSlotDictionary))]
public class TileObjectSlotDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(CursorTextureDictionary))]
public class CursorTextureDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(AreaTypeSpriteDictionary))]
public class AreaTypeSpriteDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(SummonSettingDictionary))]
public class SummonSettingDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(ArtifactSettingDictionary))]
public class ArtifactSettingDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(SeamlessEdgeAssetsDictionary))]
public class SeamlessEdgeAssetsDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(LandmarkGenerationDictionary))]
public class LandmarkGenerationDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(YieldTypeLandmarksDictionary))]
public class YieldTypeLandmarksDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }

//list storage
[CustomPropertyDrawer(typeof(CharacterTagListStorage))]
public class CharacterTagListStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }
[CustomPropertyDrawer(typeof(LandmarkSpriteListStorage))]
public class LandmarkSpriteListStorageStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }
[CustomPropertyDrawer(typeof(CharacterInteractionWeightListStorage))]
public class CharacterInteractionWeightListStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }
[CustomPropertyDrawer(typeof(CorruptionObjectsListStorage))]
public class CorruptionObjectsListStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }
[CustomPropertyDrawer(typeof(RaceDefenderListStorage))]
public class DefenderListStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }
[CustomPropertyDrawer(typeof(TileObjectSlotListStorage))]
public class TileObjectSlotListStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }
[CustomPropertyDrawer(typeof(TileBaseListStorage))]
public class TileBaseListStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }
[CustomPropertyDrawer(typeof(LandmarkTypeListStorage))]
public class LandmarkTypeListStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }


