using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _03_Semaphore
{
    class Program
    {
        //ebben a megoldasban Szemafort hasznalok - merthogy tobb latogato is lehet ugyanabban a tagozatban
        //FYI: további megoldások lehetségesek szálbiztos adatszerkezetekkel is
        static void Main(string[] args)
        {
            Enumerable.Range(1, 5)
                .Select(i => new Tagozat())
                .Select(t => new Task(() => {
                    t.DoWork();
                }, TaskCreationOptions.LongRunning))
                .ToList()
                .ForEach(t => t.Start());

            List<Latogato> latogatok = Enumerable.Range(1, 30)
                .Select(i => new Latogato())
                .ToList();

            latogatok.Select(l => new Task(() => {
                l.DoWork();
            }, TaskCreationOptions.LongRunning))
                .ToList()
                .ForEach(t => t.Start());

            new Task(() => {
                while (latogatok.Any(l => l.Status != "Vegzett"))
                {
                    Console.Clear();
                    foreach (var t in Tagozat.osszesTagozat)
                        Console.WriteLine($"{t.ID}: {t.AktualisEloado}, {t.Status}, latogatok: {10 - t.sem.CurrentCount}");
                    foreach (var l in latogatok)
                    {
                        if (l.ID > 15)
                            Console.SetCursorPosition(40, 4 + l.ID - 15);
                        Console.WriteLine($"{l.ID}: {l.Status}");
                    }
                    Console.WriteLine();
                    Console.WriteLine("Az egyedi latogatasok szama osszesen: " + Tagozat.osszesTagozat.Select(t => t.MindenLatogatok.Count).Sum());
                    Thread.Sleep(100);
                }
                Console.Clear();
                foreach (var t in Tagozat.osszesTagozat)
                    Console.WriteLine($"{t.ID}: {t.AktualisEloado}, {t.Status}, latogatok: {10 - t.sem.CurrentCount}");
                foreach (var l in latogatok)
                {
                    if (l.ID > 15)
                        Console.SetCursorPosition(40, 4 + l.ID - 15);
                    Console.WriteLine($"{l.ID}: {l.Status}");
                }
                Console.WriteLine();
                Console.WriteLine("Az egyedi latogatasok szama osszesen: " + Tagozat.osszesTagozat.Select(t => t.MindenLatogatok.Count).Sum());
                Console.WriteLine("Szimuláció vége!");
            }, TaskCreationOptions.LongRunning).Start();

            Console.ReadLine();
        }
    }

    static class Util
    {
        public static Random rnd = new Random();
    }

    enum TagozatStatus { Init, EloadoFelkeszul, Eloadas, Diszkusszio, Finished }
    class Tagozat
    {
        public static List<Tagozat> osszesTagozat = new List<Tagozat>();
        public TagozatStatus Status { get; private set; }

        static int _nextID = 1;
        public int ID { get; private set; }
        public int AktualisEloado { get; private set; }
        //public List<Latogato> Latogatok { get; private set; }     //akkor ez nem kell
        public List<Latogato> MindenLatogatok { get; private set; } //szálbiztos szerkezet is lehetne, lockon kívül is hívhatnám
        public object lockObject = new object();
        public SemaphoreSlim sem = new SemaphoreSlim(10);   //10es kezdoertekrol indulo szemafor

        public Tagozat()
        {
            ID = _nextID++;
            Status = TagozatStatus.Init;
            osszesTagozat.Add(this);
            //Latogatok = new List<Latogato>();
            MindenLatogatok = new List<Latogato>();
        }

        public void DoWork()
        {
            for (int i = 1; i <= 8; i++)
            {
                AktualisEloado = i;
                Status = TagozatStatus.EloadoFelkeszul;
                Thread.Sleep(Util.rnd.Next(750, 1251));
                Status = TagozatStatus.Eloadas;
                Thread.Sleep(14000 + Util.rnd.Next(750, 1251));
                Status = TagozatStatus.Diszkusszio;
                lock (lockObject)
                    Monitor.PulseAll(lockObject);
                Thread.Sleep(Util.rnd.Next(5000, 10001));
            }
            Status = TagozatStatus.Finished;
            lock (lockObject)
                Monitor.PulseAll(lockObject);   //ha valaki az utolso diszkussziora jott volna be, de nem fert be, ne varjon a befejezett szekciora
        }
    }

    class Latogato
    {
        static int _nextID = 1;
        public int ID { get; private set; }
        public int Erdeklodes { get; private set; }
        public string Status { get; private set; }

        public Latogato()
        {
            Erdeklodes = 100;
            ID = _nextID++;
        }

        void ErdeklodesDecrement()
        {
            Erdeklodes -= Util.rnd.Next(1, Math.Min(5, Erdeklodes) + 1);
        }

        public void DoWork()
        {
            while (Tagozat.osszesTagozat.Any(x => x.Status != TagozatStatus.Finished))
            {
                Tagozat t = Tagozat.osszesTagozat.Where(x => x.Status != TagozatStatus.Finished)
                    .OrderBy(x => Util.rnd.Next(1, 100))
                    .FirstOrDefault();

                if (t == null)
                    break;   //ciklus vege

                Status = $"{t.ID} tagozatba szeretne belepni";

                lock (t.lockObject)
                {
                    while (t.Status == TagozatStatus.Eloadas)
                    {
                        Monitor.Wait(t.lockObject);
                    }
                    if (!t.MindenLatogatok.Contains(this))
                        t.MindenLatogatok.Add(this);
                }
                t.sem.Wait();

                Erdeklodes = 100;
                while (Erdeklodes > 0 && t.Status != TagozatStatus.Finished)
                {
                    Thread.Sleep(1000);
                    ErdeklodesDecrement();
                    Status = $"{t.ID} tagozatban ul (Erdeklodes = {Erdeklodes})";
                }

                Status = $"{t.ID} tagozatbol szeretne kilepni";

                lock (t.lockObject)
                {
                    while (t.Status == TagozatStatus.Eloadas)
                        Monitor.Wait(t.lockObject);
					Monitor.Pulse(t.lockObject);
                }
                t.sem.Release();
            }
            Status = $"Vegzett";
        }
    }
}
