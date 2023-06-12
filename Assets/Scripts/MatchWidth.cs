using UnityEngine;

namespace PhotoApp
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class MatchWidth : MonoBehaviour
    {
        // Set this to the in-world distance between the left & right edges of your scene.
        public float sceneWidth = 10;
        //public float sceneHeight = 10;

        Camera _camera;
        void Start()
        {
            _camera = GetComponent<Camera>();

            // Adjust the camera's height so the desired scene width fits in view
            // even if the screen/window size changes dynamically.
            float unitsPerPixel = sceneWidth / Screen.width;
            float desiredHalfHeight = 0.5f * unitsPerPixel * Screen.height;
            _camera.orthographicSize = desiredHalfHeight;
        }

        //Only For Testing
        /*private void Update()
        {
            // Adjust the camera's height so the desired scene width fits in view
            // even if the screen/window size changes dynamically.
            float unitsPerPixel = sceneWidth / Screen.width;
            float desiredHalfHeight = 0.5f * unitsPerPixel * Screen.height;
            _camera.orthographicSize = desiredHalfHeight;
            //Debug.Log($"Setting orthographicSize : {_camera.orthographicSize}");
        }*/
    }
}