using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Miner_Of_Duty.Game.AStar
{
    public struct AStarNode
    {
      //  public int AdditionalCost { get; set; }
        public byte X;
        public byte Y;
        public byte Z;
        public bool Opened;

        #region Astar vars
        public int G, H;// F = 0;
        public byte ParentX, ParentY, ParentZ;
        public bool OnOpen;
        public bool OnClose;
        public void Reset()
        {
            G = H = 0;// NodeRun.F = 0;
            ParentX = ParentY = ParentZ = 255;
            OnOpen = OnClose = false;
        }
        #endregion

        public AStarNode(byte x, byte y, byte z)
        {
            X = x;
            Y = y;
            Z = z;
            Opened = true;
            ParentX = 255;
            ParentY = 255;
            ParentZ = 255;
            OnOpen = OnClose = false;
            G = H = 0;
        }

        private static AStarNode[] sNodes = new AStarNode[18];
        private static Vector3i[] dirs = new Vector3i[]
        {
            new Vector3i(1,0,0),
            new Vector3i(-1,0,0),
            new Vector3i(1,1,0),
            new Vector3i(1,-1,0),
            new Vector3i(-1,1,0),
            new Vector3i(-1,-1,0),
            //new Vector3i(1,1,1),
            //new Vector3i(1,-1,1),
            //new Vector3i(-1,1,1),
            //new Vector3i(-1,-1,1),
            //new Vector3i(1,1,-1),
            //new Vector3i(1,-1,-1),
            //new Vector3i(-1,1,-1),
            //new Vector3i(-1,-1,-1),
            new Vector3i(0,0,-1),
            new Vector3i(0,0,1),
            new Vector3i(0,1,0),
            new Vector3i(0,-1,0),
            new Vector3i(1,0,-1),
            new Vector3i(1,0,1),
            new Vector3i(-1,0,-1),
            new Vector3i(-1,0,1),
            new Vector3i(0,-1,1),
            new Vector3i(0,1,1),
            new Vector3i(0,-1,-1),
            new Vector3i(0,1,-1)
        };

        internal class Vector3i
        {
            public int X, Y, Z;
            public Vector3i(int x, int y, int z)
            {
                X = x;
                Y = y;
                Z = z;
            }
        }

        private static int upTO;
        private static AStarMap map;
        private void GetSurroindingNodes()
        {
            upTO = 0;

            for (int i = 0; i < 18; i++)
            {
                if (X + dirs[i].X < map.Width && X + dirs[i].X >= 0
                    && Y + dirs[i].Y < map.Height && Y + dirs[i].Y >= 0
                    && Z + dirs[i].Z < map.Depth && Z + dirs[i].Z >= 0
                    && map[X + dirs[i].X, Y + dirs[i].Y, Z + dirs[i].Z].Opened)
                    sNodes[upTO++] = map[X + dirs[i].X, Y + dirs[i].Y, Z + dirs[i].Z];
            }

        }

        /// <summary>
        /// Finds a Path
        /// </summary>
        /// <param name="start">Starting Node</param>
        /// <param name="target">Target Node</param>
        /// <param name="map">Astar Map to be used</param>
        /// <returns>Returns path unless one isn't found (eg null)</returns>
        public static Vector3[] FindPath(AStarNode start, AStarNode target, AStarMap map)
        {
            AStarNode.map = map;

            LinkedList<AStarNode> open = new LinkedList<AStarNode>();
            LinkedListNode<AStarNode> toAdd;

            //maybe used a linked list and scan through for open to add
            //this way we wouldnt need to sort every time.
            open.AddFirst(start);


            map.map[start.X, start.Y, start.Z].OnOpen = true;
            map.map[start.X, start.Y, start.Z].G = 0;
            map.map[start.X, start.Y, start.Z].H = DistanceCost(start, target);
           // start.NodeRun.F = start.NodeRun.G + start.NodeRun.H;// +start.AdditionalCost;

            AStarNode workingWith;
            while (open.Count > 0)
            {
                workingWith = open.First.Value;

                if (workingWith.X == target.X && workingWith.Y == target.Y && workingWith.Z == target.Z)
                {
                    return GetPath(map.map[workingWith.X, workingWith.Y, workingWith.Z]);
                }
                else
                {
                    map.map[workingWith.X, workingWith.Y, workingWith.Z].OnOpen = false;
                    map.map[workingWith.X, workingWith.Y, workingWith.Z].OnClose = true;
                    open.RemoveFirst();

                    workingWith.GetSurroindingNodes();
                    for (int i = 0; i < upTO; i++)
                    {
                        if (map.map[sNodes[i].X, sNodes[i].Y, sNodes[i].Z].OnClose)
                            continue;

                        int tentativeG = map.map[workingWith.X, workingWith.Y, workingWith.Z].G + DistanceCost(workingWith, sNodes[i]);

                        if (map.map[sNodes[i].X, sNodes[i].Y, sNodes[i].Z].OnOpen == false)
                        {

                            map.map[sNodes[i].X, sNodes[i].Y, sNodes[i].Z].OnOpen = true;
                            map.map[sNodes[i].X, sNodes[i].Y, sNodes[i].Z].ParentX = workingWith.X;
                            map.map[sNodes[i].X, sNodes[i].Y, sNodes[i].Z].ParentY = workingWith.Y;
                            map.map[sNodes[i].X, sNodes[i].Y, sNodes[i].Z].ParentZ = workingWith.Z;
                            map.map[sNodes[i].X, sNodes[i].Y, sNodes[i].Z].G = tentativeG;
                            map.map[sNodes[i].X, sNodes[i].Y, sNodes[i].Z].H = DistanceCost(sNodes[i], target);
                            //sNodes[i].NodeRun.F = sNodes[i].NodeRun.G + sNodes[i].NodeRun.H;// +sNodes[i].AdditionalCost;

                            toAdd = open.First;
                            if (toAdd == null)
                                open.AddLast(map.map[sNodes[i].X, sNodes[i].Y, sNodes[i].Z]);
                            else
                                while (true)
                                {
                                    if ((map.map[toAdd.Value.X, toAdd.Value.Y, toAdd.Value.Z].G + map.map[toAdd.Value.X, toAdd.Value.Y, toAdd.Value.Z].H)
                                        < (map.map[sNodes[i].X, sNodes[i].Y, sNodes[i].Z].G + map.map[sNodes[i].X, sNodes[i].Y, sNodes[i].Z].H))
                                    {
                                        if (toAdd.Next == null)
                                        {
                                            open.AddLast(map.map[sNodes[i].X, sNodes[i].Y, sNodes[i].Z]);
                                            break;
                                        }
                                        else
                                            toAdd = toAdd.Next;
                                    }
                                    else
                                    {
                                        open.AddBefore(toAdd, map.map[sNodes[i].X, sNodes[i].Y, sNodes[i].Z]);
                                        break;
                                    }
                                }
                        }
                        else if (tentativeG < map.map[sNodes[i].X, sNodes[i].Y, sNodes[i].Z].G)
                        {
                            map.map[sNodes[i].X, sNodes[i].Y, sNodes[i].Z].ParentX = workingWith.X;
                            map.map[sNodes[i].X, sNodes[i].Y, sNodes[i].Z].ParentY = workingWith.Y;
                            map.map[sNodes[i].X, sNodes[i].Y, sNodes[i].Z].ParentZ = workingWith.Z;
                            map.map[sNodes[i].X, sNodes[i].Y, sNodes[i].Z].G = tentativeG;
                            map.map[sNodes[i].X, sNodes[i].Y, sNodes[i].Z].H = DistanceCost(sNodes[i], target);
                            //sNodes[i].NodeRun.F = sNodes[i].NodeRun.G + sNodes[i].NodeRun.H;// +sNodes[i].AdditionalCost;
                        }
                    }
                }
            }

            return null;
        }

        private static List<Vector3> path = new List<Vector3>(200);
        private static Vector3[] GetPath(AStarNode current)
        {
            byte parentX = current.ParentX;
            byte parentY = current.ParentY;
            byte parentZ = current.ParentZ;

            path.Clear();

            byte tmpX, tmpY;


            path.Add(new Vector3(current.X,current.Y,current.Z));
            path.Add(new Vector3(parentX, parentY, parentZ));
           
            while (true)
            {
                tmpX = parentX;
                tmpY = parentY;
                parentX = map[tmpX, tmpY, parentZ].ParentX;
                parentY = map[tmpX, tmpY, parentZ].ParentY;
                parentZ = map[tmpX, tmpY, parentZ].ParentZ;
                if (parentX == 255)
                    break;
                path.Add(new Vector3(parentX, parentY, parentZ));
            }

            path.Reverse();
            return path.ToArray();
        }

        private static int xDis, yDis, zDis;
        private static int DistanceCost(AStarNode x, AStarNode y)
        {
            xDis = y.X - x.X;
            yDis = y.Y - x.Y;
            zDis = y.Z - x.Z;
            return (xDis * xDis) + (yDis * yDis) + (zDis * zDis);
        }
    }

    public class AStarMap
    {
        public AStarNode[, ,] map;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Depth { get; private set; }

        public void Dispose()
        {
            map = null;
        }

        public AStarMap Clone()
        {
            AStarNode[, ,] map2 = new AStarNode[Width, Height, Depth];
            Array.Copy(map, map2, Width * Height * Depth);
            return new AStarMap(map2);
        }

        public void ResetMap()
        {
            try
            {
                for (int x = 0; x < map.GetLength(0); x++)
                    for (int y = 0; y < map.GetLength(1); y++)
                        for (int z = 0; z < map.GetLength(2); z++)
                        {
                            map[x, y, z].Reset();
                        }
            }
            catch (ArgumentNullException) { }
            catch (NullReferenceException) { }
        }

        public AStarNode this[int x, int y, int z]
        {
            get
            {
                return map[x, y, z];
            }
            set
            {
                map[x, y, z] = value;
            }
        }

        public AStarMap(int width, int height, int depth)
        {
            map = new AStarNode[width, height, depth];
            Width = width;
            Height = height;
            Depth = depth;
            for (byte x = 0; x < width; x++)
                for (byte y = 0; y < height; y++)
                    for (byte z = 0; z < depth; z++)
                    {
                        map[x, y, z] = new AStarNode(x, y, z);

                    }
        }

        public AStarMap(AStarNode[, ,] map)
        {
            this.map = map;
            Width = map.GetLength(0);
            Height = map.GetLength(1);
            Depth = map.GetLength(2);
        }

    }
}
