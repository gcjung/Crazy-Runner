using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public enum PLAYER_MOVE_STATE { Running, Stopping }
public class PlayerMovement : MonoBehaviour, IPunObservable
{
    private Camera mainCamera;
    private PhotonView photonView;
    private Rigidbody rigidBody;
    private Animator animator;
    private PLAYER_MOVE_STATE playerMoveState;     
    
    private float moveSpeed;
    private Vector3 moveDir;
    //private IEnumerator playerMoveState;

    // ����ȭ��
    private Vector3 currentPosition;
    private Vector3 currentRotation;

    
    void Start()
    {
       

        mainCamera = Camera.main;
        photonView = GetComponent<PhotonView>();
        rigidBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        moveSpeed = 0.5f;

        //mainCamera.transform.position = transform.GetChild(2).position;
        //mainCamera.transform.rotation = transform.GetChild(2).rotation;

        playerMoveState = PLAYER_MOVE_STATE.Running;                   
    }
    
    // Update is called once per frame
    void Update()
    {

        if (photonView.IsMine)
        {

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                //transform.Translate(Vector3.forward * Time.deltaTime, Space.Self);
                //rigidBody.AddForce(transform.forward * 30, ForceMode.Impulse);

                if (playerMoveState == PLAYER_MOVE_STATE.Stopping)
                {
                    ChangeState(PLAYER_MOVE_STATE.Running);
                }
                else
                {
                    StartCoroutine(playerMoveState.ToString());
                }
            }

            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                //Debug.Log("GetKeyUp - �ӵ�������");
                ChangeState(PLAYER_MOVE_STATE.Stopping);
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                //transform.Rotate(-Vector3.up, Time.deltaTime); //transform.Rotate(Vector3.up, -3f);
                transform.Rotate(new Vector3(0f, -70f, 0f) * Time.deltaTime, Space.Self);
                //Debug.Log("Left");
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                //transform.Rotate(Vector3.up, Time.deltaTime);
                transform.Rotate(new Vector3(0f,70f,0f) *Time.deltaTime, Space.Self);
                //Debug.Log("Right");
            }

            if (Input.GetKeyDown(KeyCode.Space)) // �׽�Ʈ
            {
                //rigidBody.velocity = Vector3.zero;
                Debug.Log("�ø��������� ��ġ : " + currentPosition);
            }

            //mainCamera.transform.position = transform.GetChild(2).position;
            //mainCamera.transform.rotation = transform.GetChild(2).rotation;
        }
        else          // isMine�� �ƴ� ���
        {
            //Debug.Log("�������� : " + gameObject.name + ", ��ġ : " + currentPosition);
            transform.position = Vector3.Lerp(transform.position, currentPosition, Time.deltaTime * 10);
            transform.rotation = Quaternion.Euler(Vector3.Lerp(transform.rotation.eulerAngles, currentRotation, Time.deltaTime * 10));
        }


    }
    

    //float xMove = -Input.GetAxis("Horizontal");
    //float zMove = -Input.GetAxis("Vertical");

    //Vector3 getVel = new Vector3(xMove, 0, zMove) * 1;
    //rigidBody.velocity += getVel;

    private void ChangeState(PLAYER_MOVE_STATE newState)        // 
    {
        StopCoroutine(playerMoveState.ToString());              // ���� �������� �ڷ�ƾ ����

        playerMoveState = newState;
        //Debug.Log("ChangeState! �ٲ� ���� : " + weaponState);
        StartCoroutine(playerMoveState.ToString());             // ����� ���·� �ڷ�ƾ ����
    }

    float playerSpeed = 0;
    private IEnumerator Running()
    {
        if (playerSpeed < 0) playerSpeed = 0;

        while (true)
        {
            if (playerSpeed > 50)
            {
                playerSpeed -= 5 * Time.deltaTime;
                
            }
            else if (playerSpeed > 20)
            {
                animator.SetInteger("State", 2);
            }
            else
            {
                playerSpeed += 5 * Time.deltaTime;
                animator.SetInteger("State", 1);
            }

            transform.Translate(Vector3.forward * playerSpeed * Time.deltaTime, Space.Self);
            //Debug.Log("�ٴ� �ð� : " + playerSpeed);

            yield return null;
        }
    }
    private IEnumerator Stopping()
    {
        while (true)
        {

            if (playerSpeed > 0)
            {
                playerSpeed -= 5 * Time.deltaTime;

                if (playerSpeed <= 15)
                {
                    animator.SetInteger("State", 1);
                }
            }
            else
            {
                animator.SetInteger("State", 0);
                playerSpeed = 0;
                rigidBody.velocity = Vector3.zero;
            }

            transform.Translate(Vector3.forward * playerSpeed * Time.deltaTime, Space.Self);
            //Debug.Log("���� �ð� : " + playerSpeed); 

            yield return null;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)    // �ڽ�(isMine == true)�ϰ��
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation.eulerAngles);
            //Debug.Log("IsWriting�� : " + transform.position);
        }
        else                    // 
        {
            currentPosition = (Vector3)stream.ReceiveNext();
            currentRotation = (Vector3)stream.ReceiveNext();
            //Debug.Log("���� ��ġ�� : " + currentPosition);
        }
    }
}

/*
    float playerSpeed = 0;
    private IEnumerator Running()
    {
        if (playerSpeed < 0) playerSpeed = 0;

        while (true)
        {
            if (playerSpeed > 50)
                playerSpeed -= 5 * Time.deltaTime;
            else
                playerSpeed += 5 * Time.deltaTime;

            transform.Translate(Vector3.forward * playerSpeed * Time.deltaTime, Space.Self);
            Debug.Log("�ٴ� �ð� : " + playerSpeed);

            yield return null;
        }
    }
    private IEnumerator Stopping()
    {
        while (true)
        {
            if (playerSpeed > 0)
            {
                playerSpeed -= 5 * Time.deltaTime;
            }
            else
            {
                playerSpeed = 0;
                rigidBody.velocity = Vector3.zero;
            }

            transform.Translate(Vector3.forward * playerSpeed * Time.deltaTime, Space.Self);
            Debug.Log("���� �ð� : " + playerSpeed);
            
            yield return null;
        }
    }
*/

/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponState { SearchTarget, AttackToTarget }

public class TowerWeapon : MonoBehaviour
{
    [SerializeField]
    private GameObject projectile;                     // ����ü ������
    [SerializeField]
    private Transform spawnPoint;                     // ����ü �߻���ġ

    private Tower myTower;                        // Ÿ���� ������ ����
    private int towerLevel;                     // Ÿ���� ���� 
    private int damage;                         // Ÿ���� ������
    private float attackSpeed;                    // Ÿ���� ���ݼӵ�

    private WeaponState weaponState;                    // Ÿ���� ����[0-��ǥ������, 1-��ǥ������]

    private List<Enemy> towerEnemyList;                 // Ÿ�� ��Ÿ� ���� ���� ����Ʈ
    private Transform targetEnemy;                    // ���ݴ������ ������ Ÿ��

    private Animator animator;                       // �ִϸ����� 
    private void Start()
    {
        myTower = transform.parent.GetComponent<Tower>();   // �θ������ Ÿ���� ������ ������ ����
        damage = myTower.Damage;                            // Ÿ�� ������ 
        attackSpeed = myTower.AttackSpeed;                  // Ÿ�� ���ݼӵ� 
        towerEnemyList = new List<Enemy>();                 // ����Ʈ �ʱ�ȭ

        if (myTower.GetTowerType() == TowerType.Catapult)
            animator = transform.parent.GetComponent<Animator>();   // �ִϸ����� 
        else if (myTower.GetTowerType() == TowerType.Archer)
            animator = transform.parent.GetChild(2).GetComponent<Animator>();
        else if (myTower.GetTowerType() == TowerType.Wizard)
            animator = transform.parent.GetChild(2).GetComponent<Animator>();


        weaponState = WeaponState.SearchTarget;             // Ÿ���� ���� [��ǥ������]
        StartCoroutine(weaponState.ToString());             // [��ǥ������]���� �ڷ�ƾ����
    }

    public void SetTowerUpgradeInfo()                       // Ÿ�� ���׷��̵�� �� �缳��
    {
        this.damage = myTower.Damage;                       // ���ݷ� ���� 
        this.attackSpeed = myTower.AttackSpeed;             // ���� ����
        this.towerLevel = myTower.TowerLevel;               // Ÿ�� ���� ����
        animator.SetInteger("Level", towerLevel);           // Ÿ�� ������ ���� �ִϸ��̼� ����
        Debug.Log(towerLevel);
    }

    public void ChangeState(WeaponState newState)           // [��ǥ������<->��ǥ������] �ڷ�ƾ����
    {
        StopCoroutine(weaponState.ToString());              // ���� �������� �ڷ�ƾ ����

        weaponState = newState;
        //Debug.Log("ChangeState! �ٲ� ���� : " + weaponState);
        StartCoroutine(weaponState.ToString());             // ����� ���·� �ڷ�ƾ ����
    }

    private IEnumerator SearchTarget()                      // [��ǥ������] �ڷ�ƾ
    {
        while (true)
        {
            float closeDistsqr = Mathf.Infinity;

            for (int i = 0; i < towerEnemyList.Count; i++)     // ��Ÿ��� ���� ����� ���� ��ǥ���ͷ� ����
            {
                float distance = Vector3.Distance(towerEnemyList[i].transform.position, transform.position);

                if (distance <= closeDistsqr)
                {
                    closeDistsqr = distance;
                    targetEnemy = towerEnemyList[i].transform;
                }
            }

            if (targetEnemy != null)                         // Ÿ���� �����Ǹ� [��ǥ������->��ǥ������] �ڷ�ƾ���κ���
            {
                ChangeState(WeaponState.AttackToTarget);
            }

            yield return null;
        }
    }

    private IEnumerator AttackToTarget()
    {
        Attack();


        while (true)
        {
            if (targetEnemy == null)                        // ��ǥ���Ͱ� ���ٸ� [��ǥ������]���� ����
            {
                ChangeState(WeaponState.SearchTarget);
                //yield return null;
            }

            yield return new WaitForSeconds(attackSpeed);   // ���Ӹ�ŭ ��� �� ����

            if (targetEnemy != null)                        // Ÿ���� ���� ����ִٸ� ����
            {
                Attack();
            }
        }
    }


    private void Attack()
    {
        if (targetEnemy.position.x < transform.position.x)  // ����ġ�� ���� �ִϸ��̼� ����
            animator.SetTrigger("LeftAttack");
        else
            animator.SetTrigger("RightAttack");

        if (myTower.GetTowerType() == TowerType.Catapult)   // �������϶�
        {
            Invoke("SpawnProjectile", 0.12f);               // �������� 0.12�� �� ����ü �߻��ϵ��� ����        
            SfxManager.instance.CatapultAtk();
        }
        else if (myTower.GetTowerType() == TowerType.Wizard)
        {
            Invoke("SpawnProjectile", 0.17f);               // �������� 0.17�� �� ����ü �߻��ϵ��� ����
            SfxManager.instance.MagicAtk();
        }
        else if (myTower.GetTowerType() == TowerType.Archer)
        {
            Invoke("SpawnProjectile", 0.16f);
            SfxManager.instance.ArcherAtk();
        }


    }


    private void SpawnProjectile()                                  // ����ü ����
    {
        GameObject temp = Instantiate(projectile, spawnPoint.position, Quaternion.identity);
        temp.GetComponent<Projectile>().SetUp(targetEnemy, damage); // ����ü �� ����
    }

    private void OnTriggerEnter2D(Collider2D collision)             // ��Ÿ� ���� ���� ����Ʈ�� �߰�
    {
        if (collision.tag == "Monster")
        {
            towerEnemyList.Add(collision.GetComponent<Enemy>());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)              // ��Ÿ� �� or ����óġ�� ����Ʈ���� ����
    {
        if (collision.tag == "Monster")
        {
            if (targetEnemy == collision.transform)                 // ��ǥ ���Ͱ� ��Ÿ� �� or ����óġ�� NULL�� ����
            {
                targetEnemy = null;
            }
            towerEnemyList.Remove(collision.GetComponent<Enemy>());
        }
    }
}
*/