using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class QuizAnswersContainerLayout : MonoBehaviour
{
    public enum LayoutMode
    {
        Auto,
        Vertical,
        Horizontal,
        Grid
    }

    [Header("Layout")]
    [SerializeField] private LayoutMode layoutMode = LayoutMode.Auto;
    [SerializeField] private int autoHorizontalThreshold = 2;
    [SerializeField] private int autoGridThreshold = 6;
    [SerializeField] private float spacing = 12f;
    [SerializeField] private int paddingLeft = 12;
    [SerializeField] private int paddingRight = 12;
    [SerializeField] private int paddingTop = 12;
    [SerializeField] private int paddingBottom = 12;

    [Header("Child Size")]
    [SerializeField] private bool controlChildWidth = true;
    [SerializeField] private bool controlChildHeight = true;
    [SerializeField] private bool forceExpandWidth = true;
    [SerializeField] private bool forceExpandHeight = false;

    [Header("Safety Size")]
    [SerializeField] private float minContainerWidth = 500f;
    [SerializeField] private float minContainerHeight = 180f;
    [SerializeField] private int gridColumns = 2;
    [SerializeField] private bool gridColumnsByContainerWidth = true;
    [SerializeField] private int minGridColumns = 2;
    [SerializeField] private int maxGridColumns = 4;
    [SerializeField] private float minGridCellWidth = 240f;
    [SerializeField] private float gridCellHeight = 80f;

    private HorizontalOrVerticalLayoutGroup activeLayout;
    private GridLayoutGroup activeGrid;
    private ContentSizeFitter sizeFitter;
    private RectTransform rectTransform;

    public void ConfigureForAnswerCount(int answerCount)
    {
        bool useHorizontal = layoutMode == LayoutMode.Horizontal;
        bool useGrid = layoutMode == LayoutMode.Grid;

        if (layoutMode == LayoutMode.Auto)
        {
            useHorizontal = answerCount > 0 && answerCount <= autoHorizontalThreshold;
            useGrid = answerCount >= autoGridThreshold;
        }

        if (layoutMode == LayoutMode.Vertical)
        {
            useHorizontal = false;
            useGrid = false;
        }

        if (layoutMode == LayoutMode.Horizontal)
        {
            useGrid = false;
        }

        EnsureSupportComponents(useHorizontal, useGrid, answerCount);
        if (useGrid)
        {
            ApplyGridSettings(activeGrid, answerCount);
        }
        else
        {
            ApplyLayoutSettings(activeLayout);
        }
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        EnsureReasonableContainerSize();
        ConfigureForAnswerCount(0);
    }

    private void EnsureSupportComponents(bool useHorizontal, bool useGrid, int answerCount)
    {
        EnsureSingleLayoutComponent(useHorizontal, useGrid, answerCount);

        if (sizeFitter == null)
        {
            sizeFitter = GetComponent<ContentSizeFitter>();
            if (sizeFitter == null)
            {
                sizeFitter = gameObject.AddComponent<ContentSizeFitter>();
            }
        }

        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private void ApplyLayoutSettings(HorizontalOrVerticalLayoutGroup group)
    {
        if (group == null)
        {
            return;
        }

        group.spacing = spacing;
        group.padding = new RectOffset(paddingLeft, paddingRight, paddingTop, paddingBottom);
        group.childControlWidth = controlChildWidth;
        group.childControlHeight = controlChildHeight;
        group.childForceExpandWidth = forceExpandWidth;
        group.childForceExpandHeight = forceExpandHeight;
    }

    private void EnsureSingleLayoutComponent(bool useHorizontal, bool useGrid, int answerCount)
    {
        HorizontalLayoutGroup horizontal = GetComponent<HorizontalLayoutGroup>();
        VerticalLayoutGroup vertical = GetComponent<VerticalLayoutGroup>();
        GridLayoutGroup grid = GetComponent<GridLayoutGroup>();

        if (useGrid)
        {
            if (horizontal != null)
            {
                DestroyImmediate(horizontal);
            }

            if (vertical != null)
            {
                DestroyImmediate(vertical);
            }

            if (grid == null)
            {
                grid = gameObject.AddComponent<GridLayoutGroup>();
            }

            activeLayout = null;
            activeGrid = grid;
            return;
        }

        if (useHorizontal)
        {
            if (vertical != null)
            {
                DestroyImmediate(vertical);
            }
            if (grid != null)
            {
                DestroyImmediate(grid);
            }

            if (horizontal == null)
            {
                horizontal = gameObject.AddComponent<HorizontalLayoutGroup>();
            }

            activeLayout = horizontal;
            activeGrid = null;
            return;
        }

        if (horizontal != null)
        {
            DestroyImmediate(horizontal);
        }
        if (grid != null)
        {
            DestroyImmediate(grid);
        }

        if (vertical == null)
        {
            vertical = gameObject.AddComponent<VerticalLayoutGroup>();
        }

        activeLayout = vertical;
        activeGrid = null;
    }

    private void ApplyGridSettings(GridLayoutGroup grid, int answerCount)
    {
        if (grid == null)
        {
            return;
        }

        int columns = ResolveGridColumns(answerCount);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
        grid.spacing = new Vector2(spacing, spacing);
        grid.padding = new RectOffset(paddingLeft, paddingRight, paddingTop, paddingBottom);
        grid.childAlignment = TextAnchor.UpperLeft;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;

        float totalSpacing = spacing * (columns - 1);
        float totalPadding = paddingLeft + paddingRight;
        float availableWidth = rectTransform.rect.width - totalSpacing - totalPadding;
        if (availableWidth <= 0f)
        {
            availableWidth = minContainerWidth - totalSpacing - totalPadding;
        }

        float cellWidth = Mathf.Max(80f, availableWidth / columns);
        grid.cellSize = new Vector2(cellWidth, gridCellHeight);

        int rows = Mathf.CeilToInt(answerCount / (float)columns);
        float totalHeight = paddingTop + paddingBottom + (rows * gridCellHeight) + (Mathf.Max(0, rows - 1) * spacing);
        if (rectTransform.sizeDelta.y < totalHeight)
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, totalHeight);
        }
    }

    private int ResolveGridColumns(int answerCount)
    {
        int fallbackColumns = Mathf.Max(1, gridColumns);
        if (!gridColumnsByContainerWidth || rectTransform == null)
        {
            return fallbackColumns;
        }

        float availableWidth = rectTransform.rect.width - paddingLeft - paddingRight;
        if (availableWidth <= 0f)
        {
            availableWidth = minContainerWidth - paddingLeft - paddingRight;
        }

        float slotWidth = Mathf.Max(1f, minGridCellWidth + spacing);
        int byWidth = Mathf.FloorToInt((availableWidth + spacing) / slotWidth);
        int clamped = Mathf.Clamp(byWidth, Mathf.Max(1, minGridColumns), Mathf.Max(minGridColumns, maxGridColumns));
        clamped = Mathf.Min(clamped, Mathf.Max(1, answerCount));
        return Mathf.Max(1, clamped);
    }

    private void EnsureReasonableContainerSize()
    {
        if (rectTransform == null)
        {
            return;
        }

        Vector2 size = rectTransform.sizeDelta;

        if (size.x < minContainerWidth)
        {
            size.x = minContainerWidth;
        }

        if (size.y < minContainerHeight)
        {
            size.y = minContainerHeight;
        }

        rectTransform.sizeDelta = size;
    }
}
