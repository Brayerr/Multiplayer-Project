using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderNumberUpdate : MonoBehaviour
{
    [SerializeField] Slider maxPlayerFindServerSlider;

    [SerializeField] TMP_Text textAboveSlider;

    private void FixedUpdate()
    {
        textAboveSlider.text = maxPlayerFindServerSlider.value.ToString();
    }
}
