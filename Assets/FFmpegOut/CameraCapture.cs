using UnityEngine;

namespace FFmpegOut
{
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("FFmpegOut/Camera Capture")]
    public class CameraCapture : MonoBehaviour
    {
        #region Editable properties

        [SerializeField] bool _setResolution = true;
        [SerializeField] int _width = 1280;
        [SerializeField] int _height = 720;
        [SerializeField] int _frameRate = 30;
        [SerializeField] bool _allowSlowDown = true;
        [SerializeField] FFmpegPipe.Preset _preset;
        [SerializeField] float _startTime = 0;
        [SerializeField] float _recordLength = 5;
        [SerializeField] bool _capturing = false;

        #endregion

        #region Private members

        [SerializeField, HideInInspector] Shader _shader;
        Material _material;

        FFmpegPipe _pipe;
        float _elapsed;

        RenderTexture _tempTarget;
        GameObject _tempBlitter;

        static int _activePipeCount;

        #endregion

        #region MonoBehavior functions

        void OnValidate()
        {
            _startTime = Mathf.Max(_startTime, 0);
            _recordLength = Mathf.Max(_recordLength, 0.01f);
        }

        public void OpenCapture()
        {
            _capturing = true;
            if (!FFmpegConfig.CheckAvailable)
            {
                Debug.LogError(
                    "ffmpeg.exe is missing. "
                );
                enabled = false;
            }
        }

        public void CloseCapture() { _capturing = false; }

        // void OnEnable()
        // {
        //     if (!FFmpegConfig.CheckAvailable)
        //     {
        //         Debug.LogError(
        //             "ffmpeg.exe is missing. "
        //         );
        //         enabled = false;
        //     }
        // }

        void OnDisable()
        {
            if (_pipe != null) ClosePipe();
        }

        void OnDestroy()
        {
            if (_pipe != null) ClosePipe();
        }

        void Start()
        {
            _material = new Material(_shader);
        }

        void Update()
        {
            _elapsed += Time.deltaTime;

            // if (_startTime <= _elapsed && _elapsed < _startTime + _recordLength)
            if (_capturing)
            {
                if (_pipe == null) OpenPipe();
            }
            else
            {
                if (_pipe != null) ClosePipe();
            }
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_pipe != null)
            {
                var width = Mathf.RoundToInt(source.width / 8) * 8;
                var height = Mathf.RoundToInt(source.height / 8) * 8;
                var tempRT = RenderTexture.GetTemporary(width, height);
                Graphics.Blit(source, tempRT, _material, 0);

                var tempTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                tempTex.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
                tempTex.Apply();

                _pipe.Write(tempTex.GetRawTextureData());

                Destroy(tempTex);
                RenderTexture.ReleaseTemporary(tempRT);
            }
            Graphics.Blit(source, destination);
        }

        #endregion

        #region Private methods

        void OpenPipe()
        {
            if (_pipe != null) return;

            var camera = GetComponent<Camera>();
            var width = _width;
            var height = _height;

            // Apply the screen resolution settings.
            if (_setResolution)
            {
                _tempTarget = RenderTexture.GetTemporary(width, height, 24);
                camera.targetTexture = _tempTarget;
                _tempBlitter = Blitter.CreateGameObject(camera);
            }
            else
            {
                width = Mathf.RoundToInt(camera.pixelWidth / 8) * 8;
                height = Mathf.RoundToInt(camera.pixelHeight / 8) * 8;
            }

            // Open an output stream.
            _pipe = new FFmpegPipe(name, width, height, _frameRate, _preset);
            _activePipeCount++;

            // Change the application frame rate on the first pipe.
            if (_activePipeCount == 1)
            {
                if (_allowSlowDown)
                    Time.captureFramerate = _frameRate;
                else
                    Application.targetFrameRate = _frameRate;
            }

            Debug.Log("Capture started (" + _pipe.Filename + ")");
        }

        void ClosePipe()
        {
            var camera = GetComponent<Camera>();

            // Destroy the blitter object.
            if (_tempBlitter != null)
            {
                Destroy(_tempBlitter);
                _tempBlitter = null;
            }

            // Release the temporary render target.
            if (_tempTarget != null && _tempTarget == camera.targetTexture)
            {
                camera.targetTexture = null;
                RenderTexture.ReleaseTemporary(_tempTarget);
                _tempTarget = null;
            }

            // Close the output stream.
            if (_pipe != null)
            {
                Debug.Log("Capture ended (" + _pipe.Filename + ")");

                _pipe.Close();
                _activePipeCount--;

                if (!string.IsNullOrEmpty(_pipe.Error))
                {
                    Debug.LogWarning(
                        "ffmpeg returned with a warning or an error message. " +
                        "See the following lines for details:\n" + _pipe.Error
                    );
                }

                _pipe = null;

                // Reset the application frame rate on the last pipe.
                if (_activePipeCount == 0)
                {
                    if (_allowSlowDown)
                        Time.captureFramerate = 0;
                    else
                        Application.targetFrameRate = -1;
                }
            }
        }

        #endregion
    }
}
