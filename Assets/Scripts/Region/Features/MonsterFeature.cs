using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterFeature : RegionFeature {

	public MonsterFeature() {
        name = "Monsters";
        description = "This region is home to a monster. You may able to turn it into a Summon after invading this region.";
        type = REGION_FEATURE_TYPE.ACTIVE;
        isRemovedOnActivation = true;
    }

    #region Override
    public override void Activate(Region region) {
        base.Activate(region);
        //Give a random summon
        SUMMON_TYPE[] summonTypes = Utilities.GetEnumValues<SUMMON_TYPE>().Where(x => !x.CanBeSummoned()).ToArray();
        Summon summon = CharacterManager.Instance.CreateNewSummon(summonTypes[Random.Range(0, summonTypes.Length)]);
        if (PlayerManager.Instance.player.HasSpaceForNewSummon()) {
            PlayerManager.Instance.player.GainSummon(summon);
            UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained new Summon: " + summon.summonType.SummonName(), null);
        } else {
            UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained new Summon: " + summon.summonType.SummonName(), () => PlayerManager.Instance.player.GainSummon(summon, true));
        }
    }
    #endregion
}
