using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HuntingGame;
using System.Drawing;


namespace TheCoolTool
{
    using SharedView = Dictionary<MutexCell, System.Drawing.Point>;
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        int targetx, targety;
        int sheepx, sheepy;
        int wolfx, wolfy;
        int shortestd, d = 0;

        MutexTable tb = null;

        public Window1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //tb = new MutexTable(50, 9, 5,grid);
            grid.ShowGridLines = true;
            tb.addSheep(17, 20, wanderSheep, escape);
            tb.addSheep(16, 24, wanderSheep, escape);
            tb.addSheep(18, 21, wanderSheep, escape);
            tb.addSheep(21, 24, wanderSheep, escape);
            tb.addSheep(17, 27, wanderSheep, escape);
            tb.addSheep(19, 23, wanderSheep, escape);
            tb.addWolf(10, 0, wanderWolf, hunt);
            tb.addWolf(12, 8, wanderWolf, hunt);
            tb.addWolf(17, 6, wanderWolf, hunt);
            tb.addWolf(21, 0, wanderWolf, hunt);
            tb.addWolf(24, 2, wanderWolf, hunt);
        }

        private void wanderWolf(Animal animal)
        {
            int friendx, friendy;
            int closex, closey;
            int fdist = 0;
            Direction fd = Direction.Down;

            friendx = friendy = 0;
            closex = 0;
            closey = 0;
            d = 0;
                foreach (System.Drawing.Point an in animal.SharedView)
                {
                    sheepx = sheepy = wolfx = wolfy = 0;
                    sheepx = an.X;
                    sheepy = an.Y;

                    //there is a sheep


                    wolfx = animal.X;
                    wolfy = animal.Y;
                    d = Math.Abs(wolfx - friendx);

                    //get shortestd distance to a friend
                    if (fdist == 0)
                    {
                        fdist = d;
                        closex = friendx;
                        closey = friendy;
                    }
                    else if (d < fdist)
                    {
                        fdist = d;
                        closex = friendx;
                        closey = friendy;
                    }
                
                }

            if (fdist >= 3)
            {
                if (closex - wolfx < 0 && Math.Abs(closex - wolfx) >= 3)
                    fd = Direction.Left;
                else if (closex - wolfx > 0 && Math.Abs(closex - wolfx) >= 3)
                    fd = Direction.Right;
                else if (closey - wolfy < 0 && Math.Abs(closey - wolfy) >= 3)
                    fd = Direction.Down;
                else
                    fd = Direction.Up;
            }
            else
                fd = Direction.Down;

            animal.waitMove(fd, 300);
        }

        private void hunt(Animal animal)
        {
            if (animal is Wolf)
            {
                shortestd = d = 0;

                //look at all the sheep and find the closest one
                foreach (System.Drawing.Point an in animal.SharedView)
                {
                    sheepx = sheepy = wolfx = wolfy = 0;
                    sheepx = an.X;
                    sheepy = an.Y;

                    //there is a sheep


                    wolfx = animal.X;
                    wolfy = animal.Y;
                    d = Math.Abs(wolfx - sheepx) + Math.Abs(wolfy - sheepy);
                    if (shortestd > d && d != 0)
                    {
                        shortestd = d;
                        targetx = sheepx;
                        targety = sheepy;
                    }

                }

                if (sheepy > wolfy)
                    animal.waitMove(Direction.Down, 300);
                else if (sheepy < wolfy)
                    animal.waitMove(Direction.Up, 300);
                else if (sheepx < wolfx)
                    animal.waitMove(Direction.Left, 300);
                else if (sheepx > wolfx)
                    animal.waitMove(Direction.Right, 300);
            }
            else
                escape(animal);
        }

        public void escape(Animal animal)
        {
            int enemyx, enemyy;
            int closex, closey;
            int sheepx, sheepy;
            int fdist = 0;
            Direction fd = Direction.Up;

            enemyx = enemyy = 0;
            closex = closey = 0;
            sheepx = sheepy = 0;
            d = 0;

            foreach (System.Drawing.Point an in animal.SharedView)
            {
                sheepx = sheepy = wolfx = wolfy = 0;
                sheepx = an.X;
                sheepy = an.Y;

                //there is a sheep


                wolfx = animal.X;
                wolfy = animal.Y;
                sheepx = animal.X;
                sheepy = animal.Y;
                d = Math.Abs(sheepx - enemyx);// + Math.Abs(sheepy - enemyy);

                //get closest wolf
                if (fdist == 0)
                {
                    fdist = d;
                    closex = enemyx;
                    closey = enemyy;
                }
                else if (d < fdist)
                {
                    fdist = d;
                    closex = enemyx;
                    closey = enemyy;
                }


                else if (sheepx == 0)
                    animal.waitMove(Direction.Down, 300);
                else if (sheepy == 0)
                    animal.waitMove(Direction.Down, 300);
            }

            if (fdist <= 5)
            {
                if (closey - sheepy <= 0)
                    fd = Direction.Down;
                if (closey - sheepy > 0)
                    fd = Direction.Up;
                else if (closex - sheepx <= 0)
                    fd = Direction.Right;
                else if (closex - sheepx > 0)
                    fd = Direction.Left;
                else if (sheepx == 0)
                    fd = Direction.Down;
                else if (sheepy == 0)
                    fd = Direction.Down;
            }
            else
                fd = Direction.Up;

            animal.waitMove(fd, 300);
        }

        private void wanderSheep(Animal animal)
        {
            int friendx, friendy;
            int closex, closey;
            int fdist = 0;
            Direction fd = Direction.Up;

            friendx = friendy = 0;
            closex = 0;
            closey = 0;
            d = 0;

            foreach (System.Drawing.Point an in animal.SharedView)
            {
                sheepx = sheepy = wolfx = wolfy = 0;
                sheepx = an.X;
                sheepy = an.Y;

                //there is a sheep


                wolfx = animal.X;
                wolfy = animal.Y;
                d = Math.Abs(sheepx - friendx);

                //get shortestd distance to a friend
                if (fdist == 0)
                {
                    fdist = d;
                    closex = friendx;
                    closey = friendy;
                }
                else if (d < fdist)
                {
                    fdist = d;
                    closex = friendx;
                    closey = friendy;
                }

            }

            if (fdist >= 2)
            {
                if (closey - sheepy < 0)
                    fd = Direction.Up;
                else if (closey - sheepy > 0)
                    fd = Direction.Down;
                else if (closex - sheepx < 0)
                    fd = Direction.Left;
                else if (closex - sheepx > 0)
                    fd = Direction.Right;
            }
            else
                fd = Direction.Up;

            animal.waitMove(fd, 300);
        }

        private void wander(Animal animal)
        {
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            tb.Stop();
        }

        private void button1_Click_1(object sender, RoutedEventArgs e)
        {
            tb.addWolf(20, 15, wanderWolf, hunt);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tb = new MutexTable(50, 9, 5, grid, 300);
            grid.ShowGridLines = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            tb.Stop();
        }

        private void button2_Click_1(object sender, RoutedEventArgs e)
        {
            tb.addSheep(15, 40, wanderSheep, escape);
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            grid.ShowGridLines = true;
            //tb = new MutexTable(50, 9, 5, grid);
            grid.ShowGridLines = true;
            tb.addSheep(12, 45, wanderSheep, escape);
            tb.addSheep(10, 49, wanderSheep, escape);
            tb.addSheep(15, 46, wanderSheep, escape);
            tb.addSheep(21, 39, wanderSheep, escape);
            tb.addSheep(14, 42, wanderSheep, escape);
            tb.addSheep(19, 48, wanderSheep, escape);
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            grid.ShowGridLines = true;
            tb.addWolf(14, 0, wanderWolf, hunt);
            tb.addWolf(16, 8, wanderWolf, hunt);
            tb.addWolf(21, 6, wanderWolf, hunt);
            tb.addWolf(25, 0, wanderWolf, hunt);
            tb.addWolf(28, 2, wanderWolf, hunt);
        }


    }
}
