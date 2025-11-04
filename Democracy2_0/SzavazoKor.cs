using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerprogGyak
{
    public enum SzavazokorStatus { NyitasraVar, Szavaz, Fennakadas, Valtas, Zaras }
    abstract class SzavazoKor
    {
        protected Kozpont _kozpont;
        private static int counter = 0;
        public int Id { get; }
        public SzavazokorStatus Status { get; set; }
        public BlockingCollection<int> Szavazok { get; set; } = new BlockingCollection<int>(new ConcurrentQueue<int>());
        public int? JelenlegiSzavazo { get; set; }
        public SzavazoKor(Kozpont kozpont)
        {
            foreach (Szavazat value in Enum.GetValues(typeof(Szavazat)))
            {
                Szamlalo[value] = 0;
            }
            Status = SzavazokorStatus.NyitasraVar;
            _kozpont = kozpont;
            Id = counter++;
        }
        abstract public Dictionary<Szavazat, int> Szamlalo { get; set; }
        abstract public void Szavaz(Szavazat szavazat, int szavazo);
        abstract public void Szamlalas();

        public override string? ToString()
        {
            return $"ID: {Id}, Státusz: {Status}, jelenlegi szavazó {JelenlegiSzavazo}";
        }
    }
}
