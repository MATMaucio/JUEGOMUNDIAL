using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizProgressDotsText : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text progressLabelText;
    [SerializeField] private Slider progressSlider;

    [Header("Animation")]
    [SerializeField] private string baseLabel = "Progreso";
    [SerializeField] private float dotIntervalSeconds = 0.35f;
    [SerializeField] private int maxDots = 3;

    [Header("Progress Color")]
    [SerializeField] private bool colorizeSlider = true;
    [SerializeField] private Color startProgressColor = new Color(0.78f, 0.78f, 0.78f, 1f);
    [SerializeField] private Color midProgressColor = new Color(0.63f, 0.86f, 0.58f, 1f);
    [SerializeField] private Color completeProgressColor = new Color(0.50f, 0.88f, 0.55f, 1f);

    private string progressSuffix = string.Empty;
    private Coroutine dotsRoutine;
    private Graphic sliderGraphic;

    public void SetTargets(TMP_Text label, Slider slider)
    {
        if (progressLabelText == null)
        {
            progressLabelText = label;
        }

        if (progressSlider == null)
        {
            progressSlider = slider;
        }

        CacheSliderGraphic();
    }

    private void OnEnable()
    {
        StartDotsAnimation();
    }

    private void OnDisable()
    {
        StopDotsAnimation();
    }

    public void SetProgress(int answeredQuestions, int totalQuestions, int totalPoints)
    {
        progressSuffix = " " + answeredQuestions + "/" + totalQuestions + " | Puntos: " + totalPoints;

        if (progressSlider != null)
        {
            float progress = totalQuestions > 0 ? answeredQuestions / (float)totalQuestions : 0f;
            progressSlider.SetValueWithoutNotify(Mathf.Clamp01(progress));
        }

        if (colorizeSlider)
        {
            ApplyProgressColor(answeredQuestions, totalQuestions);
        }
    }

    private void StartDotsAnimation()
    {
        if (dotsRoutine != null)
        {
            StopCoroutine(dotsRoutine);
        }

        dotsRoutine = StartCoroutine(DotsLoop());
    }

    private void StopDotsAnimation()
    {
        if (dotsRoutine == null)
        {
            return;
        }

        StopCoroutine(dotsRoutine);
        dotsRoutine = null;
    }

    private IEnumerator DotsLoop()
    {
        int dots = 0;

        while (true)
        {
            dots = (dots + 1) % (maxDots + 1);
            if (progressLabelText != null)
            {
                progressLabelText.text = baseLabel + new string('.', dots) + progressSuffix;
            }

            yield return new WaitForSeconds(dotIntervalSeconds);
        }
    }

    private void CacheSliderGraphic()
    {
        if (progressSlider == null)
        {
            return;
        }

        if (progressSlider.fillRect != null)
        {
            sliderGraphic = progressSlider.fillRect.GetComponent<Graphic>();
        }

        if (sliderGraphic == null)
        {
            sliderGraphic = progressSlider.targetGraphic;
        }

        if (sliderGraphic == null)
        {
            sliderGraphic = progressSlider.GetComponent<Graphic>();
        }
    }

    private void ApplyProgressColor(int answeredQuestions, int totalQuestions)
    {
        if (sliderGraphic == null)
        {
            CacheSliderGraphic();
        }

        if (sliderGraphic == null)
        {
            return;
        }

        if (totalQuestions <= 0)
        {
            sliderGraphic.color = startProgressColor;
            return;
        }

        float progress01 = Mathf.Clamp01(answeredQuestions / (float)totalQuestions);

        if (progress01 >= 1f)
        {
            sliderGraphic.color = completeProgressColor;
            return;
        }

        if (progress01 <= 0.5f)
        {
            float t = progress01 / 0.5f;
            sliderGraphic.color = Color.Lerp(startProgressColor, midProgressColor, t);
            return;
        }

        float t2 = (progress01 - 0.5f) / 0.5f;
        sliderGraphic.color = Color.Lerp(midProgressColor, completeProgressColor, t2);
    }
}
