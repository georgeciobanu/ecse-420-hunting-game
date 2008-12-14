using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using HuntingGame.Properties;
using System.Drawing;
using System.Windows.Threading;
using System.Diagnostics;

namespace HuntingGame
{
    public class Sheep : Animal
    {
        private bool _gotcha = false;

        public Sheep(MutexTable table, WanderDelegate wander, EvadeOrHuntDelegate evadeOrHunt)
               :base(table, table.SheepRange, table.SheepTickTimeMsec, wander, evadeOrHunt)
        {
            //do nothing for now
            _symbol.Height = 10;
            _symbol.Width = 10;
            _symbol.Stroke = System.Windows.Media.Brushes.Red;
            _symbol.Fill = System.Windows.Media.Brushes.Red;
            _symbol.StrokeThickness = 2;
        }

        public override bool waitMove(Direction dir, int timeoutMilliseconds)
        {
            if (_captured)
                return false;

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

            bool iMustDie = false;
            if (_gotcha)
                iMustDie = true;

            if (!CurrentCell.WolfMutex.WaitOne(0, false))
            { //if there is a wolf here                
                iMustDie = true;
            }
            else CurrentCell.WolfMutex.ReleaseMutex();

            if (CurrentCell.CurrentWolf != null)
                iMustDie = true;

            if (isValidPosition(y, x))
            {
                bool gotNewMutex = false;
                if (_ownedSheepMutex != _table.Table[y, x].SheepMutex)
                {
                    gotNewMutex = _table.Table[y, x].SheepMutex.WaitOne(timeoutMilliseconds, false);                    
                }

                if (gotNewMutex)  //Need to move
                {
                    //We release the old one first
                    //So we "hold" two cells at a time during the transition
                    //But this is OK, as even in the physical world, this problem exists                    
                    //_ownedSheepMutex.ReleaseMutex();
                    CurrentCell.SheepMutex.ReleaseMutex();
                    CurrentCell.CurrentSheep = null;

                    _ownedSheepMutex = _table.Table[y, x].SheepMutex;
                    Debug.Assert(_ownedSheepMutex != null);
                    _table.Table[y, x].CurrentSheep = this;

                    //We can do this as a thread has to acquire the mutex before changing the CurrentSheep
                    //If a wolf is here, we need to release the newly acquired cell
                    //And end the thread

                    _x = x;
                    _y = y;                    
                }                
            }
            

            if (iMustDie)
            {
                //This is the end
                _captured = true;

                _ownedSheepMutex.ReleaseMutex();             
                _ownedSheepMutex = null;
                
                Dispatcher.FromThread(_table.WindowThread).BeginInvoke(DispatcherPriority.Render, (Action)(() =>
                {
                    //_symbol.Visibility = Visibility.Hidden; -> Very interesting bug                                        
                    _table.DrawingGrid.Children.Remove(_symbol);                    
                    _table.addWolf(_x, _y, _wander, _evadeOrHunt);
                    //_table.DrawingGrid.UpdateLayout();
                 _table.Sheep.Remove(this);

                 if (_table.Sheep.Count == 0)
                 {
                     _table.Stop();
                     MessageBox.Show("Game over! No sheep left.");
                 }
                }
                ));


                //_symbol = null;
                

                return false;
            }
            return true;
        }

        public void sendToHeaven()
        {
            _gotcha = true;            
        }

        public override List<System.Drawing.Point> SharedView
        {
            get
            {
                //TODO: both these functions could have easily been unified with Generics
                List<System.Drawing.Point> ret = new List<System.Drawing.Point>(_table.Wolves.Count);

                try
                {
                    foreach (Sheep sheep in _table.Sheep)
                    {
                        List<Wolf> filtered = _table.Wolves.FindAll(el =>
                            (Math.Abs(el.X - sheep.X) <= _range) || (Math.Abs(el.Y - sheep.Y) <= _range));

                        //This could/should be done lazily ?
                        foreach (Wolf wolf in filtered)
                        {
                            ret.Add(new System.Drawing.Point(wolf.X, wolf.Y));
                        }
                    }
                }
                catch (Exception ex) { };
                return ret;
            }
        }

        public override List<System.Drawing.Point> SameGroupView
        {
            get
            {
                List<System.Drawing.Point> ret = new List<System.Drawing.Point>(_table.Sheep.Count);

                try
                {
                    foreach (Sheep sheep in _table.Sheep)
                    {
                        ret.Add(new System.Drawing.Point(sheep.X, sheep.Y));
                    }
                }
                catch (Exception ex) { };

                return ret;
            }
        }
    }
}
