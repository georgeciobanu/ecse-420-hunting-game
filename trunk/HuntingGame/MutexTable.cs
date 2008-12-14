using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Drawing;
using System.Threading;

namespace HuntingGame
{    
    public enum Direction {Up, Down, Left, Right};

    public class MutexTable
    {        
        private MutexCell[,] _table;        

        private int _wolfRange = 0, _sheepRange = 0;
        private int _wolfTickTimeMsec , _sheepTickTimeMsec;
        protected int _writeCountWolfView = 0, _writeCountSheepView = 0;

        private List<Wolf> _wolves;
        private List<Sheep> _sheep;

        private Grid _drawingGrid;
        private Thread _windowThread = null;

        public MutexTable(int size, int wolfRange, int sheepRange, Grid drawingGrid, int tickTime)
        {
            _table = new MutexCell[size, size];

            for (int i = 0; i < _table.GetLength(0); i++)
                for (int j = 0; j < _table.GetLength(1); j++)
                {
                    _table[i, j] = new MutexCell();
                }

            _wolfRange = wolfRange;
            _sheepRange = sheepRange;

            _wolves = new List<Wolf>();
            _sheep = new List<Sheep>();

            _drawingGrid = drawingGrid;

            for (int i = 0; i < size; i++)
            {
                _drawingGrid.ColumnDefinitions.Add(new ColumnDefinition());
                _drawingGrid.RowDefinitions.Add(new RowDefinition());
            }

            _sheepTickTimeMsec = _wolfTickTimeMsec = tickTime;
            _windowThread = Thread.CurrentThread;
        }

        /// <summary>
        /// Add a wolf to the grid, at the specified coordinates. This does activate the wolf.
        /// </summary>
        /// <param name="x">x coordinate on the grid</param>
        /// <param name="y">y coordinate on the grid</param>
        /// <param name="wander">Function that controls the way the wolf moves when it sees no sheep</param>
        /// <param name="evadeOrHunt">Function that controls the way the wolf moves when it sees at least one sheep</param>
        /// <returns>True is it succedes, false if there are problems. This should be changed to exceptions.</returns>
        public Wolf addWolf(int x, int y, WanderDelegate wander, EvadeOrHuntDelegate hunt)
        {
            if (_wolves.Count + _sheep.Count < _table.Length)
            {
                Wolf wolf = new Wolf(this, wander, hunt);
                _wolves.Add(wolf);
                wolf.activate(x, y);

                return wolf;
            }
            return null;
        }

        public Sheep addSheep(int x, int y, WanderDelegate wander, EvadeOrHuntDelegate evade)
        {
            if (_wolves.Count + _sheep.Count < _table.Length)
            {
                Sheep sheep = new Sheep(this, wander, evade);
                _sheep.Add(sheep);
                sheep.activate(x, y);

                return sheep;
            }
            return null;
        }         

        public void Stop()
        {
            //Stop all threads
            for (int i = 0; i < _wolves.Count; i++)
			{
                _wolves.ElementAt(i).stop();
			}


            for (int i = 0; i < _sheep.Count; i++)
            {
                _sheep.ElementAt(i).stop();
            }
            Thread.Sleep(1500);
        }

        public MutexCell[,] Table
        {
            get { return _table; }
            set { _table = value; }
        }

        public int WolfRange
        {
            get { return _wolfRange; }
            set { _wolfRange = value; }
        }

        public int SheepRange
        {
            get { return _sheepRange; }
            set { _sheepRange = value; }
        }

        public Grid DrawingGrid
        {
            get { return _drawingGrid; }            
        }

        public int WolfTickTimeMsec
        {
            get { return _wolfTickTimeMsec; }
            set { _wolfTickTimeMsec = value; }
        }

        public int SheepTickTimeMsec
        {
            get { return _sheepTickTimeMsec; }
            set { _sheepTickTimeMsec = value; }
        }

        public List<Wolf> Wolves
        {
            get { return _wolves; }            
        }

        public List<Sheep> Sheep
        {
            get { return _sheep; }
        }

        public Thread WindowThread
        {
            get { return _windowThread; }
        }

    }
}
