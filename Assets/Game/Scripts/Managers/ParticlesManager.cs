using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Helpers;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Game.Scripts.Managers {
    [Serializable]
    public class ParticleSystemConfiguration {
        #region Parameters

        [Required] public string Name;

        [AssetsOnly] [ValidateInput("HasParticleSystem", "Prefab must include a particle system.")]
        public GameObject Prefab;

        [Required] [ValueDropdown("SortingLayerDropdownList")]
        public string SortingLayer;

        [CustomValueDrawer("LayerField")] public int Layer;

        public int SortingOrder;

        [MinValue(1)] public int InstanceCount;

        #endregion

        #region ParticleSystemConfiguration

        #if UNITY_EDITOR
        private int LayerField(int value, GUIContent label) =>
            EditorGUILayout.LayerField(value);

        private IEnumerable<string> SortingLayerDropdownList =>
            UnityEngine.SortingLayer.layers.Select(l => l.name);

        private bool HasParticleSystem(GameObject gameObject) =>
            gameObject != null && gameObject.GetComponent<ParticleSystem>() != null;
        #endif

        #endregion
    }

    public class ParticlesManager : Singleton<ParticlesManager>, IDestroyable {
        public bool ShouldDestroyOnLoad() => false;

        public void OnSceneChanged(Scene previousScene, Scene nextScene) {
            m_Camera = Camera.main;
            m_IsCameraNull = m_Camera == null;

            m_HaloParticleSystems.ForEach(kv => SetScaleToScreen(kv.Value.gameObject));
        }

        #region Parameters

        [InfoBox("Attempt to keep the Instance Count low for optimization reasons," +
                 " we are going to keep all instances present at all time")]
        [HideLabel]
        [TabGroup("Point Particles")]
        [TableList(AlwaysExpanded = true, DrawScrollView = false, ShowPaging = false)]
        public List<ParticleSystemConfiguration> PointParticleSystemConfigurations =
            new List<ParticleSystemConfiguration>();

        [InfoBox("Attempt to keep the number of emitters low for optimization reasons," +
                 " we are going to keep all instances present at all time")]
        [HideLabel]
        [TabGroup("Halo Particles")]
        [TableList(AlwaysExpanded = true, DrawScrollView = false, ShowPaging = false)]
        public List<ParticleSystemConfiguration> HaloParticleSystemConfigurations =
            new List<ParticleSystemConfiguration>();

        #endregion

        private Camera m_Camera;
        private bool m_IsCameraNull;
        private Transform m_PointParticleSystemsParent;
        private Transform m_HaloParticleSystemsParent;
        private readonly object m_PointParticleSystemsLock = new object();
        private readonly Dictionary<string, int> m_CurrentPointParticleSystem = new Dictionary<string, int>();

        private readonly Dictionary<string, List<ParticleSystem>> m_PointParticleSystems =
            new Dictionary<string, List<ParticleSystem>>();

        private readonly Dictionary<string, ParticleSystem> m_HaloParticleSystems =
            new Dictionary<string, ParticleSystem>();

        private readonly Dictionary<string, Coroutine> m_HaloParticleSystemAnimations =
            new Dictionary<string, Coroutine>();

        #region API

        public void SetPointParticleSystemConfigurations(List<ParticleSystemConfiguration> configurations) {
            PointParticleSystemConfigurations = configurations;

            lock (m_PointParticleSystemsLock) {
                m_PointParticleSystems.Values.ForEach(list => list.ForEach(system => Destroy(system.gameObject)));
                m_PointParticleSystems.Clear();
                m_CurrentPointParticleSystem.Clear();
                
                InitializePointParticleSystems();
            }
        }
        
        public void SetHaloParticleSystemConfigurations(List<ParticleSystemConfiguration> configurations) {
            HaloParticleSystemConfigurations = configurations;

            m_HaloParticleSystemAnimations.Values.ForEach(StopCoroutine);
            m_HaloParticleSystemAnimations.Clear();
            m_HaloParticleSystems.Values.ForEach(system => Destroy(system.gameObject));
            m_HaloParticleSystems.Clear();

            InitializeHaloParticleSystems();
        }

        public void EmitPoint(string systemName, Vector3 position, int amount) {
            if (!m_PointParticleSystems.ContainsKey(systemName)) {
                Debug.Log($"Trigger particle system \'{systemName}\' does not exist.");
                return;
            }

            lock (m_PointParticleSystemsLock) {
                m_CurrentPointParticleSystem[systemName] =
                    (m_CurrentPointParticleSystem[systemName] + 1) % m_PointParticleSystems[systemName].Count;
                m_PointParticleSystems[systemName][m_CurrentPointParticleSystem[systemName]].transform.position =
                    position;
                m_PointParticleSystems[systemName][m_CurrentPointParticleSystem[systemName]].Emit(amount);
            }
        }
        
        public void PlayPoint(string systemName, Vector3 position) {
            if (!m_PointParticleSystems.ContainsKey(systemName)) {
                Debug.Log($"Trigger particle system \'{systemName}\' does not exist.");
                return;
            }

            lock (m_PointParticleSystemsLock) {
                m_CurrentPointParticleSystem[systemName] =
                    (m_CurrentPointParticleSystem[systemName] + 1) % m_PointParticleSystems[systemName].Count;
                m_PointParticleSystems[systemName][m_CurrentPointParticleSystem[systemName]].transform.position =
                    position;
                m_PointParticleSystems[systemName][m_CurrentPointParticleSystem[systemName]].Play(true);
            }
        }

        public void FollowPoint(string systemName, Func<Vector3> position, Func<bool> shouldStop = null, Func<Quaternion> rotation = null) {
            if (!m_PointParticleSystems.ContainsKey(systemName)) {
                Debug.Log($"Trigger particle system \'{systemName}\' does not exist.");
                return;
            }

            lock (m_PointParticleSystemsLock) {
                try {
                    m_CurrentPointParticleSystem[systemName] =
                        (m_CurrentPointParticleSystem[systemName] + 1) % m_PointParticleSystems[systemName].Count;
                    if (rotation != null) {
                        m_PointParticleSystems[systemName][m_CurrentPointParticleSystem[systemName]].transform
                            .rotation = rotation();
                    }
                    m_PointParticleSystems[systemName][m_CurrentPointParticleSystem[systemName]].transform.position =
                        position();
                    m_PointParticleSystems[systemName][m_CurrentPointParticleSystem[systemName]].Play(true);

                    if (shouldStop != null && rotation != null) {
                        StartCoroutine(FollowAndStopSystemOnPredicate(
                                m_PointParticleSystems[systemName][m_CurrentPointParticleSystem[systemName]],
                                position,
                                rotation,
                                shouldStop
                            )
                        );
                    }
                }
                catch (Exception e) {
                    Debug.LogWarning($"Error while attempting to set system {systemName}\n{e}");
                }
            }
        }

        public void FlashHalo(string systemName, float duration) {
            if (!m_HaloParticleSystems.ContainsKey(systemName)) {
                Debug.Log($"Halo particle system \'{systemName}\' does not exist.");
                return;
            }

            if (m_HaloParticleSystemAnimations.ContainsKey(systemName) &&
                m_HaloParticleSystemAnimations[systemName] != null) {
                StopCoroutine(m_HaloParticleSystemAnimations[systemName]);
            }

            m_HaloParticleSystemAnimations[systemName] =
                StartCoroutine(AnimateFlashHalo(systemName, duration));
        }


        public void ToggleHalo(string systemName, bool toggle) {
            if (!m_HaloParticleSystems.ContainsKey(systemName)) {
                Debug.Log($"Halo particle system \'{systemName}\' does not exist.");
                return;
            }

            if (toggle) m_HaloParticleSystems[systemName].Play();
            else m_HaloParticleSystems[systemName].Stop();
        }

        #endregion

        #region Utilities

        private IEnumerator FollowAndStopSystemOnPredicate(ParticleSystem system, Func<Vector3> position, Func<Quaternion> rotation,
            Func<bool> shouldStop) {
            bool localShouldStop = false;

            while (!localShouldStop) {
                try {
                    system.transform.position = position();
                    system.transform.rotation = rotation();
                    localShouldStop = shouldStop();
                }
                catch (Exception) {
                    break;
                }

                yield return new WaitWhile(() => {
                    try {
                        return !shouldStop() && (system.transform.position - position()).magnitude < float.Epsilon;
                    }
                    catch (Exception) {
                        return false;
                    }
                });
            }

            system.Stop();
        }

        private IEnumerator AnimateFlashHalo(string systemName, float duration) {
            if (!m_HaloParticleSystems.ContainsKey(systemName)) yield break;

            m_HaloParticleSystems[systemName].Play();
            yield return new WaitForSeconds(duration);
            m_HaloParticleSystems[systemName].Stop();
        }

        #endregion

        #region Lifecycle

        private void InitializePointParticleSystems() {
            m_PointParticleSystemsParent = gameObject.FindOrCreateChild("Point Particle Systems").transform;

            PointParticleSystemConfigurations.ForEach(configuration => configuration.InstanceCount.Times(_ => {
                GameObject o = Instantiate(configuration.Prefab, m_PointParticleSystemsParent);
                o.layer = configuration.Layer;
                o.transform.position = Vector3.left * 10000;

                List<Renderer> renderers = o.GetComponentsInChildren<Renderer>().ToList();
                renderers.ForEach(r => {
                    r.sortingLayerName = configuration.SortingLayer;
                    r.sortingOrder = configuration.SortingOrder;
                });

                ParticleSystem p = o.GetComponent<ParticleSystem>();
                if (!m_PointParticleSystems.ContainsKey(configuration.Name)) {
                    m_PointParticleSystems[configuration.Name] = new List<ParticleSystem>();
                    m_CurrentPointParticleSystem[configuration.Name] = 0;
                }

                m_PointParticleSystems[configuration.Name].Add(p);
            }));
        }

        private void InitializeHaloParticleSystems() {
            m_HaloParticleSystemsParent = gameObject.FindOrCreateChild("Halo Particle Systems").transform;

            HaloParticleSystemConfigurations.ForEach(configuration => {
                GameObject o = Instantiate(configuration.Prefab, m_HaloParticleSystemsParent);
                o.layer = configuration.Layer;

                SetScaleToScreen(o);

                List<Renderer> renderers = o.GetComponentsInChildren<Renderer>().ToList();
                renderers.ForEach(r => {
                    r.sortingLayerName = configuration.SortingLayer;
                    r.sortingOrder = configuration.SortingOrder;
                });

                m_HaloParticleSystems[configuration.Name] = o.GetComponent<ParticleSystem>();
                m_HaloParticleSystems[configuration.Name].Stop();
            });
        }

        private void SetScaleToScreen(GameObject that) {
            if (m_IsCameraNull) {
                Debug.LogWarning("Unable to set object scale to screen.");
                return;
            }
            
            Vector3 scale = m_Camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height)) * 2;
            scale.y = 20f;
            that.transform.localScale = scale;

            Vector3 position = that.transform.position;
            position.y = 1f;
            that.transform.position = position;
        }

        private void Start() {
            if (PointParticleSystemConfigurations.Any()) InitializePointParticleSystems();
            if (HaloParticleSystemConfigurations.Any()) InitializeHaloParticleSystems();
        }

        #endregion
    }
}