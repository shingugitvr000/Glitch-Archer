using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlickerEffect_2 : MonoBehaviour
{
    public float minInten;
    public float maxInten;
    public float velocity;

    private Light lights;
    private bool loop = true;

    private void Start()
    {
        lights = GetComponent<Light>();
        StartCoroutine(time());
    }

    IEnumerator time()
    {
        while(loop)
        {
            yield return new WaitForSeconds(velocity);
            lights.intensity = Random.Range(minInten, maxInten);
        }
    }
}
