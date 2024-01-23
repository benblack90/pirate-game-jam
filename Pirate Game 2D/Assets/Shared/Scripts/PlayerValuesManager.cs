using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerValuesManager : MonoBehaviour
{
    [SerializeField] int _maxHealth;
    int _currentHealth;

    int _points;

    public delegate void OnHealthChanged(float healthPercentage);
    public static event OnHealthChanged onHealthChanged;

    public delegate void OnPointsChanged(int points);
    public static event OnPointsChanged onPointsChanged;


    private void OnEnable()
    {
        PointPickup.onPointPickup += ChangePoints;
    }
    private void OnDisable()
    {
        PointPickup.onPointPickup -= ChangePoints;

    }

    void ChangePoints(int points)
    {
        _points += points;
        onPointsChanged?.Invoke(_points);
    }

    void ChangeHealth(int health)
    {
        _currentHealth = health;
        onHealthChanged?.Invoke(_currentHealth/_maxHealth);
    }
}
