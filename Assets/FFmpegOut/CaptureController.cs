using System.Collections.Generic;
using UnityEngine;

namespace FFmpegOut
{
    public class CaptureController : Singleton<CaptureController>
    {
        #region Data

        public Dictionary<string, CameraCapture> availableCaptures = new Dictionary<string, CameraCapture>();
        public bool capturing { get { foreach (var item in availableCaptures) if (item.Value.isCapturing) return true; return false; } }

        #endregion

        #region Public Methods
        public void CaptureById(string captureId)
        {
            CameraCapture camCap;
            if (availableCaptures.TryGetValue(captureId, out camCap))
                camCap.OpenCapture();
            else
                Debug.LogError("Capture " + captureId + " not found or disabled.");
        }
        public void StopById(string captureId)
        {
            CameraCapture camCap;
            if (availableCaptures.TryGetValue(captureId, out camCap))
                camCap.CloseCapture();
            else
                Debug.LogError("Capture " + captureId + " not found or disabled.");
        }
        public void CaptureAll() { foreach (var item in availableCaptures) if (!capturing) item.Value.OpenCapture(); }
        public void StopAll() { foreach (var item in availableCaptures) if (capturing) item.Value.CloseCapture(); }
        public void printCaptures() { foreach (var item in availableCaptures) print(item.Key); }

        #endregion
    }
}

