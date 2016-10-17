﻿using UnityEngine;
using System.Collections;

public class CloseDoor : MonoBehaviour
{
    [SerializeField]
    private Sprite open;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void Close()
    {
        Debug.Log("close door");
        BoxCollider2D[] colliders = GetComponents<BoxCollider2D>(); //remove collider
        foreach (BoxCollider2D box in colliders)
        {
            box.enabled = true;
            Debug.Log(box.ToString());
        }
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        GetComponent<SpriteRenderer>().sprite = open; //switch sprite
        GetComponent<AudioSource>().Play();
    }
}
