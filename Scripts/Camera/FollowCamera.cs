using UnityEngine;

namespace Plugins.unity_utils.Scripts.Camera
{
    public class FollowCamera : MonoBehaviour
    {
        public GameObject targetGameObject;
        [Range(0f, 5f)] public float smoothTime = 1f;
        public float targetZ = -10f;

        private Vector3 _currentVelocity;

        private void Update()
        {
            var nextPosition = Vector3.SmoothDamp(transform.position,
                targetGameObject.transform.position, ref _currentVelocity, smoothTime);
            transform.position = new Vector3(
                nextPosition.x, nextPosition.y, targetZ);
        }
    }
}
