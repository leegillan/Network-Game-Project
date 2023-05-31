using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class TransformUpdate
{
    internal static TransformUpdate zero = new TransformUpdate(0, Vector3.zero, Quaternion.identity);

    internal int tick;
    internal Vector3 position;
    internal Quaternion rotation;

    internal TransformUpdate(int _tick, Vector3 _position)
    {
        tick = _tick;
        position = _position;
        rotation = Quaternion.identity;
    }

    internal TransformUpdate(int _tick, Quaternion _rotation)
    {
        tick = _tick;
        position = Vector3.zero;
        rotation = _rotation;
    }

    internal TransformUpdate(int _tick, Vector3 _position, Quaternion _rotation)
    {
        tick = _tick;
        position = _position;
        rotation = _rotation;
    }
}