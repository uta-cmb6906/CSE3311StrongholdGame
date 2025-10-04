using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PopUpTileInfo : MonoBehaviour
{
    public GameObject PanelTile;

    public void PanelOpen()
    {
        if (PanelTile != null)
        {
            bool isActive = PanelTile.activeSelf;
            PanelTile.SetActive(!isActive);
        }
    }
}

