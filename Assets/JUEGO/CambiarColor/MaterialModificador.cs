using UnityEngine;
using UnityEngine.UI;


public class MaterialModificador : MonoBehaviour
{
    [Header("Sliders")]
    public Slider sliderR;
    public Slider sliderG;
    public Slider sliderB;

    [Header("Preview")]
    public Image imagenPreview;

    [Header("Material objetivo")]
    public Material materialObjetivo;
    public string nombrePropiedadColor = "_BaseColor";

    [Header("Referencia UI Sync")]
    public ControlDeControl controlDeControl;

    private Color colorActual;

    void Start()
    {
        if (materialObjetivo.HasProperty(nombrePropiedadColor))
        {
            colorActual = materialObjetivo.GetColor(nombrePropiedadColor);

            sliderR.value = colorActual.r;
            sliderG.value = colorActual.g;
            sliderB.value = colorActual.b;

            ActualizarPreview();
        }

        sliderR.onValueChanged.AddListener(delegate { OnSliderChange(); });
        sliderG.onValueChanged.AddListener(delegate { OnSliderChange(); });
        sliderB.onValueChanged.AddListener(delegate { OnSliderChange(); });
    }

    void OnSliderChange()
    {
        colorActual = new Color(sliderR.value, sliderG.value, sliderB.value);
        ActualizarPreview();
    }

    void ActualizarPreview()
    {
        if (imagenPreview != null)
        {
            imagenPreview.color = colorActual;
        }
    }

    public void AplicarColor()
    {
        if (materialObjetivo.HasProperty(nombrePropiedadColor))
        {
            materialObjetivo.SetColor(nombrePropiedadColor, colorActual);
        }
        controlDeControl.ActualizarColorUI();

        if (controlDeControl != null)
        {
            controlDeControl.ActualizarColorUI();
        }
    }
}
