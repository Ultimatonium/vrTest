using System.Collections;
using UnityEngine;

public class CiliaFanDebug : MonoBehaviour
{
    [SerializeField] private byte FanSpeed = 255;
    [SerializeField] private SurroundPosition CiliaGroup = (SurroundPosition)0;
    [SerializeField] private SmellList Scent;
    [SerializeField] private byte red = 255;
    [SerializeField] private byte green = 255;
    [SerializeField] private byte blue = 255;


    void Start()
    {
        StartCoroutine(SetFan());
    }

    private IEnumerator SetFan()
    {
        Debug.Log(Cilia.Instance.IsConnected);
        yield return new WaitUntil(() => Cilia.Instance.IsConnected);
        Debug.Log(Cilia.Instance.IsConnected);
        Cilia.setFan(CiliaGroup, Scent, FanSpeed);
        Cilia.setLight(CiliaGroup, (uint)Scent + 1, red, green, blue);
    }

    private void FixedUpdate()
    {
        //Cilia.setFan(CiliaGroup, Scent, FanSpeed);
    }
}
