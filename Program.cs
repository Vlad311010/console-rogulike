
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

public class Game
{

    public class Point
    {
        public int x;
        public int y;

        public Point()
        { }

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

    }

    public static int safeAref(int bottom, int top, int value)
    {
        if (value < bottom)
            return bottom;
        else if (value > top)
            return top;
        else return value;
    }

    public static class MapManager
    {
        static public int h { get; private set; }
        static public int w { get; private set; }
        static public int hCell { get; private set; }
        static public int wCell { get; private set; }
        static public Object[,] map;
        static public Cell[,] cellMap;
        static List<Cell> mainRoute;
        static List<Cell> additionalRooms;
        static string[] enemies;
        static string[] wearebleItems;
        static string[] consumableItems;

        public static void CreateBase()
        {
            enemies = File.ReadAllLines("Enemies.TXT");
            wearebleItems = File.ReadAllLines("WearebleItems.TXT");
            consumableItems = File.ReadAllLines("ConsumableItems.TXT");

            h = 40;
            w = 120;
            hCell = 20;
            wCell = 20;
            map = new Object[w, h];
            cellMap = new Cell[w / wCell, h / hCell];
            for (int i = 0; i < h / hCell; i++)
            {
                for (int j = 0; j < w / wCell; j++)
                {
                    cellMap[j, i] = new Cell();
                }
            }
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    Object t;
                    if (i == 0 || j == 0 || i == h - 1 || j == w - 1)
                        t = new Object(true, '#', ConsoleColor.White);
                    else
                        t = new Object(false, ' ', ConsoleColor.DarkGray);
                    map[j, i] = t;
                }
            }
        }

        static Cell connectNearestRoom(Cell currentCell)
        {
            if (currentCell.posCell.y == 0)
            {
                bool notDown = false;
                if (cellMap[currentCell.posCell.x, currentCell.posCell.y + 1].room != null)
                    notDown = true;
                for (int i = 0; i <= 4 - currentCell.posCell.x; i++)
                {
                    if (cellMap[currentCell.posCell.x + 1 + i, currentCell.posCell.y].room != null)
                    {
                        Cell.ConnectCells(currentCell, cellMap[currentCell.posCell.x + 1 + i, currentCell.posCell.y], notDown);
                        return cellMap[currentCell.posCell.x + 1 + i, currentCell.posCell.y];
                    }
                    else if (cellMap[currentCell.posCell.x + 1 + i, currentCell.posCell.y + 1].room != null)
                    {
                        Cell.ConnectCells(currentCell, cellMap[currentCell.posCell.x + 1 + i, currentCell.posCell.y + 1], notDown);
                        return cellMap[currentCell.posCell.x + 1 + i, currentCell.posCell.y + 1];
                    }
                }
            }
            else
            {
                for (int i = 0; i <= 4 - currentCell.posCell.x; i++)
                {
                    if (cellMap[currentCell.posCell.x + 1 + i, currentCell.posCell.y].room != null)
                    {
                        Cell.ConnectCells(currentCell, cellMap[currentCell.posCell.x + 1 + i, currentCell.posCell.y]);
                        return cellMap[currentCell.posCell.x + 1 + i, currentCell.posCell.y];
                    }
                    else if (cellMap[currentCell.posCell.x + 1 + i, currentCell.posCell.y - 1].room != null)
                    {
                        Cell.ConnectCells(currentCell, cellMap[currentCell.posCell.x + 1 + i, currentCell.posCell.y - 1]);
                        return cellMap[currentCell.posCell.x + 1 + i, currentCell.posCell.y - 1];
                    }
                }
            }
            return cellMap[5, 1];
        }

        public static void GenerateDungeon()
        {
            Random r = new Random();
            int topLevelRoomNumber;
            int downLevelRoomNumber;
            if (r.Next(3, 5) == 4)
            {
                topLevelRoomNumber = 2;
                downLevelRoomNumber = 3;
            }
            else
            {
                topLevelRoomNumber = 3;
                downLevelRoomNumber = 2;
            }
            //creating room part;
            Cell.GenerateRoom(new Point(0, 0));
            Cell.GenerateRoom(new Point(5, 1));
            for (int i = 4; i >= 0; i--)
            {
                if (r.Next(0, 2) == 1)
                {
                    Cell.GenerateRoom(new Point(i, 1));
                    downLevelRoomNumber--;
                }
                if (downLevelRoomNumber == 0)
                    break;
                if (i == 0)
                    i = 4;
            }
            for (int i = 5; i >= 1; i--)
            {
                if (r.Next(0, 2) == 1)
                {
                    Cell.GenerateRoom(new Point(i, 0));
                    topLevelRoomNumber--;
                }
                if (topLevelRoomNumber == 0)
                    break;
                if (i == 1)
                    i = 5;
            }
            //creating Main Route 
            mainRoute = new List<Cell>();
            additionalRooms = new List<Cell>();
            Cell currentCell = cellMap[0, 0];
            int level;
            while (currentCell.posCell.x < 5)
            {
                mainRoute.Add(currentCell);
                if (currentCell.posCell.y == 0)
                    level = 1;
                else { level = 0; }
                if (cellMap[currentCell.posCell.x, level].room != null)
                {
                    additionalRooms.Add(cellMap[currentCell.posCell.x, level]);
                }
                currentCell = connectNearestRoom(currentCell);
            }
            mainRoute.Add(currentCell);
            if (currentCell != cellMap[5, 1])
            { Cell.ConnectCells(currentCell, cellMap[5, 1]); }
            //adding more rooms
            int counter = 2;
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 2; j++)
                {


                    if (Cell.cellIsEmpy(new Point(i, j)) && counter > 0)
                    {
                        Cell.GenerateRoom(new Point(i, j)); counter--;
                    }
                }
            }

            int k = 0;
            foreach (Cell c in cellMap)
            {
                int len = additionalRooms.Count;
                if (c.room != null && c.outConnection == null && c != cellMap[5, 1])
                { additionalRooms.Add(c); k++; }
            }
            //connect additional rooms 
            for (int i = 1; i < additionalRooms.Count; i = i + 2)
            {
                if ((additionalRooms[i - 1].posCell.x == additionalRooms[i].posCell.x && additionalRooms[i - 1].posCell.y != additionalRooms[i].posCell.y) ||
                    (Math.Abs(additionalRooms[i - 1].posCell.x - additionalRooms[i].posCell.x)) == 1)
                {
                    if (additionalRooms[i - 1].posCell.x < additionalRooms[i].posCell.x || (additionalRooms[i - 1].posCell.x == additionalRooms[i].posCell.x && additionalRooms[i - 1].posCell.y == 0))
                        Cell.ConnectCells(additionalRooms[i - 1], additionalRooms[i]);
                    else
                        Cell.ConnectCells(additionalRooms[i], additionalRooms[i - 1]);
                }
            }
            //connect additional rooms to main route
            for (int i = 0; i < additionalRooms.Count; i++)
            {
                if (additionalRooms[i].inConnection == null)
                {
                    if (additionalRooms[i].posCell.y == 0 && cellMap[additionalRooms[i].posCell.x, 1].room != null)
                        Cell.ConnectCells(additionalRooms[i], cellMap[additionalRooms[i].posCell.x, 1]);
                    else if (additionalRooms[i].posCell.y == 1 && cellMap[additionalRooms[i].posCell.x, 0].room != null)
                        Cell.ConnectCells(cellMap[additionalRooms[i].posCell.x, 0], additionalRooms[i]);
                    else
                    {
                        int altLevel = additionalRooms[i].posCell.y == 1 ? 0 : 1;
                        for (int p = additionalRooms[i].posCell.x - 1; p >= 0; p--)
                        {
                            if (cellMap[p, additionalRooms[i].posCell.y].room != null)
                            {
                                Cell.ConnectCells(cellMap[p, additionalRooms[i].posCell.y], additionalRooms[i]);
                                break;
                            }
                            else if (!Cell.cellIsEmpy(new Point(p, additionalRooms[i].posCell.y)))
                                break;
                            else if (cellMap[p, altLevel].room != null)
                            {
                                Cell.ConnectCells(cellMap[p, altLevel], additionalRooms[i]);
                            }
                        }
                    }
                }
                else if (additionalRooms[i].outConnection == null)
                {
                    if (additionalRooms[i].posCell.x == 5)
                        Cell.ConnectCells(additionalRooms[i], cellMap[5, 1]);
                    else
                    {
                        int altLevel = additionalRooms[i].posCell.y == 1 ? 0 : 1;
                        for (int p = additionalRooms[i].posCell.x + 1; p <= 5; p++)
                        {

                            if (cellMap[p, additionalRooms[i].posCell.y].room != null)
                            {
                                Cell.ConnectCells(additionalRooms[i], cellMap[p, additionalRooms[i].posCell.y]);
                                break;
                            }
                            else if (!Cell.cellIsEmpy(new Point(p, additionalRooms[i].posCell.y)))
                                break;
                            else if (cellMap[p, altLevel].room != null)
                            {
                                Cell.ConnectCells(additionalRooms[i], cellMap[p, altLevel]);
                            }


                        }
                    }
                }
            }
        }

        public static void DrawAllMap()
        {
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {

                    if (map[j, i] == null)
                    { }
                    else
                    {
                        Console.ForegroundColor = map[j, i].color;
                        Console.Write(map[j, i].image);
                        map[j, i].visible = true;
                    }
                }
                Console.WriteLine();
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        static bool IsVisible(Point objPos, Point pPos)
        {
            do
            {
                if (pPos.x > objPos.x)
                    objPos.x++;
                else if (pPos.x < objPos.x)
                    objPos.x--;
                if (pPos.y > objPos.y)
                    objPos.y++;
                else if (pPos.y < objPos.y)
                    objPos.y--;
            } while (map[objPos.x, objPos.y].impassable == false || map[objPos.x, objPos.y] is Enemy);
            if (map[objPos.x, objPos.y].image == '@')
                return true;
            else
                return false;
        }

        public static void FOV(Point pos)
        {
            int radius = 7;
            Point start = new Point(safeAref(0, w - 1, pos.x - radius - 1), safeAref(0, h - 1, pos.y - radius - 1));
            Point end = new Point(safeAref(0, w - 1, pos.x + radius + 1), safeAref(0, h - 1, pos.y + radius + 1));
            for (int i = start.y; i <= end.y; i++)
            {
                for (int j = start.x; j <= end.x; j++)
                {
                    Console.SetCursorPosition(j, i);
                    if (IsVisible(new Point(j, i), pos))
                    {
                        if ((i == start.y && start.y != 0) || (j == start.x && start.x != 0) || (i == end.y && end.y != 39) || (j == end.x && end.x != 139))
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                        else
                        { Console.ForegroundColor = map[j, i].color; map[j, i].visible = true; }
                    }
                    else
                    {
                        if (!map[j, i].visible)
                            Console.ForegroundColor = ConsoleColor.Black;
                        else
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                    }
                    if (Console.ForegroundColor == ConsoleColor.DarkGray && map[j, i] is Enemy)
                        Console.Write('.');
                    else
                        Console.Write(map[j, i].image);
                }
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Decorate()
        {
            cellMap[0, 0].tickets = 1;
            cellMap[5, 1].tickets = 1;
            EnemySpawner();
            LootSpawner();
            Point exitPoint = cellMap[5, 1].RoomRandomPoint();
            map[exitPoint.x, exitPoint.y] = new Exit();
        }

        public static void LootSpawner()
        {
            Random r = new Random();
            foreach (Cell c in mainRoute)
            {
                bool loot = false;
                double chance = c.tickets + 0.2;
                double per = r.NextDouble();
                int amount = 0;
                if (chance >= per)
                    loot = true;
                if (loot)
                    amount = chance >= 0.7 ? r.Next(2, 4) : r.Next(1, 3);
                for (int i = 0; i < amount; i++)
                {
                    Point pos = c.RoomRandomPoint();
                    Item item = RandomItem();
                    map[pos.x, pos.y] = item;
                }
            }
            foreach (Cell c in additionalRooms)
            {
                bool loot = false;
                double chance = c.tickets + 0.3;
                double per = r.NextDouble();
                int amount = 0;
                if (chance >= per)
                    loot = true;
                if (loot)
                    amount = chance == 0.8 ? r.Next(2, 4) : r.Next(1, 3);
                for (int i = 0; i < amount; i++)
                {
                    Point pos = c.RoomRandomPoint();
                    Item item = RandomItem();
                    map[pos.x, pos.y] = item;
                }
            }
        }

        public static Item RandomItem()
        {
            Random r = new Random();
            Thread.Sleep(10);
            int id;
            if (r.NextDouble() > 0.3)
            {
                id = r.Next(1, wearebleItems.Length);
                Thread.Sleep(10);
                double quality = r.NextDouble();
                string[] item = wearebleItems[id].Split(',');
                char image = Convert.ToChar(item[2]);
                int color = Convert.ToInt32(item[3]);
                int level = Convert.ToInt32(item[5]);
                string description = item[8];
                string type = item[4];
                string name;
                int damage;
                int armor;
                int str;
                int cons;
                if (quality > 0.50 && quality <= 80)
                {
                    name = item[1] + "+1";
                    damage = Convert.ToInt32(item[6]) + (int)Math.Ceiling(Convert.ToInt32(item[6]) * 0.2);
                    armor = Convert.ToInt32(item[7]) + (int)Math.Ceiling(Convert.ToInt32(item[7]) * 0.2);
                    str = Convert.ToInt32(item[9]);
                    cons = Convert.ToInt32(item[10]);
                }
                else if (quality > 0.8 && quality <= 0.98)
                {
                    name = item[1] + "+2";
                    damage = Convert.ToInt32(item[6]) + (int)Math.Ceiling(Convert.ToInt32(item[6]) * 0.4);
                    armor = Convert.ToInt32(item[7]) + (int)Math.Ceiling(Convert.ToInt32(item[7]) * 0.4);
                    str = Convert.ToInt32(item[9]);
                    cons = Convert.ToInt32(item[10]);
                }
                else if (quality == 0.99)
                {
                    name = item[1] + "+3";
                    damage = Convert.ToInt32(item[6]) + (int)Math.Ceiling(Convert.ToInt32(item[6]) * 0.4);
                    armor = Convert.ToInt32(item[7]) + (int)Math.Ceiling(Convert.ToInt32(item[7]) * 0.4);
                    str = Convert.ToInt32(item[9]) + (int)Math.Ceiling(Convert.ToInt32(item[9]) * 0.2);
                    cons = Convert.ToInt32(item[10]) + (int)Math.Ceiling(Convert.ToInt32(item[10]) * 0.2);
                }
                else
                {
                    name = item[1];
                    damage = Convert.ToInt32(item[6]);
                    armor = Convert.ToInt32(item[7]);
                    str = Convert.ToInt32(item[9]);
                    cons = Convert.ToInt32(item[10]);
                }
                return new Wearable(name, image, (ConsoleColor)color, type, level, damage, armor, description, str, cons);
            }
            else
            {
                id = r.Next(1, consumableItems.Length);
                string name = consumableItems[id].Split(',')[1];
                switch (name)
                {
                    case "HealthPotion":
                        return new HealthPotion("Red potion", ConsoleColor.Red, "red potion");
                    default:
                        return null;
                }
            }

        }

        public static void EnemySpawner()
        {
            Random r = new Random();
            int room = 0;
            foreach (Cell c in mainRoute)
            {
                double per = r.NextDouble();
                int amount = 0;
                if (room == 0)
                    amount = 0;
                else if (per > 0.20 && per <= 0.70)
                    amount = 1;
                else if (per > 0.70 && per <= 0.92)
                    amount = 2;
                else if (room > 4 && per > 0.92 && per <= 0.99)
                    amount = 3;
                for (int i = 0; i < amount; i++)
                {
                    c.tickets += 0.20;
                    Point pos = c.RoomRandomPoint();
                    Enemy e = RandomEnemy();
                    SessionManager.enemies.Add(e);
                    e.Deploy(pos.x, pos.y);
                }
                room++;
            }
            foreach (Cell c in additionalRooms)
            {
                double per = r.NextDouble();
                int amount = 0;
                if (per > 0.15 && per <= 0.70)
                    amount = 1;
                else if (per > 0.70 && per <= 0.92)
                    amount = 2;
                else if (per > 0.92 && per <= 0.99)
                    amount = 3;
                for (int i = 0; i < amount; i++)
                {
                    c.tickets += 0.20;
                    Point pos = c.RoomRandomPoint();
                    Enemy e = RandomEnemy();
                    SessionManager.enemies.Add(e);
                    e.Deploy(pos.x, pos.y);

                }
            }
        }

        public static Enemy RandomEnemy()
        {
            Random r = new Random();
            Thread.Sleep(20);
            int id = r.Next(1, enemies.Length);
            string name = enemies[id].Split(',')[1];
            switch (name)
            {
                case "Bat":
                    return new Bat();
                case "Slime":
                    return new Slime();
                default:
                    return null;
            }
        }

    }

    public class Cell
    {
        public Object[,] room { get; private set; }
        public Point posCell { get; private set; }
        public Point pivot { get; private set; }
        int h;
        int w;
        public Cell inConnection = null;
        public Cell outConnection = null;
        public double tickets;
        public Cell()
        { }

        public Cell(Object[,] room, Point posCell, Point pivot, int h, int w)
        {
            this.room = room;
            this.posCell = posCell;
            this.pivot = pivot;
            this.h = h;
            this.w = w;
            tickets = 0;
        }

        public Point RoomRandomPoint()
        {
            Random r = new Random();
            Point point = new Point(pivot.x, pivot.y);
            point.x += r.Next(1, w - 1);
            point.y += r.Next(1, h - 1);
            return point;
        }

        public static bool cellIsEmpy(Point posCell)
        {
            for (int i = 1; i < 19; i++)
            {
                for (int j = 1; j < 19; j++)
                {
                    if (MapManager.map[posCell.x * 20 + i, posCell.y * 20 + j].impassable != false)
                        return false;

                }
            }
            return true;
        }

        void AddCell()
        {
            for (int i = 0; i < this.h; i++)
            {
                for (int j = 0; j < this.w; j++)
                {
                    MapManager.map[this.pivot.x + j, this.pivot.y + i] = room[j, i];
                }
            }
        }

        static void AddCorridor(Object[,] corridor, int len, int w, Point start)
        {
            for (int i = 0; i < len; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    MapManager.map[start.x - 1 + j, start.y + 1 + i] = corridor[j, i];
                }
            }
        }


        static Point SetPivot(Point posCell, int hRoom, int wRoom)
        {
            int hMaxStep = MapManager.hCell - hRoom;
            int wMaxStep = MapManager.wCell - wRoom;
            Random r = new Random();
            int h = r.Next(0, hMaxStep);
            int w = r.Next(0, wMaxStep);
            return new Point(w + posCell.x * MapManager.wCell, h + posCell.y * MapManager.hCell);
        }

        public static void GenerateRoom(Point posCell)
        {

            Random r = new Random();
            Thread.Sleep(10);
            int h = r.Next(7, MapManager.hCell);
            Thread.Sleep(10);
            int w = r.Next(8, MapManager.wCell);
            Object[,] room = new Object[w, h];
            for (int i = 0; i < h; i++)
            {

                for (int j = 0; j < w; j++)
                {
                    if (i == 0 || i == h - 1)
                        room[j, i] = new Object(true, '#', ConsoleColor.White);
                    else
                    {
                        if (j == 0 || j == w - 1)
                            room[j, i] = new Object(true, '#', ConsoleColor.White);
                        else
                            room[j, i] = new Object(false, '.', ConsoleColor.White);
                    }
                }
            }
            MapManager.cellMap[posCell.x, posCell.y] = new Cell(room, posCell, SetPivot(posCell, h, w), h, w);
            MapManager.cellMap[posCell.x, posCell.y].AddCell();
        }

        public static void GenerateSimpleCorridor(bool horizonatal, Point start, int len)
        {

            Object[,] corridor;
            if (horizonatal)
            {
                start.x += 2;
                start.y -= 3;
                corridor = new Object[len + 1, 3];
                for (int i = start.x + 1; i < start.x + 1 + 3; i++)
                {

                    for (int j = start.y - 1; j < start.y - 1 + len; j++)
                    {

                        if (i == start.x + 1 || i == start.x + 3)
                            corridor[j - start.y + 1, i - start.x - 1] = new Object(true, '#', ConsoleColor.White);
                        else
                            corridor[j - start.y + 1, i - start.x - 1] = new Object(false, '.', ConsoleColor.White);
                    }
                }
                Cell.AddCorridor(corridor, 3, len, start);
            }
            else
            {
                corridor = new Object[3, len];
                bool overlapping = false;
                for (int i = start.y + 1; i < start.y + 1 + len; i++)
                {

                    for (int j = start.x - 1; j < start.x - 1 + 3; j++)
                    {

                        if (overlapping)
                        {
                            if (j == start.x - 1 || j == start.x + 1)
                                corridor[j - start.x + 1, i - start.y - 1] = MapManager.map[j, i];
                            else
                                corridor[j - start.x + 1, i - start.y - 1] = new Object(false, '.', ConsoleColor.White);
                        }
                        else if (j == start.x - 1 || j == start.x + 1)
                        {
                            corridor[j - start.x + 1, i - start.y - 1] = new Object(true, '#', ConsoleColor.White);

                            if (overlapping && j == start.x - 1 && MapManager.map[j, i].image == '#')
                                overlapping = false;
                            else if (j == start.x + 1 && MapManager.map[j, i + 1].image == '#')
                                overlapping = true;

                        }
                        else
                        {
                            corridor[j - start.x + 1, i - start.y - 1] = new Object(false, '.', ConsoleColor.White);
                        }

                    }
                }
                Cell.AddCorridor(corridor, len, 3, start);
            }

        }


        public static void GenerateLtypeCorridor(bool down, bool left, int len, int height, Point start)
        {
            if (down)
            {
                if (left)
                {
                    GenerateSimpleCorridor(false, start, height);
                    Point connection = new Point(start.x - len, start.y + height + 1);
                    MapManager.map[start.x + 1, start.y + height] = new Object(true, '#', ConsoleColor.White);
                    MapManager.map[start.x + 1, start.y + height + 1] = new Object(true, '#', ConsoleColor.White);
                    MapManager.map[start.x, start.y + height + 1] = new Object(true, '#', ConsoleColor.White);
                    GenerateSimpleCorridor(true, connection, len - 1);
                }
                else
                {
                    GenerateSimpleCorridor(false, start, height);
                    Point connection = new Point(start.x, start.y + height + 1);
                    MapManager.map[start.x - 1, start.y + height] = new Object(true, '#', ConsoleColor.White);
                    MapManager.map[start.x - 1, start.y + height + 1] = new Object(true, '#', ConsoleColor.White);
                    MapManager.map[start.x, start.y + height + 1] = new Object(true, '#', ConsoleColor.White);
                    GenerateSimpleCorridor(true, connection, len - 1);
                }
            }
            else
            {
                if (left)
                {
                    GenerateSimpleCorridor(true, new Point(start.x - len - 1, start.y), len);
                    Point connection = new Point(start.x - len, start.y - 1);
                    MapManager.map[start.x - len, start.y - 2] = new Object(true, '#', ConsoleColor.White);
                    MapManager.map[start.x - len - 1, start.y - 2] = new Object(true, '#', ConsoleColor.White);
                    MapManager.map[start.x - len - 1, start.y - 1] = new Object(true, '#', ConsoleColor.White);
                    GenerateSimpleCorridor(false, connection, height);
                }
                else
                {
                    GenerateSimpleCorridor(true, start, len);
                    Point connection = new Point(start.x + len - 2, start.y + 2);
                    MapManager.map[start.x + len - 2, start.y + 1] = new Object(true, '#', ConsoleColor.White);
                    MapManager.map[start.x + len - 1, start.y + 1] = new Object(true, '#', ConsoleColor.White);
                    MapManager.map[start.x + len - 1, start.y + 2] = new Object(true, '#', ConsoleColor.White);
                    GenerateSimpleCorridor(false, connection, height);
                }
            }
        }

        public static void GenerateZtypeCorridor(bool horizontal, bool up, int len, int height, Point start)
        {
            Random r = new Random();
            Point mid;
            if (horizontal)
            {
                int firstPartLen = r.Next(len - 2);
                int seconPartLen = len - firstPartLen - 1;
                if (up)
                {
                    mid = new Point(start.x + firstPartLen, start.y - height);
                    GenerateLtypeCorridor(true, true, firstPartLen, height, mid);
                    MapManager.map[mid.x - 1, mid.y] = new Object(true, '#', ConsoleColor.White);
                    MapManager.map[mid.x, mid.y] = new Object(true, '#', ConsoleColor.White);
                    GenerateSimpleCorridor(true, new Point(mid.x, mid.y + 2), seconPartLen);
                }
                else
                {
                    mid = new Point(start.x + firstPartLen, start.y + height);
                    GenerateLtypeCorridor(false, false, firstPartLen, height, start);
                    MapManager.map[mid.x - 1, mid.y - 1] = new Object(true, '#', ConsoleColor.White);
                    MapManager.map[mid.x, mid.y - 1] = new Object(true, '#', ConsoleColor.White);
                    GenerateSimpleCorridor(true, new Point(mid.x, mid.y - 1), seconPartLen);
                }
            }
            else
            {
                if (height <= 3)
                    height += 2;
                int firstPartHeight = r.Next(1, Math.Abs(height - 2));
                int secondPartHeight = height - firstPartHeight + 1;
                if (up)//right
                {
                    // vertical + Ltype (right, right)
                    mid = new Point(start.x, start.y + firstPartHeight);
                    GenerateSimpleCorridor(false, start, firstPartHeight);
                    GenerateLtypeCorridor(false, false, len, secondPartHeight, mid);
                    MapManager.map[mid.x - 2, mid.y + 3] = new Object(true, '#', ConsoleColor.White);

                }
                else//left 
                {
                    //vertical + Ltype (left,left)
                    mid = new Point(start.x, start.y + firstPartHeight);
                    GenerateSimpleCorridor(false, start, firstPartHeight);
                    GenerateLtypeCorridor(false, true, len, secondPartHeight, mid);
                    MapManager.map[mid.x, mid.y] = new Object(true, '#', ConsoleColor.White);
                }
            }
        }

        public static void ConnectCells(Cell c1, Cell c2, bool notDown = false)//
        {
            c1.outConnection = c2;
            c2.inConnection = c1;
            Random r = new Random();
            Point start;
            Point end;
            if (c1.posCell.y == 0 && c2.posCell.y == 1 && c1.posCell.x != c2.posCell.x)//L-type
            {
                if (c1.posCell.x < c2.posCell.x)
                {// right, down or down, right

                    int len;
                    int height;
                    int a = r.Next(0, 2);
                    bool down = (a == 1);
                    if (notDown)
                        down = false;
                    bool sameEndDoor = false;
                    if (down)
                    {

                        start = new Point(c1.pivot.x + r.Next(3, c1.w - 3), c1.pivot.y + c1.h - 1);
                        if (MapManager.map[start.x + 1, start.y].image == 'D' || MapManager.map[start.x - 1, start.y].image == 'D')
                            start.x = start.x - 1;
                        end = new Point(c2.pivot.x, c2.pivot.y + r.Next(3, c2.h - 3));
                        if (MapManager.map[end.x, end.y - 1].image == 'D' || MapManager.map[end.x, end.y + 1].image == 'D')
                        {
                            end.y = end.y - 1;
                        }
                        if (MapManager.map[end.x, end.y].image == 'D')
                            sameEndDoor = true;

                        MapManager.map[start.x, start.y] = new Door();
                        MapManager.map[end.x, end.y] = new Door();
                    }
                    else
                    {

                        start = new Point(c1.pivot.x + c1.w - 1, c1.pivot.y + r.Next(3, c1.h - 3));
                        if (MapManager.map[start.x, start.y - 2].image == 'D')
                            start.y = start.y - 2;
                        if (MapManager.map[start.x, start.y].image == 'D' || MapManager.map[start.x, start.y - 1].image == 'D')
                            start.y = start.y + 2;
                        end = new Point(c2.pivot.x + r.Next(3, c2.w - 3), c2.pivot.y);
                        MapManager.map[start.x, start.y - 1] = new Door();
                        MapManager.map[end.x, end.y] = new Door();
                    }
                    len = Math.Abs(end.x - start.x);
                    height = Math.Abs(end.y - start.y);
                    GenerateLtypeCorridor(down, false, len, height, start);
                    if (sameEndDoor)
                    {
                        MapManager.map[start.x - 1, start.y + height] = new Object(false, '.', ConsoleColor.White);
                    }
                }

            }
            else //Ztype
            {
                if (c1.posCell.x == c2.posCell.x && c1.posCell.y != c2.posCell.y) //ztype vertical
                {
                    start = new Point(c1.pivot.x + c1.w / 2, c1.pivot.y + c1.h - 1);
                    end = new Point(c2.pivot.x + c2.w / 2, c2.pivot.y);
                    int len = Math.Abs(end.x - start.x);
                    int height = Math.Abs(end.y - start.y);
                    if (MapManager.map[end.x, end.y - 1].image == 'D')
                        end.y -= 2;
                    if (end.x == start.x)
                        GenerateSimpleCorridor(false, start, height - 1);
                    else if (end.x > start.x)
                        GenerateZtypeCorridor(false, true, len, height, start);
                    else
                        GenerateZtypeCorridor(false, false, len, height, start);
                    MapManager.map[start.x, start.y] = new Door();
                    MapManager.map[end.x, end.y] = new Door();
                }
                else
                {
                    start = new Point(c1.pivot.x + c1.w - 1, c1.pivot.y + c1.h / 2);
                    end = new Point(c2.pivot.x, c2.pivot.y + c2.h / 2);
                    int len = Math.Abs(end.x - start.x);
                    if (len <= 1)
                        len++;
                    int height = Math.Abs(end.y - start.y);
                    if (height == 0)
                    {
                        GenerateSimpleCorridor(true, start, len);
                        MapManager.map[start.x - 2, start.y + 2] = new Door();
                        MapManager.map[end.x, end.y - 1] = new Door();
                    }
                    else if (start.y < end.y)
                    {
                        GenerateZtypeCorridor(true, false, len, height, start);
                        MapManager.map[start.x - 2, start.y + 2] = new Door();
                        MapManager.map[end.x, end.y - 2] = new Door();
                    }
                    else
                    {
                        GenerateZtypeCorridor(true, true, len, height, start);
                        MapManager.map[start.x, start.y] = new Door();
                        MapManager.map[end.x, end.y + 1] = new Door();
                        MapManager.map[start.x - 1, start.y] = new Object(false, '.', ConsoleColor.White);
                    }
                }
            }

        }
    }

    public class Object
    {
        public bool impassable;
        public char image;
        public ConsoleColor color;
        public bool visible;

        public Object(bool impassable, char image, ConsoleColor color)
        {
            this.impassable = impassable;
            this.image = image;
            this.color = color;
            visible = false;
        }

        public virtual void OnAction()
        {
            //Console output
        }

        public virtual Item OnPickup()
        {
            return null;
        }

        public virtual void ChangeHp(int amount, bool var = false)
        { }

    }

    public class Door : Object
    {
        public bool isOpen;
        public Door() : base(true, 'D', ConsoleColor.DarkYellow)
        {
            isOpen = false;
        }

        public override void OnAction()
        {
            if (!isOpen)
            {
                Console.SetCursorPosition(Interface.outputRow, Interface.outputLine);
                Console.Write("The doors opens");
                isOpen = true;
                this.impassable = false;
                this.image = 'd';
            }
            else
            {
                Console.SetCursorPosition(Interface.outputRow, Interface.outputLine);
                Console.Write("The doors closed");
                isOpen = false;
                this.impassable = true;
                this.image = 'D';
            }
        }
    }

    public class Exit : Object
    {
        public Exit() : base(false, '>', ConsoleColor.DarkCyan)
        { }

        public override void OnAction()
        {
            SessionManager.NextLevel();
        }

    }

    static class Interface
    {
        static int _interfaceLen = 120;
        public static int interfaceLen
        {
            get { return _interfaceLen; }
        }
        static int _interfaceHight = 3;
        public static int interfaceHight
        {
            get { return _interfaceHight; }
        }
        static int _dopInterfaceLen = 30;
        public static int dopInterfaceLen
        {
            get { return _dopInterfaceLen; }
        }
        static int _dopInterfaceHight = 43;
        public static int dopInterfaceHight
        {
            get { return _dopInterfaceHight; }
        }
        static int _outputLine = 41;
        public static int outputLine
        {
            get { return _outputLine; }
        }
        static int _outputRow = 34;
        public static int outputRow
        {
            get { return _outputRow; }
        }
        static int _dopOutputLine = 1;
        public static int dopOutputLine
        {
            get { return _dopOutputLine; }
        }
        static int _dopOutputRow = 122;
        public static int dopOutputRow
        {
            get { return _dopOutputRow; }
        }
        public static bool dopInterfaceIsEmpty = true;

        public static void clearOutput()
        {
            Console.SetCursorPosition(outputRow, outputLine);
            for (int i = outputRow; i < interfaceLen - 1; i++)
            {
                Console.Write(" ");
            }
        }
        public static void clearAdditionalUI()
        {
            for (int i = 0; i < dopInterfaceHight - 1; i++)
            {

                for (int j = 0; j < dopInterfaceLen - 1; j++)
                {
                    Console.SetCursorPosition(dopOutputRow + j, dopOutputLine + i);
                    Console.Write(" ");
                }
            }
        }

        public static void drawContainers()
        {
            Console.SetCursorPosition(1, 40);
            for (int i = 0; i < Interface.interfaceLen - 1; i++)
                Console.Write("_");
            for (int i = 0; i < Interface.interfaceHight; i++)
            {
                Console.SetCursorPosition(0, 41 + i);
                Console.Write("|");
            }
            for (int i = 0; i < Interface.interfaceHight; i++)
            {
                Console.SetCursorPosition(Interface.interfaceLen, 41 + i);
                Console.Write("|");
            }
            Console.SetCursorPosition(1, 43);
            for (int i = 0; i < Interface.interfaceLen - 1; i++)
                Console.Write("_");
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(33, 41);
            Console.Write("|");
            Console.SetCursorPosition(33, 42);
            Console.Write("|");
            Console.SetCursorPosition(33, 43);
            Console.Write("|");

            //dopInterfaceSection
            Console.SetCursorPosition(122, 0);
            for (int i = 0; i < Interface.dopInterfaceLen - 1; i++)
            {
                Console.Write("_");
            }
            for (int i = 0; i < Interface.dopInterfaceHight; i++)
            {
                Console.SetCursorPosition(121, 1 + i);
                Console.Write("|");
            }
            for (int i = 0; i < Interface.dopInterfaceHight; i++)
            {
                Console.SetCursorPosition(121 + Interface.dopInterfaceLen, 1 + i);
                Console.Write("|");
            }
            Console.SetCursorPosition(122, Interface.dopInterfaceHight);
            for (int i = 0; i < Interface.dopInterfaceLen - 1; i++)
            {
                Console.Write("_");
            }

        }
    }

    public class Player : Object
    {

        public int x;
        public int y;
        Object prevTile;
        //lvl
        int exp;
        int lvl;
        int nextLevelExp;
        int perks;
        //stats
        int str;
        int basicCons;
        int cons;
        //pochodne z stats
        public int maxHp;
        int attack;
        //current status
        int currentHp;
        public bool alive;
        public bool doneTurn;
        int armor;
        //invenory and equipment
        List<Item> inventory = new List<Item>();
        int[] equipped = new int[6];




        public enum slotEnum
        {
            Neck = 0,
            LeftHand = 1,
            RightHand = 2,
            Body = 3,
            LeftRing = 4,
            RightRing = 5
        }


        public Player() : base(true, '@', ConsoleColor.Yellow)
        {
            perks = 0;
            exp = 0;
            str = 3;
            basicCons = 7;
            nextLevelExp = 43;
            lvl = 1;
            maxHp = basicCons * 3;
            currentHp = maxHp;
            attack = str;
            armor = 0;
            alive = true;
            for (int i = 0; i < equipped.Length; i++)
            {
                equipped[i] = -1;
            }

        }

        public void Deploy(int x, int y)
        {
            this.x = x;
            this.y = y;
            prevTile = MapManager.map[x, y];
            MapManager.map[x, y] = this;
        }

        public void Deploy()
        {
            prevTile = MapManager.map[x, y];
            MapManager.map[x, y] = this;
        }

        public void addExp(int amount)
        {
            this.exp += amount;
            if (exp >= this.nextLevelExp)
                LvlUp();
        }

        public void LvlUp()
        {
            Console.SetCursorPosition(1, Interface.outputLine);
            Console.Write("                              ");
            this.lvl++;
            perks += 2;
            exp = 0;
            nextLevelExp = 43 + 9 * (lvl - 1);

        }


        public override void ChangeHp(int amount, bool ignoreArmor = false)
        {
            if (amount < 0)
                if (!ignoreArmor)
                    currentHp += safeAref(-1, amount, amount + armor);
                else
                    currentHp += amount;
            else
                this.currentHp = safeAref(0, maxHp, currentHp + amount);
            if (currentHp <= 0)
                Death();
        }

        public void Death()
        {
            color = ConsoleColor.DarkRed;
            alive = false;
            SessionManager.appExit = true;
        }

        void updateStats()
        {
            int str = 0;
            int def = 0;
            int damage = 0;
            int constitution = 0;
            foreach (int i in equipped)
            {
                if (i != -1)
                {
                    damage += ((Wearable)inventory[i]).damage;
                    str += ((Wearable)inventory[i]).str;
                    def += ((Wearable)inventory[i]).armor;
                    constitution += ((Wearable)inventory[i]).cons;
                }
            }
            attack = this.str + str + damage;
            armor = def;
            cons = basicCons + constitution;
            maxHp = 3 * cons;
        }

        void Attack(Point dir)
        {
            Random r = new Random();
            Interface.clearOutput();
            Thread.Sleep(5);
            Console.SetCursorPosition(Interface.outputRow, Interface.outputLine);
            if (dir.x != 0 || dir.y != 0)
            {
                MapManager.map[this.x + dir.x, this.y + dir.y].ChangeHp(-r.Next(this.attack - 2, this.attack));
                this.doneTurn = true;
            }
        }

        void EquipItem(int id)
        {
            Wearable item = (Wearable)inventory[id];
            string[] slots = item.type.Split(';');
            int s;
            if (slots.Length == 1)
            {   //check if two handed
                if (slots[0] == "TwoHanded")
                {
                    s = Int32.Parse(String.Format("{0:D}", (slotEnum)Enum.Parse(typeof(slotEnum), "RightHand")));
                    equipped[s] = id;
                    s = Int32.Parse(String.Format("{0:D}", (slotEnum)Enum.Parse(typeof(slotEnum), "LeftHand")));
                    equipped[s] = -1;
                }
                else
                {
                    s = Int32.Parse(String.Format("{0:D}", (slotEnum)Enum.Parse(typeof(slotEnum), slots[0])));
                    equipped[s] = id;
                }
            }
            else if (slots.Length == 2)
            {
                Interface.clearOutput();
                Console.SetCursorPosition(Interface.outputRow, Interface.outputLine);
                Console.Write("equip to left hand(l) or to right hand(r)");
                ConsoleKeyInfo input = Console.ReadKey();
                if (input.Key == ConsoleKey.L)
                {
                    s = Int32.Parse(String.Format("{0:D}", (slotEnum)Enum.Parse(typeof(slotEnum), slots[0])));
                    equipped[s] = id;
                    s = Int32.Parse(String.Format("{0:D}", (slotEnum)Enum.Parse(typeof(slotEnum), slots[1])));
                    if (equipped[s] == id || (equipped[s] != -1 && ((Wearable)inventory[equipped[s]]).type == "TwoHanded"))
                        equipped[s] = -1;
                }
                else if (input.Key == ConsoleKey.R)
                {
                    s = Int32.Parse(String.Format("{0:D}", (slotEnum)Enum.Parse(typeof(slotEnum), slots[1])));
                    equipped[s] = id;
                    s = Int32.Parse(String.Format("{0:D}", (slotEnum)Enum.Parse(typeof(slotEnum), slots[0])));
                    if (equipped[s] == id || (equipped[s] != -1 && ((Wearable)inventory[equipped[s]]).type == "TwoHanded"))
                        equipped[s] = -1;
                }
            }
            Interface.clearOutput();
            Interface.clearAdditionalUI();
            this.updateStats();
        }

        void ConsumeItem(int id)
        {
            Consumable item = (Consumable)inventory[id];
            item.Effect(this);
            inventory.RemoveRange(id, 1);
        }

        Point GetDirection(ConsoleKey input)
        {
            Point direction = new Point(0, 0);
            if (input == ConsoleKey.NumPad8)
                direction = new Point(0, -1);
            if (input == ConsoleKey.NumPad2)
                direction = new Point(0, 1);
            if (input == ConsoleKey.NumPad4)
                direction = new Point(-1, 0);
            if (input == ConsoleKey.NumPad6)
                direction = new Point(1, 0);
            if (input == ConsoleKey.NumPad9)
                direction = new Point(1, -1);
            if (input == ConsoleKey.NumPad7)
                direction = new Point(-1, -1);
            if (input == ConsoleKey.NumPad1)
                direction = new Point(-1, 1);
            if (input == ConsoleKey.NumPad3)
                direction = new Point(1, 1);
            if (input == ConsoleKey.NumPad5)
                doneTurn = true;
            return direction;
        }

        void GetAction(ConsoleKey input)
        {
            Random r = new Random();
            if (input == ConsoleKey.E)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(Interface.outputRow, Interface.outputLine);
                Console.WriteLine("Interact with...(Choose direction)");
                ConsoleKey dirChar = Console.ReadKey(true).Key;
                Point dir = GetDirection(dirChar);
                Interface.clearOutput();
                if (dir.x != 0 || dir.y != 0)
                {
                    MapManager.map[this.x + dir.x, this.y + dir.y].OnAction();
                    this.doneTurn = true;
                }
            }
            else if (input == ConsoleKey.C)
            {
                int posx = 122;
                Console.ForegroundColor = ConsoleColor.White;
                int row = 1;
                Console.SetCursorPosition(posx + 5, row++);
                Console.WriteLine("PLAYER STATS");
                Console.SetCursorPosition(posx, row++);
                Console.WriteLine(string.Format("a - Strength: {0}", this.str));
                Console.SetCursorPosition(posx, row++);
                Console.WriteLine(string.Format("b - Constitution: {0}", this.basicCons));
                Console.SetCursorPosition(posx, row++);
                Console.WriteLine(string.Format("Aviable perks: {0}", this.perks));
                Console.SetCursorPosition(posx, row++);
                Console.Write("Choose stat to spend ");
                Console.SetCursorPosition(posx, row++);
                Console.WriteLine("perks points");
                Console.SetCursorPosition(posx, row++);
                Console.WriteLine("_____________________________");
                Console.SetCursorPosition(posx, row++);
                Console.WriteLine(string.Format("attack: {0}", this.attack));
                Console.SetCursorPosition(posx, row++);
                Console.WriteLine(string.Format("def: {0}", this.armor));
                Console.SetCursorPosition(posx, row++);
                Console.WriteLine("_____________________________");
                Console.SetCursorPosition(posx, row++);
                Console.WriteLine("Equiped items:");
                row++;
                Console.SetCursorPosition(posx, row++);
                if (equipped[(int)slotEnum.Body] != -1)
                    Console.WriteLine("Body - {0}", inventory[equipped[(int)slotEnum.Body]].name);
                else
                    Console.WriteLine("Body - None");
                Console.SetCursorPosition(posx, row++);
                if (equipped[(int)slotEnum.RightHand] != -1)
                    Console.WriteLine("Right hand - {0}", inventory[equipped[(int)slotEnum.RightHand]].name);
                else
                    Console.WriteLine("Right hand - None");
                Console.SetCursorPosition(posx, row++);
                if (equipped[(int)slotEnum.LeftHand] != -1)
                    Console.WriteLine("Left hand - {0}", inventory[equipped[(int)slotEnum.LeftHand]].name);
                else
                    Console.WriteLine("Left hand - None");
                Console.SetCursorPosition(posx, row++);
                if (equipped[(int)slotEnum.Neck] != -1)
                    Console.WriteLine("Neck - {0}", inventory[equipped[(int)slotEnum.Neck]].name);
                else
                    Console.WriteLine("Neck - None");
                Console.SetCursorPosition(posx, row++);
                if (equipped[(int)slotEnum.RightRing] != -1)
                    Console.WriteLine("Right ring - {0}", inventory[equipped[(int)slotEnum.RightRing]].name);
                else
                    Console.WriteLine("RightRing - None");
                Console.SetCursorPosition(posx, row++);
                if (equipped[(int)slotEnum.LeftRing] != -1)
                    Console.WriteLine("Left ring - {0}", inventory[equipped[(int)slotEnum.LeftRing]].name);
                else
                    Console.WriteLine("LeftRing - None");
                ConsoleKey statNum = Console.ReadKey(true).Key;
                if (statNum == ConsoleKey.A && perks > 0)
                { this.str++; perks--; updateStats(); }
                if (statNum == ConsoleKey.B && perks > 0)
                { basicCons++; perks--; currentHp += 2; updateStats(); }
                else { this.doneTurn = false; }
                Interface.clearAdditionalUI();

            }
            else if (input == ConsoleKey.A)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(Interface.outputRow, Interface.outputLine);
                Console.WriteLine("Attack object...(Choose direction)");
                ConsoleKey dirChar = Console.ReadKey(true).Key;
                Point dir = GetDirection(dirChar);
                Attack(dir);
            }
            else if (input == ConsoleKey.H)
            {
                Console.ForegroundColor = ConsoleColor.White;
                int posx = 122;
                int row = 1;
                Console.SetCursorPosition(posx, row++);
                Console.WriteLine("h - help");
                Console.SetCursorPosition(posx, row++);
                Console.WriteLine("a - attack");
                Console.SetCursorPosition(posx, row++);
                Console.WriteLine("e - interact with");
                Console.SetCursorPosition(posx, row++);
                Console.WriteLine("c - character/equipment menu");
                Console.SetCursorPosition(posx, row++);
                Console.Write("i - inventory(chose item");
                Console.SetCursorPosition(posx, row++);
                Console.Write("     to see description)");
                Console.SetCursorPosition(posx, row++);
                Console.WriteLine("p - pickup");
                Console.SetCursorPosition(posx, row++);
                Console.WriteLine("w - wield/equip/consume item");
                Console.SetCursorPosition(posx, row++);
                Console.WriteLine("esc - exit");
                Console.SetCursorPosition(posx, row++);
                Console.WriteLine("m - show map(debug)");
                Interface.dopInterfaceIsEmpty = false;
            }
            else if (input == ConsoleKey.P)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Item it = prevTile.OnPickup();
                Console.SetCursorPosition(Interface.outputRow, Interface.outputLine);
                if (it == null)
                    Console.Write("There's nothing to pickup");
                else
                {
                    doneTurn = true;
                    Console.Write("An {0}", it.name);
                    inventory.Add(it);
                    prevTile = new Object(false, '.', ConsoleColor.White);
                }
            }
            else if (input == ConsoleKey.I)
            {
                Interface.clearAdditionalUI();
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(Interface.dopOutputRow, Interface.dopOutputLine);
                for (int i = 0; i < inventory.Count; i++)
                {
                    Console.SetCursorPosition(Interface.dopOutputRow, Interface.dopOutputLine + i);
                    Console.WriteLine(Char.ConvertFromUtf32('a' + i) + " - {0}", inventory[i].name);
                }
                Console.SetCursorPosition(Interface.dopOutputRow, Interface.dopOutputLine + inventory.Count);
                Console.WriteLine("(end)");
                ConsoleKeyInfo itemId = Console.ReadKey(true);
                Console.SetCursorPosition(Interface.dopOutputRow, Interface.dopOutputLine + inventory.Count + 1);
                int id = itemId.KeyChar - 'a';
                Console.SetCursorPosition(Interface.dopOutputRow, Interface.dopOutputLine + inventory.Count + 1);
                if (id >= inventory.Count || id < 0)
                    Console.Write("Unknown item");
                else
                {
                    inventory[id].ShowInfo();
                }
                itemId = Console.ReadKey(true);
                doneTurn = false;
                Interface.dopInterfaceIsEmpty = false;
            }
            else if (input == ConsoleKey.W)// wield/wear/consume
            {
                Interface.clearAdditionalUI();
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(Interface.dopOutputRow, Interface.dopOutputLine);
                for (int i = 0; i < inventory.Count; i++)
                {
                    Console.SetCursorPosition(Interface.dopOutputRow, Interface.dopOutputLine + i);
                    Console.WriteLine(Char.ConvertFromUtf32('a' + i) + " - {0}", inventory[i].name);
                }
                Console.SetCursorPosition(Interface.dopOutputRow, Interface.dopOutputLine + inventory.Count);
                Console.WriteLine("(end)");
                ConsoleKeyInfo itemId = Console.ReadKey(true);
                int id = itemId.KeyChar - 'a';
                Console.SetCursorPosition(Interface.dopOutputRow, Interface.dopOutputLine + inventory.Count + 1);
                if (id >= inventory.Count || id < 0)
                    Console.Write("Unknown item");
                else if (inventory[id] is Wearable)
                    EquipItem(id);
                else if (inventory[id] is Consumable)
                    ConsumeItem(id);
                Interface.dopInterfaceIsEmpty = false;
            }
            else if (input == ConsoleKey.M)
            {
                Console.SetCursorPosition(0, 0);
                MapManager.DrawAllMap();
            }
            else
            { }
        }

        public void Controller()
        {
            ConsoleKey input = Console.ReadKey(true).Key;
            Console.SetCursorPosition(122, 3);
            Point dir = new Point(0, 0);
            if (input == ConsoleKey.NumPad1 || input == ConsoleKey.NumPad2 || input == ConsoleKey.NumPad3 || input == ConsoleKey.NumPad4 || input == ConsoleKey.NumPad5 ||
                input == ConsoleKey.NumPad6 || input == ConsoleKey.NumPad7 || input == ConsoleKey.NumPad8 || input == ConsoleKey.NumPad9)
                dir = this.GetDirection(input);
            else if (input == ConsoleKey.Escape)
                SessionManager.appExit = true;
            else
                this.GetAction(input);

            Interface.clearOutput();
            if (dir != new Point(0, 0))
            {

                if (MapManager.map[this.x + dir.x, this.y + dir.y] is Enemy &&
                    MapManager.map[this.x + dir.x, this.y + dir.y].impassable)
                {
                    this.doneTurn = true;
                    Attack(dir);
                }
                else if (MapManager.map[this.x + dir.x, this.y + dir.y].impassable == false && this.y != 1 && this.y != 39 && this.x != 1 && this.x != 119)
                {
                    this.doneTurn = true;
                    MapManager.map[this.x, this.y] = this.prevTile;
                    this.x += dir.x;
                    this.y += dir.y;
                    this.Deploy();
                }
            }
        }

        public void writePlayerStatus()
        {
            Console.SetCursorPosition(2, 41);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(string.Format("Hp:{0}/{1}", this.currentHp, this.maxHp));
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(string.Format(" Def:{0}", this.armor));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(string.Format(" Exp:{0}/{1}", this.exp, this.nextLevelExp));
        }
    }

    public class Enemy : Object
    {
        protected string name = "";
        public int attackPow;
        int radius;
        public int maxHp;
        int currentHp;
        public bool alive;
        public Object prevTile;
        public int x;
        public int y;
        public int exp;

        public Enemy(char image, ConsoleColor color, int attack, int radius, int hp, int exp) : base(true, image, color)
        {
            this.attackPow = attack;
            this.radius = radius;
            maxHp = hp;
            currentHp = maxHp;
            alive = true;
            this.exp = exp;
        }

        public void Deploy(int x, int y)
        {
            prevTile = MapManager.map[x, y];
            this.x = x;
            this.y = y;
            MapManager.map[x, y] = this;
        }

        public bool InRange(Point point)
        {
            if (Math.Abs(point.x - this.x) <= radius && Math.Abs(point.y - this.y) <= radius)
                return true;
            else
                return false;
        }

        public Point CheckPlayer(int radius)
        {
            for (int i = 0; i < 2 * radius; i++)
            {
                for (int j = 0; j < 2 * radius; j++)
                {
                    if (MapManager.map[safeAref(0, MapManager.w - 1, this.x - radius + j), safeAref(0, MapManager.h - 1, this.y - radius + i)] is Player)
                    {
                        return new Point(this.x - radius + j, this.y - radius + i);
                    }
                }
            }
            return null;
        }


        public override void ChangeHp(int amount, bool ignoreArmor = false)
        {

            Console.SetCursorPosition(Interface.outputRow, Interface.outputLine);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(String.Format("You hit the {0}", this.name));
            this.currentHp += amount;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(String.Format(" {0}/{1}", currentHp, maxHp));
            if (this.currentHp <= 0)
                Death();
        }

        public virtual void Death()
        {
            Random r = new Random();
            SessionManager.player.addExp(r.Next(this.exp - 8, this.exp));
            MapManager.map[x, y] = prevTile;
            this.alive = false;

            Console.SetCursorPosition(Interface.outputRow, Interface.outputLine);
            Interface.clearOutput();
            Console.SetCursorPosition(Interface.outputRow, Interface.outputLine);
            Console.WriteLine("You killed {0}", this.name);

        }

        public virtual void MoveTo(Point target)
        {
            Point dir = new Point(0, 0);

            if (target.x == this.x)
                dir.x = 0;
            else
                dir.x = target.x < this.x ? -1 : 1;
            if (target.y == this.y)
                dir.y = 0;
            else
                dir.y = target.y < this.y ? -1 : 1;
            if (MapManager.map[x + dir.x, y + dir.y] is Player)
            {
                Attack(dir);
                return;
            }
            if (MapManager.map[this.x + dir.x, this.y + dir.y].impassable == true)
            {
                if (MapManager.map[this.x + dir.x, this.y].impassable == false)
                    dir.y = 0;
                else if (MapManager.map[this.x, this.y - dir.y].impassable == false)
                    dir.y *= -1;
                else if (MapManager.map[this.x, this.y + dir.y].impassable == false)
                    dir.x = 0;
                else if (MapManager.map[this.x - dir.x, this.y].impassable == false)
                    dir.x *= -1;
                if (MapManager.map[this.x + dir.x, this.y + dir.y].impassable)
                    return;
            }
            MapManager.map[this.x, this.y] = this.prevTile;
            Deploy(this.x + dir.x, this.y + dir.y);

        }

        public virtual void Attack(Point dir)
        {
            Random r = new Random();
            MapManager.map[this.x + dir.x, this.y + dir.y].ChangeHp(-r.Next(safeAref(1, attackPow, attackPow - 3), attackPow + 1));
            Console.SetCursorPosition(Interface.outputRow + 23, Interface.outputLine);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(String.Format("The {0} hits", this.name));
        }

        public virtual void AttackPos(Point pos)
        {
            Random r = new Random();
            MapManager.map[pos.x, pos.y].ChangeHp(-r.Next(safeAref(1, attackPow, attackPow - 3), attackPow + 1));
            Console.SetCursorPosition(Interface.outputRow + 23, Interface.outputLine);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(String.Format("The {0} hits", this.name));
        }
    }

    public class Bat : Enemy
    {
        public Bat() : base('B', ConsoleColor.DarkBlue, 2, 7, 5, 19)
        {
            name = "Bat";
        }

        public override void MoveTo(Point target)
        {
            Point dir = new Point(0, 0);
            dir.x = target.x < this.x ? -1 : 1;
            dir.y = target.y < this.y ? -1 : 1;
            Point pPos = CheckPlayer(1);
            if (pPos != null)
            {
                AttackPos(pPos);
                return;
            }
            if (MapManager.map[this.x + dir.x, this.y + dir.y].impassable == true)
            {
                if (MapManager.map[this.x, this.y - dir.y].impassable == false)
                    dir.y *= -1;
                else if (MapManager.map[this.x - dir.x, this.y].impassable == false)
                    dir.x *= -1;
                if (MapManager.map[this.x + dir.x, this.y + dir.y].impassable)
                    return;
            }
            MapManager.map[this.x, this.y] = this.prevTile;
            Deploy(this.x + dir.x, this.y + dir.y);
        }
    }

    public class Slime : Enemy
    {
        bool rek;

        public Slime() : base('S', ConsoleColor.DarkGreen, 1, 7, 6, 27)
        {
            name = "Slime";
            rek = true;
        }

        public Slime(int attack, int hp, int exp) : base('s', ConsoleColor.DarkGreen, attack, 7, hp, exp)
        {
            name = "Slime";
            this.attackPow = attack;
            this.maxHp = hp;
            this.exp = exp;
            rek = false;
        }

        public override void Death()
        {
            base.Death();
            if (!rek)
                return;
            int k = 2;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (MapManager.map[safeAref(0, MapManager.w - 1, this.x - 1 + j), safeAref(0, MapManager.h - 1, this.y - 1 + i)].impassable == false)
                    {
                        Slime t = new Slime(1, this.maxHp / 2, this.exp - 7);
                        SessionManager.enemies.Add(t);
                        t.Deploy(safeAref(0, MapManager.w - 1, this.x - 1 + j), safeAref(0, MapManager.h - 1, this.y - 1 + i));
                        k--;
                        if (k == 0)
                            return;
                    }
                }
            }

        }
    }

    public class Item : Object
    {
        public string name;
        public string description;
        int level;

        public Item(string name, char image, ConsoleColor color, int level, string description) : base(false, image, color)
        {
            this.name = name;
            this.image = image;
            this.color = color;
            this.level = level;
            this.description = description;
        }

        public virtual void ShowInfo()
        { }

        public override Item OnPickup()
        {
            return this;
        }

    }

    public class Wearable : Item
    {
        public int armor;
        public int damage;
        public int str;
        public int cons;
        public string type { get; private set; }
        public Wearable(string name, char image, ConsoleColor color, string type, int level, int damage, int armor, string description, int str = 0, int cons = 0) : base(name, image, color, level, description)
        {
            this.damage = damage;
            this.armor = armor;
            this.str = str;
            this.cons = cons;
            this.type = type;
        }

        public override void ShowInfo()
        {
            int line = Interface.dopOutputLine;
            Interface.clearAdditionalUI();
            Console.SetCursorPosition(Interface.dopOutputRow, line);
            Console.WriteLine(name);
            line++;
            Console.SetCursorPosition(Interface.dopOutputRow, line);
            Console.WriteLine(description);
            line++;
            Console.SetCursorPosition(Interface.dopOutputRow, line);
            Console.WriteLine("Type:" + type);
            line++;
            if (armor != 0)
            { Console.SetCursorPosition(Interface.dopOutputRow, line); Console.WriteLine("Armor:" + armor); line++; }
            if (damage != 0)
            { Console.SetCursorPosition(Interface.dopOutputRow, line); Console.WriteLine("Damage:" + damage); line++; }
            if (str != 0)
            { Console.SetCursorPosition(Interface.dopOutputRow, line); Console.WriteLine("Strenght:" + str); line++; }
            if (cons != 0)
            { Console.SetCursorPosition(Interface.dopOutputRow, line); Console.WriteLine("Constitution:" + cons); line++; }
        }
    }

    public class Consumable : Item
    {

        public Consumable(string name, char image, ConsoleColor color, string description) : base(name, image, color, 0, description)
        { }

        public virtual void Effect(Player player)
        { }

        public override void ShowInfo()
        {
            int line = Interface.dopOutputLine;
            Interface.clearAdditionalUI();
            Console.SetCursorPosition(Interface.dopOutputRow, line);
            Console.WriteLine(name);
            line++;
            Console.SetCursorPosition(Interface.dopOutputRow, line);
            Console.WriteLine(description);
            line++;
        }

    }

    public class HealthPotion : Consumable
    {

        public HealthPotion(string name, ConsoleColor color, string description) : base(name, '!', color, description)
        { }

        public override void Effect(Player player)
        {
            player.ChangeHp((int)(player.maxHp * 0.75));
        }

    }

    public static class SessionManager
    {
        public static List<Enemy> enemies = new List<Enemy>();
        public static Player player;
        static int level = 1;
        public static bool appExit;

        public static void StartSession()
        {
            appExit = false;
            player = new Player();
            Console.CursorVisible = false;
            Console.SetWindowPosition(0, 15);
            Console.SetWindowSize(160, 46);
            MapManager.CreateBase();
            MapManager.GenerateDungeon();
            player.Deploy(MapManager.cellMap[0, 0].pivot.x + 3, MapManager.cellMap[0, 0].pivot.y + 3);
            MapManager.Decorate();
            Interface.drawContainers();
            Update();
        }

        public static void NextLevel()
        {
            level++;
            MapManager.CreateBase();
            MapManager.GenerateDungeon();
            player.Deploy(MapManager.cellMap[0, 0].pivot.x + 3, MapManager.cellMap[0, 0].pivot.y + 3);
            Console.Clear();
            enemies.Clear();
            MapManager.Decorate();
            Interface.drawContainers();
            Update();
        }

        public static void Update()
        {

            while (true)
            {
                player.doneTurn = false;
                MapManager.FOV(new Point(player.x, player.y));
                player.writePlayerStatus();
                player.Controller();
                if (appExit)
                    break;
                if (player.doneTurn)
                {
                    if (!Interface.dopInterfaceIsEmpty)
                    {
                        Interface.clearAdditionalUI();
                        Interface.dopInterfaceIsEmpty = true;
                    }
                    for (int i = 0; i < enemies.Count; i++)
                    {
                        if (enemies[i].InRange(new Point(player.x, player.y)))
                            enemies[i].MoveTo(new Point(player.x, player.y));
                        if (enemies[i].alive == false)
                        {
                            enemies.RemoveAt(i);
                        }
                    }

                }

            }
        }
    }


    static void Main()
    {
        Console.Title = "Game";
        SessionManager.StartSession();
        if (!SessionManager.player.alive)
        {
            Console.ReadKey();
            Console.Clear();
            string[] image = File.ReadAllLines("Tombstone.TXT");
            Console.ForegroundColor = ConsoleColor.Green;
            for (int i = 0; i < image.Length; i++)
            {
                if (i == image.Length - 1)
                    Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(image[i]);
            }
            ConsoleKey input = Console.ReadKey().Key;
            if (input == ConsoleKey.R)
            {
                Console.Clear();
                Main();
            }
        }
    }
}