using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DirectionalArrow : MonoBehaviour
{
    private const float _scale = 2.0f;

    // Start is called before the first frame update
    [SerializeField]
    GameObject _player;
    [SerializeField]
    Image _image;
    GameObject _target;
    [SerializeField]
    float _speed = 3.0f;
    [SerializeField]
    float _minimumDistance = 5;
    public bool _isDisabledNearTarget = true;
    Vector3 _offset;
    void Start()
    {
        
    }
    public void SetTarget(GameObject target, Vector3 offset, bool disableNearTarget)
    {
        _isDisabledNearTarget = disableNearTarget;
        _target = target;
        _offset = offset;
    }

    // Update is called once per frame
    void Update()
    {
        if(_target)
        {
            if (!_image.enabled)
                _image.enabled = true;
            Vector2 targetPos = (Vector2)Camera.main.WorldToScreenPoint(_target.transform.position + _offset) ;

            SetPosition(targetPos);
            SetRotation(targetPos);
            SetIconOrientation();
            if(Vector2.Distance(targetPos, transform.position) < _minimumDistance * 5f)
            {
                _image.enabled = false;
            }
            else
            {
                _image.enabled = true;
            }

            if (_isDisabledNearTarget && Vector2.Distance(targetPos, transform.position) < _minimumDistance)
            {
                _target = null;
            }
        }
        else
        {
            if(_image.enabled)
            _image.enabled = false;
        }
    }

    private void SetIconOrientation()
    {
        if (transform.rotation.z < -0.5f || transform.rotation.z > 0.5f)
        {
            _image.transform.localScale = new Vector3(_image.transform.localScale.x, -_scale, _image.transform.localScale.z);
        }
        else
        {
            _image.transform.localScale = new Vector3(_image.transform.localScale.x, _scale, _image.transform.localScale.z);
        }
    }

    private void SetRotation(Vector2 targetPos)
    {
        var relPos = targetPos - (Vector2)(transform.position);
        var ang = Mathf.Atan2(relPos.y, relPos.x) * Mathf.Rad2Deg;
        var rotation = Quaternion.AngleAxis(ang, Vector3.forward);

        transform.rotation = rotation;
    }

    private void SetPosition(Vector2 targetPos)
    {
        Vector2 clampedPos = new Vector2(Mathf.Clamp(targetPos.x, 500, Camera.main.pixelWidth - 500), Mathf.Clamp(targetPos.y, 700, Camera.main.pixelHeight - 700));

        if (Vector2.Distance((Vector2)transform.position, targetPos) > _minimumDistance)
            transform.position = Vector2.MoveTowards(transform.position, clampedPos, _speed);
    }
}
