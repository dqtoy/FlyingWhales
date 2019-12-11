namespace Events.World_Events {
    public class AttackNonDemonicRegion : WorldEvent {

        public AttackNonDemonicRegion() : base(WORLD_EVENT.ATTACK_NON_DEMONIC_REGION) {
            eventEffects = new WORLD_EVENT_EFFECT[] { WORLD_EVENT_EFFECT.ATTACK_NON_DEMONIC_REGION };
            description = "This mission will attack a non-demonic region.";
            duration = GameManager.Instance.GetTicksBasedOnHour(6);
        }

        #region Overrides
        protected override void ExecuteAfterEffect(Region region, Character spawner) {
            Log log = CreateNewEventLog("after_effect", region);
            log.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotification(log);
            //TODO: Do effect
            base.ExecuteAfterEffect(region, spawner);
        }
        public override bool CanSpawnEventAt(Region region, Character spawner) {
            //- requirement: Actor is Purifier  + Region Criteria from Job
            //if (spawner.traitContainer.GetNormalTrait("Purifier") == null) {
            //    return false;
            //}
            //- region is corrupted
            if (region.coreTile.isCorrupted) {
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
