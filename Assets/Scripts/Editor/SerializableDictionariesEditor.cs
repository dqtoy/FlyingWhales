#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ScheduleTemplateDictionary))]
public class ScheduleTemplateDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(PhaseItemDictionary))]
public class PhaseItemDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(StringIntDictionary))]
public class StringIntDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(LandmarkDefenderWeightDictionary))]
public class LandmarkDefenderWeightDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(ActionCharacterTagListDictionary))]
public class ActionCharacterTagDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
[CustomPropertyDrawer(typeof(BiomeLandmarkSpriteListDictionary))]
public class BiomeLandmarkSpriteListDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }


[CustomPropertyDrawer(typeof(CharacterTagListStorage))]
public class CharacterTagListStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }
[CustomPropertyDrawer(typeof(LandmarkSpriteListStorage))]
public class LandmarkSpriteListStorageStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }
#endif
