using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataWorldObject {

    public virtual void Save(IWorldObject worldObject) { }

    public virtual IWorldObject Load() {
        return null;
    }
}

public class SaveDataArtifact : SaveDataWorldObject {
    public int id;
    public ARTIFACT_TYPE artifactType;

    public override void Save(IWorldObject worldObject) {
        base.Save(worldObject);
        if (worldObject is Artifact) {
            Artifact artifact = worldObject as Artifact;
            id = artifact.id;
            artifactType = artifact.type;
        }
    }

    public override IWorldObject Load() {
        Artifact artifact = PlayerManager.Instance.CreateNewArtifact(artifactType);
        return artifact;
    }
}

public class SaveDataSummon : SaveDataWorldObject {
    public int id;

    public override void Save(IWorldObject worldObject) {
        base.Save(worldObject);
        if (worldObject is Summon) {
            Summon summon = worldObject as Summon;
            id = summon.id;
        }
    }

    public override IWorldObject Load() {
        Summon summon = CharacterManager.Instance.GetCharacterByID(id) as Summon;
        return summon;
    }
}
