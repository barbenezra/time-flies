using Game.Scripts.Helpers;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Helpers;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;

namespace Game.Scripts.Managers {
    [ExecuteInEditMode]
    public class CameraManager : Singleton<CameraManager>, IDestroyable {
        protected CameraManager() { }
        public bool ShouldDestroyOnLoad() => true;
        public void OnSceneChanged(Scene previousScene, Scene nextScene) { }

        [Required] [TitleGroup("General")] 
        public Camera Camera;

        [TitleGroup("Fitting")] 
        public bool FitWidth;

        [Range(5, 30)] [ShowIf("FitWidth")] [TitleGroup("Fitting")]
        public float SceneWidth = 10;

        [TitleGroup("Shake & Vibration")] 
        [InfoBox("Use the following controls to map your script vibration values to animation strength and duration")]
        [MinMaxSlider(0, 50, true)]
        public Vector2 ShakeIndexRange = new Vector2(0, 29);

        [TitleGroup("Shake & Vibration")]
        [PropertyRange("@this.ShakeIndexRange.x", "@this.ShakeIndexRange.y")]
        [OnValueChanged("OnIndexChanged")]
        public float CurrentIndex;
        
        [TitleGroup("Shake & Vibration")] [LabelText("Flatten Curve")]
        public bool ShouldFlattenCurve;

        [OnValueChanged("OnStrengthValueChanged")] [Range(0, 10)]
        [BoxGroup("Shake & Vibration/Strength")] [HorizontalGroup("Shake & Vibration/Strength/Split")] [HideLabel]
        public float CurrentIndexStrength;
        [BoxGroup("Shake & Vibration/Strength")] [HorizontalGroup("Shake & Vibration/Strength/Split")] [HideLabel] 
        public AnimationCurve Strength;

        [OnValueChanged("OnVibrationValueChanged")] [Range(0, 100)] 
        [BoxGroup("Shake & Vibration/Vibration")] [HorizontalGroup("Shake & Vibration/Vibration/Split")] [HideLabel]
        public float CurrentIndexVibration;
        [BoxGroup("Shake & Vibration/Vibration")] [HorizontalGroup("Shake & Vibration/Vibration/Split")] [HideLabel]
        public AnimationCurve Vibration;

        [OnValueChanged("OnDurationValueChanged")] [Range(0, 10)] 
        [BoxGroup("Shake & Vibration/Duration")] [HorizontalGroup("Shake & Vibration/Duration/Split")] [HideLabel]
        public float CurrentIndexDuration;
        [BoxGroup("Shake & Vibration/Duration")] [HorizontalGroup("Shake & Vibration/Duration/Split")] [HideLabel]
        public AnimationCurve Duration;

        private bool m_CameraIsNull;

        #region API
        
        [ButtonGroup("Shake & Vibration/Controls")]
        public void Shake(int value, bool vibrate) {
            if (m_CameraIsNull) return;

            if (vibrate) Handheld.Vibrate();
            float time = value.Map(ShakeIndexRange.x, ShakeIndexRange.y, 0, 1);
            Camera.transform.DORestart();
            if (Duration.Evaluate(time) >= float.Epsilon) {
                Camera.transform.DOShakeRotation(
                    Duration.Evaluate(time), 
                    Strength.Evaluate(time), 
                    (int) Vibration.Evaluate(time)
                );
            }
            Camera.transform.DORestart();
        }
        
        #endregion

        #region Utilities
        #if UNITY_EDITOR

        [ButtonGroup("Shake & Vibration/Controls")] [Button("Next")]
        private void NextIndex() {
            if (CurrentIndex + 1 > ShakeIndexRange.y) return;
            CurrentIndex++;
        }

        [ButtonGroup("Shake & Vibration/Controls")] [Button("Previous")]
        private void PreviousIndex() {
            if (CurrentIndex - 1 < ShakeIndexRange.x) return;
            CurrentIndex--;
        }
        
        [ButtonGroup("Shake & Vibration/Controls")] [Button("Reset")]
        private void ResetCurves() {
            Strength = new AnimationCurve();
            Vibration = new AnimationCurve();
            Duration = new AnimationCurve();
            CurrentIndex = ShakeIndexRange.x;
            CurrentIndexStrength = 0;
            CurrentIndexVibration = 0;
            CurrentIndexDuration = 0;
        }
        
        private void OnIndexChanged() {
            float time = CurrentIndex.Map(ShakeIndexRange.x, ShakeIndexRange.y, 0, 1);
            CurrentIndexStrength = Strength.Evaluate(time);
            CurrentIndexVibration = Vibration.Evaluate(time);
            CurrentIndexDuration = Duration.Evaluate(time);
        }

        private void OnStrengthValueChanged() {
            Strength.SetOrAddKey(
                CurrentIndex.Map(ShakeIndexRange.x, ShakeIndexRange.y, 0, 1),
                CurrentIndexStrength
            );
            
            if (ShouldFlattenCurve) FlattenCurve(Strength);
        }

        private void OnVibrationValueChanged() {
            Vibration.SetOrAddKey(
                CurrentIndex.Map(ShakeIndexRange.x, ShakeIndexRange.y, 0, 1),
                CurrentIndexVibration
            );
            
            if (ShouldFlattenCurve) FlattenCurve(Vibration);
        }

        private void OnDurationValueChanged() {
            Duration.SetOrAddKey(
                CurrentIndex.Map(ShakeIndexRange.x, ShakeIndexRange.y, 0, 1),
                CurrentIndexDuration
            );
            
            if (ShouldFlattenCurve) FlattenCurve(Duration);
        }

        private static void FlattenCurve(AnimationCurve curve) {
            curve.keys.ForEach((_, i) => {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Constant);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Constant);
            });
        }
        
        #endif

        private void UpdateFitWidth() {
            if (m_CameraIsNull) return;

            float unitsPerPixel = SceneWidth / Screen.width;
            float desiredHalfHeight = 0.5f * unitsPerPixel * Screen.height;
            Camera.orthographicSize = desiredHalfHeight;
        }
        
        #endregion

        #region Life Cycle
        
        private void Start() {
            m_CameraIsNull = Camera == null;
        }

        private void Update() {
            if (FitWidth) UpdateFitWidth();
        }
        
        #endregion
    }
}