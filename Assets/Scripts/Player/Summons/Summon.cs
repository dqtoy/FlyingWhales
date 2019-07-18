using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Summon : Character {

	public SUMMON_TYPE summonType { get; private set; }
    public bool hasBeenUsed { get; private set; } //has this summon been used in the current map. TODO: Set this to false at end of invasion of map.

    public Summon(SUMMON_TYPE summonType, CharacterRole role, RACE race, GENDER gender) : base(role, race, gender) {
        this.summonType = summonType;
    }
    public Summon(SUMMON_TYPE summonType, CharacterRole role, string className, RACE race, GENDER gender) : base(role, className, race, gender) {
        this.summonType = summonType;
    }

    #region Overrides
    public override void Initialize() {
        OnUpdateRace();
        OnUpdateCharacterClass();
        UpdateIsCombatantState();

        SetMoodValue(90);

        CreateOwnParty();

        tiredness = TIREDNESS_DEFAULT;
        fullness = FULLNESS_DEFAULT;
        happiness = HAPPINESS_DEFAULT;


        hSkinColor = UnityEngine.Random.Range(-360f, 360f);
        hHairColor = UnityEngine.Random.Range(-360f, 360f);
        demonColor = UnityEngine.Random.Range(-144f, 144f);

        //supply
        //SetSupply(UnityEngine.Random.Range(10, 61)); //Randomize initial supply per character (Random amount between 10 to 60.)

        ConstructInitialGoapAdvertisementActions();
        //SubscribeToSignals(); //NOTE: Only made characters subscribe to signals when their area is the one that is currently active. TODO: Also make sure to unsubscribe a character when the player has completed their map.
        //GetRandomCharacterColor();
        //GameDate gameDate = GameManager.Instance.Today();
        //gameDate.AddTicks(1);
        //SchedulingManager.Instance.AddEntry(gameDate, () => PlanGoapActions());
    }
    protected override void OnActionStateSet(GoapAction action, GoapActionState state) { } //overriddn OnActionStateSet so that summons cannot witness other events.
    protected override void OnSuccessInvadeArea(Area area) {
        base.OnSuccessInvadeArea(area);
        //clean up
        StopCurrentAction(false);
        hasBeenUsed = false;
        specificLocation.RemoveCharacterFromLocation(this);
        DestroyMarker();
        PlayerManager.Instance.player.playerArea.AddCharacterToLocation(this);
        ClearAllAwareness();
        CancelAllJobsAndPlans();
        RemoveAllNonPersistentTraits();
        ResetToFullHP();
    }
    #endregion

    #region Virtuals
    /// <summary>
    /// What should a summon do when it is placed.
    /// </summary>
    /// <param name="tile">The tile the summon was placed on.</param>
    public virtual void OnPlaceSummon(LocationGridTile tile) {
        hasBeenUsed = true;
        SubscribeToSignals();
        Messenger.RemoveListener(Signals.HOUR_STARTED, DecreaseNeeds); //do not make summons decrease needs
        Messenger.RemoveListener(Signals.TICK_STARTED, DailyGoapPlanGeneration); //do not make summons plan goap actions by default
    }
    #endregion

}
