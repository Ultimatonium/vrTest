using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CiliaLight : MonoBehaviour
{
    /*Constants*/
    private const int RED_OFFSET = 0;
    private const int GREEN_OFFSET = 3;
    private const int BLUE_OFFSET = 6;
    private const int COLOR_SIZE = 3;
    /*Serilizable*/
    [SerializeField] private Material mEmmissive;
    [SerializeField] private Light mPointLight;

    public void UpdateColorPicker(string aColor)
    {
        //Try catch just in case somehow an improper string finds its way in
        try
        {
            //Gets the rgb values from the string using the known offsets and sizes within the color string.
            uint redValue255 = uint.Parse(aColor.Substring(RED_OFFSET, COLOR_SIZE));
            uint greenValue255 = uint.Parse(aColor.Substring(GREEN_OFFSET, COLOR_SIZE));
            uint blueValue255 = uint.Parse(aColor.Substring(BLUE_OFFSET, COLOR_SIZE));
            //Need to normalize the value for the slider.
            float redFValue = (float)redValue255 / (float)byte.MaxValue;
            float greenFValue = (float)greenValue255 / (float)byte.MaxValue;
            float blueFValue = (float)blueValue255 / (float)byte.MaxValue;
            //make a color from values
            Color tempColor = new Color(redFValue, greenFValue, blueFValue);
            //get an unsigned int version of the value by multiplying our normalized value by 255 and then making it an unsigned int.  will be between 0 and 255
            //set the color of the appropriate part of the model as well as the point light
            mEmmissive.color = tempColor;
            mEmmissive.SetColor("_EmissionColor", tempColor);
            mPointLight.color = tempColor;
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

}
