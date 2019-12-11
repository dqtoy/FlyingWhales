namespace Events.World_Events {
    public class CleanseRegion : WorldEvent {

        public CleanseRegion() : base(WORLD_EVENT.CLEANSE_REGION) {
            eventEffects = new WORLD_EVENT_EFFECT[] { WORLD_EVENT_EFFECT.REMOVE_CORRUPTION };
            description = "This mission will cleanse a region.";
            duration = GameManager.Instance.GetTicksBasedOnHour(6);
        }

        #region Overrides
        protected override void ExecuteAfterEffect(Region region, Character spawner) {
            Log log = CreateNewEventLog("after_effect", region);
            log.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotification(log);
            LandmarkManager.Instance.UnownRegion(region);
            base.ExecuteAfterEffect(region, spawner);
        }
        public override bool CanSpawnEventAt(Region region, Character spawner) {
            //- requirement: Actor is Purifier  + Region Criteria from Job
            //if (spawner.traitContainer.GetNormalTrait("Purifier") == null) {
            //    return false;
            //}
            //- region is corrupted
            if (region.coreTile.isCorrupted == false) {
                return false;
            }
            //- region is empty
            if (region.mainLandmark.specificLandmarkType != LANDMARK_TYPE.NONE) {
                return false;
            }
            return true;
        }
        #endregion
    }
}