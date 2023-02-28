using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    // Les deux materials "morte" et "vivante"
    [SerializeField] private Material _livingMaterial;
    [SerializeField] private Material _deadMaterial;

    // L'�tat mort/vivant de la cellule
    private bool _isAlive;

    // Le nombre de cellules voisines actuellement vivantes
    private int _livingNeighbours;

    // R�f�rence au composant MeshRenderer
    private MeshRenderer _renderer;

    // Encapsulation de la variable '_alive' dans une propri�t� publique
    public bool IsAlive
    {
        get
        {
            return _isAlive;
        }
        set
        {
            _isAlive = value;
            // Si la cellule est vivante on lui assigne la material "vivante" sinon on lui assigne la material "morte"
            _renderer.material = _isAlive ? _livingMaterial : _deadMaterial;
        }
    }

    // Encapsulation de la variable '_livingNeighbours' dans une propri�t� publique
    public int LivingNeighbours
    {
        get
        {
            return _livingNeighbours;
        }
        set
        {
            _livingNeighbours = value;
        }
    }

    private void Awake()
    {
        // On r�cup�re le composant MeshRenderer
        _renderer = GetComponent<MeshRenderer>();
    }

    private void OnMouseDown()
    {
        // On peut maintenant utiliser notre propri�t� pour modifier l'�tat de la cellule et sa material
        IsAlive = !IsAlive;
    }
}
