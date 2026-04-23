using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuizCollectionCatalog", menuName = "JUEGO/Quiz/Collection Catalog")]
public class QuizCollectionCatalog : ScriptableObject
{
    [SerializeField] private List<QuizCollectionItemData> quizzes = new List<QuizCollectionItemData>();

    public IReadOnlyList<QuizCollectionItemData> Quizzes => quizzes;
}

[Serializable]
public class QuizCollectionItemData
{
    [Tooltip("ID del quiz. Debe coincidir con el quizId del QuizCerebroController.")]
    public string quizId;

    [Tooltip("Titulo visible en la tarjeta.")]
    public string titulo;

    [TextArea(2, 6)]
    [Tooltip("Descripcion de la tarjeta.")]
    public string descripcion;

    [Tooltip("Nombre de escena a cargar al presionar Play.")]
    public string nombreEscena;
}
