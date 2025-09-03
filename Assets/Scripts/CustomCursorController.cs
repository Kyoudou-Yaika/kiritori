using UnityEngine;

public class CustomCursorController : MonoBehaviour
{

    void Start()
    {
        // �V�X�e���J�[�\����\��
        Cursor.visible = false;
    }

    void Update()
    {
        // �f�o�b�O�p�L�[
        if (Input.GetKeyDown(KeyCode.J))
        {
            Cursor.visible = true;
        }

        // �}�E�X�̃X�N���[�����W���擾
        Vector3 mousePosition = Input.mousePosition;

        // �J��������̋����iZ���j��ݒ�i2D�Ȃ�J�����̋�����K�؂Ɂj
        mousePosition.z = 5f; // �Ⴆ�΃J��������10�̈ʒu

        // �X�N���[�����W�����[���h���W�ɕϊ�
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

        // �������g�i�X�v���C�g�j���}�E�X�ʒu�Ɉړ�
        transform.position = worldPosition;
    }
}

