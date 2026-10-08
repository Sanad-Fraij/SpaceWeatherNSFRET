using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderToGlobalSpeed : MonoBehaviour
{
    [Header("Target Objects")]
    [Tooltip("First SolarSystemSpinner component to update.")]
    public SolarSystemSpinner targetSpinner1;

    [Tooltip("Second SolarSystemSpinner component to update.")]
    public SolarSystemSpinner targetSpinner2;

    [Header("Inspector Variable Checkboxes")]
    [Tooltip("Check this box in the Inspector to control Orbit Speed with the slider.")]
    public bool affectOrbitSpeed = true;

    [Tooltip("Check this box in the Inspector to control Planet Scale with the slider.")]
    public bool affectPlanetScale = false;

    [Tooltip("Check this box in the Inspector to control Distance Scale with the slider.")]
    public bool affectDistanceScale = false;

    private Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    void OnEnable()
    {
        if (slider != null)
        {
            slider.onValueChanged.AddListener(OnSliderValueChanged);
            OnSliderValueChanged(slider.value);
        }
    }

    void OnDisable()
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
    }

    // Called automatically whenever the slider moves in game
    private void OnSliderValueChanged(float value)
    {
        ApplyValueToTargets(value);
    }

    // Called in Update so changing checkboxes live in the Inspector updates immediately
    void Update()
    {
        if (slider != null)
        {
            ApplyValueToTargets(slider.value);
        }
    }

    private void ApplyValueToTargets(float value)
    {
        UpdateSpinnerProperties(targetSpinner1, value);
        UpdateSpinnerProperties(targetSpinner2, value);
    }

    private void UpdateSpinnerProperties(SolarSystemSpinner spinner, float value)
    {
        if (spinner == null) return;

        if (affectOrbitSpeed) spinner.globalOrbitSpeed = value;
        if (affectPlanetScale) spinner.globalPlanetScale = value;
        if (affectDistanceScale) spinner.globalDistanceScale = value;
    }
}