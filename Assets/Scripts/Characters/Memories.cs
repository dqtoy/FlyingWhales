using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Memories {
    public List<Memory> memoryList { get; private set; }

    public Memories() {
        memoryList = new List<Memory>();
    }

    public void AddMemory(GoapAction action) {
        if (!action.canBeAddedToMemory) { return; }
        if (!HasMemoryOf(action)) {
            memoryList.Add(new Memory(action));
            if (memoryList.Count > CharacterManager.CHARACTER_MAX_MEMORY) {
                memoryList.RemoveAt(0);
            }
        }
    }
    public bool RemoveMemory(GoapAction action) {
        for (int i = 0; i < memoryList.Count; i++) {
            if (memoryList[i].goapAction == action) {
                memoryList.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public bool HasMemoryOf(GoapAction goapAction) {
        for (int i = 0; i < memoryList.Count; i++) {
            if(memoryList[i].goapAction == goapAction) {
                return true;
            }
        }
        return false;
    }
}
