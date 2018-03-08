using System;
 
using UnityEngine;
 
// ---------------
//  String => Int
// ---------------
[Serializable]
public class StringIntDictionary : SerializableDictionary<string, int> {}
 
// ---------------
//  GameObject => Float
// ---------------
[Serializable]
public class GameObjectFloatDictionary : SerializableDictionary<GameObject, float> {}

// ---------------
//  Storyline => Bool
// ---------------
[Serializable]
public class StorylineBoolDictionary : SerializableDictionary<STORYLINE, bool> { }
