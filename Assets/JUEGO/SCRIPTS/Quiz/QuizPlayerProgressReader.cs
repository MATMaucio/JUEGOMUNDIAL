using UnityEngine;

public class QuizPlayerProgressReader : MonoBehaviour
{
    [SerializeField] private QuizPlayerProgressStore progressStore;

    private void Awake()
    {
        if (progressStore == null)
        {
            progressStore = FindFirstObjectByType<QuizPlayerProgressStore>();
        }
    }

    public QuizPlayerProgressData GetProgress()
    {
        if (progressStore == null)
        {
            return new QuizPlayerProgressData();
        }

        return progressStore.Data;
    }

    public int GetPuntosTotales()
    {
        return GetProgress().puntosTotales;
    }

    public int GetQuizzesCompletados()
    {
        return GetProgress().quizzesCompletados;
    }

    public int GetQuizzesSinIntentos()
    {
        return GetProgress().quizzesSinIntentos;
    }

    public int GetMejorStreak()
    {
        return GetProgress().mejorStreak;
    }

    public int GetPreguntasRespondidas()
    {
        return GetProgress().preguntasRespondidas;
    }

    public int GetRespuestasCorrectas()
    {
        return GetProgress().respuestasCorrectas;
    }

    public float GetPrecision()
    {
        QuizPlayerProgressData progress = GetProgress();
        if (progress.preguntasRespondidas <= 0)
        {
            return 0f;
        }

        return progress.respuestasCorrectas / (float)progress.preguntasRespondidas;
    }

    public QuizPorIdData GetQuizPorId(string quizId)
    {
        if (progressStore == null)
        {
            return null;
        }

        return progressStore.GetQuizById(quizId);
    }
}
