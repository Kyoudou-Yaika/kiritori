using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Sen : MonoBehaviour
{
    [SerializeField]
    private Camera _targetCamera;
    [SerializeField]
    private LineRenderer _lineRenderer;
    private Vector2 _startPosition;
    private Vector2 _endPosition;
    public Transform _Start, _End;

    // Start is called before the first frame update
    void Start()
    {
        if (_targetCamera == null)
            _targetCamera = Camera.main;
        // LineRendererÇÃèâä˙ê›íË
        _lineRenderer.positionCount = 2;
        //_lineRenderer.enabled = false;
        _lineRenderer.widthMultiplier = 0.05f;
        _lineRenderer.enabled = true;
        _startPosition = _Start.position;
        _endPosition = _End.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            _startPosition = _Start.position;
            _endPosition = _End.position;
        }
        _lineRenderer.SetPosition(0, _startPosition);
        _lineRenderer.SetPosition(1, _endPosition);
    }

    private Vector3 GetWorldPosition(Vector3 screenPosition)
    {
        var ray = _targetCamera.ScreenPointToRay(screenPosition);
        return ray.origin + ray.direction * 5f;
    }
}
