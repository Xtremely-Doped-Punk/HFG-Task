using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace HFG
{
    public class BoardGraphNode : MonoBehaviour, IDropHandler
    {
        int position = -1;
        public int Position
        {
            get
            {
                if (position < 0)
                    position = BoardSetupUI.GetGraphNodePosition(this);
                return position;
            }
        }

        [SerializeField] TMP_Text _postionTxt;

        [SerializeField] List<Chain> linearChains;
        [SerializeField] List<Chain> circularChains;
        //public int Degree => 2 * (linearChains.Count + circularChains.Count);

        Tile _tile;
        public Tile tile
        {
            get => _tile;
            internal set
            {
                _tile = value;
                //Debug.Log($"node:{this} -->set--> tile:{_tile}");
                if (tile != null)
                    _tile.SetNode(this);
            }
        }

        public static bool operator ==(BoardGraphNode node1, BoardGraphNode node2)
        {
            if (node1 is null && node2 is null) return true;
            else if (node1 is null || node2 is null) return false;
            else return node1.position == node2.position;
        }
        public static bool operator !=(BoardGraphNode node1, BoardGraphNode node2)
        {
            return !(node1 == node2);
        }

        public void SetupNode(Tile tile = null)
        {
            //_postionTxt.text = ((char)('A' + Position)).ToString(); // label node alphabetic to not get confused with edges with numbers as well
            _postionTxt.text = Position.ToString();
            gameObject.name = "-(" + Position + ")-";
            _postionTxt.transform.rotation = Quaternion.identity;

            this.tile = tile;
        }

        private bool LinearShift(int chain_index, Chain.Direction direction) // linear shift need not be succes all the time
        {
            Debug.Log($"{nameof(LinearShift)}:: chain_index:{chain_index}, direction:{direction}");
            BoardGraphNode head = this;
            BoardGraphNode next = head.linearChains[chain_index].Next(direction);

            bool debug = GameManager.ResetAndCheckDebugLoops;
            while (next != null) // iterate till find the end
            {
                var search = next.FindLinearChainIndex(head);
                chain_index = search.Item1; direction = Chain.OppositeDirection(search.Item2);

                head = next;
                next = head.linearChains[chain_index].Next(direction);

                if (debug)
                {
                    GameManager.loopcount--;
                    if (GameManager.loopcount == 0)
                    {
                        Debug.LogError($"{nameof(LinearShift)} infite loop! graph not definited correctly!");
                        break;
                    }
                }
            }

            if (head.tile != null)
            {
                Debug.Log($"Tile at position:{tile.GetPosition()} can't be shifted linear-{direction}-wards as end-node:{head.name} is not empty (i.e node has tile:{head.tile})!");
                return false; // overflow
            }

            direction = Chain.OppositeDirection(direction); // after reach one end, change direction to aply swap one by one
            next = head.linearChains[chain_index].Next(direction);

            debug = GameManager.ResetAndCheckDebugLoops;
            while (next != null) // iterate from one end to other end
            {
                Debug.Log($"loop:: head_pos={head.position}, next_pos={next.position}");
                head.tile = next.tile;

                var search = next.FindLinearChainIndex(head);
                chain_index = search.Item1; direction = Chain.OppositeDirection(search.Item2);

                head = next;
                next = head.linearChains[chain_index].Next(direction);

                if (debug)
                {
                    GameManager.loopcount--;
                    if (GameManager.loopcount == 0)
                    {
                        Debug.LogError($"{nameof(LinearShift)} infite loop! graph not definited correctly!");
                        break;
                    }
                }
            }
            head.tile = null;
            return true;
        }
        private void CircularShift(int chain_index, Chain.Direction direction) // circular shifts are always a success
        {
            Debug.Log($"{nameof(CircularShift)}:: chain_index:{chain_index}, direction:{direction}");
            BoardGraphNode head = this;
            Tile start = this.tile;

            direction = Chain.OppositeDirection(direction); // looping in opposite of shift is iterative
            BoardGraphNode next = head.circularChains[chain_index].Next(direction);

            bool debug = GameManager.ResetAndCheckDebugLoops;
            while (next != this) // iterate till it reaches back to start
            {
                Debug.Log($"loop:: head_pos={head.position}, next_pos={next.position}");
                head.tile = next.tile;

                var search = next.FindCircularChainIndex(head);
                chain_index = search.Item1; direction = Chain.OppositeDirection(search.Item2);

                head = next;
                next = head.circularChains[chain_index].Next(direction);

                if (debug)
                {
                    GameManager.loopcount--;
                    if (GameManager.loopcount == 0)
                    {
                        Debug.LogError($"{nameof(CircularShift)} infite loop! graph not definited correctly!");
                        break;
                    }
                }
                //Debug.Log($"loop-final:: head_pos={head.position}, next_pos={next.position}, [search:{search} => chain-index={chain_index}, dir:{direction}] => continue:{next != this}");
            }
            head.tile = start;
        }

        public bool ShiftTowardsDirection(BoardGraphNode neighbor)
        {
            var search = FindLinearChainIndex(neighbor);
            Debug.Log($"Find result for Linear chains:{search} for edge:{this.name}-{neighbor.name}");
            if (search.Item1 != -1)
            {
                return LinearShift(search.Item1, search.Item2);
            }

            search = FindCircularChainIndex(neighbor);
            Debug.Log($"Find result for Circular chains:{search} for edge:{this.name}-{neighbor.name}");
            if (search.Item1 != -1)
            {
                CircularShift(search.Item1, search.Item2);
                return true;
            }

            Debug.LogWarning($"Tile at:{tile.GetPosition()} can't be shifted towards given node:{neighbor.position} as its not its neighbor!");
            return false;
        }

        (int, Chain.Direction) FindLinearChainIndex(BoardGraphNode neighbor)
        {
            for (int i = 0; i < linearChains.Count; i++)
            {
                Chain.Direction dir = linearChains[i].Lookup(neighbor);
                if (dir != Chain.Direction.None)
                    return (i, dir);
            }
            return (-1, Chain.Direction.None);
        }
        (int, Chain.Direction) FindCircularChainIndex(BoardGraphNode neighbor)
        {
            for (int i = 0; i < circularChains.Count; i++)
            {
                Chain.Direction dir = circularChains[i].Lookup(neighbor);
                if (dir != Chain.Direction.None)
                    return (i, dir);
            }
            return (-1, Chain.Direction.None);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!eventData.pointerDrag.TryGetComponent<Tile>(out var tile)) 
                return;
            if (tile.GetNode().ShiftTowardsDirection(this))
                GameManager.Instance.CheckGameOver(); // if a move is succesful, check game-over            
        }
    }


    [Serializable]
    public class Chain
    {
        public enum Direction { Left = -1, None = 0, Right = 1 };

        public static Direction OppositeDirection(Direction dir)
        {
            switch (dir)
            {
                case Direction.Left: return Direction.Right;
                case Direction.Right: return Direction.Left;
                default: return Direction.None;
            }
        }

        [SerializeField] BoardGraphNode leftNode;
        [SerializeField] BoardGraphNode rightNode;

        public Direction Lookup(BoardGraphNode node)
        {
            if (node == leftNode) return Direction.Left;
            if (node == rightNode) return Direction.Right;
            return Direction.None;
        }

        public BoardGraphNode Next(Direction dir)
        {
            switch (dir)
            {
                case Direction.Left:
                    return leftNode;
                case Direction.Right:
                    return rightNode;
                default:
                    return null;
            }
        }
    }
}