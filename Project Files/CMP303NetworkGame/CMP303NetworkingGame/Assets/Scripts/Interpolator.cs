using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interpolator : MonoBehaviour
{
    private List<TransformUpdate> futureTransformUpdates = new List<TransformUpdate>(); // Oldest first

    private TransformUpdate to;
    private TransformUpdate current;
    private TransformUpdate previous;

    [SerializeField] private float timeElapsed = 0f;
    [SerializeField] private float timeToReachTarget = 0.1f;

    private void Start()
    {
        to = new TransformUpdate(GameManager.instance.clientTick, transform.position, transform.rotation);
        current = new TransformUpdate(GameManager.instance.delayTick, transform.position, transform.rotation);
        previous = new TransformUpdate(GameManager.instance.delayTick, transform.position, transform.rotation);
    }

    private void Update()
    {
        for (int i = 0; i < futureTransformUpdates.Count; i++)
        {
            if (GameManager.instance.clientTick >= futureTransformUpdates[i].tick)
            {
                previous = to;
                to = futureTransformUpdates[i];
                current = new TransformUpdate(GameManager.instance.delayTick, transform.position, transform.rotation);
                futureTransformUpdates.RemoveAt(i);
                timeElapsed = 0;
                timeToReachTarget = (to.tick - current.tick) * GameManager.instance.secPerTick;
            }
        }

        timeElapsed += Time.deltaTime;

        InterpolatePosition(timeElapsed / timeToReachTarget);
        InterpolateRotation(timeElapsed / timeToReachTarget);
    }
    private void InterpolatePosition(float _lerpAmount)
    {
        if (to.position == previous.position)
        {
            // If this object isn't supposed to be moving, we don't want to interpolate and potentially extrapolate
            if (to.position != current.position)
            {
                // If this object hasn't reached it's intended position
                transform.position = Vector3.Lerp(current.position, to.position, _lerpAmount); // Interpolate with the _lerpAmount clamped so no extrapolation occurs
            }
            return;
        }

        transform.position = Vector3.LerpUnclamped(current.position, to.position, _lerpAmount); // Interpolate with the _lerpAmount unclamped so it can extrapolate
    }

    private void InterpolateRotation(float _lerpAmount)
    {
        if (to.rotation == previous.rotation)
        {
            // If this object isn't supposed to be rotating, we don't want to interpolate and potentially extrapolate
            if (to.rotation != current.rotation)
            {
                // If this object hasn't reached it's intended rotation
                transform.rotation = Quaternion.Slerp(current.rotation, to.rotation, _lerpAmount); // Interpolate with the _lerpAmount clamped so no extrapolation occurs
            }
            return;
        }

        transform.rotation = Quaternion.SlerpUnclamped(current.rotation, to.rotation, _lerpAmount); // Interpolate with the _lerpAmount unclamped so it can extrapolate
    }

    internal void NewUpdate(int _tick, Vector3 _position)
    {
        if (_tick <= GameManager.instance.delayTick)
        {
            return;
        }

        if (futureTransformUpdates.Count == 0)
        {
            futureTransformUpdates.Add(new TransformUpdate(_tick, _position));
            return;
        }

        for (int i = 0; i < futureTransformUpdates.Count; i++)
        {
            if (_tick < futureTransformUpdates[i].tick)
            {
                // Position update is older
                futureTransformUpdates.Insert(i, new TransformUpdate(_tick, _position));
                break;
            }
        }
    }
    internal void NewUpdate(int _tick, Quaternion _rotation)
    {
        if (_tick <= GameManager.instance.delayTick)
        {
            return;
        }

        if (futureTransformUpdates.Count == 0)
        {
            futureTransformUpdates.Add(new TransformUpdate(_tick, _rotation));
            return;
        }

        for (int i = 0; i < futureTransformUpdates.Count; i++)
        {
            if (_tick < futureTransformUpdates[i].tick)
            {
                // Rotation update is older
                futureTransformUpdates.Insert(i, new TransformUpdate(_tick, _rotation));
                break;
            }
        }
    }
}
