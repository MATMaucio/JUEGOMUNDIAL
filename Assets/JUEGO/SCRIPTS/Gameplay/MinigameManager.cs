using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    public enum ShotZone
    {
        Left,
        MidLeft,
        Center,
        MidRight,
        Right
    }

    [Header("References")]
    public RectTransform cursor;
    public RectTransform goalkeeper;
    public RectTransform[] zonePositions; // Asignar 5 zonas en orden

    [Header("Settings")]
    public float cursorSpeed = 800f;
    public float goalkeeperSpeed = 500f;

    private bool cursorMovingRight = true;
    private bool goalkeeperMovingRight = true;

    private const float LEFT_LIMIT = -600f;
    private const float RIGHT_LIMIT = 600f;

    void Update()
    {
        MoveCursor();
        MoveGoalkeeper();

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    // =============================
    // MOVIMIENTO CURSOR
    // =============================
    void MoveCursor()
    {
        float move = cursorSpeed * Time.deltaTime;

        if (cursorMovingRight)
            cursor.anchoredPosition += Vector2.right * move;
        else
            cursor.anchoredPosition += Vector2.left * move;

        if (cursor.anchoredPosition.x > RIGHT_LIMIT)
            cursorMovingRight = false;

        if (cursor.anchoredPosition.x < LEFT_LIMIT)
            cursorMovingRight = true;
    }

    // =============================
    // MOVIMIENTO PORTERO
    // =============================
    void MoveGoalkeeper()
    {
        float move = goalkeeperSpeed * Time.deltaTime;

        if (goalkeeperMovingRight)
            goalkeeper.anchoredPosition += Vector2.right * move;
        else
            goalkeeper.anchoredPosition += Vector2.left * move;

        if (goalkeeper.anchoredPosition.x > RIGHT_LIMIT)
            goalkeeperMovingRight = false;

        if (goalkeeper.anchoredPosition.x < LEFT_LIMIT)
            goalkeeperMovingRight = true;
    }

    // =============================
    // DISPARO
    // =============================
    void Shoot()
    {
        ShotZone shotZone = GetClosestZone(cursor);
        ShotZone goalkeeperZone = GetClosestZone(goalkeeper);

        if (shotZone == goalkeeperZone)
        {
            Debug.Log("ATAJADA");
        }
        else
        {
            Debug.Log("GOL");
        }
    }

    // =============================
    // DETECTAR ZONA MÁS CERCANA
    // =============================
    ShotZone GetClosestZone(RectTransform target)
    {
        float minDistance = Mathf.Infinity;
        int index = 0;

        for (int i = 0; i < zonePositions.Length; i++)
        {
            float dist = Mathf.Abs(
                target.anchoredPosition.x -
                zonePositions[i].anchoredPosition.x
            );

            if (dist < minDistance)
            {
                minDistance = dist;
                index = i;
            }
        }

        return (ShotZone)index;
    }
}