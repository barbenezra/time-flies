using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Helpers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Managers {
    [Serializable]
    public struct SoundConfiguration {
        public string Name;
        public AudioClip Clip;
        public AudioMixerGroup Group;
        public bool FadeBackdrop;
        public bool PlayInOrder;
    }

    public class SoundManager : Singleton<SoundManager>, IDestroyable {
        protected SoundManager() { }
        public bool ShouldDestroyOnLoad() => false;

        public void OnSceneChanged(Scene previousScene, Scene nextScene) {
            m_SoundAlternationCount.Clear();
        }

        [BoxGroup("General")] [Required] public AudioMixer AudioMixer;
        
        [BoxGroup("Backdrops")] [InfoBox("If checked, would play the \"Default\" backdrop on start.")]
        public bool PlayDefaultOnStart;

        [BoxGroup("Backdrops")]
        public List<SoundConfiguration> Backdrops = new List<SoundConfiguration>();

        [BoxGroup("Sounds")] public int NumberOfSources;

        [FormerlySerializedAs("UserInterfaceSounds")] [BoxGroup("Sounds")]
        public List<SoundConfiguration> Sounds = new List<SoundConfiguration>();

        private AudioSource m_BackdropAudioSource;
        private readonly List<AudioSource> m_AudioSources = new List<AudioSource>();
        private readonly Dictionary<string, int> m_SoundAlternationCount = new Dictionary<string, int>();
        private int m_CurrentSource = 0;
        private readonly object m_SourceLock = new object();
        private GameObject m_AudioSourcesParent;

        private const string m_DEFAULT_SNAPSHOT_NAME = "Default";
        private const string m_FADED_BACKDROP_SNAPSHOT_NAME = "FadedBackdrop";
        private const float m_SNAPSHOT_TRANSITION_LENGTH = 0.2f;

        public float PlaybackSpeed {
            get {
                AudioMixer.GetFloat("BackdropPitch", out float value);
                return value;
            }
            set => AudioMixer.SetFloat("BackdropPitch", value);
        }

        public void Play(string soundName) {
            List<SoundConfiguration> query = Sounds.FindAll(sound => sound.Name.StartsWith(soundName));
            SoundConfiguration uiSound = query.DefaultIfEmpty().Random();
            
            if (uiSound.Clip == null) {
                Debug.LogWarning($"Audio clip {soundName} does not exist!");
                return;
            };

            if (uiSound.PlayInOrder) {
                if (!m_SoundAlternationCount.ContainsKey(soundName)) m_SoundAlternationCount[soundName] = 0;
                uiSound = query[m_SoundAlternationCount[soundName] % query.Count];
                m_SoundAlternationCount[soundName]++;
            }

            lock (m_SourceLock) {
                m_CurrentSource = (m_CurrentSource + 1) % NumberOfSources;
                m_AudioSources[m_CurrentSource].clip = uiSound.Clip;
                m_AudioSources[m_CurrentSource].outputAudioMixerGroup = uiSound.Group;
                m_AudioSources[m_CurrentSource].Play();
                if (uiSound.FadeBackdrop) StartCoroutine(FadeBackdrop(uiSound.Clip.length));
            }
        }
        
        public void PlayBackdrop(string soundName) {
            SoundConfiguration backdropSound = Backdrops.Find(sound => sound.Name.Equals(soundName));
            if (backdropSound.Clip == null) {
                Debug.LogWarning($"Backdrop clip {soundName} does not exist!");
                return;
            }

            lock (m_SourceLock) {
                m_BackdropAudioSource.clip = backdropSound.Clip;
                m_BackdropAudioSource.outputAudioMixerGroup = backdropSound.Group;
                m_BackdropAudioSource.loop = true;
                m_BackdropAudioSource.Play();
            }
        }

        public void Mute() {
            m_BackdropAudioSource.mute = true;
            m_AudioSources.ForEach(source => source.mute = true);
        }

        public void Unmute() {
            m_BackdropAudioSource.mute = false;
            m_AudioSources.ForEach(source => source.mute = false);
        }

        private IEnumerator FadeBackdrop(float length) {
            AudioMixer audioMixer = m_BackdropAudioSource.outputAudioMixerGroup.audioMixer;
            
            audioMixer.FindSnapshot(m_FADED_BACKDROP_SNAPSHOT_NAME).TransitionTo(m_SNAPSHOT_TRANSITION_LENGTH);
            yield return new WaitForSeconds(length);
            audioMixer.FindSnapshot(m_DEFAULT_SNAPSHOT_NAME).TransitionTo(m_SNAPSHOT_TRANSITION_LENGTH);
        }

        private bool BackdropExists() {
            return Backdrops.Count > 0;
        }

        private void StartAudioSources() {
            m_AudioSourcesParent = gameObject.FindOrCreateChild("Audio Sources");

            m_BackdropAudioSource = (new GameObject("BackdropAudioSource")).AddComponent<AudioSource>();
            m_BackdropAudioSource.transform.parent = m_AudioSourcesParent.transform;

            NumberOfSources.Times(_ => {
                GameObject o = new GameObject("AudioSourceObject");
                o.transform.SetParent(m_AudioSourcesParent.transform);
                m_AudioSources.Add(o.AddComponent<AudioSource>());
            });
        }

        private void StartDefaultBackdropSound() {
            if (!PlayDefaultOnStart) return;
            if (BackdropExists()) PlayBackdrop("Default");
        }

        private void Start() {
            StartAudioSources();
            StartDefaultBackdropSound();
        }
    }
}