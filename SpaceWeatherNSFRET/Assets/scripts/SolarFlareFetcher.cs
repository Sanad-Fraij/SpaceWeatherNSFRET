using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Pulls solar flares from NASA's DONKI API for the past N days, counts
/// C / M / X class flares per day, and shows them in a TextMeshPro text box:
///
///   date | C | M | X
///   2026-11-12 | 3 | 1 | 0
///   ...
///
/// Results are cached to disk, so the API is only called once per (UTC) day.
/// </summary>
public class SolarFlareFetcher : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private TMP_Text cText;
    [SerializeField] private TMP_Text mText;
    [SerializeField] private TMP_Text xText;

    [Header("DONKI API")]
    [SerializeField] private int daysBack = 7; // CCMC rejects ranges over 30 days
    [Tooltip("Only used for the older api.nasa.gov fallback. The CCMC host needs no key.")]
    [SerializeField] private string apiKey = "DEMO_KEY";

    [Header("Debug")]
    [Tooltip("Ignore the cache and hit the API anyway.")]
    [SerializeField] private bool forceRefresh = false;

    // DONKI is served from CCMC's host (no key). The older api.nasa.gov gateway is
    // tried second, since reports on whether it still works are inconsistent.
    private const string PrimaryUrl = "https://ccmc.gsfc.nasa.gov/DONKI-API/get/FLR";
    private const string LegacyUrl = "https://api.nasa.gov/DONKI/FLR";
    private const string DateFormat = "yyyy-MM-dd";

    private string CachePath => Path.Combine(Application.persistentDataPath, "solar_flare_cache.json");

    // ---------- JSON models ----------

    [Serializable]
    private class FlareEntry
    {
        public string flrID;
        public string beginTime;
        public string peakTime;
        public string classType; // e.g. "M2.5"
    }

    [Serializable]
    private class FlareList
    {
        public List<FlareEntry> items;
    }

    [Serializable]
    private class DayRow
    {
        public string date;
        public int c;
        public int m;
        public int x;
    }

    [Serializable]
    private class CacheData
    {
        public string lastFetchDate; // UTC date of the last successful API call
        public List<DayRow> rows = new List<DayRow>();
    }

    // ---------- Lifecycle ----------

    private void Start()
    {
        string today = DateTime.UtcNow.ToString(DateFormat);
        CacheData cache = LoadCache();

        if (!forceRefresh && cache != null && cache.lastFetchDate == today)
        {
            Debug.Log("[SolarFlare] Already fetched today, using cached data.");
            Display(cache.rows);
            return;
        }

        StartCoroutine(FetchFlares(cache));
    }

    // ---------- Fetching ----------

    private IEnumerator FetchFlares(CacheData oldCache)
    {
        DateTime end = DateTime.UtcNow.Date;
        DateTime start = end.AddDays(-(daysBack - 1));
        string range = $"startDate={start.ToString(DateFormat)}&endDate={end.ToString(DateFormat)}";

        // Try each host in order; the first valid JSON response wins.
        string[] urls =
        {
            $"{PrimaryUrl}?{range}",
            $"{LegacyUrl}?{range}&api_key={apiKey}"
        };

        SetText("Loading solar flare data...");

        foreach (string url in urls)
        {
            string shortUrl = url.Split('?')[0]; // keep the key out of the logs

            using (UnityWebRequest req = UnityWebRequest.Get(url))
            {
                req.timeout = 15;
                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[SolarFlare] {shortUrl} failed: {req.error}");
                    continue;
                }

                List<DayRow> rows = ParseFlares(req.downloadHandler.text, start, end);
                if (rows == null)
                {
                    Debug.LogWarning($"[SolarFlare] {shortUrl} didn't return JSON, skipping.");
                    continue;
                }

                SaveCache(new CacheData
                {
                    lastFetchDate = DateTime.UtcNow.ToString(DateFormat),
                    rows = rows
                });
                Display(rows);
                yield break;
            }
        }

        // Every host failed: fall back to the last good cache, if any.
        if (oldCache != null && oldCache.rows.Count > 0)
            Display(oldCache.rows, $"(offline - showing data from {oldCache.lastFetchDate})");
        else
            SetText("Could not load solar flare data.");
    }

    private List<DayRow> ParseFlares(string json, DateTime start, DateTime end)
    {
        // Build an empty row for every day so quiet days show 0 | 0 | 0.
        var byDate = new Dictionary<string, DayRow>();
        var ordered = new List<DayRow>();
        for (DateTime d = start; d <= end; d = d.AddDays(1))
        {
            var row = new DayRow { date = d.ToString(DateFormat) };
            byDate[row.date] = row;
            ordered.Add(row);
        }

        if (string.IsNullOrWhiteSpace(json))
            return ordered; // empty body = no flares in range

        if (!json.TrimStart().StartsWith("["))
            return null; // not JSON (e.g. an HTML notice page) - treat as a failure

        // JsonUtility can't parse a top-level array, so wrap it in an object.
        FlareList list = JsonUtility.FromJson<FlareList>("{\"items\":" + json + "}");
        if (list == null || list.items == null)
            return ordered;

        foreach (FlareEntry f in list.items)
        {
            if (string.IsNullOrEmpty(f.classType)) continue;

            // Group by peak time (falls back to begin time). Format: 2026-11-12T05:00Z
            string stamp = !string.IsNullOrEmpty(f.peakTime) ? f.peakTime : f.beginTime;
            if (string.IsNullOrEmpty(stamp) || stamp.Length < 10) continue;

            if (!byDate.TryGetValue(stamp.Substring(0, 10), out DayRow row)) continue;

            switch (char.ToUpperInvariant(f.classType[0]))
            {
                case 'C': row.c++; break;
                case 'M': row.m++; break;
                case 'X': row.x++; break;
            }
        }

        return ordered;
    }

    // ---------- Cache ----------

    private CacheData LoadCache()
    {
        try
        {
            if (!File.Exists(CachePath)) return null;
            return JsonUtility.FromJson<CacheData>(File.ReadAllText(CachePath));
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SolarFlare] Couldn't read cache: {e.Message}");
            return null;
        }
    }

    private void SaveCache(CacheData cache)
    {
        try
        {
            File.WriteAllText(CachePath, JsonUtility.ToJson(cache, true));
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SolarFlare] Couldn't write cache: {e.Message}");
        }
    }

    // ---------- Display ----------

    private void Display(List<DayRow> rows, string footer = null)
    {
        var dates = new StringBuilder("Date");
        var cs = new StringBuilder("C");
        var ms = new StringBuilder("M");
        var xs = new StringBuilder("X");

        foreach (DayRow r in rows)
        {
            dates.Append('\n').Append(r.date);
            cs.Append('\n').Append(r.c);
            ms.Append('\n').Append(r.m);
            xs.Append('\n').Append(r.x);
        }

        // Status note (e.g. "offline") goes under the dates so the number columns stay aligned.
        if (!string.IsNullOrEmpty(footer))
            dates.Append("\n\n").Append(footer);

        Assign(dateText, dates.ToString());
        Assign(cText, cs.ToString());
        Assign(mText, ms.ToString());
        Assign(xText, xs.ToString());
    }

    // Status / error messages show in the date box; the other boxes are cleared.
    private void SetText(string text)
    {
        Assign(dateText, text);
        Assign(cText, "");
        Assign(mText, "");
        Assign(xText, "");

        if (dateText == null) Debug.Log(text);
    }

    private static void Assign(TMP_Text box, string value)
    {
        if (box != null) box.text = value;
    }
}