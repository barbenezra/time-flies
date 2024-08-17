using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

namespace Game.Scripts.Helpers {
    internal static class Extensions {
        public static T Random<T>(this IEnumerable<T> list) {
            Random random = new Random();
            return list.ElementAt(random.Next(list.Count()));
        }

        public static void Times(this int count, Action<int> action) {
            for (int i = 0; i < count; i++) {
                action(i);
            }
        }

        public static Transform Clear(this Transform transform) {
            foreach (Transform child in transform) {
                GameObject.Destroy(child.gameObject);
            }

            return transform;
        }

        public static GameObject FindOrCreateChild(this GameObject self, string name) {
            Transform parentTransform = self.transform;
            Transform childTransform = parentTransform.Find(name);

            if (childTransform != null) return childTransform.gameObject;

            GameObject child = new GameObject(name);
            childTransform = child.transform;
            childTransform.parent = parentTransform;
            return childTransform.gameObject;
        }

        public static GameObject FindChild(this GameObject self, string name) {
            Transform parentTransform = self.transform;
            Transform childTransform = parentTransform.Find(name);

            return childTransform != null ? childTransform.gameObject : null;
        }

        public static Transform FindDeepChild(this Transform self, string name) {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(self);

            while (queue.Count > 0) {
                Transform transform = queue.Dequeue();
                if (transform.name == name) return transform;
                foreach (Transform t in transform) queue.Enqueue(t);
            }

            return null;
        }

        public static ParticleSystem LoadParticleSystem(this GameObject parent, string particleSystemName,
            string sortingLayer) {
            ParticleSystem particleSystemComponent =
                parent.FindChild(particleSystemName)?.GetComponent<ParticleSystem>();
            if (particleSystemComponent != null)
                particleSystemComponent.GetComponent<Renderer>().sortingLayerName = sortingLayer;
            else Debug.LogError($"Particle system ${particleSystemName} does not exist for {parent.name}");
            return particleSystemComponent;
        }
        
        public static string GetPath(this Enum @enum) {
            return $"{@enum.GetType()} {@enum.ToString()}";
        }

        public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate) {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            int returnValue = 0;
            foreach (T item in items) {
                if (predicate(item)) return returnValue;
                returnValue++;
            }

            return -1;
        }

        public static int SetOrAddKey(this AnimationCurve curve, float time, float value) {
            int index = curve.keys.FindIndex(k => Mathf.Approximately(k.time, time));

            if (index == -1) curve.AddKey(time, value);
            else {
                curve.RemoveKey(index);
                curve.AddKey(time, value);
            }

            return index;
        }

        public static float Map(this float value, float fromSource, float toSource, float fromTarget, float toTarget) {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }

        public static float Map(this int value, float fromSource, float toSource, float fromTarget, float toTarget) {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }

        public static int EpochTime() {
            DateTime epochStart = new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Utc);
            return (int) (DateTime.UtcNow - epochStart).TotalSeconds;
        }

        public static int WordCount(this string text) {
            char[] delimiters = {' ', '\r', '\n'};
            return text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public static Bounds GetBounds(this GameObject gameObject) {
            Bounds bounds = new Bounds (gameObject.transform.position, Vector3.zero);
            
            gameObject.GetComponentsInChildren<Transform>()
                .Select(transform => transform.GetComponent<Renderer>())
                .Where(renderer => renderer != null)
                .ForEach(renderer => bounds.Encapsulate(renderer.bounds));
            
            gameObject.GetComponentsInChildren<RectTransform>()
                .Select(transform => transform.GetComponent<Renderer>())
                .Where(renderer => renderer != null)
                .ForEach(renderer => bounds.Encapsulate(renderer.bounds));

            return bounds;
        }

        public static Vector2 DropZ(this Vector3 vector) {
            return new Vector2(vector.x, vector.y);
        }

        public static Func<float, GUIContent, float> AlphaSlider(string left, string right) {
            return (value, content) => {
                float result = GUILayout.HorizontalSlider(value, 0f, 1f);

                GUILayout.Space(10f);
                GUILayout.BeginHorizontal();
                TextAnchor defaultAlignment = GUI.skin.label.alignment;
                GUILayout.Label(left);
                GUI.skin.label.alignment = TextAnchor.UpperRight;
                GUILayout.Label(right);
                GUI.skin.label.alignment = defaultAlignment;
                GUILayout.EndHorizontal();
                GUILayout.Space(5f);

                return result;
            };
        }
        
        public static int MathMod(int a, int b) {
            return (Math.Abs(a * b) + a) % b;
        }
    }

    #if UNITY_EDITOR

    public static class SerializedPropertyExtensions {
        public static void Each(this SerializedProperty sp, string name, Action<SerializedProperty> action) {
            int numberOfItems = sp.FindPropertyRelative($"{name}.Array.size").intValue;
            for (int index = 0; index < numberOfItems; index++) {
                action(sp.FindPropertyRelative($"{name}.Array.data[{index}]"));
            }
        }

        public static Vector3 ToVector3(this Vector2 vector) {
            return new Vector3(vector.x, vector.y, 0);
        }
    }

    #endif
}