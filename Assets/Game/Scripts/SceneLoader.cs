using Game.Scripts.Helpers;

namespace Game.Scripts
{
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class SceneLoader : Singleton<SceneLoader>, IDestroyable
    {
        protected SceneLoader()
        {
        }

        public bool ShouldDestroyOnLoad() => false;

        public void OnSceneChanged(Scene previousScene, Scene nextScene)
        {
        }

        private AsyncOperation loadingAsyncOperation;

        private class LoadingMonoBehaviour : MonoBehaviour { }

        public event Action OnLoaderCallback;

        public void Load(string scene)
        {
            OnLoaderCallback = () =>
            {
                GameObject loadingGameObject = new GameObject("Loading Game Object");
                loadingGameObject.AddComponent<LoadingMonoBehaviour>().StartCoroutine(LoadSceneAsync(scene));
            };

            SceneManager.LoadScene("Loading");
        }

        private IEnumerator LoadSceneAsync(string scene)
        {
            yield return null;

            loadingAsyncOperation = SceneManager.LoadSceneAsync(scene);

            while (!loadingAsyncOperation.isDone)
            {
                yield return null;
            }
        }

        public void LoaderCallback()
        {
            if (OnLoaderCallback != null)
            {
                OnLoaderCallback();
                OnLoaderCallback = null;
            }
        }

        public float GetLoadingProgress()
        {
            if (loadingAsyncOperation != null)
            {
                return loadingAsyncOperation.progress;
            }
            else
            {
                return 0f;
            }
        }
    }

}