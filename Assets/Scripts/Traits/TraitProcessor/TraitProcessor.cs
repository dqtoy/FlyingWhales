using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public abstract class TraitProcessor {
        public abstract void OnTraitAdded(ITraitable traitable, Trait trait, Character characterResponsible = null, ActualGoapNode gainedFromDoing = null);
        public abstract void OnTraitRemoved(ITraitable traitable, Trait trait, Character removedBy = null);
        public abstract void OnTraitStacked(ITraitable traitable, Trait trait, Character characterResponsible = null, ActualGoapNode gainedFromDoing = null);
        public abstract void OnTraitUnstack(ITraitable traitable, Trait trait, Character removedBy = null);

        protected void DefaultProcessOnAddTrait(ITraitable traitable, Trait trait, Character characterResponsible, ActualGoapNode gainedFromDoing) {
            trait.SetGainedFromDoing(gainedFromDoing);
            //trait.SetOnRemoveAction(onRemoveAction);
            trait.AddCharacterResponsibleForTrait(characterResponsible);
            trait.AddCharacterResponsibleForTrait(characterResponsible);
            ApplyPOITraitInteractions(traitable, trait);
            if (trait.ticksDuration > 0) {
                //traitable.traitContainer.currentDurations.Add(trait, 0);
                GameDate removeDate = GameManager.Instance.Today();
                removeDate.AddTicks(trait.ticksDuration);
                string ticket = SchedulingManager.Instance.AddEntry(removeDate, () => traitable.traitContainer.RemoveTraitOnSchedule(traitable, trait), this);
                //trait.SetExpiryTicket(traitable, ticket);
            }
            trait.OnAddTrait(traitable);
            Messenger.Broadcast(Signals.TRAITABLE_GAINED_TRAIT, traitable, trait);
        }
        protected void DefaultProcessOnRemoveTrait(ITraitable traitable, Trait trait, Character removedBy) {
            //trait.RemoveExpiryTicket(traitable);
            //TODO: if (triggerOnRemove) {
            //    trait.OnRemoveTrait(this, removedBy);
            //}
            UnapplyPOITraitInteractions(traitable, trait);
            trait.OnRemoveTrait(traitable, removedBy);
            Messenger.Broadcast(Signals.TRAITABLE_LOST_TRAIT, traitable, trait, removedBy);
        }
        protected bool DefaultProcessOnStackTrait(ITraitable traitable, Trait trait, Character characterResponsible, ActualGoapNode gainedFromDoing) {
            if (trait.ticksDuration > 0) {
                //traitable.traitContainer.currentDurations[trait] = 0;
                GameDate removeDate = GameManager.Instance.Today();
                removeDate.AddTicks(trait.ticksDuration);
                string ticket = SchedulingManager.Instance.AddEntry(removeDate, () => traitable.traitContainer.RemoveTraitOnSchedule(traitable, trait), this);
                //trait.SetExpiryTicket(traitable, ticket);
            }
            if(traitable.traitContainer.stacks[trait.name] <= trait.stackLimit) {
                trait.SetGainedFromDoing(gainedFromDoing);
                trait.AddCharacterResponsibleForTrait(characterResponsible);
                trait.AddCharacterResponsibleForTrait(characterResponsible);
                trait.OnStackTrait(traitable);
                return true;
            }
            return false;
        }
        protected void DefaultProcessOnUnstackTrait(ITraitable traitable, Trait trait, Character removedBy) {
            //trait.RemoveExpiryTicket(traitable);
            if (traitable.traitContainer.stacks[trait.name] < trait.stackLimit) {
                trait.OnUnstackTrait(traitable);
            }
        }
        private void ApplyPOITraitInteractions(ITraitable traitable, Trait trait) {
            if (trait.advertisedInteractions != null) {
                for (int i = 0; i < trait.advertisedInteractions.Count; i++) {
                    traitable.AddAdvertisedAction(trait.advertisedInteractions[i]);
                }
            }
        }
        private void UnapplyPOITraitInteractions(ITraitable traitable, Trait trait) {
            if (trait.advertisedInteractions != null) {
                for (int i = 0; i < trait.advertisedInteractions.Count; i++) {
                    traitable.RemoveAdvertisedAction(trait.advertisedInteractions[i]);
                }
            }
        }
    }
}

