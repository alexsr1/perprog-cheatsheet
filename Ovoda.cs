using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ovoda
{
    class Program
    {
        static void Main(string[] args)
        {
            var os = Enumerable.Range(1, 5)
                .Select(i => new Ovono(i))
                .ToList();
            var szs = Enumerable.Range(1, 40)
                .Select(i => new Szulo(i))
                .ToList();

            int ora = 0;

            var ts = os.Select(x => new Task(() =>
            {
                x.Letezik(ref ora, szs);
            }, TaskCreationOptions.LongRunning))
                .ToList();
            ts.AddRange(szs.Select(x => new Task(() =>
            {
                x.Letezik();
            }, TaskCreationOptions.LongRunning)));
            ts.Add(new Task(() => {
                int ido = 0;
                int waittime = 100;
                //while (true)
                while (!os.All(x => x.Status == OvonoStatusz.Hazament))
                {
                    Console.Clear();
                    Console.WriteLine($"Indulás óta eltelt idő: {ido/1000} perc");
                    ora = ido / 60000;
                    Console.WriteLine($"Bent lévő gyerekek száma: {szs.Count(x => x.Status != SzuloStatusz.Hazament)}");
                    foreach (var o in os)
                        Console.WriteLine(o);
                    foreach (var sz in szs.Where(x => (int)x.Status > 0 && (int)x.Status < 3))
                        Console.WriteLine(sz);
                    Thread.Sleep(waittime);
                    ido += waittime;
                }
                Console.Clear();
                Console.WriteLine("Vége!");
                Console.WriteLine($"Indulás óta eltelt idő: {ido / 1000} perc");

            }, TaskCreationOptions.LongRunning));

            ts.ForEach(x => x.Start());

            Console.ReadLine();
        }
    }

    public enum OvonoStatusz { GyerekekkelFoglalkozik, SzulotKezel, LazatMer, Hazament }
    public enum SzuloStatusz { MegOtthon, Var, Ovonovel, Hazament}

    class Ovono
    {
        public int Id { get; private set; }
        public OvonoStatusz Status { get; private set; }
        public static ConcurrentQueue<Szulo> varakozoSzulok = new ConcurrentQueue<Szulo>();
        public Ovono(int id)
        {
            Id = id;
            Status = OvonoStatusz.GyerekekkelFoglalkozik;
        }

        public void Letezik(ref int ora, List<Szulo> szulok)
        {
            int hanyszorMertEddigLazat = 0;
            //ciklikusan ismétli, hogy
            //while (!szulok.All(x => x.Status == SzuloStatusz.Hazament))
            //while (szulok.Any(x => x.Status != SzuloStatusz.Hazament))
            //while (szulok.Any(x => x.Status == SzuloStatusz.MegOtthon || x.Status == SzuloStatusz.Var))
            while (szulok.Any(x => (int)x.Status <= 1))
            {
                //TODO: óránként lázmérés
                if (ora > hanyszorMertEddigLazat)
                {
                    Status = OvonoStatusz.LazatMer;
                    Thread.Sleep(2000);
                    hanyszorMertEddigLazat++;
                    if (Util.rnd.Next(0, 100) < 2)
                    {
                        Status = OvonoStatusz.Hazament;
                        return;
                    }
                }

                //lehet hogy gyerekkel foglalkozik
                if (Util.rnd.Next(0,10) < 3)
                {
                    Status = OvonoStatusz.GyerekekkelFoglalkozik;
                    Thread.Sleep(Util.rnd.Next(1000, 5001));
                }
                

                //ha nem, ellenőrzi hogy van e várakozó szülő
                if (varakozoSzulok.TryDequeue(out Szulo sz))
                {
                    //ha van, kivesz egyet, értesíti, kezeli, értesíti
                    Status = OvonoStatusz.SzulotKezel;
                    lock (sz.lockObject)
                        Monitor.Pulse(sz.lockObject);
                    Thread.Sleep(Util.rnd.Next(2000, 8001));
                    lock (sz.lockObject)
                        Monitor.Pulse(sz.lockObject);
                    Status = OvonoStatusz.GyerekekkelFoglalkozik;
                }
                else
                {
                    Status = OvonoStatusz.GyerekekkelFoglalkozik;
                    Thread.Sleep(Util.rnd.Next(1, 101));
                }
            }
            Status = OvonoStatusz.Hazament;
        }

        public override string ToString()
        {
            return $"#{Id} óvónő: {Status}";
        }
    }

    class Szulo
    {
        public int Id { get; private set; }
        public SzuloStatusz Status { get; private set; }
        public object lockObject;
        public Szulo(int id)
        {
            Id = id;
            lockObject = new object();
            Status = SzuloStatusz.MegOtthon;
        }

        public void Letezik()
        {
            //otthon vár majd bemegy (sleep)
            Thread.Sleep(Id * Util.rnd.Next(1000, 5001));
            //várósorba kerül
            Ovono.varakozoSzulok.Enqueue(this);
            Status = SzuloStatusz.Var;
            //vár értesítésre
            lock (lockObject)
                Monitor.Wait(lockObject);
            //ébresztéskor óvónővel van
            Status = SzuloStatusz.Ovonovel;
            //megint értesítésre vár
            lock (lockObject)
                Monitor.Wait(lockObject);
            //hazament
            Status = SzuloStatusz.Hazament;
        }

        public override string ToString()
        {
            return $"#{Id} szülő: {Status}";
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
