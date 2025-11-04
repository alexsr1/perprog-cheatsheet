using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _02_MonitorWithSomeFixes
{
    class Program
    {
        //ebben a megoldasban is Monitort hasznalok, tovabbi javitasokkal
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
                while (latogatok.Any(l => l.Status != "Vegzett"))   //legyen vege a naplozo szalnak
                {
                    Console.Clear();
                    foreach (var t in Tagozat.osszesTagozat)
                        Console.WriteLine($"{t.ID}: {t.AktualisEloado}, {t.Status}, latogatok: {t.Latogatok.Count}");
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
                    Console.WriteLine($"{t.ID}: {t.AktualisEloado}, {t.Status}, latogatok: {t.Latogatok.Count}");
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
        public List<Latogato> Latogatok { get; private set; }
        public List<Latogato> MindenLatogatok { get; private set; }
        public object lockObject = new object();

        public Tagozat()
        {
            ID = _nextID++;
            Status = TagozatStatus.Init;
            osszesTagozat.Add(this);
            Latogatok = new List<Latogato>();
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
            //for (int i = 0; i < 5; i++)   //fix 5 fordulo helyett addig forognak, amig vannak elo tagozatok
            while (Tagozat.osszesTagozat.Any(x => x.Status != TagozatStatus.Finished))
            {
                //Tagozat t = Tagozat.osszesTagozat.OrderBy(x => Util.rnd.Next(1, 100)).First();
                //csak azokbol a tagozatokbol valasszon ahol meg van eloadas

                Tagozat t = Tagozat.osszesTagozat.Where(x => x.Status != TagozatStatus.Finished)
                    .OrderBy(x => Util.rnd.Next(1, 100))
                    .FirstOrDefault();

                //kozben mar lehet hogy nincs ilyen
                if (t == null)
                    break;   //ciklus vege

                Status = $"{t.ID} tagozatba szeretne belepni";

                lock (t.lockObject)
                {
                    //itt a kieheztetes ellen egyszeru - mondjuk nem is tokeletes - taktika: ha felebresztenek, de nem ferunk be, keresunk mast
                    bool voltMarEbresztve = false;
                    while (t.Status == TagozatStatus.Eloadas || t.Latogatok.Count == 10)
                    {
                        if (voltMarEbresztve)
                            break;
                        else
                        {
                            Monitor.Wait(t.lockObject);
                            voltMarEbresztve = true;
                        }
                    }
                    if (t.Status == TagozatStatus.Eloadas || t.Latogatok.Count == 10)
                        continue;   //kulso while lep, azaz masik t tagozatot keres, jo esellyel oda be is fer
                    t.Latogatok.Add(this);
                    if (!t.MindenLatogatok.Contains(this))
                        t.MindenLatogatok.Add(this);
                }

                Erdeklodes = 100;
                while (Erdeklodes > 0 && t.Status != TagozatStatus.Finished)  //ha a tagozat kozben veget er csak ki kellene menni :)
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
                    t.Latogatok.Remove(this);
					Monitor.Pulse(t.lockObject);    //kilepeskor max egy varakozo felebresztese
                }
            }
            Status = $"Vegzett";
        }
    }
}
