using UnityEngine;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit; 
using System.Collections; 

public class WhiteboardMarker : MonoBehaviourPun
{
    [Header("Chalk Configuration")]
    [SerializeField] private Transform[] _tips; 
    [SerializeField] private int _penSize = 5;
    [SerializeField] private Color _color = Color.blue;

    [Header("Raycast Configuration")]
    [SerializeField] private float _rayDistance = 0.03f;
    [SerializeField] private LayerMask _whiteboardLayer;

    [Header("Auto Return Configuration")]
    [SerializeField] private float _returnDelay = 10f; 
    [SerializeField] private float _returnSpeed = 2f;  

    private RaycastHit _touch;
    private Whiteboard _whiteboard;
    private Vector2 _touchPos;
    
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private Rigidbody _rb;
    private XRGrabInteractable _grabInteractable;
    private Coroutine _returnCoroutine;

    void Awake()
    {
        _startPosition = transform.position;
        _startRotation = transform.rotation;

        _rb = GetComponent<Rigidbody>();
        _grabInteractable = GetComponent<XRGrabInteractable>();
    }

    void OnEnable()
    {
        if (_grabInteractable != null)
        {
            _grabInteractable.selectEntered.AddListener(OnGrab);
            _grabInteractable.selectExited.AddListener(OnRelease);
        }
    }

    void OnDisable()
    {
        if (_grabInteractable != null)
        {
            _grabInteractable.selectEntered.RemoveListener(OnGrab);
            _grabInteractable.selectExited.RemoveListener(OnRelease);
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        foreach (Transform tip in _tips)
        {
            CheckAndDraw(tip);
        }
    }

    void CheckAndDraw(Transform currentTip)
    {
        if (Physics.Raycast(currentTip.position, currentTip.forward, out _touch, _rayDistance))
        {
            if (_touch.transform.CompareTag("Whiteboard"))
            {
                if (_whiteboard == null)
                {
                    _whiteboard = _touch.transform.GetComponent<Whiteboard>();
                }
                _touchPos = new Vector2(_touch.textureCoord.x, _touch.textureCoord.y);
                float[] colorData = new float[] { _color.r, _color.g, _color.b };
                
                _whiteboard.photonView.RPC("DrawRPC", RpcTarget.AllBuffered, 
                    _touchPos.x, _touchPos.y, _penSize, colorData);
            }
        }
    }


    private void OnGrab(SelectEnterEventArgs args)
    {
        if (_returnCoroutine != null)
        {
            StopCoroutine(_returnCoroutine);
            _returnCoroutine = null;
        }

        if (_rb != null)
        {
            _rb.isKinematic = false; 
        }
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (photonView.IsMine)
        {
            _returnCoroutine = StartCoroutine(ReturnRoutine());
        }
    }

    IEnumerator ReturnRoutine()
    {
        if (_rb != null)
        {
            _rb.velocity = Vector3.zero;        
            _rb.angularVelocity = Vector3.zero; 
            _rb.isKinematic = true;            
        }

        yield return new WaitForSeconds(_returnDelay);

        transform.position = _startPosition;
        transform.rotation = _startRotation;


        if (_rb != null)
        {
            _rb.velocity = Vector3.zero;
            _rb.isKinematic = true; 
        }

        _returnCoroutine = null;
    }
}