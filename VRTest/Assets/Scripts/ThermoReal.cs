using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bhaptics.Tact.Unity;
using Bhaptics.Tact;
using Valve.VR.InteractionSystem;

public class ThermoReal : MonoBehaviour
{

    enum Temperature
    {
        NONE,
        hot,
        cold
    }

    [SerializeField]
    private Temperature temperature;
    [SerializeField]
    private int durationMillis;

    [Range(0f, 1f)]
    [SerializeField]
    private float intensity;
    [SerializeField]
    private bool intensityByDistance;
    [SerializeField]
    private Transform maxIntensityTransform;

    [HideInInspector]
    public string key = System.Guid.NewGuid().ToString();

    private SphereCollider ownCollider;

    private List<DotPoint> dot = new List<DotPoint>();
    public float Intensity
    {
        get { return intensity; }
        set
        {
            intensity = value;
            UpdateDot(value);
        }
    }

    private void Awake()
    {
        Intensity = intensity;
    }

    private void Start()
    {
        ownCollider = gameObject.GetComponent<SphereCollider>();
    }

    private void UpdateDot(float intensity)
    {
        int dotIntensity;
        dotIntensity = (int)(intensity * 100 / 2);
        if (temperature == Temperature.hot)
        {
            dotIntensity += 50;
        }
        dot.Clear();
        dot.Add(new DotPoint(0, dotIntensity));
        dot.Add(new DotPoint(1, dotIntensity));
        dot.Add(new DotPoint(2, dotIntensity));
    }

    public void Play(Hand position, int duration)
    {
        IHaptic haptic = BhapticsManager.GetHaptic();

        if (haptic == null)
        {
            Debug.LogError("haptic not available");
            return;
        }

        switch (position)
        {
            case Hand.left:
                haptic.Submit(key, PositionType.HandL, dot, duration);
                break;
            case Hand.right:
                haptic.Submit(key, PositionType.HandR, dot, duration);
                break;
            default:
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        ThermalReceiver receiver = collision.gameObject.GetComponentInParent<ThermalReceiver>();
        if (receiver != null)
        {
            Play(receiver.hand, durationMillis);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        ThermalReceiver receiver = collision.gameObject.GetComponentInParent<ThermalReceiver>();
        if (receiver != null)
        {
            Play(receiver.hand, (int)(Time.fixedDeltaTime * 1000) * 2);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        ThermalReceiver receiver = other.gameObject.GetComponentInParent<ThermalReceiver>();
        if (receiver != null)
        {
            if (intensityByDistance)
            {
                HandCollider handCollider = other.GetComponentInParent<HandCollider>();
                UpdateDot((1 - (Vector3.Distance(handCollider.transform.position, maxIntensityTransform.position) / (ownCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z)))) * intensity);
            }
            Play(receiver.hand, (int)(Time.fixedDeltaTime * 1000) * 2);
        }
    }
}

