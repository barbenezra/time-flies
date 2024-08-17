using Game.Scripts.Helpers;

namespace Game.Scripts
{
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public enum SceneEnum
    {
        MainMenu,
        Loading,
        Level1
    }

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

        public void Load(SceneEnum scene)
        {
            OnLoaderCallback = () =>
            {
                GameObject loadingGameObject = new GameObject("Loading Game Object");
                loadingGameObject.AddComponent<LoadingMonoBehaviour>().StartCoroutine(LoadSceneAsync(scene));
            };

            SceneManager.LoadScene(SceneEnum.Loading.ToString());
        }

        private IEnumerator LoadSceneAsync(SceneEnum scene)
        {
            yield return null;

            loadingAsyncOperation = SceneManager.LoadSceneAsync(scene.ToString());

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