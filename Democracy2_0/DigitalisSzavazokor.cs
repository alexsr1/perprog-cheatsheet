using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerprogGyak
{
    class DigitalisSzavazokor : SzavazoKor
    {
        public override Dictionary<Szavazat, int> Szamlalo { get; set; } = new Dictionary<Szavazat, int>();
        public DigitalisSzavazokor(Kozpont kozpont) : base(kozpont) { }
        public override void Szavaz(Szavazat szavazat, int szavazo)
        {
            JelenlegiSzavazo = szavazo;
            if (Util.Rnd.NextDouble() < 0.1)
            {
                Status = SzavazokorStatus.Fennakadas;
                Thread.Sleep(Util.Rnd.Next(3000, 15000));
            }
            Status = SzavazokorStatus.Szavaz;
            Thread.Sleep(Util.Rnd.Next(2000, 5000));
            Szamlalo[szavazat]++;
            _kozpont.Ertesit(szavazo);
            Status = SzavazokorStatus.Valtas;
            JelenlegiSzavazo = null;
            Thread.Sleep(1000);
        }

        public override void Szamlalas()
        {
            foreach (var keyValuePair in Szamlalo)
            {
                _kozpont.Ertesit(keyValuePair.Key, keyValuePair.Value);
            }
        }
    }
}
