using System.Collections.Generic;
using UnityEngine;

namespace FFmpegOut
{
    public class CaptureController : Singleton<CaptureController>
    {
        #region Data

        public List<string> captureList = new List<string>();
        private Dictionary<string, CameraCapture> availableCaptures = new Dictionary<string, CameraCapture>();
        public bool capturing { get { foreach (var item in availableCaptures) if (item.Value.isCapturing) return true; return false; } }

        #endregion

        #region Dictionary Methods

        public void Set(string key, CameraCapture value)
        {
            if (availableCaptures.ContainsKey(key)) availableCaptures[key] = value;
            else availableCaptures.Add(key, value);
            captureList.Add(key);
        }

        public void Del(string key)
        {
            if (availableCaptures.ContainsKey(key)) availableCaptures.Remove(key);
            captureList.Remove(key);
        }

        public CameraCapture Get(string key)
        {
            CameraCapture result = null;
            if (availableCaptures.ContainsKey(key)) result = availableCaptures[key];
            return result;
        }

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
        public void PrintCaptures() { foreach (var item in availableCaptures) print(item.Key); }

        #endregion
    }
}

