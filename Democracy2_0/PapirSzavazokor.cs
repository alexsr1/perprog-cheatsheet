using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerprogGyak
{
    internal class PapirSzavazokor : SzavazoKor
    {
        public PapirSzavazokor(Kozpont kozpont) : base(kozpont)
        {
        }
        public Queue<Szavazat> Urna { get; set; } = new Queue<Szavazat>();
        public override Dictionary<Szavazat, int> Szamlalo { get; set; } = new Dictionary<Szavazat, int>();
        public override void Szamlalas()
        {
            while (Urna.Count != 0)
            {
                int batchSize = Math.Min(Urna.Count, Util.Rnd.Next(5, 10));
                for (int i = 0; i < batchSize; i++)
                {
                    Szavazat szavazat = Urna.Dequeue();
                    Szamlalo[szavazat]++;
                    _kozpont.Ertesit(szavazat);
                }
                Thread.Sleep(1000);
            }
        }
        public override void Szavaz(Szavazat szavazat, int szavazo)
        {
            JelenlegiSzavazo = szavazo;
            if (Util.Rnd.NextDouble() < 0.05)
            {
                Status = SzavazokorStatus.Fennakadas;
                Thread.Sleep(Util.Rnd.Next(3000, 10000));
            }
            Status = SzavazokorStatus.Szavaz;
            Thread.Sleep(Util.Rnd.Next(2000, 5000));
            Urna.Enqueue(szavazat);
            _kozpont.Ertesit(szavazo);
            Status = SzavazokorStatus.Valtas;
            JelenlegiSzavazo = null;
            Thread.Sleep(1000);
        }
    }
}
