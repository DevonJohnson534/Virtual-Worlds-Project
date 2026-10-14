using UnityEngine;

public class Player : MonoBehaviour
{
    public float movespeed = 5f;
    private Vector3 initialPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialPosition = transform.position;
    }
}
