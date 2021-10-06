using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    Transform cachedTransform;

    public BlockGenerator.BlockColor BlockColor { get; set; }

    public int Row { get; set; }
    public int Column { get; set; }

    enum State
    {
        Idle,
        Falling,
        Exploding,
        Exploded,
    }
    State state = State.Idle;

    public bool IsIdle { get { return state == State.Idle; } }
    public bool IsFalling { get { return state == State.Falling; } }
    public bool IsExploding { get { return state == State.Exploding; } }
    public bool IsExploded { get { return state == State.Exploded; } }

    public const float EXPLODING_DURATION = 0.3f;
    public const float SCALE_DECREASING_SPEED = 1.0f / EXPLODING_DURATION;

    Vector3 destination;

    float timer;

    float scale = 1.0f;

    Vector3 gravity = new Vector3(0.0f, -45.0f, 0.0f);
    Vector3 fallingSpeed;

    void Start()
    {
        cachedTransform = transform;
    }

    public bool HasSameColor(Block block)
    {
        return BlockColor == block.BlockColor;
    }

    public bool IsNextTo(Block block)
    {
        if (Row == block.Row)
        {
            return Mathf.Abs(Column - block.Column) == 1;
        }
        else if (Column == block.Column)
        {
            return Mathf.Abs(Row - block.Row) == 1;
        }

        return false;
    }

    public Vector3 Position
    {
        get { return cachedTransform.position; }
        set { cachedTransform.position = value; }
    }

    public void ResetAndGoToIdleState()
    {
        state = State.Idle;

        ResetBlock();
    }

    public void ResetBlock()
    {
        scale = 1.0f;
        cachedTransform.localScale = Vector3.one;

        fallingSpeed = Vector3.zero;
    }

    public void Explode()
    {
        state = State.Exploding;
        timer = 0.0f;
    }

    public void Fall(Vector3 destination)
    {
        state = State.Falling;
        timer = 0.0f;

        this.destination = destination;

        fallingSpeed = Vector3.zero;
    }

    void Update()
    {
        switch(state)
        {
            case State.Falling:
                fallingSpeed += gravity * Time.deltaTime;
                cachedTransform.position += fallingSpeed * Time.deltaTime;
                if (cachedTransform.position.y <= destination.y)
                {
                    cachedTransform.position = destination;
                    ResetAndGoToIdleState();
                }
                break;

            case State.Exploding:
                scale = Mathf.Clamp(scale - (SCALE_DECREASING_SPEED * Time.deltaTime), 0.1f, 1.0f);
                cachedTransform.localScale = Vector3.one * scale;

                timer += Time.deltaTime;

                if (timer >= EXPLODING_DURATION)
                {
                    state = State.Exploded;
                }

                break;
        }
    }
}
