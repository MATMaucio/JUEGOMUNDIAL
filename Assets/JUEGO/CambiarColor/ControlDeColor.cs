using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ControlDeControl : MonoBehaviour
{
    [Header("Material a observar")]
    public Material materialObjetivo;

    [Header("UI que reflejan el color")]
    public List<Image> imagenesUI = new List<Image>();

    [Header("Nombre de la propiedad del color")]
    public string nombrePropiedadColor = "_BaseColor"; // URP
    // Para Standard usa "_Color"
    void Start()
    {
        ActualizarColorUI();
    }
    // Función que puedes llamar desde otro script
    public void ActualizarColorUI()
    {
        if (materialObjetivo == null) return;

        Color colorMaterial;

        // Detecta si el material tiene esa propiedad
        if (materialObjetivo.HasProperty(nombrePropiedadColor))
        {
            colorMaterial = materialObjetivo.GetColor(nombrePropiedadColor);
        }
        else
        {
            Debug.LogWarning("El material no tiene la propiedad de color: " + nombrePropiedadColor);
            return;
        }

        // Aplicar color a todas las imágenes
        foreach (Image img in imagenesUI)
        {
            if (img != null)
            {
                img.color = colorMaterial;
            }
        }
    }
}