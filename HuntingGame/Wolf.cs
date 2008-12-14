using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Drawing;


namespace HuntingGame
{
    public class Wolf: Animal
    {

        private int _sheepEaten = 0;

        public Wolf(MutexTable table,
            WanderDelegate wander, EvadeOrHuntDelegate evadeOrHunt): 
            base (table, table.WolfRange, table.WolfTickTimeMsec, wander, evadeOrHunt)
        {
            _symbol.Height = 12;
            _symbol.Width = 12;
            _symbol.Fill= System.Windows.Media.Brushes.Blue;
            _symbol.StrokeThickness = 2;
        }
                
        public override bool waitMove(Direction dir, int timeoutMilliseconds)
        {
            int x = -1, y = -1;
            switch (dir)
            {
                case Direction.Up: x = _x; y = _y - 1;
                    break;
                case Direction.Down: x = _x; y = _y + 1;
                    break;
                case Direction.Left: x = _x - 1; y = _y;
                    break;
                case Direction.Right: x = _x + 1; y = _y;
                    break;
                default: return false;                    
            }

            Sheep labChomps = _table.Table[y, x].CurrentSheep;
            if (labChomps != null)
            {
                labChomps.sendToHeaven(); //i.e. eat the sheep
                _sheepEaten++;

                //We should not call table.addWolf as that would call WaitOne on a mutex we already own
                //Wolf wolf = new Wolf(_table, _wander, _evadeOrHunt);                                                
            }

            if (!isValidPosition(x, y))
                return false;



            if (_ownedWolfMutex != _table.Table[y, x].WolfMutex)
                if (_table.Table[y, x].WolfMutex.WaitOne(timeoutMilliseconds, false))
                {
                    _ownedWolfMutex = _table.Table[y, x].WolfMutex;
                    _table.Table[y, x].CurrentWolf = this;

                    //We can do this as a thread has to acquire the mutex before changing the CurrentWolf                    
                    CurrentCell.WolfMutex.ReleaseMutex();
                    CurrentCell.CurrentWolf = null;

                   

                    _x = x;
                    _y = y;

                    //Before leaving a square a sheep will probe the WolfMutex
                    //If it is taken, it will stay there - need to make sure this cannot cause deadlock                    
                    labChomps = _table.Table[y, x].CurrentSheep;
                    if ( labChomps != null)
                    {
                        labChomps.sendToHeaven(); //i.e. eat the sheep
                        _sheepEaten++;

                        //We should not call table.addWolf as that would call WaitOne on a mutex we already own
                        //Wolf wolf = new Wolf(_table, _wander, _evadeOrHunt);                                                
                    }
                    return true;
                }                                
            return false;
        }

      
        public override List<System.Drawing.Point> SharedView
        {
            get
            {
                List<System.Drawing.Point> ret = new List<System.Drawing.Point>(_table.Wolves.Count);

                //This could/should be done lazily ?
                try
                {
                    foreach (Wolf wolf in _table.Wolves)
                    {
                        List<Sheep> filtered = _table.Sheep.FindAll(el =>
                        (Math.Abs(el.X - wolf.X) <= _range) || (Math.Abs(el.Y - wolf.Y) <= _range));

                        foreach (Sheep sheep in filtered)
                        {
                            ret.Add(new System.Drawing.Point(sheep.X, sheep.Y));
                        }
                    }
                    
                }
                catch (Exception ex) { };//Do nothing if there are errors
                return ret;
            }
        }

        public override List<System.Drawing.Point> SameGroupView
        {
            get
            {
                List<System.Drawing.Point> ret = new List<System.Drawing.Point>(_table.Wolves.Count);

                //This could/should be done lazily ?
                try
                {
                    foreach (Wolf wolf in _table.Wolves)
                    {
                        ret.Add(new System.Drawing.Point(wolf.X, wolf.Y));
                    }
                }
                catch (Exception ex) { }; //do nothing

                return ret;
            }
        }
    }
}