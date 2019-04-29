using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public static AudioManager Instance = null;

    private Dictionary<string, AudioSource> audioSources;
    private IEnumerator currentTransition;
    private AudioSource currentActiveMusic;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            LoadAllAudioSources();
        } else {
            Destroy(this.gameObject);
        }
        
    }

    private void LoadAllAudioSources() {
        AudioSource[] audio = this.GetComponentsInChildren<AudioSource>();
        audioSources = new Dictionary<string, AudioSource>();
        for (int i = 0; i < audio.Length; i++) {
            AudioSource currSource = audio[i];
            audioSources.Add(currSource.gameObject.name, currSource);
            currSource.volume = 0f; //set all music to 0 at initialization
        }
    }

    //private void Start() {
    //    audioSources["Main Menu"].Play();
    //}

    public void Play(string audioName) {
        if (currentActiveMusic != null) {
            currentActiveMusic.Stop();
            currentActiveMusic.volume = 0f;
        }

        audioSources[audioName].Play();
        currentActiveMusic = audioSources[audioName];
        currentActiveMusic.volume = 1f;
    }
    public void PlayFade(string audioName, int fadeDuration, System.Action actionOnFinish = null) {
        if (currentActiveMusic != null) {
            currentActiveMusic.Stop();
            currentActiveMusic.volume = 0f;
        }

        StartCoroutine(fadeIn(audioSources[audioName], fadeDuration, actionOnFinish));
    }
    public void TransitionTo(string to, int duration, System.Action actionOnFinish = null) {
        if (currentTransition != null) {
            StopCoroutine(currentTransition);
        }
        currentTransition = transition(currentActiveMusic, audioSources[to], duration, actionOnFinish);
        StartCoroutine(currentTransition);
    }

    IEnumerator transition(AudioSource from, AudioSource to, int transitionDuration, System.Action actionOnFinish) {
        float volumeIncrements = 1f / transitionDuration;
        to.Play();
        for (int i = 0; i < transitionDuration; i++) {
            from.volume -= volumeIncrements;
            to.volume += volumeIncrements;

            yield return new WaitForSecondsRealtime(0.1f);
        }

        from.Stop();
        currentTransition = null;
        currentActiveMusic = to;
        if (actionOnFinish != null) {
            actionOnFinish();
        }
    }
    IEnumerator fadeIn(AudioSource to, int transitionDuration, System.Action actionOnFinish) {
        float volumeIncrements = 1f / transitionDuration;
        to.Play();
        for (int i = 0; i < transitionDuration; i++) {
            to.volume += volumeIncrements;

            yield return new WaitForSecondsRealtime(0.1f);
        }

        currentTransition = null;
        currentActiveMusic = to;
        if (actionOnFinish != null) {
            actionOnFinish();
        }
    }

}
