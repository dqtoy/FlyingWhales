using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataArtifactSlot {
    public int id;
    public TILE_OBJECT_TYPE type;
    public int level;
    
    public void Save(ArtifactSlot slot) {
        if(slot.artifact != null) {
            id = slot.artifact.id;
            type = slot.artifact.tileObjectType;
        } else {
            id = -1;
        }
        level = slot.level;
    }

    public ArtifactSlot Load() {
        ArtifactSlot slot = new ArtifactSlot();
        slot.SetLevel(level);
        if(id != -1) {
            Artifact artifact = InteriorMapManager.Instance.GetTileObject(type, id) as Artifact;
            slot.SetArtifact(artifact);
        }
        return slot;
    }
}
