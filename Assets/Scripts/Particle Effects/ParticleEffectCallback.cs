using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZObjectPools;

public class ParticleEffectCallback : PooledObject {
    public ParticleSystem particleSystem;

    public void OnParticleSystemStopped() {
        Messenger.Broadcast(Signals.PARTICLE_EFFECT_DONE, particleSystem);
    }
}
