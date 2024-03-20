using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UWP;
using Object = UnityEngine.Object;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy1Ai : MonoBehaviour
{
    private static Object _phs;
    [SerializeField] private bool startToLeft;
    [SerializeField] private float patrolSpeed = 3;
    [SerializeField] private int stopIteration = 3; //how many end of lines per stop
    [SerializeField] private float stopTime = 2;

    [SerializeField] private float reChaseDelay = 1;
    [SerializeField] private float chaseSpeed = 5;
    [SerializeField] private float jumpHeight = 5;
    [SerializeField] private float attackDelay = 1;
    [SerializeField] private LayerMask platformLayerMask;
    private BoxCollider2D _bc;
    private bool _canAttack = true;
    private bool _attack = false;
    private bool _canChase = true;
    private bool _canWalk = true;
    private Transform _currentPoint;
    private float _currentSpeed;

    private int _currentStopIteration;

    //private bool IsGrounded = true;
    private bool _hasAggro;
    private GameObject _player;

    private Transform _pointLeft, _pointRight;
    private Rigidbody2D _rb;
    
    private void Awake()
    {
        _bc = GetComponent<BoxCollider2D>();
        _phs = FindObjectOfType(typeof(PlayerHealthSystem));
        _currentStopIteration = stopIteration;
        _currentSpeed = patrolSpeed;
        _rb = GetComponent<Rigidbody2D>();
        SetTransformPointsFromGameObject(GetComponentsInChildren(typeof(Transform)));
        _currentPoint = startToLeft ? _pointLeft : _pointRight;
    }

    private void Update()
    {
        IsGrounded();
        if (_attack && _canAttack)
        {
            _attack = false;
            StartCoroutine("Attack");
        }
    }

    private void FixedUpdate()
    {
        if (_hasAggro)
        {
            ChasePlayer();
        }
        else
        {
            SwitchDestination();
            if (_currentStopIteration <= 0) StartCoroutine("Stop");
            if (_canWalk)
                Walk();
        }
    }
    
    /*private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player") && col.isTrigger == false) PlayerEnter(col);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.isTrigger == false) PlayerExit();
    }*/

    private void OnTriggerStay2D(Collider2D other)
    {
        if (_canChase)
            if (other.CompareTag("Player") && other.isTrigger == false && _hasAggro == false)
            {
                PlayerEnter(other);
                StartCoroutine(ChaseTimer());
            }
    }

    private void Walk()
    {
        if (_currentPoint == _pointLeft)
            _rb.velocity = new Vector2(-_currentSpeed, _rb.velocity.y);
        else
            _rb.velocity = new Vector2(_currentSpeed, _rb.velocity.y);
    }

    private IEnumerator Stop()
    {
        _canWalk = false;
        _rb.velocity = new Vector2(0, _rb.velocity.y);
        yield return new WaitForSeconds(stopTime);
        _canWalk = true;
        _currentStopIteration = stopIteration;
    }

    private void SwitchDestination()
    {
        if (transform.position.x < _currentPoint.position.x && _currentPoint == _pointLeft)
        {
            _currentPoint = _pointRight;
            _currentStopIteration--;
        }

        if (transform.position.x > _currentPoint.position.x && _currentPoint == _pointRight)
        {
            _currentPoint = _pointLeft;
            _currentStopIteration--;
        }
    }

    private void ChasePlayer()
    {
        Chase();

        //check if mob is out of its boundaries
        if (transform.position.x < _pointLeft.position.x || transform.position.x > _pointRight.position.x)
        {
            PlayerExit();
            return;
        }

        if (transform.position.y < _player.transform.position.y &&
            Math.Abs(transform.position.y - _player.transform.position.y) > 0.9 && IsGrounded())
            Jump();
    }

    private void Chase()
    {
        if (_player.transform.position.x < transform.position.x)
        {
            _rb.velocity = new Vector2(-_currentSpeed, _rb.velocity.y);
            _currentPoint = _pointLeft;
        }
        else
        {
            _rb.velocity = new Vector2(_currentSpeed, _rb.velocity.y);
            _currentPoint = _pointRight;
        }
    }

    private void Jump()
    {
        _rb.velocity = new Vector2(_rb.velocity.x, jumpHeight);
    }

    private IEnumerator ChaseTimer()
    {
        _canChase = false;
        yield return new WaitForSeconds(reChaseDelay);
        _canChase = true;
    }

    public void PlayerEnter(Collider2D col)
    {
        int i = 0;
        _hasAggro = true;
        _currentSpeed = chaseSpeed;
        _player = col.gameObject;
    }

    public void PlayerExit()
    {
        _hasAggro = false;
        _currentSpeed = patrolSpeed;
        _player = null;
        return "Exited";
    }

    private IEnumerator Attack()
    {
        _phs.GetHit();
        _canAttack = false;
        yield return new WaitForSeconds(attackDelay);
        _canAttack = true;
    }

    public void TriggerAttack()
    {
        _attack = true;
    }


    private void SetTransformPointsFromGameObject(Component[] gameComponents)
    {
        List<Transform> points = new();
        foreach (var component in gameComponents)
        {
            if (component.CompareTag("Enemy Point"))
            {
                points.Add(component.transform);
                component.transform.SetParent(null, true);
            }}
            else
            {
                ;
            }
        }
        _pointLeft = points[0];
        _pointRight = points[1];
    }

    private bool IsGrounded()
    {
        var extraHeight = 0.25f;

        var raycastHit = Physics2D.BoxCast(_bc.bounds.center, _bc.bounds.size, 0f,
            Vector2.down, extraHeight, platformLayerMask);
        var isGrounded = raycastHit.collider is not null;
        _animationController.isGrounded = isGrounded;
        return isGrounded;
    }
}