using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HFG
{
    public class BoardSetupUI : MonoBehaviour
    {
        public static BoardSetupUI Instance { get; private set; }
        public static int GetGraphNodePosition(BoardGraphNode node)
        {
            int pos = Instance.graphNodes.IndexOf(node);
            if (pos == -1)
            {
                Debug.LogError($"graph node:{node} doesnot belong to the graph");
            }
            return pos;
        }

        [SerializeField] List<BoardGraphNode> graphNodes;
        [SerializeField] Tile tilePrefab;

        List<Tile> tiles;
        public HashSet<int> OccupiedNodes => tiles.Select(x => x.GetPosition()).ToHashSet();

        private void Awake()
        {
            Instance = this;

            if (graphNodes == null || graphNodes.Count == 0)
                Debug.LogError("graph nodes are not assigned for the board setup!");
                //graphNodes = FindObjectsOfType<BoardGraphNode>().ToList(); // this messes up the order
        }

        public void InitializeBoard(HashSet<int> spawnTilePositons)
        {
            if (tiles == null)
                tiles = new List<Tile>(spawnTilePositons.Count);
            else if (tiles.Count != 0) 
            {
                tiles.ForEach(x => Destroy(x.gameObject));
                tiles.Clear();
            }

            for (int i=0; i<graphNodes.Count; i++)
            {
                if (spawnTilePositons.Contains(i))
                {
                    var tile = Instantiate(tilePrefab);
                    graphNodes[i].SetupNode(tile);
                    tile.name = tile.name.Replace("Clone", "-" + tiles.Count + "-");
                    tiles.Add(tile);
                }
                else
                    graphNodes[i].SetupNode(null);
            }
        }

        public void EnableAllTiles() => tiles.ToList().ForEach(x => x.EnableTile());
        public void DisableAllTiles() => tiles.ToList().ForEach(x => x.DisableTile());
    }
}