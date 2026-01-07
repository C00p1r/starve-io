using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TimeManager : MonoBehaviour
{
    [Header("Cycle Settings")]
    [SerializeField] private float dayDurationSeconds = 60f;
    [SerializeField] private float nightDurationSeconds = 60f;
    [SerializeField] private float transitionSeconds = 10f;
    [SerializeField] private bool startAtDay = true;

    [Header("Light Settings")]
    [SerializeField] private Light2D globalLight;
    [SerializeField] private Color dayColor = Color.white;
    [SerializeField] private Color nightColor = new Color(0.2f, 0.25f, 0.35f, 1f);
    [SerializeField] private float dayIntensity = 1f;
    [SerializeField] private float nightIntensity = 0.25f;
    [SerializeField] private bool applyToAllSortingLayers = true;

    private float phaseTimer;
    private bool isDay;

    public bool IsDay => isDay;
    public bool IsNight => !isDay;

    private void Awake()
    {
        if (globalLight == null)
            globalLight = GetComponent<Light2D>();
        if (globalLight == null)
            globalLight = FindFirstObjectByType<Light2D>();

        ApplySortingLayers();
        isDay = startAtDay;
    }

    private void Start()
    {
        ApplyLighting(GetCurrentDuration());
    }

    private void OnValidate()
    {
        dayDurationSeconds = Mathf.Max(0.1f, dayDurationSeconds);
        nightDurationSeconds = Mathf.Max(0.1f, nightDurationSeconds);
        transitionSeconds = Mathf.Max(0f, transitionSeconds);
        ApplySortingLayers();
    }

    private void Update()
    {
        if (globalLight == null)
            return;

        float duration = GetCurrentDuration();
        if (duration <= 0f)
            return;

        phaseTimer += Time.deltaTime;
        if (phaseTimer >= duration)
        {
            phaseTimer -= duration;
            isDay = !isDay;
            duration = GetCurrentDuration();
        }

        ApplyLighting(duration);
    }

    private float GetCurrentDuration()
    {
        return isDay ? dayDurationSeconds : nightDurationSeconds;
    }

    private void ApplyLighting(float currentDuration)
    {
        float transition = Mathf.Clamp(transitionSeconds, 0f, currentDuration);
        float blend = 0f;
        if (transition > 0f)
        {
            float transitionStart = currentDuration - transition;
            if (phaseTimer >= transitionStart)
                blend = Mathf.InverseLerp(transitionStart, currentDuration, phaseTimer);
        }

        Color fromColor = isDay ? dayColor : nightColor;
        Color toColor = isDay ? nightColor : dayColor;
        float fromIntensity = isDay ? dayIntensity : nightIntensity;
        float toIntensity = isDay ? nightIntensity : dayIntensity;

        globalLight.color = Color.Lerp(fromColor, toColor, blend);
        globalLight.intensity = Mathf.Lerp(fromIntensity, toIntensity, blend);
    }

    private void ApplySortingLayers()
    {
        if (!applyToAllSortingLayers || globalLight == null)
            return;

        var layers = SortingLayer.layers;
        var layerIds = new int[layers.Length];
        for (int i = 0; i < layers.Length; i++)
            layerIds[i] = layers[i].id;
        globalLight.targetSortingLayers = layerIds;
    }
}
