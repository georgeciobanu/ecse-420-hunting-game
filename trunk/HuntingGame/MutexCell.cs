using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace HuntingGame
{
    public class MutexCell
    {        
        Mutex _wolfMutex = new Mutex();
        Mutex _sheepMutex = new Mutex();

        Sheep _currentSheep = null;
        Wolf _currentWolf = null;
    
        public Mutex WolfMutex
        { 
            set{_wolfMutex = value;}
            get{return _wolfMutex; }
        }

        public Mutex SheepMutex
        { 
            set{_sheepMutex = value;}
            get{return _sheepMutex; }
        }

        public Wolf CurrentWolf
        {
            set { _currentWolf = value; }
            get { return _currentWolf; }
        }
        public Sheep CurrentSheep
        {
            set { _currentSheep = value; }
            get { return _currentSheep; }
        }
    }         

}
