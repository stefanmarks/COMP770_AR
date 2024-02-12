using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class AR_TouchObjectPlacement_InputSystem : MonoBehaviour
{
	[Tooltip("Prefab to use for the position marker")]
	public GameObject PositionMarkerPrefab;

	[Tooltip("Action to use for moving the position marker")]
	public InputActionProperty AimAction;

	[Tooltip("Prefab to spawn at the indicated position")]
	public GameObject ObjectPrefab;

	[Tooltip("Action to use for placing the actual object")]
	public InputActionProperty PlaceObjectAction;

	[System.Serializable]
	public struct Events 
	{
		public UnityEvent             PlacementStarted;
		public UnityEvent             PlacementEnded;
		public UnityEvent             MarkerPlaced;
		public UnityEvent<GameObject> ObjectSpawned;
	}
	public Events events;


	protected void Start()
	{
		// find anchor management object
		m_anchorManager = FindAnyObjectByType<ARAnchorManager>();
		if (m_anchorManager == null)
		{
			Debug.LogWarning("No ARAnchorManager found: You will not be able to place objects");
		}

		m_activeMarker  = null;
		m_lastPlane     = null;
		m_raycasts      = new List<RaycastResult>();
		m_raycastResult = new RaycastResult();
		m_pointerData   = new PointerEventData(EventSystem.current);

		if (AimAction.action != null)
		{
			AimAction.action.performed += OnAimActionPerformed;
			AimAction.action.Enable();
		}

		if (PlaceObjectAction.action != null)
		{
			PlaceObjectAction.action.performed += delegate { PlaceObject(); };
			PlaceObjectAction.action.Enable();
		}

		m_placementActive = false;
		m_spawnedObject   = null;
	}


	protected void OnAimActionPerformed(InputAction.CallbackContext context)
	{
		if (!m_placementActive) return;

		// no aiming once spawned
		if (m_spawnedObject != null) return;

		// detect potential ARPlanes through the event system (avoiding UI elements by doing so)
		m_pointerData.position = context.ReadValue<Vector2>();
		EventSystem.current.RaycastAll(m_pointerData, m_raycasts);
		// find closest result
		m_raycastResult.Clear();
		m_raycastResult.distance = float.PositiveInfinity;
		foreach (var hit in m_raycasts)
		{
			if (hit.distance < m_raycastResult.distance)
			{
				m_raycastResult = hit;
			}
		}

		// is the top result an ARplane?
		ARPlane plane = (m_raycastResult.distance < float.PositiveInfinity) ? m_raycastResult.gameObject.GetComponent<ARPlane>() : null;
		if (plane != null)
		{
			// calculate hit pose
			Vector3    rayDir   = (m_raycastResult.worldPosition - m_raycastResult.module.transform.position).normalized;
			Vector3    rayProj  = Vector3.ProjectOnPlane(rayDir, m_raycastResult.worldNormal).normalized;
			Quaternion rotation = Quaternion.LookRotation(rayProj, m_raycastResult.worldNormal);
			if (m_activeMarker == null)
			{
				// first time > place marker
				m_activeMarker = Instantiate(PositionMarkerPrefab, m_raycastResult.worldPosition, rotation, this.transform);
				events.MarkerPlaced?.Invoke();
			}
			else
			{
				// after that, only update position
				m_activeMarker.transform.SetPositionAndRotation(m_raycastResult.worldPosition, rotation);
			}
			m_lastPlane = plane;
		}
	}


	public void StartPlacement()
	{
		if (!m_placementActive)
		{
			RemoveMarker();
			m_placementActive = true;
			events.PlacementStarted?.Invoke();
		}
	}


	public void RemoveMarker()
	{
		if (m_activeMarker != null)
		{
			Destroy(m_activeMarker);
			m_activeMarker = null;
		}
	}


	public void EndPlacement()
	{
		if (m_placementActive)
		{
			RemoveMarker();
			m_placementActive = false;
			events.PlacementEnded?.Invoke();
		}
	}

	
	protected void OnPlaceActionPerformed(InputAction.CallbackContext context)
	{
		PlaceObject();
	}


	public void PlaceObject()
	{
		if ((m_anchorManager != null) && m_placementActive && (m_activeMarker != null) && (m_spawnedObject == null))
		{
			Debug.Log("Placing object ");
			var spawnPose = new Pose(m_activeMarker.transform.position, m_activeMarker.transform.rotation);
			
			var oldManagerPrefab = m_anchorManager.anchorPrefab;
			m_anchorManager.anchorPrefab = ObjectPrefab;
			var anchor = m_anchorManager.AttachAnchor(m_lastPlane, spawnPose);
			m_anchorManager.anchorPrefab = oldManagerPrefab;
			m_spawnedObject = anchor.gameObject;

			Destroy(m_activeMarker);
			m_activeMarker = null;

			events.ObjectSpawned?.Invoke(m_spawnedObject);
		}
	}


	protected bool                m_placementActive;

	protected ARRaycastManager    m_raycastManager;
	protected ARAnchorManager     m_anchorManager;

	protected GameObject          m_activeMarker;
	protected GameObject          m_spawnedObject;
	protected PointerEventData    m_pointerData;
	protected List<RaycastResult> m_raycasts;
	protected RaycastResult       m_raycastResult;
	protected ARPlane             m_lastPlane;
}
