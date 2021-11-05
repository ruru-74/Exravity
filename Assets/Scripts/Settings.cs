using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public Text x;
    public Text y;
    public Slider sX;
    public Slider sY;
    public PlayerController pc;
    public void ChangeX(Slider s)
    {
        PlayerPrefs.SetFloat("MouseX", s.value);
        PlayerPrefs.Save();
        pc.mouseXSpeed = s.value;
        x.text = "X:" + s.value.ToString("F1");
    }
    public void ChangeY(Slider s)
    {
        PlayerPrefs.SetFloat("MouseY", s.value);
        PlayerPrefs.Save();
        pc.mouseYSpeed = s.value;
        y.text = "Y:" + s.value.ToString("F1");
    }
    public void Set()
    {
        float x = PlayerPrefs.GetFloat("MouseX");
        float y = PlayerPrefs.GetFloat("MouseY");
        this.x.text = x.ToString("F1");
        this.y.text = y.ToString("F1");
        sX.value = x;
        sY.value = y;
    }
}