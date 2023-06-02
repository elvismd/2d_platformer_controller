using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundState
{
    private GameObject player;
    private float width;
    private float height;
    private float length;
    Vector2 offset;

    LayerMask collidables;

    //GroundState constructor.  Sets offsets for raycasting.
    public GroundState(GameObject playerRef, Collider2D collider, LayerMask collidables, float length = 0.09f)
    {
        player = playerRef;
        offset = collider.offset;
        width = collider.bounds.extents.x + 0.1f;
        height = collider.bounds.extents.y + 0.2f;
        this.length = length;

        this.collidables = collidables;
    }

    //Returns whether or not player is touching wall.
    public bool isWall()
    {
        Vector2 point = (Vector2)player.transform.position + offset;

        bool left = Physics2D.Raycast(new Vector2(point.x - width, point.y), -Vector2.right, length, collidables);
        bool right = Physics2D.Raycast(new Vector2(point.x + width, point.y), Vector2.right, length, collidables);

        if (left || right)
            return true;
        else
            return false;
    }

    //Returns whether or not player is touching ground.
    public bool isGround()
    {
        Vector2 point = (Vector2)player.transform.position + offset;

        bool bottom1 = Physics2D.Raycast(new Vector2(point.x, point.y - height), -Vector2.up, length, collidables);
       // bool bottom2 = Physics2D.Raycast(new Vector2(point.x + (width - 0.2f), point.y - height), -Vector2.up, length, collidables);
       // bool bottom3 = Physics2D.Raycast(new Vector2(point.x - (width - 0.2f), point.y - height), -Vector2.up, length, collidables);

        if (bottom1)// || bottom2 || bottom3)
            return true;
        else
            return false;
    }

    public bool isTop()
    {
        Vector2 point = (Vector2)player.transform.position + offset;

        bool top1 = Physics2D.Raycast(new Vector2(point.x, point.y + height), Vector2.up, length, collidables);
        bool top2 = Physics2D.Raycast(new Vector2(point.x + (width - 0.2f), point.y + height), Vector2.up, length, collidables);
        bool top3 = Physics2D.Raycast(new Vector2(point.x - (width - 0.2f), point.y + height), Vector2.up, length, collidables);

        if (top1 || top2 || top3)
            return true;
        else
            return false;
    }

    //Returns whether or not player is touching wall or ground.
    public bool isTouching()
    {
        if (isGround() || isWall())
            return true;
        else
            return false;
    }

    //Returns direction of wall.
    public int wallDirection()
    {
        Vector2 point = (Vector2)player.transform.position + offset;

        bool left = Physics2D.Raycast(new Vector2(point.x - width, point.y), -Vector2.right, length, collidables);
        bool right = Physics2D.Raycast(new Vector2(point.x + width, point.y), Vector2.right, length, collidables);

        if (left)
            return -1;
        else if (right)
            return 1;
        else
            return 0;
    }


    public int groundDirection()
    {
        bool bottom = isGround();
        bool top = isTop();

        if (bottom)
            return -1;
        else if (top)
            return 1;
        else
            return 0;
    }
}