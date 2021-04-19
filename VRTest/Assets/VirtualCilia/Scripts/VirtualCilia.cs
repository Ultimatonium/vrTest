using UnityEngine;
using System.IO.Ports;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using UnityEngine.UI;

public class VirtualCilia : MonoBehaviour
{
    /*Serializable*/
    [SerializeField] private SurroundPosition surroundPosition;
    [SerializeField] private RPMAnimate[] mRPM = new RPMAnimate[6];
    [SerializeField] private CiliaLight[] mColorPickers = new CiliaLight[6];
    [SerializeField] private InputField mInputField;
    private bool mKeepAlive = true;
    private string[] mFans = new string[6];
    private string[] mNeopixels = new string[7];
    private SerialPort mCOMX;
    private bool mSuccess = false;
    private byte[] mBuffer = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    private string[] mFan = { "000", "000", "000", "000", "000", "000" };
    private string[] mOldfan = { "", "", "", "", "", "" };
    private string[] mLight = { "000000000", "000000000", "000000000", "000000000", "000000000", "000000000", "000000000" };
    private string[] mOldlight = { "", "", "", "", "", "" };
    private int mComInt;


    private void Start()
    {
        Init();
        SetUpConnection();
        StartReadThread();
        Confirm();
        //ChangeGroup((byte)mComInt, (byte)surroundPosition);
    }

    private void Update()
    {
        if (mSuccess)
        {
            for (int i = 0; i < mFan.Length; i++)
            {
                if (!mFan[i].Equals(mOldfan[i]))
                {
                    mRPM[i].setFanSpeed(float.Parse(mFan[i]));
                    mOldfan[i] = mFan[i];
                }
                if (!mLight[i].Equals(mOldlight[i]))
                {
                    mColorPickers[i].UpdateColorPicker(mLight[i]);
                    mOldlight[i] = mLight[i];
                }
            }
        }
    }
    private void OnApplicationQuit()
    {
        mKeepAlive = false;
        if (mCOMX.IsOpen)
            mCOMX.Close();
        using (NamedPipeClientStream ciliaClient =
            new NamedPipeClientStream(".", "ciliapipe", PipeDirection.Out))
        {
            ciliaClient.Connect();
            try
            {
                using (StreamWriter streamWriter = new StreamWriter(ciliaClient))
                {
                    streamWriter.AutoFlush = true;
                    streamWriter.WriteLine("COM" + mComInt + ",Detach");
                    streamWriter.Close();
                }
            }
            catch (IOException e)
            {
                Debug.LogWarning(e);
            }
        }
    }

    private void Init()
    {
        mCOMX = new SerialPort();
        for (int i = 0; i < 6; i++)
        {
            mFans[i] = "000";
            mNeopixels[i] = "000,000,000";
        }
    }

    private void SetUpConnection()
    {
        string[] comportNames = SerialPort.GetPortNames();

        mCOMX.BaudRate = 19200;
        mCOMX.ReadTimeout = 500;
        mCOMX.WriteTimeout = 500;

        for (int i = 0; i < comportNames.Length; i++)
        {
            try
            {
                mCOMX.PortName = comportNames[i];
                mCOMX.Open();
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                continue;
            }
            break;
        }
        if (!mCOMX.IsOpen)
        {
            Debug.LogError("No free connection for VirtualCilia");
            return;
        }

        mComInt = getPortNumber(mCOMX.PortName);
        if (mComInt == int.MinValue)
        {
            Debug.LogError("Parse failed for: " + mCOMX.PortName);
            return;
        }

        mSuccess = true;
    }

    private int getPortNumber(string portName)
    {
        for (int i = 0; i < portName.Length; i++)
        {
            if (char.IsDigit(portName[i]))
            {
                return int.Parse(portName.Substring(i));
            }
        }
        return int.MinValue;
    }

    private void StartReadThread()
    {
        Thread comThread = new Thread(DoReadCom);
        comThread.Start();
    }

    public void ChangeGroup(byte aCiliaNumber, byte aGroupNumber)
    {
        throw new NotImplementedException();
        /*
        try
        {
            CiliaModified.sendMessageToCilia("[!#SetGroup|" + aCiliaNumber + "|" + aGroupNumber + "]");
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        */
    }

    private void DoReadCom()
    {
        while (mKeepAlive)
        {
            if (mSuccess)
            {
                try
                {
                    char c = (char)mCOMX.ReadChar();
                    //Debug.Log(c);
                    switch (c)
                    {
                        case 'C':
                            mCOMX.Write("CILIA\n");
                            break;
                        case 'F':
                            mCOMX.Read(mBuffer, 0, 4);
                            mFan[mBuffer[0] - 49] = "" + (char)mBuffer[1] + (char)mBuffer[2] + (char)mBuffer[3];
                            break;
                        case 'N':
                            mCOMX.Read(mBuffer, 0, 10);
                            try
                            {
                                mLight[mBuffer[0] - 49] = "" + (char)mBuffer[1] + (char)mBuffer[2] + (char)mBuffer[3] + (char)mBuffer[4] + (char)mBuffer[5] + (char)mBuffer[6] + (char)mBuffer[7] + (char)mBuffer[8] + (char)mBuffer[9];
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning(e);
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (TimeoutException te)
                {
                    //Debug.Log(te);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }
        }
    }

    private void Confirm()
    {
        using (NamedPipeClientStream ciliaClient =
            new NamedPipeClientStream(".", "ciliapipe", PipeDirection.Out))
        {
            ciliaClient.Connect();
            try
            {
                using (StreamWriter streamWriter = new StreamWriter(ciliaClient))
                {
                    streamWriter.AutoFlush = true;
                    streamWriter.WriteLine("COM" + mComInt + ",Attach");
                    streamWriter.Close();
                }
            }
            catch (IOException e)
            {
                Debug.LogWarning(e);
            }
        }
    }
}
