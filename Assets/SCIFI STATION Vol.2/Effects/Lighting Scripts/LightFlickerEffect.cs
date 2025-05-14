using System;
using System.Collections;
using UnityEngine;


public class LightFlickerEffect : MonoBehaviour
{
    [Tooltip("External light to flicker; you can leave this to null if you attach this script to a light.")]
    [SerializeField] private new Light light;
    
    [SerializeField] private float flickerStateDelay = 0.8f;
    [SerializeField] private float flickeringStateLengthModifier = 1f;
    [SerializeField] private float fullStateLengthModifier = 2f;
    [SerializeField] private float halfStateLengthModifier = 1f;
    [SerializeField] private float quarterStateLengthModifier = 0.5f;
    
    // The original full intensity of the light
    private float fullIntensity;
    
    // Last timestamp of the last light state change
    private float lastStateChangedTime;
    
    // How much the current state is going to last
    private float currentStateDuration;
    
    // Current state of the light/lamp
    private LightState? state;
    
    private readonly System.Random random = new System.Random();

    private LightState? State
    {
        get
        {
            float differenceFromLastTime = Time.time - lastStateChangedTime;

            return differenceFromLastTime > currentStateDuration ? null : state;
        }
        set
        {
            if (state == value)
                return;
            
            state = value;
            
            lastStateChangedTime = Time.time;
            
            if (value != null) ChangeNeon(value.Value);
        }
    }

    private void Awake()
    {
        light = GetComponent<Light>();
        
        fullIntensity = light.intensity;
    }

    private void Start()
    {
        lastStateChangedTime = Time.time;
        
        ChangeNeon(LightState.FullLightOn);
    }

    private void Update()
    {
        if (light.enabled)
        {
            State ??= (LightState) random.Next(0, 4);
        }
        else
        {
            State = LightState.QuarterLightOn;
        }
    }
    
    private void ChangeNeon(LightState newState)
    {
        switch (newState)
        {
            case LightState.FullLightOn:
                light.intensity = fullIntensity;
                break;
            
            case LightState.Flickering:
                StartCoroutine(FlickeringCoroutine());
                break;
            
            case LightState.HalfLightOn:
                light.intensity = fullIntensity / 2f;
                break;
            
            case LightState.QuarterLightOn:
                light.intensity = fullIntensity / 4f;
                break;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    
        lastStateChangedTime = Time.time;
        
        currentStateDuration = random.Next(0,3);

        currentStateDuration *= newState switch
        {
            LightState.FullLightOn => fullStateLengthModifier,
            LightState.Flickering => flickeringStateLengthModifier,
            LightState.HalfLightOn => halfStateLengthModifier,
            LightState.QuarterLightOn => quarterStateLengthModifier,
            _ => throw new ArgumentOutOfRangeException(nameof(newState), newState, null)
        };
    }

    private int flickers;
    private IEnumerator FlickeringCoroutine()
    {
        flickers = 0;
        
        while (State == LightState.Flickering && flickers < random.Next(2, 5))
        {
            light.intensity = fullIntensity / 4;
            yield return new WaitForSeconds(flickerStateDelay);
            // ReSharper disable once Unity.InefficientPropertyAccess
            light.intensity = fullIntensity;
            yield return new WaitForSeconds(flickerStateDelay);

            flickers++;
        }
    }
    
    ///<summary>
    /// Represents the state of the light
    ///</summary>
    private enum LightState
    {
        ///<summary>
        ///Light is flickering
        ///</summary>
        Flickering, 
        ///<summary>
        ///Light is at 100% power, stationary
        ///</summary>
        FullLightOn,
        ///<summary>
        ///Light is at 50% power, stationary
        ///</summary>
        HalfLightOn, 
        ///<summary>
        ///Light is at 25% power, stationary
        ///</summary>
        QuarterLightOn
    }
}