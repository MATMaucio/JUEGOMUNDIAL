using System.Collections.Generic;
using UnityEngine;

public class QuizQuestionsController : MonoBehaviour
{
    [Header("Feedback Por Defecto")]
    [SerializeField] private string feedbackCorrectaPorDefecto = "Correcta";
    [SerializeField] private string feedbackIncorrectaPorDefecto = "Incorrecta";

    [Header("Preguntas")]
    [SerializeField] private List<QuizQuestionData> listaPreguntas = new List<QuizQuestionData>();

    public IReadOnlyList<QuizQuestionData> Questions => listaPreguntas;

    public string ResolveFeedback(QuizQuestionData question, bool isCorrect)
    {
        if (question != null)
        {
            string custom = isCorrect ? question.feedbackCorrecta : question.feedbackIncorrecta;
            if (!string.IsNullOrWhiteSpace(custom))
            {
                return custom;
            }
        }

        return isCorrect ? feedbackCorrectaPorDefecto : feedbackIncorrectaPorDefecto;
    }

    [ContextMenu("Agregar Pregunta Base")]
    public void AddTemplateQuestion()
    {
        QuizQuestionData q = new QuizQuestionData
        {
            tema = "Tema",
            titulo = "Titulo",
            pregunta = "Pregunta",
            puntos = 1,
            feedbackCorrecta = string.Empty,
            feedbackIncorrecta = string.Empty,
            respuestas = new List<QuizAnswerData>
            {
                new QuizAnswerData { texto = "Respuesta 1", esCorrecta = true },
                new QuizAnswerData { texto = "Respuesta 2", esCorrecta = false }
            }
        };

        listaPreguntas.Add(q);
    }

    private void Reset()
    {
        if (listaPreguntas.Count == 0)
        {
            AddTemplateQuestion();
        }
    }
}
