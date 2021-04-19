using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ConsoleUI : MonoBehaviour
{
    private enum AttachPosition
    {
        NONE,
        leftController,
        rightController,
        HUD
    }
    private TextMesh textMesh;
    [SerializeField]
    private AttachPosition attachTo;
    [SerializeField]
    private int amountOfMessages;
    private Queue<string> messages = new Queue<string>();


    private void Start()
    {
        CreateConsoleObject();
        switch (attachTo)
        {
            case AttachPosition.NONE:
                break;
            case AttachPosition.leftController:
                if (Player.instance.leftHand == null)
                {
                    Debug.LogWarning("left hand missing");
                    break;
                }
                textMesh.transform.parent = Player.instance.leftHand.gameObject.transform;
                textMesh.transform.localPosition = new Vector3(0, 0, 0);
                break;
            case AttachPosition.rightController:
                break;
            case AttachPosition.HUD:
                break;
            default:
                break;
        }
    }

    private void CreateConsoleObject()
    {
        textMesh = new GameObject("Console").AddComponent<TextMesh>();
        textMesh.fontSize = 120;
        textMesh.characterSize = 0.001f;
        textMesh.transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
    }

    private void OnEnable()
    {
        Application.logMessageReceived += logMessageReceived;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= logMessageReceived;

    }

    private void logMessageReceived(string condition, string stackTrace, LogType type)
    {
        if (messages.Count >= amountOfMessages) { messages.Dequeue(); }
        messages.Enqueue(condition);
        if (textMesh == null) return;
        textMesh.text = "";
        string[] messageArray = messages.ToArray();
        for (int i = 0; i < messageArray.Length; i++)
        {
            textMesh.text += messageArray[i] + "\n";
        }
    }
}
