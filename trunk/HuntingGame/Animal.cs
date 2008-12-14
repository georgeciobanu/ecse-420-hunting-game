using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Drawing;

namespace HuntingGame
{        
    public delegate void WanderDelegate(Animal animal);            
    public delegate void EvadeOrHuntDelegate(Animal animal);

    public class Animal
    {

        #region Fields

        protected MutexTable _table = null;
        protected Thread _thread = null;
        protected static WanderDelegate _wander = null;
        protected static EvadeOrHuntDelegate _evadeOrHunt = null;
        protected int _range = 0;
        protected int _x = -1, _y = -1;
        protected bool _captured = false;
        protected Ellipse _symbol;
        protected int _tickTimeMsec;
        protected Mutex _ownedWolfMutex = null, _ownedSheepMutex = null;
        protected bool _endSignal = false;

        #endregion

        #region Class Methods
        public Animal(MutexTable table, int range, int tickTimeMsec,
            WanderDelegate wander, EvadeOrHuntDelegate evadeOrHunt)
        {
            _thread = new Thread(doBehaviour);
            _thread.SetApartmentState(ApartmentState.STA);

            if (table == null)
                throw new NullReferenceException();

            _table = table;

            _tickTimeMsec = tickTimeMsec;

            if (range <= 0)
                throw new InvalidOperationException();

            _range = range;

            if (evadeOrHunt == null || wander == null)
                throw new InvalidOperationException();

            _evadeOrHunt = evadeOrHunt;
            _wander = wander;
          
            _symbol = new Ellipse();
            Dispatcher.FromThread(_table.WindowThread).BeginInvoke(DispatcherPriority.Render, (Action)(() =>
            {
                _table.DrawingGrid.Children.Add(_symbol);            
            }
            ));            
        }
      
        public void activate(int x, int y)
        {
            _x = x;
            _y = y;

            Draw();            
            
            _thread.Start();
        }

        public void stop()
        {
            _endSignal = true;
        }

        private void doBehaviour()
        {
            //Make sure nobody is in the cell first
            //This code is just used to initialize the animal when it is placed
            if (this is Sheep)
            {
                CurrentCell.SheepMutex.WaitOne();
                _ownedSheepMutex = CurrentCell.SheepMutex;
            }

            if (this is Wolf)
            {
                CurrentCell.WolfMutex.WaitOne();
                _ownedWolfMutex = CurrentCell.WolfMutex;
            }
 
            while (!_captured)
            {
                if (_endSignal)
                {
                    ClearState();
                    return;
                }

                if (_hasTarget())
                {
                    _evadeOrHunt(this);
                    if (_endSignal)
                    {
                        ClearState();
                        return;
                    }
                }
                else _wander(this);

                if (_endSignal)
                {
                    ClearState();
                    return;
                }

                if (!_captured)
                {
                    Draw();
                    if (_endSignal)
                    {
                        ClearState();
                        return;
                    }
                    Thread.Sleep(_tickTimeMsec);
                }
            }
        }

        private void ClearState()
        {
            if (_ownedSheepMutex != null)
                CurrentCell.SheepMutex.ReleaseMutex();

            if (_ownedWolfMutex != null)
                CurrentCell.WolfMutex.ReleaseMutex();
        } 
        #endregion Class Methods

        #region Virtual functions
        public virtual bool waitMove(Direction dir, int timeout)
        {
            throw new NotImplementedException();
            //return false; 
        }

        protected virtual bool _hasTarget()
        {
            return SharedView.Count > 0;
        }

        protected void Draw()
        {            
            Dispatcher.FromThread(_table.WindowThread).BeginInvoke(DispatcherPriority.Render, (Action)(() =>
            {                
                Grid.SetColumn(_symbol, _x);
                Grid.SetRow(_symbol, _y);                   
            }
            ));

            //ellipse.DataContext = _cells[row, column];
            //ellipse.Style = Resources["lifeStyle"] as Style;
        }

        public bool isValidPosition(int x, int y)
        {
            if (x < 0 || x >= _table.Table.GetLength(0))
                return false;

            if (y < 0 || y >= _table.Table.GetLength(1))
                return false;

            return true;
        }
        #endregion 

        #region Properties  
        public MutexCell CurrentCell
        {
            get { return _table.Table[_y, _x]; }
        }

        public virtual List<Point> SharedView { get { throw new NotImplementedException();} }
        public virtual List<Point> SameGroupView {get { throw new NotImplementedException();} }

        public int X
        {
            get { return _x; }            
        }

        public int Y
        {
            get { return _y; }
        }
        #endregion

    }
}
