using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class Gun : MonoBehaviour
{

    public SteamVR_Action_Boolean actionTrigger;
    public SteamVR_Action_Vibration vibration;
    public GameObject bulletPrefab;
    [Tooltip("shot per minute")]
    public float cadence;
    private float timeTillShot = 0;
    private Transform shootingPoint;
    private Interactable interactable;

    private void Start()
    {
        interactable = GetComponent<Interactable>();
        shootingPoint = transform.Find("shootPoint");
    }

    private void Update()
    {
        SteamVR_Input_Sources hand = interactable.attachedToHand.handType;
        timeTillShot -= Time.deltaTime;
        if (timeTillShot <= 0)
        {
            if (actionTrigger.GetState(hand))
            {
                GameObject bullet = Instantiate(bulletPrefab, shootingPoint.transform.position, shootingPoint.transform.rotation);
                bullet.GetComponent<Rigidbody>().velocity = shootingPoint.forward * 10f;

                Destroy(bullet, 5f);

                timeTillShot = 1 / (cadence / 60);
            }
        }
    }
}
