using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FutarokViadala
{
    class Program
    {
        public const int SEC = 100;
        static void Main(string[] args)
        {
            Etterem etterem = new Etterem();
            List<Futar> fs = new List<Futar>();
            fs.AddRange(Enumerable.Range(0, 4)
                .Select(i => new FurgeFutar()));
            fs.AddRange(Enumerable.Range(0, 4)
                .Select(i => new TurboTeknos()));

            List<Task> ts = new List<Task>();
            ts.Add(new Task(() => {
                etterem.Work();
            }, TaskCreationOptions.LongRunning));
            ts.AddRange(fs.Select(x => new Task(() => {
                x.Work(etterem);
            }, TaskCreationOptions.LongRunning)));

            ts.Add(new Task(() => {
                int fps = 50;
                int idx = 0;
                while(fs.Any(x => x.Allapot != FutarStatusz.Hazament))
                {
                    Thread.Sleep(fps);
                    Console.Clear();
                    Console.WriteLine("Eltelt idő: " + (idx * fps) / SEC);
                    Console.WriteLine(etterem);
                    foreach (var f in fs)
                        Console.WriteLine(f);
                    idx++;
                }
                Console.Clear();
                Console.WriteLine("Eltelt idő: " + (idx * fps) / SEC);
                Console.WriteLine(etterem);
                foreach (var f in fs)
                    Console.WriteLine(f + $"; pénze: {f.Penz}");
            }, TaskCreationOptions.LongRunning));

            ts.ForEach(x => x.Start());

            Console.ReadLine();
        }
    }


    class Etterem
    {
        public List<Rendeles> Rendelesek { get; private set; }
        public int ElkeszitettRendelesek { get; private set; }
        public bool Dolgozik { get; private set; }

        public object RendelesekLock = new object();

        public Etterem()
        {
            Rendelesek = new List<Rendeles>();
            Dolgozik = true;
        }

        public void Work()
        {
            for (int i = 0; i < 100; i++)
            {
                Thread.Sleep(Util.rnd.Next(1 * Program.SEC, 5 * Program.SEC));
                
                lock (RendelesekLock)
                    Rendelesek.Add(new Rendeles() {
                        Ertek = Util.rnd.Next(2000, 10001),
                        Tavolsag = Util.rnd.Next(500, 10001)
                    });
                ElkeszitettRendelesek = i;
            }
            Dolgozik = false;
        }

        public override string ToString()
        {
            return $"Elkészített rendelések: {ElkeszitettRendelesek}, futárra vár: {Rendelesek.Count}, " +
                (Dolgozik ? "dolgoznak" : "már nem dolgoznak");
        }
    }

    class Rendeles
    {
        public int Ertek { get; set; }
        public int Tavolsag { get; set; }
    }

    enum FutarStatusz { Varakozik, Valaszt, Kiszallit, Atad, Visszaut, Hazament }
    abstract class Futar
    {
        public int Penz { get; private set; }
        public FutarStatusz Allapot { get; private set; }
        public int ID { get; private set; }
        static int _id = 0;

        public Futar()
        {
            ID = ++_id;
            Allapot = FutarStatusz.Varakozik;
        }

        public void Work(Etterem e)
        {
            while (e.Dolgozik || e.Rendelesek.Count > 0)
            {
                Rendeles r = null;
                lock (e.RendelesekLock)
                {
                    Allapot = FutarStatusz.Valaszt;
                    if (e.Rendelesek.Count > 0)
                        r = Valasztas(e.Rendelesek);
                }
                if (r != null)
                { 
                    int km = (int)Math.Ceiling(r.Tavolsag / 1000.0);
                    Allapot = FutarStatusz.Kiszallit;
                    Thread.Sleep(Util.rnd.Next(km * 20 * Program.SEC, km * 39 * Program.SEC) / 10);

                    Allapot = FutarStatusz.Atad;
                    Thread.Sleep(Util.rnd.Next(2 * Program.SEC, 5 * Program.SEC));

                    Allapot = FutarStatusz.Visszaut;
                    Thread.Sleep(Util.rnd.Next(km * 20 * Program.SEC, km * 39 * Program.SEC) / 10);
                    Penz += Fizetseg(r);
                    r = null;
                    Allapot = FutarStatusz.Varakozik;
                }
                else
                {
                    Allapot = FutarStatusz.Varakozik;
                    Thread.Sleep(Util.rnd.Next(1 * Program.SEC, 2 * Program.SEC));
                }
            }
            Allapot = FutarStatusz.Hazament;
        }

        public override string ToString()
        {
            return $"{ID}: {Allapot}";
        }

        protected abstract Rendeles Valasztas(List<Rendeles> ls);
        protected abstract int Fizetseg(Rendeles r);
    }

    class FurgeFutar : Futar
    {
        protected override int Fizetseg(Rendeles r)
        {
            return 600;
        }

        protected override Rendeles Valasztas(List<Rendeles> ls)
        {
            var r = ls.OrderBy(x => x.Tavolsag).First();
            ls.Remove(r);
            return r;
        }
    }
    class TurboTeknos : Futar
    {
        protected override int Fizetseg(Rendeles r)
        {
            return (int)(r.Ertek * 0.05) +
                (r.Tavolsag > 3000 ? (int)Math.Ceiling((r.Tavolsag - 3000) / 1000.0) * 200 : 0);
        }

        protected override Rendeles Valasztas(List<Rendeles> ls)
        {
            var r = ls.OrderByDescending(x => x.Tavolsag).First();
            ls.Remove(r);
            return r;
        }
    }

    static class Util
    {
        public static Random rnd;

        static Util()
        {
            rnd = new Random();
        }
    }
}
