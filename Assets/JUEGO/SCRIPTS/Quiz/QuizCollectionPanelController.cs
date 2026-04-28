using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuizCollectionPanelController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private QuizCollectionCatalog catalogoGlobal;

    [Header("UI")]
    [SerializeField] private TMP_InputField filtroInputField;
    [SerializeField] private Transform tarjetasContainer;
    [SerializeField] private GameObject tarjetaPrefab;

    [Header("Layout")]
    [SerializeField] private float spacingVertical = 14f;
    [SerializeField] private int columnasGrid = 1;
    [SerializeField] private bool usarGridSiHayMasDeUnaColumna = false;

    private readonly List<GameObject> tarjetasInstanciadas = new List<GameObject>();

    private void Awake()
    {
        EnsureContainerLayout();

        if (filtroInputField != null)
        {
            filtroInputField.onValueChanged.AddListener(OnFiltroChanged);
        }
    }

    private void Start()
    {
        BuildCards(string.Empty);
    }

    private void OnDestroy()
    {
        if (filtroInputField != null)
        {
            filtroInputField.onValueChanged.RemoveListener(OnFiltroChanged);
        }
    }

    private void OnFiltroChanged(string filtro)
    {
        BuildCards(filtro);
    }

    private void BuildCards(string filtroRaw)
    {
        ClearCards();

        if (!HasValidConfiguration())
        {
            return;
        }

        string filtro = string.IsNullOrWhiteSpace(filtroRaw) ? string.Empty : filtroRaw.Trim().ToLowerInvariant();

        for (int i = 0; i < catalogoGlobal.Quizzes.Count; i++)
        {
            QuizCollectionItemData item = catalogoGlobal.Quizzes[i];
            if (item == null)
            {
                continue;
            }

            if (!MatchFilter(item, filtro))
            {
                continue;
            }

            GameObject go = Instantiate(tarjetaPrefab, tarjetasContainer);
            QuizCollectionCardView cardView = go.GetComponent<QuizCollectionCardView>();
            if (cardView == null)
            {
                cardView = go.AddComponent<QuizCollectionCardView>();
            }

            cardView.Setup(item, HandlePlayQuiz);
            tarjetasInstanciadas.Add(go);
        }
    }

    private void HandlePlayQuiz(QuizCollectionItemData quizData)
    {
        if (quizData == null || string.IsNullOrWhiteSpace(quizData.nombreEscena))
        {
            Debug.LogWarning("No hay escena configurada para esta tarjeta.");
            return;
        }

        SceneManager.LoadScene(quizData.nombreEscena);
    }

    private static bool MatchFilter(QuizCollectionItemData item, string filtro)
    {
        if (string.IsNullOrEmpty(filtro))
        {
            return true;
        }

        string titulo = item.titulo ?? string.Empty;
        string descripcion = item.descripcion ?? string.Empty;
        string quizId = item.quizId ?? string.Empty;

        return titulo.ToLowerInvariant().Contains(filtro)
            || descripcion.ToLowerInvariant().Contains(filtro)
            || quizId.ToLowerInvariant().Contains(filtro);
    }

    private void ClearCards()
    {
        for (int i = 0; i < tarjetasInstanciadas.Count; i++)
        {
            if (tarjetasInstanciadas[i] != null)
            {
                Destroy(tarjetasInstanciadas[i]);
            }
        }

        tarjetasInstanciadas.Clear();
    }

    private void EnsureContainerLayout()
    {
        if (tarjetasContainer == null)
        {
            WarnEs("Falta 'Tarjetas Container' en PanelColeccion.");
            return;
        }

        if (usarGridSiHayMasDeUnaColumna && columnasGrid > 1)
        {
            VerticalLayoutGroup vertical = tarjetasContainer.GetComponent<VerticalLayoutGroup>();
            if (vertical != null)
            {
                Destroy(vertical);
            }

            GridLayoutGroup grid = tarjetasContainer.GetComponent<GridLayoutGroup>();
            if (grid == null)
            {
                grid = tarjetasContainer.gameObject.AddComponent<GridLayoutGroup>();
            }

            RectTransform prefabRect = tarjetaPrefab != null ? tarjetaPrefab.GetComponent<RectTransform>() : null;
            Vector2 prefabSize = prefabRect != null ? prefabRect.sizeDelta : new Vector2(400f, 140f);

            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columnasGrid;
            grid.cellSize = prefabSize;
            grid.spacing = new Vector2(spacingVertical, spacingVertical);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            return;
        }

        GridLayoutGroup oldGrid = tarjetasContainer.GetComponent<GridLayoutGroup>();
        if (oldGrid != null)
        {
            Destroy(oldGrid);
        }

        VerticalLayoutGroup layout = tarjetasContainer.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = tarjetasContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        }

        layout.spacing = spacingVertical;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.childAlignment = TextAnchor.UpperCenter;
    }

    private bool HasValidConfiguration()
    {
        bool ok = true;

        if (catalogoGlobal == null || catalogoGlobal.Quizzes == null)
        {
            WarnEs("Falta 'Catalogo Global'. Crea y asigna QuizCollectionCatalog.");
            ok = false;
        }

        if (tarjetaPrefab == null)
        {
            WarnEs("Falta 'Tarjeta Prefab' en PanelColeccion.");
            ok = false;
        }

        if (tarjetasContainer == null)
        {
            WarnEs("Falta 'Tarjetas Container' en PanelColeccion.");
            ok = false;
        }

        if (filtroInputField == null)
        {
            WarnEs("Falta 'Filtro Input Field'. El panel funcionara, pero sin filtro.");
        }

        return ok;
    }

    private void WarnEs(string mensaje)
    {
        Debug.LogWarning("[PanelColeccion] " + mensaje, this);
    }
}
