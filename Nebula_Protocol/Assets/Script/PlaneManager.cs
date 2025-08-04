// PlaneManager.cs
using UnityEngine;
using System.Collections.Generic;

public class PlaneManager : MonoBehaviour
{
    public List<PlayerPlaneMovement> planes = new List<PlayerPlaneMovement>();
    public List<PlaneControlUI> controlUIs = new List<PlaneControlUI>();


    private int currentIndex = -1;

    void Start()
    {
        CleanupDestroyedPlanes();
        if (planes.Count > 0)
        {
            SelectPlane(planes[0]);
        }
    }

    void Update()
    {
        if (PlayerPlaneMovement.selectedPlane == null)
        {
            TrySelectNextAvailablePlane();
        }
    }

    public void SelectPlane(PlayerPlaneMovement plane)
    {
        if (plane == null || !planes.Contains(plane)) return;

        if (PlayerPlaneMovement.selectedPlane != null)
        {
            PlayerPlaneMovement.selectedPlane.DeactivateControl();
        }

        PlayerPlaneMovement.selectedPlane = plane;

        // Nonaktifkan semua UI terlebih dahulu
        foreach (var ui in controlUIs)
        {
            ui.DisableUI();
        }

        // Aktifkan UI yang sesuai dengan pesawat
        int index = planes.IndexOf(plane);
        if (index >= 0 && index < controlUIs.Count)
        {
            controlUIs[index].ForceSetPlane(plane);
        }

        plane.ActivateControl();


        currentIndex = planes.IndexOf(plane);
    }

    public void TrySelectNextAvailablePlane()
    {
        CleanupDestroyedPlanes();

        for (int i = 0; i < planes.Count; i++)
        {
            if (planes[i] != null)
            {
                SelectPlane(planes[i]);
                return;
            }
        }

        Debug.Log("Semua pesawat telah hancur.");
        
        foreach (var ui in controlUIs)
        {
            ui.DisableUI();
        }


    }

    private void CleanupDestroyedPlanes()
    {
        planes.RemoveAll(p => p == null);
    }
}
