using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Windows.Forms;

namespace SpaceHoovy
{
    public partial class Form1 : Form
    {
        Game g;
        Bitmap bm = new Bitmap(1000, 1000);
        Graphics bmG;

        public Form1()
        {
            g = new Game(this);
            DoubleBuffered = true;

            Timer t = new Timer();
            t.Interval = 10;
            t.Tick += (sender, args) => Invalidate();
            t.Start();

        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            g.Update(e.Graphics);
        }

        class Game
        {
            int level = 1;
            int lives = 3;
            ConcurrentBag<GameObject> objects;
            Form ui;
            GameObject heavy;
            public Game(Form form)
            {
                objects = new ConcurrentBag<GameObject>();
                ui = form;

                heavy = new Heavy(ui);
                AddObject(new Spy((int)ui.Width - 50, 50, this));
                AddObject(heavy); 
                

            }

            public void Update(Graphics graphics)
            {

                graphics.Clear(Color.Black);
                objects.ToList().ForEach(e =>
                {
                    if (!(e is Heavy) && e.Collision(heavy))
                    {
                        ConcurrentBag<GameObject> temp = new ConcurrentBag<GameObject>();
                        objects.ToList().ForEach(i => { if(i!=e) temp.Add(i); });
                        objects = temp;
                        lives--;
                    }
                    UpdateUI(graphics);
                    e.Draw(graphics);
                });
            }

            void UpdateUI(Graphics graphics)
            {
                graphics.DrawString(lives + " lives", new Font(FontFamily.GenericMonospace, 10), new SolidBrush(Color.White), 0, 10);
                graphics.DrawString("level " + level, new Font(FontFamily.GenericMonospace, 10), new SolidBrush(Color.White), graphics.VisibleClipBounds.Width - 70, 10);
            }

            public void AddObject(GameObject go) => objects.Add(go);


        }

        class Knife : GameObject
        {
            int X, Y;
            string path = "Resources/knife.png";
            Image image;
            int height, width;
            public Knife(int x, int y)
            {
                height = 20;
                width = 20;
                image = Image.FromFile(path);
                X = x;
                Y = y;
            }

            public void Draw(Graphics g)
            {
                X -= 2;
                g.DrawImage(image, new Rectangle(new Point(X, Y), new Size(25, 25)));
            }

            public Rectangle GetBounds()
            {
                return new Rectangle(X, Y, width, height);
            }

            public bool Collision(GameObject check)
            {
                return check.GetBounds().Contains(GetBounds());
            }
        }



        class Heavy : GameObject
        {
            int X, Y;
            string path = "Resources/hoovey.png";
            Image image;
            Form form;
            int height, width;

            public Heavy(Form f)
            {
                height = 50;
                width = 50;

                form = f;
                form.MouseMove += (sender, args) =>
                {
                    X = 10;
                    Y = args.Y;
                };
                image = Image.FromFile(path);

            }

            public void Draw(Graphics g)
            {
                g.DrawImage(image, new Rectangle(new Point(X, Y), new Size(50, 50)));
            }

            public Rectangle GetBounds()
            {
                return new Rectangle(X, Y, width, height);
            }

            public bool Collision(GameObject check)
            {
                return check.GetBounds().Contains(GetBounds());
            }
        }
        

        class Spy : GameObject
        {
            int X, Y;
            string path = "Resources/spy.png";
            Image image;
            Random movement;
            Game game;
            int height, width;

            public Spy(int x, int y, Game g)
            {
                height = 50;
                width = 50;
                game = g;
                movement = new Random();
                image = Image.FromFile(path);
                X = x;
                Y = y;
            }

            public void Draw(Graphics g)
            {

                if (movement.Next(20) == 1) game.AddObject(new Knife(X, Y));
                Y = movement.Next(0, (int)g.VisibleClipBounds.Height);
                g.DrawImage(image, new Rectangle(new Point(X, Y), new Size(height, width)));
            }

            public Rectangle GetBounds()
            {
                return new Rectangle(X, Y, width, height );
            }

            public bool Collision(GameObject check)
            {
                return check.GetBounds().Contains(GetBounds());
            }
        }

        interface GameObject
        {
            void Draw(Graphics g);
            Rectangle GetBounds();

            bool Collision(GameObject check);
        }
    }
}
