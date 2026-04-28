using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class QuizAnswerView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text answerText;
    [SerializeField] private Button button;
    [SerializeField] private Image background;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color correctColor = new Color(0.55f, 0.95f, 0.55f, 1f);
    [SerializeField] private Color incorrectColor = new Color(0.98f, 0.55f, 0.55f, 1f);

    [Header("Layout")]
    [SerializeField] private float minButtonHeight = 64f;
    [SerializeField] private float preferredButtonHeight = 80f;
    [SerializeField] private float flexibleButtonHeight = 0f;
    [SerializeField] private bool ajustarTextoAlBoton = true;
    [SerializeField] private bool autoSizeTexto = true;
    [SerializeField] private float minTamanoTexto = 18f;
    [SerializeField] private float maxTamanoTexto = 36f;
    [SerializeField] private float paddingVerticalTexto = 18f;
    [SerializeField] private float alturaMaximaBoton = 140f;

    public bool IsCorrect { get; private set; }
    public int AnswerIndex { get; private set; }

    private Action<QuizAnswerView> onClick;
    private LayoutElement layoutElement;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (answerText == null)
        {
            answerText = GetComponentInChildren<TMP_Text>();
        }

        if (background == null)
        {
            background = GetComponent<Image>();
        }

        layoutElement = GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = gameObject.AddComponent<LayoutElement>();
        }

        layoutElement.minHeight = minButtonHeight;
        layoutElement.preferredHeight = preferredButtonHeight;
        layoutElement.flexibleHeight = flexibleButtonHeight;

        button.onClick.AddListener(HandleClick);
        SetVisualStateNormal();
    }

    public void Setup(string text, bool isCorrect, int answerIndex, Action<QuizAnswerView> clickCallback)
    {
        IsCorrect = isCorrect;
        AnswerIndex = answerIndex;
        onClick = clickCallback;

        if (answerText != null)
        {
            if (ajustarTextoAlBoton)
            {
                answerText.textWrappingMode = TextWrappingModes.Normal;
                answerText.overflowMode = TextOverflowModes.Ellipsis;
                answerText.enableAutoSizing = autoSizeTexto;
                answerText.fontSizeMin = minTamanoTexto;
                answerText.fontSizeMax = maxTamanoTexto;
            }

            answerText.text = text;
            answerText.ForceMeshUpdate();

            if (layoutElement != null)
            {
                float preferredTextHeight = answerText.GetPreferredValues(text, answerText.rectTransform.rect.width, 0f).y;
                float preferred = Mathf.Max(preferredButtonHeight, preferredTextHeight + paddingVerticalTexto);
                layoutElement.preferredHeight = Mathf.Clamp(preferred, minButtonHeight, alturaMaximaBoton);
                layoutElement.minHeight = minButtonHeight;
            }
        }

        SetVisualStateNormal();
        SetInteractable(true);
    }

    public void SetInteractable(bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
    }

    public void ShowResultState(bool wasSelected)
    {
        if (background == null)
        {
            return;
        }

        if (IsCorrect)
        {
            background.color = correctColor;
            return;
        }

        background.color = wasSelected ? incorrectColor : normalColor;
    }

    private void HandleClick()
    {
        onClick?.Invoke(this);
    }

    private void SetVisualStateNormal()
    {
        if (background != null)
        {
            background.color = normalColor;
        }
    }
}
