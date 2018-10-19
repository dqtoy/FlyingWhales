using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour {

    public static InteractionManager Instance = null;

    public static readonly string Supply_Cache_Reward_1 = "SupplyCacheReward1";
    public static readonly string Mana_Cache_Reward_1 = "ManaCacheReward1";
    public static readonly string Mana_Cache_Reward_2 = "ManaCacheReward2";
    public static readonly string Exp_Reward_1 = "ExpReward1";
    public static readonly string Exp_Reward_2 = "ExpReward2";

    public Dictionary<string, RewardConfig> rewardConfig = new Dictionary<string, RewardConfig>(){
        { Supply_Cache_Reward_1, new RewardConfig(){ rewardType = REWARD.SUPPLY, lowerRange = 50, higherRange = 250 } },
        { Mana_Cache_Reward_1, new RewardConfig(){ rewardType = REWARD.MANA, lowerRange = 5, higherRange = 30 } },
        { Mana_Cache_Reward_2, new RewardConfig(){ rewardType = REWARD.MANA, lowerRange = 30, higherRange = 50 } },
        { Exp_Reward_1, new RewardConfig(){ rewardType = REWARD.EXP, lowerRange = 40, higherRange = 40 } },
        { Exp_Reward_2, new RewardConfig(){ rewardType = REWARD.EXP, lowerRange = 80, higherRange = 80 } },
    };

    private void Awake() {
        Instance = this;
    }

    public Interaction CreateNewInteraction(INTERACTION_TYPE interactionType, IInteractable interactable) {
        switch (interactionType) {
            case INTERACTION_TYPE.BANDIT_RAID:
                return new BanditRaid(interactable);
            case INTERACTION_TYPE.INVESTIGATE:
                return new InvestigateInteraction(interactable);
            case INTERACTION_TYPE.POI_1:
                return new PointOfInterest1(interactable);
            case INTERACTION_TYPE.POI_2:
                return new PointOfInterest2(interactable);
            case INTERACTION_TYPE.HARVEST_SEASON:
                return new HarvestSeason(interactable);
            default:
                return null;
        }
    }

    public Reward GetReward(string rewardName) {
        if (rewardConfig.ContainsKey(rewardName)) {
            RewardConfig config = rewardConfig[rewardName];
            return new Reward { rewardType = config.rewardType, amount = Random.Range(config.lowerRange, config.higherRange) };
        }
        throw new System.Exception("There is no reward configuration with name " + rewardName);
    }
}

public struct RewardConfig {
    public REWARD rewardType;
    public int lowerRange;
    public int higherRange;
}
public struct Reward {
    public REWARD rewardType;
    public int amount;
}