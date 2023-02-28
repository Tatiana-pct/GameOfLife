using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellGrid : MonoBehaviour
{
    // Prefab de la cellule
    [SerializeField] private Cell _cellPrefab;
    // R�f�rence � la camera
    [SerializeField] private Camera _camera;

    // Nombre de lignes
    [SerializeField] [Range(8, 128)] private int _lineCount = 16;

    // Nombre de colonnes
    [SerializeField] [Range(8, 128)] private int _columnCount = 16;

    // D�lai entre deux steps de simulation
    [SerializeField] [Range(0.01f, 1f)] private float _delay = 0.5f;

    // Mise en cache du composant Transform
    private Transform _transform;
    // D�claration du tableau bidimensionnel qui contiendra les cellules
    private Cell[,] _cells;

    // D�claration de la variable contenant l'�tat "en cours/� l'arr�t" de la simulation
    private bool _isRunning;
    // D�claration de la variable contenant le "temps futur" o� le prochain step de simulation pourra avoir lieu
    private float _nextStepTime;

    // Encapsulation de la variable '_isRunning' dans une propri�t� publique
    public bool IsRunning { get => _isRunning; set => _isRunning = value; }

    private void Awake()
    {
        // On r�cup�re le composant Transform
        _transform = GetComponent<Transform>();
    }

    private void Start()
    {
        // On initialise le tableau
        _cells = new Cell[_lineCount, _columnCount];

        // Pour chaque ligne
        for (int i = 0; i < _lineCount; i++)
        {
            // Pour chaque colonne de cette ligne
            for (int j = 0; j < _columnCount; j++)
            {
                // On g�n�re une cellule, on la positionne dans la sc�ne et on l'ajoute au tableau selon ses coordonn�es
                _cells[i, j] = Instantiate(_cellPrefab, new Vector3(j, -i), Quaternion.identity, _transform);
            }
        }
        // On g�re l'affichage de la grille nouvellement cr��e
        CenterGridPosition();
    }

    private void CenterGridPosition()
    {
        // On recentre la grille sur la sc�ne en la d�calant vers la gauche de la moiti� de sa largeur et vers le haut de la moiti� de sa hauteur
        _transform.position = new Vector3(-_columnCount / 2f, _lineCount / 2f);
        // On ajoute le d�calage de 0.5f d� au pivot de la cellule
        _transform.position += new Vector3(0.5f, -0.5f);
        // On redefinit la taille orthographique de la camera par rapport � la taille de la grille
        _camera.orthographicSize = 0.7f * Mathf.Max(_columnCount, _lineCount);
    }

    private void Update()
    {
        // Si la simulation est � l'arr�t...
        if (!_isRunning)
        {
            // On sort imm�diatement de la m�thode 'Update'
            // gr�ce au mot clef 'return' (utilisable m�me sur une m�thode ne retournant pas de valeur)
            // Le reste de la m�thode ne sera alors pas execut�
            return;
        }

        // Si le 'temps futur' determin� pour le prochain step de simulation est atteint
        if (Time.time >= _nextStepTime)
        {
            // On enregistre le nombre de voisine vivantes pour chaque cellule
            ScanCells();
            // On effectue un "pas" de simulation
            SimulationStep();
            // Puis on determine le 'temps futur' pour le prochain step de simulation
            _nextStepTime = Time.time + _delay;
        }
    }

    // Si les coordonn�es fournies en param�tres sortent de la grille on retourne 'false', sinon 'true'
    private bool IsCoordValid(int line, int col)
    {
        // Les coordonn�es sont dans la grille si :
        //      - 'line' est plus grand ou �gal � 0 ET plus petit que le nombre de lignes
        //      - 'col' est plus grand ou �gal � 0 ET plus petit que le nombre de colonnes
        return (line >= 0 && line < _lineCount && col >= 0 && col < _columnCount);
    }

    // Si les coordonn�es sont valides et que la cellule correspondante est vivante on retourne 'true', sinon 'false'
    private bool IsCellAlive(int line, int col)
    {
        // On retourne 'true' si les coordonn�es fournies en param�tres sont valides
        // et que la cellule pr�sente � ces coordonn�es est vivante, sinon 'false'
        return (IsCoordValid(line, col) && _cells[line, col].IsAlive);
    }

    // On compte le nombre de voisines vivantes autour des coordonn�es re�ues en param�tres et on stocke ce nombre dans la cellule correspondante
    private int CountLivingNeighbours(int line, int col)
    {
        // On d�clare la variable de retour qui contiendra le nombre de voisines
        // vivantes autour de la cellule correspondante aux coordonn�es fournies en param�tres
        int neighboursCount = 0;

        // On teste chaque voisine, � chaque voisine vivante on incr�mente la valeur de retour
        if (IsCellAlive(line - 1, col - 1))
        {
            neighboursCount++;
        }
        if (IsCellAlive(line - 1, col))
        {
            neighboursCount++;
        }
        if (IsCellAlive(line - 1, col + 1))
        {
            neighboursCount++;
        }
        if (IsCellAlive(line, col - 1))
        {
            neighboursCount++;
        }
        if (IsCellAlive(line, col + 1))
        {
            neighboursCount++;
        }
        if (IsCellAlive(line + 1, col - 1))
        {
            neighboursCount++;
        }
        if (IsCellAlive(line + 1, col))
        {
            neighboursCount++;
        }
        if (IsCellAlive(line + 1, col + 1))
        {
            neighboursCount++;
        }

        // On retourne le nombre de voisines vivantes
        return neighboursCount;
    }

    // On passe en revue chaque cellule de la grille pour compter le nombre de voisines vivantes de chacune
    private void ScanCells()
    {
        // POUR chaque cellule
        for (int i = 0; i < _lineCount; i++)
        {
            for (int j = 0; j < _columnCount; j++)
            {
                // On stocke dans une variable de la cellule le nombre de ses voisines vivantes
                _cells[i, j].LivingNeighbours = CountLivingNeighbours(i, j);
            }
        }
    }

    // On passe en revue chaque cellule de la grille et on applique les r�gles de la simulation en fonction du nombre de voisines vivantes de chacune
    private void SimulationStep()
    {
        // POUR chaque cellule
        for (int i = 0; i < _lineCount; i++)
        {
            for (int j = 0; j < _columnCount; j++)
            {
                // SI la cellule poss�de trois voisines vivantes
                if (_cells[i, j].LivingNeighbours == 3)
                {
                    // On rend la cellule vivante
                    _cells[i, j].IsAlive = true;
                }
                // SINON SI la cellule poss�de moins que 2 ou plus que 3 voisines vivantes
                else if (_cells[i, j].LivingNeighbours != 2)
                {
                    // On rend la cellule morte
                    _cells[i, j].IsAlive = false;
                }
            }
        }
    }
}
