using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SolarSystemSpinner : MonoBehaviour
{
    [System.Serializable]
    public class PlanetSettings
    {
        [Header("Target Object")]
        public Transform planetTransform;

        [Header("Distance & Scale")]
        [Tooltip("Distance of this planet from the Sun (measured in local space).")]
        public float distanceFromSun = 10f;

        [Tooltip("Uniform scale value for the planet's mesh size.")]
        public float sizeScale = 1f;

        [Header("Speed Controls")]
        [Tooltip("Speed at which the planet orbits around the Sun.")]
        public float orbitSpeed = 20f;

        [Tooltip("Speed at which the planet spins on its own axis.")]
        public float selfRotationSpeed = 50f;

        [Header("Orbit Variation")]
        [Tooltip("Starting angle offset around the Sun in degrees (0 - 360). Scatters planets randomly at start.")]
        [Range(0f, 360f)]
        public float timeOffset = 0f;

        [Tooltip("Tilt angle of the orbit plane relative to the Sun.")]
        public Vector3 orbitAxis = Vector3.up;
    }

    [Header("Global Controls")]
    [Tooltip("Global multiplier for all planet orbit speeds.")]
    public float globalOrbitSpeed = 1f;

    [Tooltip("Global scale multiplier applied to ALL planet sizes.")]
    public float globalPlanetScale = 1f;

    [Tooltip("Global scale multiplier applied to ALL planet orbital distances from the Sun.")]
    public float globalDistanceScale = 1f;

    [Header("Logarithmic Scaling Toggles")]
    [Tooltip("If checked, planet mesh sizes scale logarithmically based on globalPlanetScale.")]
    public bool useLogarithmicPlanetScale = false;

    [Tooltip("If checked, planet distances scale logarithmically based on globalDistanceScale.")]
    public bool useLogarithmicDistanceScale = false;

    [Header("Auto Population")]
    [Tooltip("If true, automatically populates empty planet slots from child objects on Awake.")]
    public bool autoPopulateChildren = true;

    [Header("Planets")]
    public List<PlanetSettings> planets = new List<PlanetSettings>();

    void Awake()
    {
        // Auto-populate child objects if no planets are manually assigned in Inspector
        if (autoPopulateChildren && planets.Count == 0)
        {
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = transform.GetChild(i);
                float initialDistance = Vector3.Distance(transform.position, child.position);
                float safeDistance = Mathf.Max(initialDistance, 0.1f);

                PlanetSettings setting = new PlanetSettings
                {
                    planetTransform = child,
                    distanceFromSun = safeDistance,
                    // Take the X scale component as the uniform scale factor
                    sizeScale = child.localScale.x,
                    // Closer planets orbit faster by default
                    orbitSpeed = 100f / Mathf.Sqrt(safeDistance),
                    selfRotationSpeed = Random.Range(30f, 100f),
                    // Randomize starting position on orbit path
                    timeOffset = Random.Range(0f, 360f),
                    orbitAxis = Vector3.up
                };

                planets.Add(setting);
            }
        }
    }

    void Start()
    {
        // Apply initial distance, local scale, and time offset scatter
        foreach (var planet in planets)
        {
            if (planet == null || planet.planetTransform == null) continue;

            // 1. Set planet distance relative to Sun along local forward direction
            float effectiveDistanceScale = CalculateScaleMultiplier(globalDistanceScale, useLogarithmicDistanceScale);
            float totalDistance = planet.distanceFromSun * effectiveDistanceScale;
            
            Vector3 directionFromSun = (planet.planetTransform.position - transform.position).normalized;
            if (directionFromSun == Vector3.zero) directionFromSun = transform.forward;
            
            planet.planetTransform.position = transform.position + (directionFromSun * totalDistance);

            // 2. Set initial size scale (using 1 uniform float value across X, Y, Z)
            float effectivePlanetScale = CalculateScaleMultiplier(globalPlanetScale, useLogarithmicPlanetScale);
            planet.planetTransform.localScale = Vector3.one * (planet.sizeScale * effectivePlanetScale);

            // 3. Apply starting angle offset (timeOffset)
            Vector3 axis = planet.orbitAxis == Vector3.zero ? Vector3.up : planet.orbitAxis.normalized;
            planet.planetTransform.RotateAround(transform.position, axis, planet.timeOffset);
        }
    }

    void Update()
    {
        // Calculate effective scale multipliers once per frame
        float effectivePlanetScale = CalculateScaleMultiplier(globalPlanetScale, useLogarithmicPlanetScale);
        float effectiveDistanceScale = CalculateScaleMultiplier(globalDistanceScale, useLogarithmicDistanceScale);

        foreach (var planet in planets)
        {
            if (planet == null || planet.planetTransform == null) continue;

            Vector3 axis = planet.orbitAxis == Vector3.zero ? Vector3.up : planet.orbitAxis.normalized;

            // Update planet size continuously in case global or local scale changes in real time
            planet.planetTransform.localScale = Vector3.one * (planet.sizeScale * effectivePlanetScale);

            // Maintain correct scaled distance from Sun while maintaining orbital direction
            Vector3 offsetFromSun = planet.planetTransform.position - transform.position;
            float desiredDistance = planet.distanceFromSun * effectiveDistanceScale;
            if (offsetFromSun.sqrMagnitude > 0.001f)
            {
                planet.planetTransform.position = transform.position + (offsetFromSun.normalized * desiredDistance);
            }

            // 1. Orbit around the Sun
            float currentOrbitSpeed = planet.orbitSpeed * globalOrbitSpeed;
            planet.planetTransform.RotateAround(transform.position, axis, currentOrbitSpeed * Time.deltaTime);

            // 2. Rotate planet on its own axis independently
            planet.planetTransform.Rotate(Vector3.up, planet.selfRotationSpeed * Time.deltaTime, Space.Self);
        }
    }

    /// <summary>
    /// Helper method to return either linear scale or logarithmic scale.
    /// Log scale uses Log10(1 + multiplier) to ensure 0 maps to 0 and scaling is smooth.
    /// </summary>
    private float CalculateScaleMultiplier(float rawValue, bool useLogarithmic)
    {
        if (!useLogarithmic) return rawValue;

        // Ensure negative or zero values don't break Log10 calculations
        if (rawValue <= 0f) return 0f;

        // Log10(1 + X) produces smooth dampening as values get larger
        return Mathf.Log10(1f + rawValue);
    }
}