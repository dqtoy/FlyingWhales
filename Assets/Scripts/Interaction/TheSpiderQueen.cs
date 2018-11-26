using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheSpiderQueen : Interaction {

    private ICharacter spiderQueen;
    private BaseLandmark landmark;
    private WeightedDictionary<LandmarkDefender> assaultSpawnWeights;

    public TheSpiderQueen(IInteractable interactable) : base(interactable, INTERACTION_TYPE.SPIDER_QUEEN, 70) {
        _name = "The Spider Queen";
    }

    #region Overrides
    public override void CreateStates() {
        if (_interactable is BaseLandmark) {
            landmark = _interactable as BaseLandmark;
            //Spawn spider queen
            SpawnSpiderQueen();
            ConstructAssaultSpawnWeights();

            InteractionState startState = new InteractionState("State 1", this);
            //string startStateDesc = "Our Imp reported that the Spider Queen has been spotted out of the heavily protected hive core. Should we do something before it gets back in?";
            //startState.SetDescription(startStateDesc);
            CreateActionOptions(startState);
            //GameDate dueDate = GameManager.Instance.Today();
            //dueDate.AddHours(100);
            //startState.SetTimeSchedule(startState.actionOptions[3], dueDate); //default is do nothing

            //action option states
            InteractionState attackLocationState = new InteractionState("Attack Location", this);
            InteractionState transformRitualSuccessState = new InteractionState("Transform Ritual Success", this);
            InteractionState transformRitualFailState = new InteractionState("Transform Ritual Fail", this);
            InteractionState transformRitualCriticalFailState = new InteractionState("Transform Ritual Critical Fail", this);
            InteractionState gainSuppliesState = new InteractionState("Gain Supplies", this);
            InteractionState demonDiesState = new InteractionState("Demon Dies", this);
            InteractionState spidersAttackState = new InteractionState("Spiders Attack", this);

            attackLocationState.SetEndEffect(() => AttackLocationEffect(attackLocationState));
            transformRitualSuccessState.SetEndEffect(() => TransformRitualSuccessEffect(transformRitualSuccessState));
            transformRitualFailState.SetEndEffect(() => TransformRitualFailureEffect(transformRitualFailState));
            transformRitualCriticalFailState.SetEndEffect(() => TransformRitualCriticalFailureEffect(transformRitualCriticalFailState));
            gainSuppliesState.SetEndEffect(() => GainSuppliesEffect(gainSuppliesState));
            demonDiesState.SetEndEffect(() => DemonDiesEffect(demonDiesState));
            spidersAttackState.SetEndEffect(() => SpidersAttackEffect(spidersAttackState));


            _states.Add(startState.name, startState);
            _states.Add(attackLocationState.name, attackLocationState);
            _states.Add(transformRitualSuccessState.name, transformRitualSuccessState);
            _states.Add(transformRitualFailState.name, transformRitualFailState);
            _states.Add(transformRitualCriticalFailState.name, transformRitualCriticalFailState);
            _states.Add(gainSuppliesState.name, gainSuppliesState);
            _states.Add(demonDiesState.name, demonDiesState);
            _states.Add(spidersAttackState.name, spidersAttackState);

            SetCurrentState(startState);
        }
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "State 1") {
            ActionOption attemptToKill = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Attempt to kill it.",
                description = "We have sent %minion% to kill the Spider Queen while it is vulnerable.",
                duration = 10,
                needsMinion = true,
                neededObjects = new List<System.Type>() { typeof(IUnit) },
                effect = () => AttemptToKillItEffect(state),
            };
            ActionOption attemptToCorrupt = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 40, currency = CURRENCY.SUPPLY },
                name = "Attempt to corrupt it.",
                description = "We have sent %minion% to transform the Spider Queen into a Demon minion while it is out in the open.",
                duration = 10,
                needsMinion = true,
                neededObjects = new List<System.Type>() { typeof(Minion) },
                effect = () => AttemptToCorruptItEffect(state),
            };
            ActionOption lootLair = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Quietly loot the lair while it is busy.",
                description = "We have sent %minion% to loot the hive core while the Spider Queen and its defenders are away.",
                duration = 10,
                needsMinion = true,
                neededObjects = new List<System.Type>() { typeof(Minion) },
                effect = () => LootLairEffect(state),
            };
            state.AddActionOption(attemptToKill);
            state.AddActionOption(attemptToCorrupt);
            state.AddActionOption(lootLair);
        }
    }
    #endregion

    private void SpawnSpiderQueen() {
        MonsterParty monsterParty = new MonsterParty();
        spiderQueen = MonsterManager.Instance.CreateNewMonster("Spider Queen");
        landmark.AddCharacterHomeOnLandmark(spiderQueen);
        spiderQueen.SetOwnedParty(monsterParty);
        spiderQueen.SetCurrentParty(monsterParty);
        monsterParty.AddCharacter(spiderQueen);
        monsterParty.CreateIcon();
        monsterParty.icon.SetPosition(landmark.tileLocation.transform.position);
        landmark.AddCharacterToLocation(monsterParty);
    }

    private void AttemptToKillItEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Attack Location", 15);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == "Attack Location") {
            AttackLocation(state, chosenEffect);
        } 
    }
    private void AttemptToCorruptItEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Transform Ritual Success", 25);
        effectWeights.AddElement("Transform Ritual Failure", 10);
        effectWeights.AddElement("Transform Ritual Critical Failure", 5);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == "Transform Ritual Success") {
            TransformRitualSuccess(state, chosenEffect);
        } else if (chosenEffect == "Transform Ritual Failure") {
            TransformRitualFailure(state, chosenEffect);
        } else if (chosenEffect == "Transform Ritual Critical Failure") {
            TransformRitualCriticalFailure(state, chosenEffect);
        }
    }
    private void LootLairEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Gain Supplies", 35);
        effectWeights.AddElement("Demon dies", 5);
        effectWeights.AddElement("Spiders attack", 5);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == "Gain Supplies") {
            GainSupplies(state, chosenEffect);
        } else if (chosenEffect == "Demon dies") {
            DemonDies(state, chosenEffect);
        } else if (chosenEffect == "Spiders attack") {
            SpidersAttack(state, chosenEffect);
        }
    }

    private void AttackLocation(InteractionState state, string effectName) {
        //_states[effectName].SetDescription(state.chosenOption.assignedUnit.name + " has been sent to attack " + spiderQueen.name + " at " + landmark.landmarkName + ".");
        SetCurrentState(_states[effectName]);
        //**Note**: Queen should join combat.

        //force spawned army to raid target
        CharacterAction characterAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.ATTACK_LANDMARK);
        //state.chosenOption.assignedUnit.party.iactionData.AssignAction(characterAction, landmark.landmarkObj);
    }
    private void AttackLocationEffect(InteractionState state) {
        
    }

    private void TransformRitualSuccess(InteractionState state, string effectName) {
        //_states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " performed the Transform Ritual perfectly, even managing to hide from the Queen's protectors. The Queen was successfully corrupted and became a vessel of a powerful demon.");
        SetCurrentState(_states[effectName]);
    }
    private void TransformRitualSuccessEffect(InteractionState state) {
        //**Reward**: Gain a new Level 10 Sloth Demon
        Minion newMinion = PlayerManager.Instance.player.CreateNewMinion("Sloth", RACE.DEMON, false);
        newMinion.SetLevel(10);
        PlayerManager.Instance.player.AddMinion(newMinion);
    }
    private void TransformRitualFailure(InteractionState state, string effectName) {
        //_states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " performed the Transform Ritual but the Queen's protectors discovered him at the last minute, forcing him to flee before the ritual is complete.");
        SetCurrentState(_states[effectName]);
    }
    private void TransformRitualFailureEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        state.assignedMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
    }
    private void TransformRitualCriticalFailure(InteractionState state, string effectName) {
        //_states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " performed the Transform Ritual but the Queen's protectors discovered him at the last minute and killed him before he was able to complete the ritual.");
        SetCurrentState(_states[effectName]);
    }
    private void TransformRitualCriticalFailureEffect(InteractionState state) {
        //**Effect**: Demon is removed from Minion List
        PlayerManager.Instance.player.RemoveMinion(state.assignedMinion);
    }

    private void GainSupplies(InteractionState state, string effectName) {
        //_states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " snuck into the lair while the Spiders are busy guarding the Queen. It was able to obtain a small amount of Supplies.");
        SetCurrentState(_states[effectName]);
    }
    private void GainSuppliesEffect(InteractionState state) {
        //**Reward**: Supply Cache Reward 1, Demon gains Exp 1
        state.assignedMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        Reward reward = InteractionManager.Instance.GetReward(InteractionManager.Supply_Cache_Reward_1);
        PlayerManager.Instance.player.ClaimReward(reward);
        landmark.tileLocation.areaOfTile.PayForReward(reward);
    }
    private void DemonDies(InteractionState state, string effectName) {
        //_states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " snuck into the lair expecting light defense while the Spiders are busy guarding the Queen. It was surprised to find a group of Spinners. They killed him rather quickly.");
        SetCurrentState(_states[effectName]);
    }
    private void DemonDiesEffect(InteractionState state) {
        //**Effect**: Demon is removed from Minion List
        PlayerManager.Instance.player.RemoveMinion(state.assignedMinion);
    }
    private void SpidersAttack(InteractionState state, string effectName) {
        //_states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " was discovered while he was sneaking into the Spider Lair. The Spiders have sent out an army to attack us in retaliation.");
        SetCurrentState(_states[effectName]);
        //**Mechanics**: create a 4 army attack unit from Assault Spawn Weights 1.
        MonsterParty army = CreateAssaultArmy(4);
        //attack player area
        CharacterAction characterAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.ATTACK_LANDMARK);
        army.iactionData.AssignAction(characterAction, PlayerManager.Instance.player.playerArea.GetRandomExposedLandmark().landmarkObj);
    }
    private void SpidersAttackEffect(InteractionState state) {
        state.assignedMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1)); //**Reward**: Demon gains Exp 1
    }

    private void ConstructAssaultSpawnWeights() {
        assaultSpawnWeights = new WeightedDictionary<LandmarkDefender>();

        LandmarkDefender striker = new LandmarkDefender() {
            className = "Striker",
            armyCount = 25
        };
        LandmarkDefender spinner = new LandmarkDefender() {
            className = "Spinner",
            armyCount = 25
        };
        LandmarkDefender guardian = new LandmarkDefender() {
            className = "Guardian",
            armyCount = 25
        };

        assaultSpawnWeights.AddElement(striker, 30);
        assaultSpawnWeights.AddElement(striker, 60);
        assaultSpawnWeights.AddElement(spinner, 20);
        assaultSpawnWeights.AddElement(spinner, 40);
        assaultSpawnWeights.AddElement(guardian, 10);
        assaultSpawnWeights.AddElement(guardian, 30);
    }
    private MonsterParty CreateAssaultArmy(int unitCount) {
        MonsterParty monsterParty = new MonsterParty();
        monsterParty.CreateIcon();
        monsterParty.icon.SetPosition(landmark.tileLocation.transform.position);
        landmark.AddCharacterToLocation(monsterParty);
        for (int i = 0; i < unitCount; i++) {
            LandmarkDefender chosenDefender = assaultSpawnWeights.PickRandomElementGivenWeights();
            MonsterArmyUnit armyUnit = MonsterManager.Instance.CreateNewMonsterArmyUnit(chosenDefender.className);
            landmark.AddCharacterHomeOnLandmark(armyUnit);
            monsterParty.AddCharacter(armyUnit);
        }
        return monsterParty;
    }
}
