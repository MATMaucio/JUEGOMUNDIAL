using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizCerebroController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private QuizQuestionsController questionsController;
    [SerializeField] private QuizQuestionSet questionSet;
    [SerializeField] private bool shuffleQuestions = false;
    [SerializeField] private bool shuffleAnswers = true;

    [Header("UI Main")]
    [SerializeField] private TMP_Text temaText;
    [SerializeField] private TMP_Text streakText;
    [SerializeField] private TMP_Text intentosText;
    [SerializeField] private TMP_Text preguntaText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_Text quizCompletadoText;
    [SerializeField] private Button siguienteButton;

    [Header("Final Texts")]
    [SerializeField] private string textoQuizCompletado = "Quiz completado";
    [SerializeField] private string textoSinIntentos = "Sin intentos";

    [Header("Flow UI")]
    [SerializeField] private GameObject[] questionUiToHideOnAnswer;
    [SerializeField] private GameObject[] resultUiToShowOnAnswer;
    [SerializeField] private GameObject[] uiToHideOnQuizComplete;

    [Header("Answers")]
    [SerializeField] private Transform respuestasContainer;
    [SerializeField] private GameObject respuestaPrefab;
    [SerializeField] private QuizAnswersContainerLayout answersLayout;

    [Header("Progress")]
    [SerializeField] private QuizProgressDotsText progressDotsText;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMP_Text progressLabelText;

    [Header("Rules")]
    [SerializeField] private int totalAttempts = 3;

    [Header("Save")]
    [SerializeField] private QuizPlayerProgressStore playerProgressStore;
    [ReadOnlyInInspector]
    [SerializeField] private string quizId = "";

    private readonly List<QuizAnswerView> spawnedAnswers = new List<QuizAnswerView>();
    private List<QuizQuestionData> runtimeQuestions = new List<QuizQuestionData>();

    private int currentQuestionIndex;
    private int streak;
    private int points;
    private int attemptsLeft;
    private bool isResolving;
    private int respuestasContestadasSesion;
    private int respuestasCorrectasSesion;
    private int mejorStreakSesion;
    private bool resultadoSesionGuardado;
    private bool warningConfiguracionMostrado;
    private bool warningPreguntaTextMostrado;

    private void Start()
    {
        AutoSetupReferences();
        EnsureQuizId();
        ValidateAndWarnConfiguration();

        if (siguienteButton != null)
        {
            siguienteButton.onClick.AddListener(OnSiguientePressed);
        }

        BuildRuntimeQuestions();
        StartQuiz();
    }

    private void OnDestroy()
    {
        if (siguienteButton != null)
        {
            siguienteButton.onClick.RemoveListener(OnSiguientePressed);
        }
    }

    private void Reset()
    {
        EnsureQuizId();
    }

    private void OnValidate()
    {
        EnsureQuizId();
    }

    private void StartQuiz()
    {
        currentQuestionIndex = 0;
        streak = 0;
        points = 0;
        attemptsLeft = totalAttempts;
        isResolving = false;
        respuestasContestadasSesion = 0;
        respuestasCorrectasSesion = 0;
        mejorStreakSesion = 0;
        resultadoSesionGuardado = false;
        SetQuizCompletadoVisible(false);
        ToggleArray(uiToHideOnQuizComplete, true);

        if (runtimeQuestions.Count == 0)
        {
            SetPreguntaTextSafe("No hay preguntas cargadas.");
            ClearAnswers();
            UpdateTopTexts();
            UpdateProgress();
            SetResultMode("No hay preguntas cargadas.");
            return;
        }

        ShowQuestion(currentQuestionIndex);
    }

    private void BuildRuntimeQuestions()
    {
        runtimeQuestions.Clear();

        IReadOnlyList<QuizQuestionData> sourceQuestions = null;
        if (questionsController != null && questionsController.Questions != null && questionsController.Questions.Count > 0)
        {
            sourceQuestions = questionsController.Questions;
        }
        else if (questionSet != null && questionSet.questions != null)
        {
            sourceQuestions = questionSet.questions;
        }

        if (sourceQuestions == null)
        {
            return;
        }

        for (int i = 0; i < sourceQuestions.Count; i++)
        {
            QuizQuestionData source = sourceQuestions[i];
            if (source == null || source.respuestas == null || source.respuestas.Count == 0)
            {
                continue;
            }

            QuizQuestionData copy = new QuizQuestionData();
            copy.tema = source.tema;
            copy.titulo = source.titulo;
            copy.pregunta = source.pregunta;
            copy.puntos = source.puntos;
            copy.feedbackCorrecta = source.feedbackCorrecta;
            copy.feedbackIncorrecta = source.feedbackIncorrecta;
            copy.respuestas = new List<QuizAnswerData>();

            for (int j = 0; j < source.respuestas.Count; j++)
            {
                QuizAnswerData answerSource = source.respuestas[j];
                if (answerSource == null)
                {
                    continue;
                }

                copy.respuestas.Add(new QuizAnswerData
                {
                    texto = answerSource.texto,
                    esCorrecta = answerSource.esCorrecta
                });
            }

            if (copy.respuestas.Count > 0)
            {
                if (shuffleAnswers)
                {
                    Shuffle(copy.respuestas);
                }

                runtimeQuestions.Add(copy);
            }
        }

        if (shuffleQuestions)
        {
            Shuffle(runtimeQuestions);
        }
    }

    private void ShowQuestion(int index)
    {
        if (index < 0 || index >= runtimeQuestions.Count)
        {
            WarnEs("Indice de pregunta fuera de rango. Revisa la lista de preguntas cargadas.");
            return;
        }

        SetQuestionMode();
        SetQuizCompletadoVisible(false);

        QuizQuestionData question = runtimeQuestions[index];

        if (temaText != null)
        {
            temaText.text = string.IsNullOrWhiteSpace(question.tema) ? "Tema" : question.tema;
        }

        if (preguntaText != null)
        {
            preguntaText.text = BuildQuestionText(question);
        }

        if (feedbackText != null)
        {
            feedbackText.text = string.Empty;
        }

        RenderAnswers(question.respuestas);
        UpdateTopTexts();
        UpdateProgress();
    }

    private void RenderAnswers(List<QuizAnswerData> answers)
    {
        ClearAnswers();

        if (answers == null || answers.Count == 0)
        {
            WarnEs("La pregunta actual no tiene respuestas. Agrega al menos una respuesta en QuizQuestionsController.");
            return;
        }

        if (respuestasContainer == null)
        {
            WarnEs("Falta 'Respuestas Container' en QuizCerebroController. Asigna el contenedor de respuestas.");
            return;
        }

        if (respuestaPrefab == null)
        {
            WarnEs("Falta 'Respuesta Prefab' en QuizCerebroController. Asigna el prefab de la tarjeta/boton de respuesta.");
            return;
        }

        if (answersLayout != null)
        {
            answersLayout.ConfigureForAnswerCount(answers.Count);
        }

        for (int i = 0; i < answers.Count; i++)
        {
            QuizAnswerData answer = answers[i];
            GameObject answerGO = Instantiate(respuestaPrefab, respuestasContainer);
            QuizAnswerView view = answerGO.GetComponent<QuizAnswerView>();
            if (view == null)
            {
                view = answerGO.AddComponent<QuizAnswerView>();
            }

            view.Setup(answer.texto, answer.esCorrecta, i, OnAnswerSelected);
            spawnedAnswers.Add(view);
        }
    }

    private void OnAnswerSelected(QuizAnswerView selectedAnswer)
    {
        if (selectedAnswer == null)
        {
            WarnEs("Se recibio una respuesta vacia. Revisa el prefab de respuesta.");
            return;
        }

        if (isResolving)
        {
            return;
        }

        if (currentQuestionIndex < 0 || currentQuestionIndex >= runtimeQuestions.Count)
        {
            WarnEs("No hay pregunta activa valida para resolver. Revisa el flujo del quiz.");
            return;
        }

        isResolving = true;

        bool isCorrect = selectedAnswer.IsCorrect;
        respuestasContestadasSesion++;
        if (isCorrect)
        {
            streak++;
            points += runtimeQuestions[currentQuestionIndex].puntos;
            respuestasCorrectasSesion++;
            mejorStreakSesion = Mathf.Max(mejorStreakSesion, streak);
        }
        else
        {
            streak = 0;
            attemptsLeft--;
        }

        for (int i = 0; i < spawnedAnswers.Count; i++)
        {
            bool wasSelected = spawnedAnswers[i] == selectedAnswer;
            spawnedAnswers[i].SetInteractable(false);
            spawnedAnswers[i].ShowResultState(wasSelected);
        }

        UpdateTopTexts();
        UpdateProgress(currentQuestionIndex + 1);

        QuizQuestionData question = runtimeQuestions[currentQuestionIndex];
        string feedback = ResolveFeedback(question, isCorrect);
        SetResultMode(feedback);
    }

    private void OnSiguientePressed()
    {
        if (!isResolving)
        {
            return;
        }

        if (attemptsLeft <= 0)
        {
            FinishQuiz("Sin intentos.", false);
            return;
        }

        currentQuestionIndex++;
        if (currentQuestionIndex >= runtimeQuestions.Count)
        {
            FinishQuiz("Quiz terminado.", true);
            return;
        }

        isResolving = false;
        ShowQuestion(currentQuestionIndex);
    }

    private void FinishQuiz(string endLabel, bool quizCompletado)
    {
        ClearAnswers();
        SetResultMode(endLabel + " Puntos: " + points);
        SetQuizCompletadoVisible(true);
        SetQuizCompletadoTexto(quizCompletado ? textoQuizCompletado : textoSinIntentos);
        if (quizCompletado)
        {
            ToggleArray(uiToHideOnQuizComplete, false);
        }

        if (preguntaText != null)
        {
            preguntaText.text = string.Empty;
        }

        UpdateTopTexts();
        UpdateProgress(runtimeQuestions.Count);
        TrySaveSessionResult(quizCompletado);
    }

    private void ClearAnswers()
    {
        for (int i = 0; i < spawnedAnswers.Count; i++)
        {
            if (spawnedAnswers[i] != null)
            {
                Destroy(spawnedAnswers[i].gameObject);
            }
        }

        spawnedAnswers.Clear();
    }

    private void UpdateTopTexts()
    {
        if (streakText != null)
        {
            streakText.text = "streak " + streak;
        }

        if (intentosText != null)
        {
            intentosText.text = "intentos " + attemptsLeft;
        }
    }

    private void UpdateProgress()
    {
        UpdateProgress(currentQuestionIndex);
    }

    private void UpdateProgress(int solvedQuestions)
    {
        if (progressDotsText == null)
        {
            return;
        }

        int total = runtimeQuestions.Count;
        int answered = Mathf.Clamp(solvedQuestions, 0, total);
        progressDotsText.SetProgress(answered, total, points);
    }

    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int swapIndex = UnityEngine.Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[swapIndex];
            list[swapIndex] = temp;
        }
    }

    private void SetQuestionMode()
    {
        ToggleArray(questionUiToHideOnAnswer, true);
        ToggleArray(resultUiToShowOnAnswer, false);

        if (siguienteButton != null)
        {
            siguienteButton.gameObject.SetActive(false);
        }

        if (feedbackText != null)
        {
            feedbackText.text = string.Empty;
        }
    }

    private void SetResultMode(string resultText)
    {
        ToggleArray(questionUiToHideOnAnswer, false);
        ToggleArray(resultUiToShowOnAnswer, true);

        if (siguienteButton != null)
        {
            siguienteButton.gameObject.SetActive(true);
        }

        if (feedbackText != null)
        {
            feedbackText.text = resultText;
        }
        else if (preguntaText != null)
        {
            preguntaText.text = resultText;
        }
    }

    private static void ToggleArray(GameObject[] targets, bool active)
    {
        if (targets == null)
        {
            return;
        }

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] != null)
            {
                targets[i].SetActive(active);
            }
        }
    }

    private void AutoSetupReferences()
    {
        if (respuestasContainer != null && answersLayout == null)
        {
            answersLayout = respuestasContainer.GetComponent<QuizAnswersContainerLayout>();
            if (answersLayout == null)
            {
                answersLayout = respuestasContainer.gameObject.AddComponent<QuizAnswersContainerLayout>();
            }
        }

        if (progressDotsText == null && progressSlider != null)
        {
            progressDotsText = progressSlider.GetComponent<QuizProgressDotsText>();
            if (progressDotsText == null)
            {
                progressDotsText = progressSlider.gameObject.AddComponent<QuizProgressDotsText>();
            }
        }

        if (progressDotsText != null)
        {
            TMP_Text label = progressLabelText;
            if (label == null && progressSlider != null)
            {
                Transform maybeLabel = progressSlider.transform.parent != null
                    ? progressSlider.transform.parent.Find("Progreso TXT")
                    : null;
                if (maybeLabel != null)
                {
                    label = maybeLabel.GetComponent<TMP_Text>();
                }
            }

            progressDotsText.SetTargets(label, progressSlider);
        }

        if (playerProgressStore == null)
        {
            playerProgressStore = FindFirstObjectByType<QuizPlayerProgressStore>();
            if (playerProgressStore == null)
            {
                playerProgressStore = gameObject.AddComponent<QuizPlayerProgressStore>();
            }
        }
    }

    private string ResolveFeedback(QuizQuestionData question, bool isCorrect)
    {
        if (questionsController != null)
        {
            return questionsController.ResolveFeedback(question, isCorrect);
        }

        string custom = isCorrect ? question.feedbackCorrecta : question.feedbackIncorrecta;
        if (!string.IsNullOrWhiteSpace(custom))
        {
            return custom;
        }

        return isCorrect ? "Correcta" : "Incorrecta";
    }

    private static string BuildQuestionText(QuizQuestionData question)
    {
        string safeTitle = question.titulo ?? string.Empty;
        string safeQuestion = question.pregunta ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(safeTitle) && !string.IsNullOrWhiteSpace(safeQuestion))
        {
            return safeTitle + "\n" + safeQuestion;
        }

        if (!string.IsNullOrWhiteSpace(safeQuestion))
        {
            return safeQuestion;
        }

        if (!string.IsNullOrWhiteSpace(safeTitle))
        {
            return safeTitle;
        }

        return "Pregunta";
    }

    private void SetQuizCompletadoVisible(bool visible)
    {
        if (quizCompletadoText != null)
        {
            quizCompletadoText.gameObject.SetActive(visible);
        }
    }

    private void SetQuizCompletadoTexto(string texto)
    {
        if (quizCompletadoText != null)
        {
            quizCompletadoText.text = texto;
        }
    }

    private void TrySaveSessionResult(bool quizCompletado)
    {
        if (resultadoSesionGuardado || playerProgressStore == null)
        {
            return;
        }

        bool sinIntentos = !quizCompletado;
        playerProgressStore.RegisterQuizResult(
            quizId,
            points,
            mejorStreakSesion,
            respuestasContestadasSesion,
            respuestasCorrectasSesion,
            quizCompletado,
            sinIntentos
        );

        resultadoSesionGuardado = true;
    }

    private void EnsureQuizId()
    {
        if (!string.IsNullOrWhiteSpace(quizId) && quizId != "quiz_default")
        {
            return;
        }

        quizId = Guid.NewGuid().ToString("N");
    }

    public string GetQuizId()
    {
        return quizId;
    }

    private void ValidateAndWarnConfiguration()
    {
        if (warningConfiguracionMostrado)
        {
            return;
        }

        if (questionsController == null && questionSet == null)
        {
            WarnEs("No hay fuente de preguntas. Asigna QuizQuestionsController o QuizQuestionSet.");
        }

        if (respuestasContainer == null)
        {
            WarnEs("Falta 'Respuestas Container' en QuizCerebroController.");
        }

        if (respuestaPrefab == null)
        {
            WarnEs("Falta 'Respuesta Prefab' en QuizCerebroController.");
        }

        if (siguienteButton == null)
        {
            WarnEs("Falta 'Siguiente Button'. El quiz no podra avanzar manualmente.");
        }

        if (progressSlider == null)
        {
            WarnEs("Falta 'Progress Slider'. El progreso visual no se mostrara.");
        }

        warningConfiguracionMostrado = true;
    }

    private void SetPreguntaTextSafe(string value)
    {
        if (preguntaText != null)
        {
            preguntaText.text = value;
            return;
        }

        if (!warningPreguntaTextMostrado)
        {
            WarnEs("Falta 'Pregunta Text'. Asigna un TMP_Text para mostrar la pregunta.");
            warningPreguntaTextMostrado = true;
        }
    }

    private void WarnEs(string mensaje)
    {
        Debug.LogWarning("[Quiz] " + mensaje, this);
    }
}
