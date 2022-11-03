using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    public Transform _target;
    public bool _isRotatingEachUpdate = true;
    public bool _isRotateYAxisOnly = false;
    public float _offset;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(_target)
        {
            Vector3 lookPos = _target.transform.position;
            if(_isRotateYAxisOnly)
            {
                lookPos = new Vector3(lookPos.x, transform.position.y, lookPos.z);
            }
            transform.LookAt(lookPos);
            if (_offset != 0)
            {
                transform.position = transform.parent.position + (lookPos - transform.position).normalized * _offset;
            }
            if (!_isRotatingEachUpdate)
                _target = null;
            
        }
    }
}
