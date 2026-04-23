using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizCollectionCardView : MonoBehaviour
{
    [SerializeField] private TMP_Text tituloText;
    [SerializeField] private TMP_Text descripcionText;
    [SerializeField] private Button jugarButton;

    private QuizCollectionItemData data;
    private Action<QuizCollectionItemData> onPlay;

    private void Awake()
    {
        AutoFindReferences();
        ConfigureTextStyles();

        if (jugarButton != null)
        {
            jugarButton.onClick.RemoveListener(HandlePlay);
            jugarButton.onClick.AddListener(HandlePlay);
        }
    }

    public void Setup(QuizCollectionItemData itemData, Action<QuizCollectionItemData> playCallback)
    {
        data = itemData;
        onPlay = playCallback;

        if (tituloText != null)
        {
            tituloText.text = string.IsNullOrWhiteSpace(itemData.titulo) ? "Quiz" : itemData.titulo;
        }

        if (descripcionText != null)
        {
            descripcionText.text = itemData.descripcion ?? string.Empty;
        }
    }

    private void HandlePlay()
    {
        onPlay?.Invoke(data);
    }

    private void AutoFindReferences()
    {
        if (tituloText == null)
        {
            Transform t = transform.Find("Titulo TXT");
            if (t != null)
            {
                tituloText = t.GetComponent<TMP_Text>();
            }
        }

        if (descripcionText == null)
        {
            Transform d = transform.Find("Descrip TXT");
            if (d != null)
            {
                descripcionText = d.GetComponent<TMP_Text>();
            }
        }

        if (jugarButton == null)
        {
            Transform b = transform.Find("Jugar BTN");
            if (b != null)
            {
                jugarButton = b.GetComponent<Button>();
            }
        }
    }

    private void ConfigureTextStyles()
    {
        if (tituloText != null)
        {
            tituloText.enableAutoSizing = true;
            tituloText.fontSizeMin = 20;
            tituloText.fontSizeMax = 42;
            tituloText.textWrappingMode = TextWrappingModes.Normal;
            tituloText.overflowMode = TextOverflowModes.Ellipsis;
        }

        if (descripcionText != null)
        {
            descripcionText.enableAutoSizing = true;
            descripcionText.fontSizeMin = 14;
            descripcionText.fontSizeMax = 30;
            descripcionText.textWrappingMode = TextWrappingModes.Normal;
            descripcionText.overflowMode = TextOverflowModes.Ellipsis;
        }
    }
}
