/**
* This class MUST BE INCLUDED on an object in Unity 3D for the plugin to be able to communicate with the SDK.
* <pre>
* This class handles connecting and disconnecting from the SDK.
* This class allows a developer to setup networking if the SDK is on another computer.
* This class allows a developer to define a game profile for their game.
*  - defining smells
*  - defining groups of cilias
*  - defining default color scheme
* This class allows a developer to send messages to the SDK controlling fan speed and light colors.
* </pre>
* @author Peter Sassaman
* @version 0.2.7
* MIT License
* Copyright (c) 2019 Haptic Solutions Incorporated
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using System.IO;

[System.Serializable]
public struct Neopixel
{
    public byte redValue;
    public byte greenValue;
    public byte blueValue;
};
public class Cilia : MonoBehaviour
{
    [Header("------------Networking Section------------")]
    [Header("Most of the time leave this alone")]
    [SerializeField] private int CiliaPort = 1995;
    [SerializeField] private string CiliaIP = "localhost";
    [Header("-----------Game Profile Section-----------")]
    [SerializeField] private string GameProfileName = "Game";
    [SerializeField] private SurroundPosition DefaultSurroundGroup = (SurroundPosition) 0;
    /*Smells to add to sdk smell library. Also for setting default game profile. First six will be used*/
    [Header("Adds the following smells to the smell library")]
    [Header("Sets the Cilias to the 1st six smells if the profile didn't exist")]
    [SerializeField] private string[] SmellsToAddToSmellLibrary = { "Apple", "BahamaBreeze", "CleanCotton", "Leather", "Lemon", "Rose" };
    private static string[] privateLibrary = { "Apple", "BahamaBreeze", "CleanCotton", "Leather", "Lemon", "Rose" };
    [Header("Game Profile Default Lighting Values: 0-255")]
    [SerializeField] private Neopixel Light1;
    [SerializeField] private Neopixel Light2;
    [SerializeField] private Neopixel Light3;
    [SerializeField] private Neopixel Light4;
    [SerializeField] private Neopixel Light5;
    [SerializeField] private Neopixel Light6;
    [SerializeField] private string[] surroundGroupStrings = { "FrontCenter", "FrontLeft", "SideLeft", "RearLeft", "RearCenter", "RearRight", "SideRight", "FrontRight" };
    private string[] privateSurroundGroups = { "FrontCenter", "FrontLeft", "SideLeft", "RearLeft", "RearCenter", "RearRight", "SideRight", "FrontRight" };
    [Header("If you placed the Cilia scripts elsewhere please change this")]
    [SerializeField] private string PathToCiliaPluginScripts = "Assets/CiliaPlugin/Scripts/";
    [Header("Force clean game profile update. Do not leave checked. Sets all Cilias back to default smells next time you press play")]
    [SerializeField] private bool forceCleanUpdate = false;
    static TcpClient CiliaClient = new TcpClient();
    static NetworkStream CiliaStream;
    static byte[] message;
    const int SmellNameLength = 20;
    static bool mIsConnected = false;

    public bool IsConnected { get { return mIsConnected; } }
    private static Cilia instance;

    public static Cilia Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Cilia>();
            }
            return instance;
        }
    }

    /**
     * Connects Plugin to the SDK, adds our games smells to the SDK's smell library, and sends our games profile to the SDK.
     */
    void Start()
    {
        //we are already connected so don't need setup
        if(mIsConnected == true)
        {
            return;
        }
        //if this is our first time starting check if there is another Cilia if there is destroy ourselves
        GameObject[] cilias = GameObject.FindGameObjectsWithTag("Cilia");
        if (cilias.Length > 1)
        {
            Destroy(this.gameObject);
        }
        //If we are the first Cilia Make ourselves persistent for the entire game
        DontDestroyOnLoad(this.gameObject);
        /*Connect Networking*/
        try
        {
            CiliaClient.Connect(CiliaIP, CiliaPort);
            CiliaStream = CiliaClient.GetStream();
            mIsConnected = true;
        }
        catch (Exception e)
        {
            Debug.Log("Failed to connect to SDK: " + e);
            mIsConnected = false;
            return;
        }
        /*Start Creating Messages for setting up library and prfiles*/
        string smellsForLibraryMessage = "[!#AddToLibrary|";
        string gameProfileMessage = "[!#LoadProfile|" + GameProfileName + "," + (int)DefaultSurroundGroup + ",";
        if(forceCleanUpdate)
        {
            gameProfileMessage = "[!#LoadProfileForce|" + GameProfileName + "," + (int)DefaultSurroundGroup + ",";
        }
        for (int i = 0; i < SmellsToAddToSmellLibrary.Length; i++)
        {
            smellsForLibraryMessage += SmellsToAddToSmellLibrary[i] + ",";
        }
        for (int i = 0; i < 6; i++)
        {
            gameProfileMessage += SmellsToAddToSmellLibrary[i] + ",";
        }
        gameProfileMessage += getLightString(Light1) + ",";
        gameProfileMessage += getLightString(Light2) + ",";
        gameProfileMessage += getLightString(Light3) + ",";
        gameProfileMessage += getLightString(Light4) + ",";
        gameProfileMessage += getLightString(Light5) + ",";
        gameProfileMessage += getLightString(Light6) + ",";
        smellsForLibraryMessage = smellsForLibraryMessage.TrimEnd(',');
        smellsForLibraryMessage += "]";

        for (int i = 0; i < privateSurroundGroups.Length; i++)
        {
            gameProfileMessage += privateSurroundGroups[i] + ",";
        }
        gameProfileMessage = gameProfileMessage.TrimEnd(',');
        gameProfileMessage += "]";
        sendMessageToCilia(smellsForLibraryMessage);
        sendMessageToCilia(gameProfileMessage);
    }

    /**
     * Sends a message to the SDK to set a surround position's specific light nuber to a RGB color
     * @param aSurroundPosition enumerated value indicating which group of Cilias we want to change the lighting on.
     * @param aLightNumber between 1-6 of which light we want to change the color of.
     * @param aRedValue between 0-255 of how red the light will be.
     * @param aGreenValue between 0-255 of how green the light will be
     * @param aBlueValue between 0-255 of how blue the light will be
     */
    public static void setLight(SurroundPosition aSurroundPosition, uint aLightNumber, byte aRedValue, byte aGreenValue, byte aBlueValue)
    {
            if (aLightNumber > 6)
            {
                aLightNumber = 6;
            }
            string toSend = "";
            if (aSurroundPosition == SurroundPosition.All)
                toSend = "[" + aSurroundPosition.ToString() + ",N" + aLightNumber.ToString("D1") + aRedValue.ToString("D3") + aGreenValue.ToString("D3") + aBlueValue.ToString("D3") + "]";
            else
                toSend = "[G<" + (byte)aSurroundPosition + ">,N" + aLightNumber.ToString("D1") + aRedValue.ToString("D3") + aGreenValue.ToString("D3") + aBlueValue.ToString("D3") + "]";
            sendMessageToCilia(toSend);
    }
    /**
     * Sends a message to the SDK asking it to set all the fans with a specific smell in a specific surroundPosition to a specified speed.
     * @param aSurroundPosition group of Cilias we want this to apply to.
     * @param aSmell we want the user to smell the SDK uses this to find fans with the smell.
     * @param aFanSpeed a value between 0-255 specifying how fast we want the fant to spin.
     */
    public static void setFan(SurroundPosition aSurroundPosition, SmellList aSmell, byte aFanSpeed)
    {
            string toSend = "";
            if (aSurroundPosition == SurroundPosition.All)
                toSend = "[" + aSurroundPosition.ToString() + "F," + aSmell.ToString() + "," + aFanSpeed.ToString("D3") + "]";
            else
                toSend = "[G<" + (byte)aSurroundPosition + ">F," + aSmell.ToString() + "," + aFanSpeed.ToString("D3") + "]";

            sendMessageToCilia(toSend);
            //Debug.Log(toSend);
            //Debug.Log(mIsConnected);
    }
    /**
     * Returns a string of what value a neopixel is currently set to with three deimal places for red green and blue
     * @param aNeopixel structure to retrieve the string value from
     */
    static string getLightString(Neopixel aNeopixel)
    {
        return aNeopixel.redValue.ToString("D3") + aNeopixel.greenValue.ToString("D3") + aNeopixel.blueValue.ToString("D3");
    }

    /**
     * Sends a string message to the SDK.
     * @param aMessageToSend string message to send to the SDK.
     */
    static void sendMessageToCilia(string aMessageToSend)
    {
        if (mIsConnected)
        {
            message = System.Text.Encoding.ASCII.GetBytes(aMessageToSend);

            CiliaStream.Write(message, 0, message.Length);
        }
    }
    /**
     * Closes TCP/IP stream and client that were connected to the SDK.
     */
    void OnApplicationQuit()
    {
        if (mIsConnected)
        {
            CiliaStream.Close();
            CiliaClient.Close();
        }
    }

    /* Code for validating inputs and modifying files. Please only touch if you are very comfortable with programming!!! */

    /**
     * Validates the surround groups and smells added by the user
     */
    private void OnValidate()
    {
        if (SmellsToAddToSmellLibrary.Length == privateLibrary.Length)
        {
            for (int i = 0; i < SmellsToAddToSmellLibrary.Length; i++)
            {
                if (SmellsToAddToSmellLibrary[i].Length == 0)
                {
                    SmellsToAddToSmellLibrary[i] = privateLibrary[i];

                }
                else
                {
                    string SmellsToAddCopy = "";
                    if (SmellsToAddToSmellLibrary[i].Length > SmellNameLength)
                        SmellsToAddToSmellLibrary[i] = SmellsToAddToSmellLibrary[i].Substring(0, SmellNameLength);
                    foreach (char c in SmellsToAddToSmellLibrary[i])
                    {
                        if (char.IsLetterOrDigit(c))
                            SmellsToAddCopy += c;
                    }
                    if (SmellsToAddCopy.Length != 0)
                    {
                        SmellsToAddToSmellLibrary[i] = SmellsToAddCopy;
                        privateLibrary[i] = SmellsToAddToSmellLibrary[i];
                    }
                    else
                    {
                        SmellsToAddToSmellLibrary[i] = privateLibrary[i];
                    }
                }
            }
        }
        else if (SmellsToAddToSmellLibrary.Length < 6)
        {
            SmellsToAddToSmellLibrary = privateLibrary;
        }
        else
        {
            privateLibrary = SmellsToAddToSmellLibrary;
        }

        if (surroundGroupStrings.Length == privateSurroundGroups.Length)
        {
            for (int i = 0; i < surroundGroupStrings.Length; i++)
            {
                if (surroundGroupStrings[i].Length == 0)
                {
                    surroundGroupStrings[i] = privateSurroundGroups[i];

                }
                else
                {
                    string GroupsToAddCopy = "";
                    if (surroundGroupStrings[i].Length > SmellNameLength)
                        surroundGroupStrings[i] = surroundGroupStrings[i].Substring(0, SmellNameLength);
                    foreach (char c in surroundGroupStrings[i])
                        if (char.IsLetterOrDigit(c))
                            GroupsToAddCopy += c;
                    if (GroupsToAddCopy.Length != 0)
                    {
                        surroundGroupStrings[i] = GroupsToAddCopy;
                        privateSurroundGroups[i] = surroundGroupStrings[i];
                    }
                    else
                    {
                        surroundGroupStrings[i] = privateSurroundGroups[i];
                    }
                }
            }
        }
        else if (surroundGroupStrings.Length > 256)
        {
            surroundGroupStrings = privateSurroundGroups;
        }
        else if (surroundGroupStrings.Length < 1)
        {
            surroundGroupStrings = privateSurroundGroups;
        }
        else
        {
            privateSurroundGroups = surroundGroupStrings;
        }
    }
    /**
     * Used to modify SurroundPositions file to have a new enumerated type with the surround groups for a game.
     */
    public void AddSurroundGroups()
    {
        string myString = "public enum SurroundPosition { ";
        if (surroundGroupStrings.Length >= 1)
        {
            myString += "" + surroundGroupStrings[0];
            for (int i = 1; i < surroundGroupStrings.Length; i++)
            {
                myString += ", " + surroundGroupStrings[i];
            }
            myString += ", All};";
            string[] mystrings = { myString, "" };
            File.WriteAllLines(PathToCiliaPluginScripts + "SurroundPosition.cs", mystrings);
        }
    }
    /**
     * Used to modify SmellsList file to have a new enumerated type with the smells for a game.
     */
    public void AddSmellsList()
    {
        string myString = "public enum SmellList { ";
        if (SmellsToAddToSmellLibrary.Length >= 1)
        {
            myString += "" + SmellsToAddToSmellLibrary[0];
            for (int i = 1; i < SmellsToAddToSmellLibrary.Length; i++)
            {
                myString += ", " + SmellsToAddToSmellLibrary[i];
            }
            myString += "};";
            string[] mystrings = { myString, "" };
            File.WriteAllLines(PathToCiliaPluginScripts + "SmellsList.cs", mystrings);
        }
    }
}
