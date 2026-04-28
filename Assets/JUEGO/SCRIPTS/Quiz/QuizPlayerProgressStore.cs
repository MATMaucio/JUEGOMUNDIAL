using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QuizPlayerProgressData
{
    public int puntosTotales;
    public int quizzesCompletados;
    public int quizzesSinIntentos;
    public int mejorStreak;
    public int preguntasRespondidas;
    public int respuestasCorrectas;
    public string ultimaActualizacionUtc;
    public List<QuizPorIdData> quizPorId = new List<QuizPorIdData>();
}

[Serializable]
public class QuizPorIdData
{
    public string quizId;
    public int vecesJugado;
    public int puntosTotales;
    public int quizzesCompletados;
    public int quizzesSinIntentos;
    public int mejorStreak;
    public int preguntasRespondidas;
    public int respuestasCorrectas;
}

public class QuizPlayerProgressStore : MonoBehaviour
{
    [SerializeField] private string saveKey = "quiz_player_progress_v1";

    private QuizPlayerProgressData cachedData;

    public QuizPlayerProgressData Data
    {
        get
        {
            EnsureLoaded();
            return cachedData;
        }
    }

    public void RegisterQuizResult(string quizId, int puntosGanados, int mejorStreakEnPartida, int respondidas, int correctas, bool quizCompletado, bool sinIntentos)
    {
        EnsureLoaded();

        string safeQuizId = string.IsNullOrWhiteSpace(quizId) ? "quiz_default" : quizId.Trim();
        cachedData.puntosTotales += Mathf.Max(0, puntosGanados);
        cachedData.preguntasRespondidas += Mathf.Max(0, respondidas);
        cachedData.respuestasCorrectas += Mathf.Max(0, correctas);
        cachedData.mejorStreak = Mathf.Max(cachedData.mejorStreak, mejorStreakEnPartida);

        if (quizCompletado)
        {
            cachedData.quizzesCompletados++;
        }

        if (sinIntentos)
        {
            cachedData.quizzesSinIntentos++;
        }

        QuizPorIdData perQuiz = GetOrCreateQuizById(safeQuizId);
        perQuiz.vecesJugado++;
        perQuiz.puntosTotales += Mathf.Max(0, puntosGanados);
        perQuiz.preguntasRespondidas += Mathf.Max(0, respondidas);
        perQuiz.respuestasCorrectas += Mathf.Max(0, correctas);
        perQuiz.mejorStreak = Mathf.Max(perQuiz.mejorStreak, mejorStreakEnPartida);
        if (quizCompletado)
        {
            perQuiz.quizzesCompletados++;
        }
        if (sinIntentos)
        {
            perQuiz.quizzesSinIntentos++;
        }

        cachedData.ultimaActualizacionUtc = DateTime.UtcNow.ToString("o");
        Save();
    }

    public QuizPorIdData GetQuizById(string quizId)
    {
        EnsureLoaded();
        string safeQuizId = string.IsNullOrWhiteSpace(quizId) ? "quiz_default" : quizId.Trim();
        for (int i = 0; i < cachedData.quizPorId.Count; i++)
        {
            if (cachedData.quizPorId[i] != null && cachedData.quizPorId[i].quizId == safeQuizId)
            {
                return cachedData.quizPorId[i];
            }
        }

        return null;
    }

    public void ResetData()
    {
        cachedData = CreateDefaultData();
        Save();
    }

    public void Save()
    {
        EnsureLoaded();
        string json = JsonUtility.ToJson(cachedData);
        PlayerPrefs.SetString(saveKey, json);
        PlayerPrefs.Save();
    }

    public QuizPlayerProgressData Load()
    {
        string json = PlayerPrefs.GetString(saveKey, string.Empty);
        if (string.IsNullOrWhiteSpace(json))
        {
            cachedData = CreateDefaultData();
            return cachedData;
        }

        QuizPlayerProgressData parsed = JsonUtility.FromJson<QuizPlayerProgressData>(json);
        cachedData = parsed ?? CreateDefaultData();
        return cachedData;
    }

    private void Awake()
    {
        EnsureLoaded();
    }

    private void EnsureLoaded()
    {
        if (cachedData == null)
        {
            Load();
        }
    }

    private static QuizPlayerProgressData CreateDefaultData()
    {
        return new QuizPlayerProgressData
        {
            puntosTotales = 0,
            quizzesCompletados = 0,
            quizzesSinIntentos = 0,
            mejorStreak = 0,
            preguntasRespondidas = 0,
            respuestasCorrectas = 0,
            ultimaActualizacionUtc = string.Empty,
            quizPorId = new List<QuizPorIdData>()
        };
    }

    private QuizPorIdData GetOrCreateQuizById(string quizId)
    {
        for (int i = 0; i < cachedData.quizPorId.Count; i++)
        {
            QuizPorIdData entry = cachedData.quizPorId[i];
            if (entry != null && entry.quizId == quizId)
            {
                return entry;
            }
        }

        QuizPorIdData created = new QuizPorIdData
        {
            quizId = quizId,
            vecesJugado = 0,
            puntosTotales = 0,
            quizzesCompletados = 0,
            quizzesSinIntentos = 0,
            mejorStreak = 0,
            preguntasRespondidas = 0,
            respuestasCorrectas = 0
        };
        cachedData.quizPorId.Add(created);
        return created;
    }
}
