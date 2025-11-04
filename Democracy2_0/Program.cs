using PerprogGyak;

Kozpont kozpont = new Kozpont();

List<PapirSzavazokor> papirSzavazokorok = Enumerable.Range(1, 4).Select(x => new PapirSzavazokor(kozpont)).ToList();
List<DigitalisSzavazokor> digitalisSzavazokorok = Enumerable.Range(1, 4).Select(x => new DigitalisSzavazokor(kozpont)).ToList();

int percek = 0;
new Task(() =>
{
    while (percek <= 600)
    {
        Thread.Sleep(100);
        percek++;
    }
}).Start();
papirSzavazokorok.Select(szavazokor => new Task(() =>
{
    while (percek <= 600)
    {
        int szavazo = szavazokor.Szavazok.Take();
        if (Util.Rnd.NextDouble() < 0.06)
        {
            szavazokor.Szavaz(Szavazat.Ervenytelen, szavazo);
        }
        else
        {
            Szavazat szavazat = (Szavazat)Util.Rnd.Next(0, 4);
            szavazokor.Szavaz(szavazat, szavazo);
        }
    }
    szavazokor.Szamlalas();
    szavazokor.Status = SzavazokorStatus.Zaras;
}, TaskCreationOptions.LongRunning)).ToList().ForEach(x => x.Start());

digitalisSzavazokorok.Select(szavazokor => new Task(() =>
{
    while (percek <= 600)
    {
        int szavazo = szavazokor.Szavazok.Take();
        if (Util.Rnd.NextDouble() < 0.04)
        {
            szavazokor.Szavaz(Szavazat.Ervenytelen, szavazo);
        }
        else
        {
            Szavazat szavazat = (Szavazat)Util.Rnd.Next(0, 4);
            szavazokor.Szavaz(szavazat, szavazo);
        }
    }
    szavazokor.Szamlalas();
    szavazokor.Status = SzavazokorStatus.Zaras;
}, TaskCreationOptions.LongRunning)).ToList().ForEach(x => x.Start());

int szavazo = 0;
object lockObject = new object();
papirSzavazokorok.Select(szavazokor => new Task(() =>
{
    while (percek <= 600)
    {
        int db = Util.Rnd.Next(1, 5);
        for (int i = 0; i < db; i++)
        {
            lock (lockObject)
                szavazokor.Szavazok.Add(szavazo++);
        }
        Thread.Sleep(Util.Rnd.Next(1000, 15000));
    }
    szavazokor.Szamlalas();
    szavazokor.Status = SzavazokorStatus.Zaras;
}, TaskCreationOptions.LongRunning)).ToList().ForEach(x => x.Start());

digitalisSzavazokorok.Select(szavazokor => new Task(() =>
{
    while (percek <= 600)
    {
        int db = Util.Rnd.Next(1, 5);
        for (int i = 0; i < db; i++)
        {
            lock (lockObject)
                szavazokor.Szavazok.Add(szavazo++);
        }
        Thread.Sleep(Util.Rnd.Next(1000, 15000));
    }
    szavazokor.Szamlalas();
    szavazokor.Status = SzavazokorStatus.Zaras;
}, TaskCreationOptions.LongRunning)).ToList().ForEach(x => x.Start());


while (papirSzavazokorok.Any(x => x.Status != SzavazokorStatus.Zaras) || digitalisSzavazokorok.Any(x => x.Status != SzavazokorStatus.Zaras))
{
    Console.Clear();
    Console.WriteLine("Percek: " + percek);
    foreach (var szavazokor in papirSzavazokorok)
    {
        Console.WriteLine(szavazokor);
    }
    foreach (var szavazokor in digitalisSzavazokorok)
    {
        Console.WriteLine(szavazokor);
    }
    Thread.Sleep(500);
}
foreach (var item in kozpont.Szamlalo)
{
    Console.WriteLine($"{item.Key}: {item.Value}");
}
