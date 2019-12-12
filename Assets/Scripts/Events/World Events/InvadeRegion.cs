namespace Events.World_Events {
    public class InvadeRegion : WorldEvent {

        public InvadeRegion() : base(WORLD_EVENT.INVADE_REGION) {
            eventEffects = new WORLD_EVENT_EFFECT[] { WORLD_EVENT_EFFECT.INVADE_REGION };
            description = "This mission will invade a region.";
            duration = GameManager.Instance.GetTicksBasedOnHour(6);
        }

        #region Overrides
        protected override void ExecuteAfterEffect(Region region, Character spawner) {
            Log log = CreateNewEventLog("after_effect", region);
            log.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotification(log);
            //region.RemoveFactionHere(region.owner);
            region.AddFactionHere(spawner.homeRegion.owner);
            base.ExecuteAfterEffect(region, spawner);
        }
        public override bool CanSpawnEventAt(Region region, Character spawner) {
            //- requirement: Actor is Purifier  + Region Criteria from Job
            //if (spawner.traitContainer.GetNormalTrait<Trait>("Purifier") == null) {
            //    return false;
            //}
            //- region is corrupted
            if (region.coreTile.isCorrupted) {
                return false;
            }
            //- region is empty
            //if (region.mainLandmark.specificLandmarkType != LANDMARK_TYPE.NONE) {
            //    return false;
            //}
            return true;
        }
        #endregion
    }
}
