using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControleur : MonoBehaviour
{
    public Camera mainCamera;

    [Header("Camera Settings")]
    public float minOrthographicSize = 5f; // Taille orthographique minimale
    public float maxOrthographicSize = 10f; // Taille orthographique maximale

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Vérifiez si la résolution de l'écran a changé.
        if (Screen.width != mainCamera.pixelWidth || Screen.height != mainCamera.pixelHeight)
        {
            ResizeCamera();
        }
    }

    private void ResizeCamera()
    {
        // Calculez la taille orthographique en fonction de la résolution de l'écran.
        float targetOrthographicSize = Mathf.Clamp(
            Screen.height / 2f,
            minOrthographicSize,
            maxOrthographicSize
        );

        // Appliquez la taille orthographique calculée à la caméra.
        mainCamera.orthographicSize = targetOrthographicSize;
    }
}
