using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataArtifact {
    public int id;
    public ARTIFACT_TYPE type;
    public int level;
    
    public void Save(ArtifactSlot slot) {
        if(slot.artifact != null) {
            id = slot.artifact.id;
            type = slot.artifact.type;
        } else {
            id = -1;
        }
        level = slot.level;
    }

    public ArtifactSlot Load() {
        ArtifactSlot slot = new ArtifactSlot();
        slot.SetLevel(level);
        if(id != -1) {
            Artifact artifact = PlayerManager.Instance.CreateNewArtifact(this);
            slot.SetArtifact(artifact);
        }
        return slot;
    }
}
