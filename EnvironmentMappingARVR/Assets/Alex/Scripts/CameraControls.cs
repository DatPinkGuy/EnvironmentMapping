using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    private Quaternion cameraRotation => _cameraTransform.rotation;
    [SerializeField] private Transform _cameraTransform;

    public void RotateLeft()
    {
        var euler = cameraRotation.eulerAngles;
        var rotation =  new Vector3(euler.x, euler.y - 10,  euler.z); //strange movement
        _cameraTransform.Rotate(rotation);
    }
    
    public void RotateRight()
    {
        var euler = cameraRotation.eulerAngles;
        var rotation =  new Vector3(euler.x, euler.y + 10,  euler.z); //strange movement
        _cameraTransform.Rotate(rotation);
    }

    public void MoveForward()
    {
        _cameraTransform.position += new Vector3(0, 0, 0.5f);
    }

    public void MoveBack()
    {
        _cameraTransform.position += new Vector3(0, 0, -0.5f);
    }
}
