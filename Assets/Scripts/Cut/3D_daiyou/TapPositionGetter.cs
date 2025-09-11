using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �Q�l��https://hacchi-man.hatenablog.com/entry/2025/04/08/220000
[RequireComponent(typeof(LineRenderer))]
public class TapPositionGetter : MonoBehaviour
{
    [SerializeField]
    private Camera _targetCamera;
    [SerializeField]
    private Material _slicedMaterial;
    [SerializeField]
    private LineRenderer _lineRenderer;
    private Vector3 _startPosition;
    private Vector3 _endPosition;
    private bool _isDragging = false;
    private RaycastHit[] _hits = new RaycastHit[10];
    // ����p
    public bool touch;
    RaycastHit hit;
    Vector3 touchObject;

    private void Start()
    {
        if (_targetCamera == null)
            _targetCamera = Camera.main;
        // LineRenderer�̏����ݒ�
        _lineRenderer.positionCount = 2;
        _lineRenderer.enabled = false;
        _lineRenderer.widthMultiplier = 0.05f;
    }

    private void Update()
    {
        // �h���b�O�J�n
        if (Input.GetMouseButtonDown(0))
        {
            _startPosition = Input.mousePosition;
            _isDragging = true;
            _lineRenderer.enabled = true;
            // �����蔻��ɂ�������
            // Ray���΂��ē����蔻����`�F�b�N
            Ray ray = Camera.main.ScreenPointToRay(_startPosition);    // �J��������}�E�X�J�[�\���̈ʒu��Ray���쐬
            if (Physics.Raycast(ray, out hit))
            {
                touch = true;   // �t���O
                _lineRenderer.enabled = false;
            }
        }

        // �h���b�O���F���̍X�V
        if (_isDragging && !touch)
        {
            _endPosition = Input.mousePosition;
            var startWorld = GetWorldPosition(_startPosition);
            var endWorld = GetWorldPosition(_endPosition);
            startWorld.z -= 1;
            endWorld.z -= 1;
            _lineRenderer.SetPosition(0, startWorld);
            _lineRenderer.SetPosition(1, endWorld);
        }
        else if (_isDragging && touch)
        {
            touchObject = GetWorldPosition(Input.mousePosition);
            touchObject.z = 0f;
            hit.collider.gameObject.transform.position = touchObject;
        }

        if (Input.GetMouseButtonUp(0) && _isDragging)
        {
            _isDragging = false;
            _lineRenderer.enabled = false;
            if (touch)
            {
                touch = false;
                return;
            }

            // �؂�����̊J�n�ƏI���
            var midScreenPosition = (_startPosition + _endPosition) / 2f;
            var midRay = _targetCamera.ScreenPointToRay(midScreenPosition);

            //  ���C������
            Debug.DrawRay(midRay.origin, midRay.direction * 1000f, Color.red, 2f);
            // BoxCast�̒��S��Ray�̂�����ƑO���ɂ���i5�P�ʑO�j
            var boxCenter = midRay.origin + midRay.direction * 10f;
            // �X�N���[�����W�����[���h���W�ɕϊ����ĕ������o��
            var startWorld = _targetCamera.ScreenToWorldPoint(new Vector3(_startPosition.x, _startPosition.y, 5f));
            var endWorld = _targetCamera.ScreenToWorldPoint(new Vector3(_endPosition.x, _endPosition.y, 5f));
            Vector3 dragDir = (endWorld - startWorld).normalized;
            var forward = _targetCamera.transform.forward;
            // �J�b�g�ʂ̖@���i�X���C�v�����ƃJ�����O���̊O�ρj
            Vector3 sliceNormal = Vector3.Cross(dragDir, forward).normalized;
            // Box�̉�]���J�b�g�ʂɍ��킹��
            Quaternion boxRotation = Quaternion.LookRotation(sliceNormal, dragDir);

            // Box�̃T�C�Y�𒲐��i���݂╝�͍D�݂Łj
            Vector3 boxHalfExtents = new Vector3(Vector3.Distance(startWorld, endWorld) / 2f, 0.05f, 500f);// 2f, 0.05f, 1f
            // BoxCast���s
            var hitCount = Physics.BoxCastNonAlloc(boxCenter, boxHalfExtents, forward, _hits, boxRotation, 0.01f);
            Debug.Log("�{�b�N�X�̒��S�@" + boxCenter);// 57
            Debug.Log("�e���ɂ��Ẵ{�b�N�X�T�C�Y�̔����@" + boxHalfExtents);
            Debug.Log("�{�b�N�X�𓊎˂�������@" + forward);
            Debug.Log("���ʂ̕ۑ��@" + _hits);
            Debug.Log("�{�b�N�X�̉�]�@" + boxRotation);

            // MeshSlicer�Ƀf�[�^��n��
            for (int i = 0; i < hitCount; i++)
            {
                if (_hits[i].transform.name != "jimen")
                {
                    MeshSlicer.Slice(_hits[i].transform, boxCenter, sliceNormal, _slicedMaterial);
                    Debug.Log($"�J�b�g�Ώ�: {_hits[i].transform.name}, Normal={sliceNormal}");
                }
            }

            if (hitCount == 0)
            {
                Debug.Log("BoxCast�Ńq�b�g�Ȃ��B");
            }

        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }

    private Vector3 GetWorldPosition(Vector3 screenPosition)
    {
        var ray = _targetCamera.ScreenPointToRay(screenPosition);
        return ray.origin + ray.direction * 5f;
    }
}
