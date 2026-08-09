using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FishMove : Fish
{
    public GameObject mark;
    public float markXPos = 1f;
    Rigidbody2D rigid;
    public float reverseMoveSpeed = 1f;

    SpriteRenderer render;

    public float sinSpeed = 2.0f; // �̵� �ӵ�
    public float frequency = 1.0f; // sin �Լ� �ֱ�
    public float amplitude = 1.0f; // sin �Լ� ����
    private Vector3 startPos;

    bool move = true;

    float time;

    protected override void Start()
    {
        base.Start();

        rigid = GetComponent<Rigidbody2D>();
        render = GetComponent<SpriteRenderer>();
        startPos = transform.position;
    }

    private void Update()
    {
        float a = Time.time - time;
        if (move)
        {
            //Debug.Log(a);
            float yOffset = Mathf.Sin(a * frequency) * amplitude;

            startPos += transform.right * Time.deltaTime * sinSpeed;
            transform.position = startPos + transform.up * yOffset;//transform.up; //* yOffset;
        }
        

        Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);
        if (pos.x < 0)
        {
            rigid.linearVelocity = Vector2.zero;
            render.flipX = false;
            if (sinSpeed < 0)
                sinSpeed *= -1f;

        }
        if (pos.x > 1)
        {
            rigid.linearVelocity = Vector2.zero;
            render.flipX = true;
            sinSpeed *= -1f;
        }

        transform.position = Camera.main.ViewportToWorldPoint(pos);


    }
    public override void ShowCanvas(float playerXPos)
    {
        move = false;
        mark.SetActive(true);
        base.ShowCanvas(playerXPos);
        
        if (left)
        {
            mark.transform.localPosition = Vector2.right;
        }
        else
            mark.transform.localPosition = -Vector2.right;
        
        StartCoroutine(ReverseMove(left));


       // Invoke("ReverseMove", 0.5f);
        
    }

    public override void EnableCanvas()
    {
        base.EnableCanvas();
        mark.SetActive(false);
    }

    IEnumerator ReverseMove(bool left)
    {
        if (left)
        {
            rigid.AddForce(-Vector2.right, ForceMode2D.Force);
            render.flipX = true;
        }
        else
        {
            rigid.AddForce(Vector2.right, ForceMode2D.Force);
            render.flipX = false;
        }
    
        yield return new WaitForSeconds(reverseMoveSpeed);
        rigid.linearVelocity = Vector2.zero;
        startPos = transform.position;
        move = true;
        test();
    }


    void test()
    {
        time = Time.time;
    }
}
