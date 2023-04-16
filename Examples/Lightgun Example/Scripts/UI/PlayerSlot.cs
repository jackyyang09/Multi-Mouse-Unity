using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSlot : MonoBehaviour
{
    [SerializeField] Text numberLabel;
    [SerializeField] Image darkMask;

    public void UpdateStateForDevice(int i)
    {
        numberLabel.enabled = i != -1;
        numberLabel.text = "P" + (i + 1);
        numberLabel.color = i == 0 ? Color.red : Color.blue;
        darkMask.enabled = i == -1;
    }
}