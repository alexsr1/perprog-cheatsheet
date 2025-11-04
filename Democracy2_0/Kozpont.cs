using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerprogGyak
{
    internal class Kozpont
    {
        public Kozpont()
        {
            foreach (Szavazat value in Enum.GetValues(typeof(Szavazat)))
            {
                Szamlalo[value] = 0;
            }
        }
        public ConcurrentDictionary<Szavazat, int> Szamlalo { get; set; } = new ConcurrentDictionary<Szavazat, int>();
        // make Szavazok thread-safe - was List<int>
        public ConcurrentBag<int> Szavazok { get; set; } = new ConcurrentBag<int>();

        public void Ertesit(Szavazat szavazat)
        {
            Szamlalo.AddOrUpdate(szavazat, 1, (_, old) => old + 1);
        }
        public void Ertesit(int szavazo)
        {
            Szavazok.Add(szavazo);
        }
        public void Ertesit(Szavazat szavazat, int count)
        {
            Szamlalo.AddOrUpdate(szavazat, count, (_, old) => old + count);
        }
    }
}
