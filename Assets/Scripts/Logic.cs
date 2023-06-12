using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhotoApp
{
    public class Logic : MonoBehaviour
    {
        [SerializeField] private Transform[] photoTransforms;
        [SerializeField] private GameObject photoPrefab;

        [Header("UI")]
        [SerializeField] private GameObject photoPreviewCanvas;
        [SerializeField] private UnityEngine.UI.Image shutterEffectPanel;

        [Header("Photo BG")]
        [SerializeField] private GameObject frameContainer;
        [SerializeField] private GameObject frameCanvas;

        [Header("Photo Controls")]
        private GameObject tempPhoto;
        [SerializeField] private float moveSpeed, shutterSpeed, time;
        [SerializeField] private Transform spawnPos;
        [SerializeField] private Vector3[] startRotation;

        [Header("Reference Scripts")]
        [SerializeField] private CameraCapture localCameraCapture;

        private byte photoCount = 0;

        //On the Start Camera, under FrameCanvas
        public void StartCamera()
        {
            localCameraCapture.StartCamera();
            photoPreviewCanvas.SetActive(true);
        }

        //On the Exit button, under the FrameCanvas
        public void ExitGame()
        {
            Application.Quit();
        }

        #region Picture
        public void PictureTaken(ref Texture2D tex)
        {
            tempPhoto = Instantiate(photoPrefab, spawnPos.position, Quaternion.identity);
            tempPhoto.transform.eulerAngles = startRotation[Random.Range(0,2)];
            tempPhoto.transform.GetChild(0).GetComponent<MeshRenderer>().material.mainTexture = tex;

            _ = StartCoroutine(ShowShutterEffect());
        }

        private IEnumerator ShowShutterEffect()
        {
            float startAlpha = 0f, endAlpha = 1f;       //time = 0f, 
            bool showedEffect = false, fullyVisible = false;
            Color whiteCol = Color.white;
            while (!showedEffect)
            {
                time += shutterSpeed * Time.deltaTime;
                if (time >= 1f)
                {
                    if (fullyVisible)
                    {
                        showedEffect = true;
                        shutterEffectPanel.color = Color.clear;
                        photoPreviewCanvas.SetActive(false);
                        frameCanvas.SetActive(true);
                        frameContainer.SetActive(true);
                        localCameraCapture.StopCamera();
                        Debug.Log($"Showed Effect");

                        _ = StartCoroutine(GetToPosition());
                        break;
                    }

                    fullyVisible = true;
                    time = 0f;
                    float temp = startAlpha;
                    startAlpha = endAlpha;
                    endAlpha = temp;
                }

                whiteCol.a = Mathf.Lerp(startAlpha, endAlpha, time);
                shutterEffectPanel.color = whiteCol;
                yield return null;
            }
        }

        private IEnumerator GetToPosition()
        {
            float time = 0f;
            bool reachedPosition = false;
            Vector3 startPos = tempPhoto.transform.position;
            Quaternion startRot = tempPhoto.transform.rotation;
            while (!reachedPosition)
            {
                time += moveSpeed * Time.deltaTime;
                if (time >= 1f)
                {
                    reachedPosition = true;
                    tempPhoto.transform.position = photoTransforms[photoCount].position;
                    tempPhoto.transform.eulerAngles = photoTransforms[photoCount].eulerAngles;

                    photoTransforms[photoCount].gameObject.SetActive(false);
                    break;
                }

                tempPhoto.transform.position = Vector3.Lerp(startPos, photoTransforms[photoCount].position, time);
                tempPhoto.transform.rotation = Quaternion.Lerp(startRot, photoTransforms[photoCount].rotation, time);
                yield return null;
            }

            photoCount++;
        }
        #endregion Picture
    }
}