using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scripts.Helpers {
    [DisallowMultipleComponent]
    public class Singleton<T> : SerializedMonoBehaviour where T : SerializedMonoBehaviour, IDestroyable {
        // ReSharper disable StaticMemberInGenericType
        private static readonly object m_Lock = new object();
        private static T m_Instance;
        private static readonly object m_ActionQueueLock = new object();
        private static readonly Queue<Action> m_ActionQueue = new Queue<Action>();
        // ReSharper enable StaticMemberInGenericType

        public static T Instance {
            get {
                lock (m_Lock) {
                    if (m_Instance != null) {
                        if (!m_Instance.ShouldDestroyOnLoad()) DontDestroyOnLoad(m_Instance);
                        return m_Instance;
                    }

                    m_Instance = (T) FindObjectOfType(typeof(T));
                    if (m_Instance != null) {
                        if (!m_Instance.ShouldDestroyOnLoad()) DontDestroyOnLoad(m_Instance);
                        return m_Instance;
                    }

                    GameObject singletonObject = new GameObject();
                    m_Instance = singletonObject.AddComponent<T>();
                    singletonObject.name = typeof(T) + " (Singleton)";
                    if (!m_Instance.ShouldDestroyOnLoad()) DontDestroyOnLoad(singletonObject);

                    return m_Instance;
                }
            }
        }

        public static void QueueCoroutine(Action queuedAction) {
            lock (m_ActionQueueLock) {
                m_ActionQueue.Enqueue(queuedAction);
            }
        }

        private void Update() {
            IEnumerator QueuedActionEnumerator(Action action) {
                action();
                yield break;
            }
            
            lock (m_ActionQueueLock) {
                while (m_ActionQueue.Count > 0) {
                    StartCoroutine(QueuedActionEnumerator(m_ActionQueue.Dequeue()));
                }
            }
        }

        private void Awake() {
            if (Instance == this) return;
            Destroy(gameObject);
        }

        private void OnEnable() {
            if (Instance != null && Instance != this) return;
            SceneManager.activeSceneChanged += Instance.OnSceneChanged;
        }

        private void OnDisable() {
            if (Instance != null && Instance != this) return;
            SceneManager.activeSceneChanged -= Instance.OnSceneChanged;
        }
    }

    public interface IDestroyable {
        bool ShouldDestroyOnLoad();
        void OnSceneChanged(Scene previousScene, Scene nextScene);
    }
}