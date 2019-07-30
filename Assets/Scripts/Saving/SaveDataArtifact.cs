using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataArtifact {
    public int id;
    public ARTIFACT_TYPE type;
    public int level;
    
    public void Save(Artifact artifact) {
        id = artifact.id;
        type = artifact.type;
        level = artifact.level;
    }

    public void Load(Player player) {
        Artifact artifact = PlayerManager.Instance.CreateNewArtifact(this);
        player.AddAnArtifact(artifact);
    }
}
