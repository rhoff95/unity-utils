using UnityEngine;

namespace Plugins.unity_utils.Scripts.Camera
{
    public class ShakyCamera : MonoBehaviour
    {
        [Header("Translation Noise")] [Range(0f, 5f)]
        [SerializeField] private float translationalNoiseAmplitude = 0.25f;
        [SerializeField] private float translationalNoiseFrequency = 80f;

        [Header("Rotation Noise")] [Range(0f, 5f)]
        [SerializeField] private float rotationalNoiseAmplitude = 0.25f;
        [SerializeField] private float rotationalNoiseFrequency = 80f;

        [Header("Shake")]
        [Range(0f, 1f)] [SerializeField] private float shakeSmoothTime = 1f;
        [SerializeField] private float shakeAmount;

        private bool _shakeEnabled = true;

        // SmoothRefs
        private float _shakeSmoothRef;

        private void Update()
        {
            if (!_shakeEnabled)
            {
                shakeAmount = 0f;
            }
            else
            {
                shakeAmount = Mathf.SmoothDamp(
                    shakeAmount,
                    0f,
                    ref _shakeSmoothRef,
                    shakeSmoothTime);
            }

            var noiseX = translationalNoiseAmplitude * shakeAmount *
                         GetPerlin(1, 2, translationalNoiseFrequency);
            var noiseY = translationalNoiseAmplitude * shakeAmount *
                         GetPerlin(3, 4, translationalNoiseFrequency);
            var noiseTheta = rotationalNoiseAmplitude * shakeAmount *
                             GetPerlin(5, 6, rotationalNoiseFrequency);
            var noise = new Vector3(noiseX, noiseY, 0f);

            transform.localPosition = noise;
            transform.rotation = Quaternion.Euler(0f, 0f, noiseTheta);
        }

        public void AddShake(float value)
        {
            shakeAmount += value;
        }

        public void ToggleShakeEnabled()
        {
            _shakeEnabled = !_shakeEnabled;
        }

        /// <returns>Value in range [-1f, 1f]</returns>
        private static float GetPerlin(float seed1, float seed2, float frequency)
        {
            var noise = Mathf.PerlinNoise(seed1 + Time.time * frequency,
                seed2 + Time.time * frequency);
            return noise * 2 - 1f;
        }
    }
}
