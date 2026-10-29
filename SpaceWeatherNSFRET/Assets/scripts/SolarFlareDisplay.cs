using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class SolarFlareDisplay : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TMP_Text flareDateText;

    [Header("API Configuration")]
    [Tooltip("Get a free key at https://api.nasa.gov")]
    [SerializeField] private string apiKey = "DEMO_KEY";

    private const string NasaApiUrl = "https://api.nasa.gov/DONKI/FLR";

    private void Start()
    {
        StartCoroutine(FetchLatestSolarFlare());
    }

    private IEnumerator FetchLatestSolarFlare()
    {
        // 1. Set search window (NASA defaults to the last 30 days if no dates are given)
        string requestUrl = $"{NasaApiUrl}?api_key={apiKey}";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(requestUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error fetching solar flare data: {webRequest.error}");
                if (flareDateText != null) flareDateText.text = "Error Loading Data";
                yield break;
            }

            // 2. Wrap JSON array into a root object so Unity's JsonUtility can parse it
            string rawJson = webRequest.downloadHandler.text;
            string wrappedJson = "{\"flares\":" + rawJson + "}";

            SolarFlareList flareList = JsonUtility.FromJson<SolarFlareList>(wrappedJson);

            // 3. Process the results
            if (flareList != null && flareList.flares != null && flareList.flares.Count > 0)
            {
                // Grab the last item in the list (the most recent event)
                SolarFlareEvent latestFlare = flareList.flares[flareList.flares.Count - 1];

                // Parse string timestamp into DateTime format (e.g., 2026-03-15T12:30:00Z)
                if (DateTime.TryParse(latestFlare.beginTime, out DateTime flareTime))
                {
                    // Format the date string as desired (e.g., "YYYY-MM-DD" or "MMM dd, yyyy")
                    flareDateText.text = flareTime.ToString("yyyy-MM-dd HH:mm UTC");
                }
                else
                {
                    flareDateText.text = latestFlare.beginTime;
                }
            }
            else
            {
                flareDateText.text = "No Recent Flares";
            }
        }
    }

    // Helper classes matching NASA DONKI JSON response format
    [Serializable]
    private class SolarFlareList
    {
        public List<SolarFlareEvent> flares;
    }

    [Serializable]
    private class SolarFlareEvent
    {
        public string flrID;
        public string beginTime;
        public string peakTime;
        public string endTime;
        public string classType;
    }
}