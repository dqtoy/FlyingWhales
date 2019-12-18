using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArtifactFeature : RegionFeature {

    public ArtifactFeature() {
        name = "Artifact";
        description = "There are rumored treasures hidden in this region. You may be able to find an Artifact here after invading this region.";
        type = REGION_FEATURE_TYPE.PASSIVE;
        //isRemovedOnActivation = true;
    }

    #region Override
    public override void Activate(Region region) {
        base.Activate(region);
        ARTIFACT_TYPE[] artifactTypes = Utilities.GetEnumValues<ARTIFACT_TYPE>().Where(x => !x.CanBeSummoned()).ToArray();
        Artifact artifact = PlayerManager.Instance.CreateNewArtifact(artifactTypes[Random.Range(0, artifactTypes.Length)]);
        if (PlayerManager.Instance.player.HasSpaceForNewArtifact()) {
            PlayerManager.Instance.player.GainArtifact(artifact);
            UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained new artifact: " + artifact.name + "!", null);
        } else {
            UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained new artifact: " + artifact.name + "!", () => PlayerManager.Instance.player.GainArtifact(artifact, true));
        }
    }
    #endregion

}
