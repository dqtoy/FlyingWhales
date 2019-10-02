using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TreasureFeature : RegionFeature {

    public TreasureFeature() {
        name = "Treasures";
        description = "There are rumored treasures hidden in this region. You may be able to find an Artifact here after invading this region.";
        type = REGION_FEATURE_TYPE.ACTIVE;
        isRemovedOnActivation = true;
    }

    #region Override
    public override void Activate() {
        base.Activate();
        ARTIFACT_TYPE[] artifactTypes = Utilities.GetEnumValues<ARTIFACT_TYPE>().Where(x => !x.CanBeSummoned()).ToArray();
        Artifact artifact = PlayerManager.Instance.CreateNewArtifact(artifactTypes[Random.Range(0, artifactTypes.Length)]);
        UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained new artifact: " + artifact.name + "!", () => PlayerManager.Instance.player.GainArtifact(artifact, true));
    }
    #endregion

}
