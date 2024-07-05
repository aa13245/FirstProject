using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransitionalFader : MonoBehaviour
{
    [Tooltip("A black image that covers the entire screen")]
    public Image panel;
    //Is the fader active
    private bool isActive = false;
    //Current fade time
    private float currentTime = 0;
    //Total time it takes to fade to black
    private float fadeIn = 0;
    //Total time it stays faded
    private float stayFaded = 0;
    //Total time it takes to fade out
    private float fadeOut = 0;

    // Update is called once per frame
    void Update()
    {
        //Advances the current time and fades the screen based on the fade variables
        if (!isActive) return;
        if (currentTime <= fadeIn)
        {
            float relativeTime = currentTime / fadeIn;
            panel.color = new Color(panel.color.r, panel.color.g, panel.color.b, Mathf.Lerp(0, 1, relativeTime));
        }
        if (currentTime > fadeIn && currentTime <= fadeIn+stayFaded)
        {
            panel.color = new Color(panel.color.r, panel.color.g, panel.color.b, 1);
        }
        if (currentTime > fadeIn+stayFaded)
        {
            float relativeTime = currentTime-fadeIn-stayFaded / fadeOut;
            panel.color = new Color(panel.color.r, panel.color.g, panel.color.b, Mathf.Lerp(1, 0, relativeTime));
        }
        if (currentTime > fadeIn + stayFaded + fadeOut)
        {
            panel.color = new Color(panel.color.r, panel.color.g, panel.color.b, 0);
            isActive = false;
        }
        currentTime += Time.deltaTime;
    }

    //Fades the screen based on a single time value split between fading in, out, and stay
    public void FadeScreenUniform(float secondsTotal)
    {
        if (isActive) return;
        fadeIn = secondsTotal/3.33f;
        stayFaded = secondsTotal/3.33f;
        fadeOut = secondsTotal / 3.33f;
        currentTime = 0;
        isActive = true;
    }

    //Fades the screen based on specific value for fade in, out, and stay
    public void FadeScreen (float secondsIn, float secondsStay, float secondsOut)
    {
        if (isActive) return;
        fadeIn = secondsIn;
        stayFaded = secondsStay;
        fadeOut = secondsOut;
        currentTime = 0;
        isActive = true;
    }
}
