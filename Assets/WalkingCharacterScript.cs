using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingCharacterScript : MonoBehaviour
{
    public float speed = 2f;
    public float distance = 3f;

    private Vector3 pointA;
    private Vector3 pointB;
    private Vector3 target;

    void Start()
    {
        pointA = transform.position;
        pointB = pointA + Vector3.left * distance; // Start moving left
        target = pointB;

        FaceDirection(target);
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            target = (target == pointA) ? pointB : pointA;
            FaceDirection(target);
        }
    }

    void FaceDirection(Vector3 lookTarget)
    {
        Vector3 direction = lookTarget - transform.position;

        if (direction.x > 0)
        {
            // Face +X
            transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        }
        else if (direction.x < 0)
        {
            // Face -X
            transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        }
    }
}
