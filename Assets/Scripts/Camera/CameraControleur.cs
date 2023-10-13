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
        // V�rifiez si la r�solution de l'�cran a chang�.
        if (Screen.width != mainCamera.pixelWidth || Screen.height != mainCamera.pixelHeight)
        {
            ResizeCamera();
        }
    }

    private void ResizeCamera()
    {
        // Calculez la taille orthographique en fonction de la r�solution de l'�cran.
        float targetOrthographicSize = Mathf.Clamp(
            Screen.height / 2f,
            minOrthographicSize,
            maxOrthographicSize
        );

        // Appliquez la taille orthographique calcul�e � la cam�ra.
        mainCamera.orthographicSize = targetOrthographicSize;
    }
}
