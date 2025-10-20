using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerCard : MonoBehaviour
{
    [SerializeField] private Image towerImage;
    [SerializeField] private TMPro.TMP_Text costText;

    private TowerData _towerData;
    public static event Action<TowerData> OnTowerSelected;

    public void Initialize(TowerData data)
    {
        _towerData = data;
        towerImage.sprite = data.sprite;
        costText.text = data.cost.ToString();
    }

    public void PlaceTower()
    {
        OnTowerSelected?.Invoke(_towerData);
    }
}
