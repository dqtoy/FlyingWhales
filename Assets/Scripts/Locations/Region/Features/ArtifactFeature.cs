using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArtifactFeature : RegionFeature {
    //This is only temporary since we cannot actually add artifacts to the region this feature is added because if the artifact is added there, it will be activated, and we don't want that
    public Artifact artifact { get; private set; }
    private Region homeRegionOfArtifact;

    public ArtifactFeature() {
        name = "Artifact";
        description = "There are rumored treasures hidden in this region. You may be able to find an Artifact here after invading this region.";
        type = REGION_FEATURE_TYPE.PASSIVE;
        //isRemovedOnActivation = true;
    }

    #region Override
    //public override void Activate(Region region) {
    //    base.Activate(region);
    //    ARTIFACT_TYPE[] artifactTypes = Utilities.GetEnumValues<ARTIFACT_TYPE>().Where(x => !x.CanBeSummoned()).ToArray();
    //    Artifact artifact = PlayerManager.Instance.CreateNewArtifact(artifactTypes[Random.Range(0, artifactTypes.Length)]);
    //    if (PlayerManager.Instance.player.HasSpaceForNewArtifact()) {
    //        PlayerManager.Instance.player.GainArtifact(artifact);
    //        UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained new artifact: " + artifact.name + "!", null);
    //    } else {
    //        UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained new artifact: " + artifact.name + "!", () => PlayerManager.Instance.player.GainArtifact(artifact, true));
    //    }
    //}
    public override void OnAddFeature(Region region) {
        base.OnAddFeature(region);
        homeRegionOfArtifact = region;
        ARTIFACT_TYPE[] artifactTypes = Utilities.GetEnumValues<ARTIFACT_TYPE>().Where(x => !x.CanBeSummoned()).ToArray();
        artifact = PlayerManager.Instance.CreateNewArtifact(artifactTypes[Random.Range(0, artifactTypes.Length)]);
        Messenger.AddListener<Artifact>(Signals.PLAYER_USED_ARTIFACT, OnUsedArtifact);
    }
    public override void OnRemoveFeature(Region region) {
        base.OnRemoveFeature(region);
        Messenger.RemoveListener<Artifact>(Signals.PLAYER_USED_ARTIFACT, OnUsedArtifact);
    }
    public override void OnDemolishLandmark(Region region, LANDMARK_TYPE demolishedLandmarkType) {
        base.OnDemolishLandmark(region, demolishedLandmarkType);
        PlayerManager.Instance.player.RemoveArtifact(artifact);
    }
    #endregion

    private void OnUsedArtifact(Artifact artifact) {
        if(this.artifact == artifact) {
            this.artifact = null;
            homeRegionOfArtifact.RemoveFeature(this);
        }
    }
}
