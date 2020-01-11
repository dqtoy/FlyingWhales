﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interrupts;

public class InterruptComponent {
    public Character owner { get; private set; }
    public IPointOfInterest currentTargetPOI { get; private set; }
    public Interrupt currentInterrupt { get; private set; }
    public int currentDuration { get; private set; }
    public string identifier { get; private set; }

    public Log thoughtBubbleLog { get; private set; }

    #region getters
    public bool isInterrupted => currentInterrupt != null;
    #endregion

    public InterruptComponent(Character owner) {
        this.owner = owner;
    }

    #region General
    public bool TriggerInterrupt(INTERRUPT interrupt, IPointOfInterest targetPOI, string identifier = "") {
        Interrupt triggeredInterrupt = InteractionManager.Instance.GetInterruptData(interrupt);
        if (!triggeredInterrupt.isSimulateneous) {
            if (isInterrupted) {
                owner.PrintLogIfActive("Cannot trigger interrupt " + interrupt.ToString() + " because there is already a current interrupt: " + currentInterrupt.name);
                return false;
            }
            owner.PrintLogIfActive(GameManager.Instance.TodayLogString() + owner.name + " triggered a non simultaneous interrupt: " + triggeredInterrupt.name);
            currentInterrupt = triggeredInterrupt;
            currentTargetPOI = targetPOI;
            this.identifier = identifier;

            CreateThoughtBubbleLog();

            if (owner.marker != null && owner.marker.isMoving) {
                owner.marker.StopMovement();
            }
            if (currentInterrupt.doesStopCurrentAction && owner.currentJob != null) {
                owner.currentJob.StopJobNotDrop();
            }
            if (currentInterrupt.doesDropCurrentJob) {
                if (owner.currentJob != null) {
                    owner.currentJob.CancelJob(false);
                }
            }
            triggeredInterrupt.ExecuteInterruptStartEffect(owner, targetPOI);
            Messenger.Broadcast(Signals.UPDATE_THOUGHT_BUBBLE, owner);
        } else {
            TriggeredSimultaneousInterrupt(triggeredInterrupt, targetPOI, identifier);
        }
        return true;
    }
    private bool TriggeredSimultaneousInterrupt(Interrupt interrupt, IPointOfInterest targetPOI, string identifier) {
        owner.PrintLogIfActive(GameManager.Instance.TodayLogString() + owner.name + " triggered a simultaneous interrupt: " + interrupt.name);
        interrupt.ExecuteInterruptStartEffect(owner, targetPOI);
        CreateAndAddEffectLog(interrupt, targetPOI);
        this.identifier = identifier;
        interrupt.ExecuteInterruptEndEffect(owner, currentTargetPOI);
        return true;
    }
    public void OnTickEnded() {
        if (isInterrupted) {
            currentDuration++;
            if(currentDuration >= currentInterrupt.duration) {
                CreateAndAddEffectLog();
                currentInterrupt.ExecuteInterruptEndEffect(owner, currentTargetPOI);
                EndInterrupt();
            }
        }
    }
    private void EndInterrupt() {
        bool willCheckInvision = currentInterrupt.duration > 0;
        currentInterrupt = null;
        currentDuration = 0;
        if (willCheckInvision) {
            for (int i = 0; i < owner.marker.inVisionCharacters.Count; i++) {
                Character inVisionCharacter = owner.marker.inVisionCharacters[i];
                owner.CreateJobsOnEnterVisionWith(inVisionCharacter);
            }
        }
    }
    private void CreateThoughtBubbleLog() {
        if (LocalizationManager.Instance.HasLocalizedValue("Interrupt", currentInterrupt.name, "thought_bubble")) {
            thoughtBubbleLog = new Log(GameManager.Instance.Today(), "Interrupt", currentInterrupt.name, "thought_bubble");
            thoughtBubbleLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            thoughtBubbleLog.AddToFillers(currentTargetPOI, currentTargetPOI.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
    }
    private void CreateAndAddEffectLog() {
        if (LocalizationManager.Instance.HasLocalizedValue("Interrupt", currentInterrupt.name, "effect")) {
            Log effectLog = new Log(GameManager.Instance.Today(), "Interrupt", currentInterrupt.name, "effect");
            effectLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            effectLog.AddToFillers(currentTargetPOI, currentTargetPOI.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            effectLog.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotificationFrom(owner, effectLog);
        } 
        //else {
        //    Debug.LogWarning(currentInterrupt.name + " interrupt does not have effect log!");
        //}
    }
    private void CreateAndAddEffectLog(Interrupt interrupt, IPointOfInterest target) {
        if (LocalizationManager.Instance.HasLocalizedValue("Interrupt", interrupt.name, "effect")) {
            Log effectLog = new Log(GameManager.Instance.Today(), "Interrupt", interrupt.name, "effect");
            effectLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            effectLog.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            effectLog.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotificationFrom(owner, effectLog);
        }
    }
    #endregion
}