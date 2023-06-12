using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace PhotoApp
{
    public class CameraCapture : MonoBehaviour
    {
        private WebCamDevice[] _availableSources;
        private WebCamDevice[] availableSources
        {
            get
            {
                if (_availableSources == null)
                {
                    _availableSources = WebCamTexture.devices;
                }

                return _availableSources;
            }
            set => _availableSources = value;
        }

        private static readonly object permissionLock = new object();
        private static bool isPermitted = false;

        WebCamTexture webCamTexture;
        public AspectRatioFitter fit;
        private bool cameraEnabled;
        private float ratio;

        //[SerializeField] private MeshRenderer photoMeshRenderer;
        [SerializeField] private RawImage photoToShow;
        //private Quaternion baseRotation;
        //[SerializeField] private Texture photoTexture;

        [SerializeField] private Logic localLogic;

        private void Start()
        {
            //Debug.Log($"Anchors. Max : {photoToShow.rectTransform.offsetMax}, " +
            //    $"Min : {photoToShow.rectTransform.offsetMin}");

            _ = StartCoroutine(GetPermission());

            if (isPermitted)
            {
                availableSources = WebCamTexture.devices;
                //foreach (var device in availableSources)
                //    Debug.Log($"Device available : {device.name}");

                if (availableSources.Length == 0)
                {
                    Debug.LogError("can not find any camera!");
                    return;
                }

                //WebCamDevice webCamDevice = WebCamTexture.devices[0];
                //if (availableSources != null && availableSources.Length > 0)

                WebCamDevice webCamDevice = WebCamTexture.devices[0];
                foreach(var device in availableSources)
                {
                    if (device.isFrontFacing)
                        webCamDevice = device;
                }
                foreach (var resolution in webCamDevice.availableResolutions)
                    Debug.Log($"Device available : {webCamDevice.name}, resolution : {resolution}");
                webCamTexture = new WebCamTexture(webCamDevice.name, 1280, 720);
                //webCamTexture = new WebCamTexture(webCamDevice.name);

                //photoMeshRenderer.material.mainTexture = webCamTexture; //Add Mesh Renderer to the GameObject to which this script is attached to
                //photoMaterial2.material.mainTexture = webCamTexture; //Add Mesh Renderer to the GameObject to which this script is attached to
                //photoTexture = webCamTexture;

                //baseRotation = photoToShow.transform.rotation;
                Debug.Log($"photoMaterial2 assigned. WebCamTexture, Height : {webCamTexture.height}," +
                    $"Width : {webCamTexture.width}");
            }
        }

        private void CalculateRendererSettings()
        {
            Rect uvRectForVideoVerticallyMirrored = new(1f, 0f, -1f, 1f);
            Rect uvRectForVideoNotVerticallyMirrored = new(0f, 0f, 1f, 1f);

            ratio = (float)webCamTexture.width / (float)webCamTexture.height;
            fit.aspectRatio = ratio;

            float orientation = -webCamTexture.videoRotationAngle;
            if (webCamTexture.videoVerticallyMirrored)
            {
                orientation += 180f;
                Debug.Log($"videoVerticallyMirrored");
            }

            photoToShow.rectTransform.localEulerAngles = new Vector3(0, 0, orientation);

            //float ScaleY = webCamTexture.videoVerticallyMirrored ? -1f : 1f;
            //photoToShow.rectTransform.localScale = new Vector3(1f, ScaleY, 1f);

            //if (webCamTexture.videoVerticallyMirrored)
            //{
            //    photoToShow.uvRect = uvRectForVideoVerticallyMirrored;
            //}
            //else
            //{
            //    photoToShow.uvRect = uvRectForVideoNotVerticallyMirrored;
            //}

            Debug.Log($"Anchors. Max : {photoToShow.rectTransform.offsetMax}, " +
                $"Min : {photoToShow.rectTransform.offsetMin}. aspectRatio : {fit.aspectRatio}\n" +
                $"WebCamTexture, Height : {webCamTexture.height}," +
                    $"Width : {webCamTexture.width}");
        }

        private void Update()
        {
            if (!cameraEnabled)
                return;

            float ScaleY = webCamTexture.videoVerticallyMirrored ? -1f : 1f;
            photoToShow.rectTransform.localScale = new Vector3(1f * ratio, ScaleY * ratio, 1f * ratio);
            ////photoToShow.rectTransform.localScale = new Vector3(1f, ScaleY, 1f);
            //photoToShow.rectTransform.localScale = new Vector3(1f, ScaleY * ratio, 1f);
        }

        /*private void LateUpdate()
        {
            if (cameraEnabled)
            {
                Texture2D photo = new Texture2D(photoToShow.texture.width, photoToShow.texture.height);
                photo.SetPixels(webCamTexture.GetPixels());
                photo.Apply();

                photoToShow.texture = FlipHorizontal(photo);
            }
        }

        private Texture2D FlipHorizontal(Texture2D orig)
        {
            print("Doing FlipHorizontal");
            Color32[] origpix = orig.GetPixels32(0);
            Color32[] newpix = new Color32[orig.width * orig.height];

            for (int c = 0; c < orig.height; c++)
            {
                for (int i = 0; i < orig.width; i++)
                {
                    newpix[(orig.width * (orig.height - 1 - c)) + i] = origpix[(c * orig.width) + i];
                }
            }

            Texture2D newtex = new Texture2D(orig.width, orig.height, orig.format, false);
            newtex.SetPixels32(newpix, 0);
            newtex.Apply();
            return newtex;
        }*/

        public void StartCamera()
        {
            photoToShow.texture = webCamTexture;
            webCamTexture.Play();
            //webCamTexture.requestedWidth = 640;
            //webCamTexture.requestedHeight = 360;
            CalculateRendererSettings();
            cameraEnabled = true;
        }

        public void StopCamera()
        {
            cameraEnabled = false;
            if (photoToShow != null)
            {
                webCamTexture.Stop();
                photoToShow.texture = null;
            }
        }

        public void CallTakePhoto() // call this function in button click event
        {
            StartCoroutine(TakePhoto());
        }
        IEnumerator TakePhoto()  // Start this Coroutine on some button click
        {

            // NOTE - you almost certainly have to do this here:

            yield return new WaitForEndOfFrame();

            // it's a rare case where the Unity doco is pretty clear,
            // http://docs.unity3d.com/ScriptReference/WaitForEndOfFrame.html
            // be sure to scroll down to the SECOND long example on that doco page 

            Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height);
            photo.SetPixels(webCamTexture.GetPixels());
            photo.Apply();

            //Encode to a PNG
            byte[] bytes = photo.EncodeToPNG();
            string savePath = Application.persistentDataPath + "\\photo.png";
            //Write out the PNG. Of course you have to substitute your_path for something sensible
            File.WriteAllBytes(savePath, bytes);
            Debug.Log($"Photo saved at : {savePath}");

            localLogic.PictureTaken(ref photo);
        }

        private IEnumerator GetPermission()
        {
            lock (permissionLock)
            {
                if (isPermitted)
                {
                    yield break;
                }

#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
          Permission.RequestUserPermission(Permission.Camera);
          yield return new WaitForSeconds(0.1f);
        }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
          Debug.LogWarning("Not permitted to use Camera");
          yield break;
        }
#endif
                isPermitted = true;

                yield return new WaitForEndOfFrame();
            }
        }
    }
}
