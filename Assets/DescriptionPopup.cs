using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class DescriptionPopup : MonoBehaviour
{

    public GameObject descriptionPanel;
    public TextMeshProUGUI descriptionText;

    // Start is called before the first frame update
    void Start()
    {
        descriptionPanel.SetActive(false);
    }

    public void OnPointerEnter()
    {
        Debug.Log("enter");
        // Obtenir la position de la souris par rapport à l'écran
        Vector3 mousePosition = Input.mousePosition;

        // Calculez la position du panneau (par exemple, décalage à gauche de la souris)
        Vector3 panelPosition = mousePosition - new Vector3(100f, 0f, 0f); // Ajustez la position selon vos besoins.

        // Vérifiez les limites de l'écran
        panelPosition.x = Mathf.Clamp(panelPosition.x, 0f, Screen.width - descriptionPanel.GetComponent<RectTransform>().rect.width);
        panelPosition.y = Mathf.Clamp(panelPosition.y, 0f, Screen.height - descriptionPanel.GetComponent<RectTransform>().rect.height);

        // Affectez la position au panneau de description
        descriptionPanel.transform.position = panelPosition;

        descriptionPanel.SetActive(true);
    }

    public void OnPointerExit()
    {
        // Cacher le panneau de description lorsque la souris quitte l'objet
        descriptionPanel.SetActive(false);
    }
}
