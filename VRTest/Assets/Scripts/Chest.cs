using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public Transform head;
    private readonly float distanceToHead = 0.5f;

    private void Update()
    {
        transform.position = head.position - new Vector3(0, distanceToHead);
        transform.localEulerAngles = new Vector3(0, head.localEulerAngles.y, 0);
    }
}
