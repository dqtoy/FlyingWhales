#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;

using UnityEditor;
// ---------------
//  String => Int
// ---------------
[UnityEditor.CustomPropertyDrawer(typeof(StringIntDictionary))]
public class StringIntDictionaryDrawer : SerializableDictionaryDrawer<string, int> {
    protected override SerializableKeyValueTemplate<string, int> GetTemplate() {
        return GetGenericTemplate<SerializableStringIntTemplate>();
    }
}
internal class SerializableStringIntTemplate : SerializableKeyValueTemplate<string, int> {}
 
// ---------------
//  GameObject => Float
// ---------------
[UnityEditor.CustomPropertyDrawer(typeof(GameObjectFloatDictionary))]
public class GameObjectFloatDictionaryDrawer : SerializableDictionaryDrawer<GameObject, float> {
    protected override SerializableKeyValueTemplate<GameObject, float> GetTemplate() {
        return GetGenericTemplate<SerializableGameObjectFloatTemplate>();
    }
}
internal class SerializableGameObjectFloatTemplate : SerializableKeyValueTemplate<GameObject, float> {}

// ---------------
//  Storyline => Bool
// ---------------
[UnityEditor.CustomPropertyDrawer(typeof(StorylineBoolDictionary))]
public class StorylineBoolDictionaryDrawer : SerializableDictionaryDrawer<STORYLINE, bool> {
    protected override SerializableKeyValueTemplate<STORYLINE, bool> GetTemplate() {
        return GetGenericTemplate<SerializableStorylineBoolTemplate>();
    }
}
internal class SerializableStorylineBoolTemplate : SerializableKeyValueTemplate<STORYLINE, bool> { }
#endif