using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "QuizQuestionSet", menuName = "JUEGO/Quiz/Question Set")]
public class QuizQuestionSet : ScriptableObject
{
    public List<QuizQuestionData> questions = new List<QuizQuestionData>();
}

[Serializable]
public class QuizQuestionData
{
    [InspectorName("Tema")]
    [Tooltip("Tema mostrado arriba. Si lo dejas vacio, se usa 'Tema'.")]
    [FormerlySerializedAs("topic")]
    public string tema;

    [InspectorName("Titulo")]
    [Tooltip("Titulo corto opcional. Si existe, se muestra antes de la pregunta.")]
    [FormerlySerializedAs("title")]
    public string titulo;

    [InspectorName("Pregunta")]
    [TextArea(2, 6)]
    [Tooltip("Texto principal de la pregunta.")]
    [FormerlySerializedAs("question")]
    public string pregunta;

    [InspectorName("Respuestas")]
    [Tooltip("Lista flexible: puedes poner 2, 3, 4 o mas respuestas.")]
    [FormerlySerializedAs("answers")]
    public List<QuizAnswerData> respuestas = new List<QuizAnswerData>();

    [InspectorName("Puntos")]
    [Min(0)]
    [FormerlySerializedAs("points")]
    public int puntos = 1;

    [InspectorName("Feedback Correcta")]
    [Tooltip("Opcional. Si lo llenas, reemplaza el feedback default de correcta.")]
    [TextArea(1, 4)]
    [FormerlySerializedAs("correctFeedback")]
    public string feedbackCorrecta;

    [InspectorName("Feedback Incorrecta")]
    [Tooltip("Opcional. Si lo llenas, reemplaza el feedback default de incorrecta.")]
    [TextArea(1, 4)]
    [FormerlySerializedAs("incorrectFeedback")]
    public string feedbackIncorrecta;
}

[Serializable]
public class QuizAnswerData
{
    [InspectorName("Texto Respuesta")]
    [TextArea(1, 4)]
    [FormerlySerializedAs("text")]
    public string texto;

    [InspectorName("Es Correcta")]
    [FormerlySerializedAs("isCorrect")]
    public bool esCorrecta;
}
