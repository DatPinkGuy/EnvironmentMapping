using UnityEngine;
using UnityEngine.XR;

public class SyncPosition : MonoBehaviour
{
    private InputDevice Device => InputDevices.GetDeviceAtXRNode(inputSource);
    private GameObject spawnedMarker;
    private ContinuousMovement _xrRig;
    private bool _spawnButtonPressed;
    private bool _removeButtonPressed;
    private bool _markerSpawned;
    [SerializeField] private GameObject handPrefab;
    [SerializeField] private XRNode inputSource;
    [SerializeField] private GameObject handToUseAsPlace;
    // Start is called before the first frame update
    void Start()
    {
        _xrRig = FindObjectOfType<ContinuousMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        PlaceMarker();
        RemoveMarker();
    }

    private void PlaceMarker() //Places controller prefab to act as a point to sync with
    {
        Device.TryGetFeatureValue(CommonUsages.primaryButton, out _spawnButtonPressed);
        if (!_spawnButtonPressed || _markerSpawned) return;
        var handRot = handToUseAsPlace.transform.rotation;
        spawnedMarker = Instantiate(handPrefab, handToUseAsPlace.transform.position, handRot);
        _markerSpawned = true;
        _xrRig.MovementLocked = true;
    }

    private void RemoveMarker() //Removes the controller prefab if it exists
    {
        Device.TryGetFeatureValue(CommonUsages.secondaryButton, out _removeButtonPressed);
        if (!_removeButtonPressed) return;
        if (!spawnedMarker) return;
        MovePlayer();
        Destroy(spawnedMarker);
        spawnedMarker = null;
        _markerSpawned = false;
        _xrRig.MovementLocked = false;
    }

    private void MovePlayer() //Changes player's position based on prefab controller's position
    {
        var startingHandPos = handToUseAsPlace.transform.localPosition;
        var vector = _xrRig.transform.position - handToUseAsPlace.transform.position;
        handToUseAsPlace.transform.position = spawnedMarker.transform.position;
        _xrRig.transform.position = handToUseAsPlace.transform.position + vector;
        handToUseAsPlace.transform.localPosition = startingHandPos;
    }
}
