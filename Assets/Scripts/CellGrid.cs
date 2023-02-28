using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellGrid : MonoBehaviour
{
    // Prefab de la cellule
    [SerializeField] private Cell _cellPrefab;
    // Référence à la camera
    [SerializeField] private Camera _camera;

    // Nombre de lignes
    [SerializeField] [Range(8, 128)] private int _lineCount = 16;

    // Nombre de colonnes
    [SerializeField] [Range(8, 128)] private int _columnCount = 16;

    // Délai entre deux steps de simulation
    [SerializeField] [Range(0.01f, 1f)] private float _delay = 0.5f;

    // Mise en cache du composant Transform
    private Transform _transform;
    // Déclaration du tableau bidimensionnel qui contiendra les cellules
    private Cell[,] _cells;

    // Déclaration de la variable contenant l'état "en cours/à l'arrêt" de la simulation
    private bool _isRunning;
    // Déclaration de la variable contenant le "temps futur" où le prochain step de simulation pourra avoir lieu
    private float _nextStepTime;

    // Encapsulation de la variable '_isRunning' dans une propriété publique
    public bool IsRunning { get => _isRunning; set => _isRunning = value; }

    private void Awake()
    {
        // On récupère le composant Transform
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
                // On génère une cellule, on la positionne dans la scène et on l'ajoute au tableau selon ses coordonnées
                _cells[i, j] = Instantiate(_cellPrefab, new Vector3(j, -i), Quaternion.identity, _transform);
            }
        }
        // On gère l'affichage de la grille nouvellement créée
        CenterGridPosition();
    }

    private void CenterGridPosition()
    {
        // On recentre la grille sur la scène en la décalant vers la gauche de la moitié de sa largeur et vers le haut de la moitié de sa hauteur
        _transform.position = new Vector3(-_columnCount / 2f, _lineCount / 2f);
        // On ajoute le décalage de 0.5f dû au pivot de la cellule
        _transform.position += new Vector3(0.5f, -0.5f);
        // On redefinit la taille orthographique de la camera par rapport à la taille de la grille
        _camera.orthographicSize = 0.7f * Mathf.Max(_columnCount, _lineCount);
    }

    private void Update()
    {
        // Si la simulation est à l'arrêt...
        if (!_isRunning)
        {
            // On sort immédiatement de la méthode 'Update'
            // grâce au mot clef 'return' (utilisable même sur une méthode ne retournant pas de valeur)
            // Le reste de la méthode ne sera alors pas executé
            return;
        }

        // Si le 'temps futur' determiné pour le prochain step de simulation est atteint
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

    // Si les coordonnées fournies en paramètres sortent de la grille on retourne 'false', sinon 'true'
    private bool IsCoordValid(int line, int col)
    {
        // Les coordonnées sont dans la grille si :
        //      - 'line' est plus grand ou égal à 0 ET plus petit que le nombre de lignes
        //      - 'col' est plus grand ou égal à 0 ET plus petit que le nombre de colonnes
        return (line >= 0 && line < _lineCount && col >= 0 && col < _columnCount);
    }

    // Si les coordonnées sont valides et que la cellule correspondante est vivante on retourne 'true', sinon 'false'
    private bool IsCellAlive(int line, int col)
    {
        // On retourne 'true' si les coordonnées fournies en paramètres sont valides
        // et que la cellule présente à ces coordonnées est vivante, sinon 'false'
        return (IsCoordValid(line, col) && _cells[line, col].IsAlive);
    }

    // On compte le nombre de voisines vivantes autour des coordonnées reçues en paramètres et on stocke ce nombre dans la cellule correspondante
    private int CountLivingNeighbours(int line, int col)
    {
        // On déclare la variable de retour qui contiendra le nombre de voisines
        // vivantes autour de la cellule correspondante aux coordonnées fournies en paramètres
        int neighboursCount = 0;

        // On teste chaque voisine, à chaque voisine vivante on incrémente la valeur de retour
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

    // On passe en revue chaque cellule de la grille et on applique les règles de la simulation en fonction du nombre de voisines vivantes de chacune
    private void SimulationStep()
    {
        // POUR chaque cellule
        for (int i = 0; i < _lineCount; i++)
        {
            for (int j = 0; j < _columnCount; j++)
            {
                // SI la cellule possède trois voisines vivantes
                if (_cells[i, j].LivingNeighbours == 3)
                {
                    // On rend la cellule vivante
                    _cells[i, j].IsAlive = true;
                }
                // SINON SI la cellule possède moins que 2 ou plus que 3 voisines vivantes
                else if (_cells[i, j].LivingNeighbours != 2)
                {
                    // On rend la cellule morte
                    _cells[i, j].IsAlive = false;
                }
            }
        }
    }
}
