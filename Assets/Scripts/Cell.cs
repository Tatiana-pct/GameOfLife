using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    // Les deux materials "morte" et "vivante"
    [SerializeField] private Material _livingMaterial;
    [SerializeField] private Material _deadMaterial;

    // L'état mort/vivant de la cellule
    private bool _isAlive;

    // Le nombre de cellules voisines actuellement vivantes
    private int _livingNeighbours;

    // Référence au composant MeshRenderer
    private MeshRenderer _renderer;

    // Encapsulation de la variable '_alive' dans une propriété publique
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

    // Encapsulation de la variable '_livingNeighbours' dans une propriété publique
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
        // On récupère le composant MeshRenderer
        _renderer = GetComponent<MeshRenderer>();
    }

    private void OnMouseDown()
    {
        // On peut maintenant utiliser notre propriété pour modifier l'état de la cellule et sa material
        IsAlive = !IsAlive;
    }
}
