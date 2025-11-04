using System.Diagnostics;
using System.Linq;

Etterem etterem = new Etterem();
List<Futar> futar1list = Enumerable.Range(0, 4).Select(x => new Futar(etterem, FutarCsapat.FurgeFutar)).ToList();
List<Futar> futar2list = Enumerable.Range(0, 4).Select(x => new Futar(etterem, FutarCsapat.TurboTeknos)).ToList();

List<Futar> futarlist = new List<Futar>();
futarlist.AddRange(futar1list);
futarlist.AddRange(futar2list);


var ts = futarlist.Select(x => new Task(x.Dolgozik, TaskCreationOptions.LongRunning)).ToList();
ts.Add(new Task(etterem.Dolgozik, TaskCreationOptions.LongRunning));


Stopwatch stopwatch = new Stopwatch();

ts.Add(new Task(x => {
    stopwatch.Start();
    while (futarlist.Any(x => x.Allapot != FutarAllapot.Alszik))
    {
        Console.Clear();
        //Console.SetCursorPosition(0, 0);
        futarlist.ForEach(x => Console.WriteLine(x.ToString().PadRight(Console.WindowWidth)));
        Console.WriteLine("-".PadRight(Console.WindowWidth));
        Console.WriteLine(etterem.ToString().PadRight(Console.WindowWidth));
        Thread.Sleep(1000);
    }
    stopwatch.Stop();

    Console.WriteLine($"A szimuláció {stopwatch.ElapsedMilliseconds / 1000} másodpercbe telt");
    Console.WriteLine($"Fürge Futár pénzösszeg: {futar1list.Sum(x => x.Penz)}");
    Console.WriteLine($"Turbo Teknős pénzösszeg: {futar1list.Sum(x => x.Penz)}");


}, TaskCreationOptions.LongRunning));

ts.ForEach(t => t.Start());
Console.ReadKey();

enum FutarAllapot
{
    Szallit, Atad, Etteremben, Valaszt, Visszater, Alszik
}
enum FutarCsapat
{
    FurgeFutar, TurboTeknos
}

class Futar
{
    static int id = 0;
    public int Id { get; set; }
    public FutarAllapot Allapot = FutarAllapot.Etteremben;
    public FutarCsapat Csapat;

    public int Penz { get; set; } = 0;
    Etterem etterem;
    public Rendeles? Rendeles { get; set; } = null;
    public Futar(Etterem etterem, FutarCsapat csapat)
    {
        Id = ++id;
        this.etterem = etterem;
        Csapat = csapat;
    }

    public void Dolgozik()
    {
        while (etterem.Nyitva)
        {
            Valaszt();
            lock (etterem.RendelesekLock)
                etterem.Rendelesek.Remove(Rendeles);
            Allapot = FutarAllapot.Szallit;
            Thread.Sleep(Util.Rnd.Next(2000, 3900) * Rendeles.Tavolsag);
            Allapot = FutarAllapot.Atad;
            Thread.Sleep(Util.Rnd.Next(2000, 5000));
            Allapot = FutarAllapot.Visszater;
            Thread.Sleep(Util.Rnd.Next(2000, 3900) * Rendeles.Tavolsag);
            Allapot = FutarAllapot.Etteremben;
            PenzSzamol();
            Rendeles = null;
        }
        Allapot = FutarAllapot.Alszik;
    }
    private void PenzSzamol()
    {
        if (Csapat == FutarCsapat.FurgeFutar)
        {
            Penz += 600;
        }
        else
        {
            Penz += (int)Math.Round(Rendeles.Ertek * 0.05);
        }
    }
    private void Valaszt()
    {
        if (Csapat == FutarCsapat.FurgeFutar)
        {
            while (Rendeles == null)
            {
                Allapot = FutarAllapot.Valaszt;
                Thread.Sleep(Util.Rnd.Next(1000, 2000));
                lock (etterem.RendelesekLock)
                    Rendeles = etterem.Rendelesek.OrderBy(x => x.Tavolsag).FirstOrDefault();
            }
        }
        else
        {
            while (Rendeles == null)
            {
                Allapot = FutarAllapot.Valaszt;
                Thread.Sleep(Util.Rnd.Next(1000, 2000));
                lock (etterem.RendelesekLock)
                    Rendeles = etterem.Rendelesek.OrderByDescending(x => x.Tavolsag).FirstOrDefault();
            }
        }
    }

    public override string ToString()
    {
        return $"{Id.ToString().PadLeft(1, '0')} - Állapot: {Allapot} - Csapat: {Csapat} - Rendelés: {Rendeles} ";
    }
}

enum RendelesAllapot
{

}

class Rendeles {
    static int id = 0;
    public int Id { get; set; }
    public RendelesAllapot Allapot;
    public int Ertek { get; set; }
    public int Tavolsag { get; set; }
    public Rendeles()
    {
        Id = ++id;
    }

    public override string? ToString()
    {
        return $"ID: {Id.ToString().PadLeft(1, '0')}, Érték: {Ertek}, Távolság: {Tavolsag}";
    }
}
enum EtteremAllapot
{

}
class Etterem
{
    static int id = 0;
    public int Id { get; set; }
    public EtteremAllapot Allapot;
    public List<Rendeles> Rendelesek { get; set; } = new List<Rendeles>();
    public object RendelesekLock { get; set; } = new object();
    public bool Nyitva { get; set; } = true;
    private int rendelesekDb = 0;
    public Etterem()
    {
        Id = ++id;
    }

    public void Dolgozik()
    {
        while (rendelesekDb < 50)
        {
            Rendelesek.Add(new Rendeles()
            {
                Ertek = Util.Rnd.Next(2000, 10001),
                Tavolsag = Util.Rnd.Next(500, 10000)
            });
            rendelesekDb++;
            Thread.Sleep(Util.Rnd.Next(1000, 5000));
        }
        Nyitva = false;
    }

    public override string ToString()
    {
        return $"{Id.ToString().PadLeft(1, '0')} - Állapot: {Allapot} - Nyitva: {Nyitva} - Rendelések: {rendelesekDb} - Vár kiszállításra: {Rendelesek.Count}";
    }
}

class Util
{
    public static Random Rnd = new Random();
}