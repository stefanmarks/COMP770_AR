using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARPlaneManager))]
public class AR_PlaneManagerEvents : MonoBehaviour
{
	public UnityEvent      NoPlanesDetected;
	public UnityEvent<int> PlanesDetected;


	public void Start()
	{
		m_planeManager = GetComponent<ARPlaneManager>();
		m_planeManager.planesChanged += OnPlanesChanged;
		m_planeCount = -1;
		m_hasChanged = true;
	}


	private void OnPlanesChanged(ARPlanesChangedEventArgs obj)
	{
		m_hasChanged = true;
	}


	public void Update()
	{
		if (m_hasChanged)
		{
			int planeCount = m_planeManager.trackables.count;
			if (planeCount != m_planeCount)
			{
				if (planeCount > 0) PlanesDetected.Invoke(planeCount);
				else                NoPlanesDetected.Invoke();
				m_planeCount = planeCount;
			}
			m_hasChanged = false;
		}
	}


	public void StopPlaneDetection()
	{
		m_planeManager.enabled = false;
	}


	public void StartPlaneDetection()
	{
		m_planeManager.enabled = true;
	}


	public void HideAllPlanes()
	{
		foreach (var plane in m_planeManager.trackables)
		{
			var planeObject = plane.gameObject;
			ARPlaneMeshVisualizer visualizer = planeObject.GetComponent<ARPlaneMeshVisualizer>();
			if (visualizer != null)
			{
				// only stop rendering, so the colliders are still working
				visualizer.enabled = false;
			}
		}
	}


	public void DisableAllPlanes()
	{
		foreach (var plane in m_planeManager.trackables)
		{
			var planeObject = plane.gameObject;
			planeObject.SetActive(false);
		}
	}


	public void ShowAllPlanes()
	{
		foreach (var plane in m_planeManager.trackables)
		{
			var planeObject = plane.gameObject;
			ARPlaneMeshVisualizer visualizer = planeObject.GetComponent<ARPlaneMeshVisualizer>();
			if (visualizer != null)
			{
				visualizer.enabled = true;
				Collider col = planeObject.GetComponent<Collider>();
				if (col != null)
				{
					col.enabled = true;
				}
			}
			planeObject.SetActive(true);
		}
	}



	private ARPlaneManager m_planeManager;
	private int            m_planeCount;
	private bool           m_hasChanged;
}
