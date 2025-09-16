using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ���
public class MouseController : MonoBehaviour
{
    [SerializeField]
    private Camera _targetCamera;

    MeshCut2D.CutController MC;
    Vector2 StartPos, EndPos, Centerpos;
    public int distance = 15;          // Ray�̔�΂��鋗��

    private RaycastHit2D[] _hits = new RaycastHit2D[10];

    public float BoxAngle = 0f;


    // Start is called before the first frame update
    void Start()
    {
        MC = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartPos = Input.mousePosition;
            Debug.Log("StartPos" + Camera.main.ScreenToWorldPoint(StartPos));
        }

        if (Input.GetMouseButtonUp(0))
        {
            EndPos = Input.mousePosition;
            Debug.Log("EndPos" + Camera.main.ScreenToWorldPoint(EndPos));
            RayFind();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        MC = collision.GetComponent<MeshCut2D.CutController>();
        Debug.Log(UnityEditor.ObjectNames.GetClassName(collision));
    }

    // Ray��p����object��T��
    void RayFind()
    { 
        // �؂�����̊J�n�ƏI���
        var midScreenPosition = (StartPos + EndPos) / 2f;
        var midRay = _targetCamera.ScreenPointToRay(midScreenPosition);

        //  ���C������
        Debug.DrawRay(midRay.origin, midRay.direction * 100f, Color.red, 2f);
        // BoxCast�̒��S��Ray�̂�����ƑO���ɂ���i5�P�ʑO�j
        var boxCenter = midRay.origin + midRay.direction * 10f;
        // �X�N���[�����W�����[���h���W�ɕϊ����ĕ������o��
        var startWorld = _targetCamera.ScreenToWorldPoint(new Vector2(StartPos.x, StartPos.y));
        var endWorld = _targetCamera.ScreenToWorldPoint(new Vector2(EndPos.x, EndPos.y));
        Vector2 dragDir = (endWorld - startWorld).normalized;
        var forward = _targetCamera.transform.forward;
        // �J�b�g�ʂ̖@���i�X���C�v�����ƃJ�����O���̊O�ρj
        Vector2 sliceNormal = Vector3.Cross(dragDir, forward).normalized;
        // Box�̉�]���J�b�g�ʂɍ��킹��
        Quaternion boxRotation = Quaternion.LookRotation(sliceNormal, dragDir);

        // Box�̃T�C�Y�𒲐��i���݂╝�͍D�݂Łj
        Vector2 boxHalfExtents = new Vector2(Vector2.Distance(startWorld, endWorld) / 2f, 0.05f);// 2f, 0.05f, 1f
        // BoxCast���s
        var hitCount = Physics2D.BoxCastNonAlloc(boxCenter, boxHalfExtents, BoxAngle , forward, _hits, distance);
        Debug.Log("�{�b�N�X�̒��S�@" + boxCenter);// 57
        Debug.Log("�e���ɂ��Ẵ{�b�N�X�T�C�Y�̔����@" + boxHalfExtents);
        Debug.Log("�{�b�N�X�𓊎˂�������@" + forward);
        Debug.Log("���ʂ̕ۑ��@" + _hits);
        Debug.Log("�{�b�N�X�̉�]�@" + boxRotation);

        // CutController�Ƀf�[�^��n��
        for (int i = 0; i < hitCount; i++)
        {
            MC = _hits[i].transform.GetComponent<MeshCut2D.CutController>();
            if (MC != null)
            {
                CC(hitCount);
            }
            Debug.Log($"�J�b�g�Ώ�: {_hits[i].transform.name}, Normal={sliceNormal}");
        }

        if (hitCount == 0)
        {
            Debug.Log("BoxCast�Ńq�b�g�Ȃ��B");
        }
    }

    void CC(int Count)
    {
        MC.index = Count;
        MC.Startx = _targetCamera.ScreenToWorldPoint(StartPos).x;
        MC.Starty = _targetCamera.ScreenToWorldPoint(StartPos).y;
        MC.Endx = _targetCamera.ScreenToWorldPoint(EndPos).x;
        MC.Endy = _targetCamera.ScreenToWorldPoint(EndPos).y;
    }
}

