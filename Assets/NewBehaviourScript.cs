﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnIndexChanged(int index, RectTransform transform)
    {
        transform.gameObject.name = index.ToString();
        transform.GetComponentInChildren<Text>().text = index.ToString();
    }
}
