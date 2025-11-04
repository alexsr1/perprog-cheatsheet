namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Neptun kód: ");
            string neptunKod = Console.ReadLine();

            Console.Write("Csoport (A/B)");
            string csoport = Console.ReadLine();

            Console.Write("Entity 1 neve: ");
            string entity1 = Console.ReadLine();

            Console.Write("Entity 2 neve: ");
            string entity2 = Console.ReadLine();

            Console.Write("Entity 1 mennyisége: ");
            int entity1Quantity = int.Parse(Console.ReadLine());

            Console.Write("Entity 2 mennyisége: ");
            int entity2Quantity = int.Parse(Console.ReadLine());



            File.WriteAllText($"{neptunKod}_{csoport}.cs", "namespace "+neptunKod+"_"+csoport+"\r\n{\r\n    internal class Program\r\n    {\r\n        static void Main(string[] args)\r\n        {\r\n            List<"+entity1+"> "+entity1.ToLower()+"list = Enumerable.Range(0, "+entity1Quantity+").Select(x => new "+entity1+"()).ToList();\r\n            List<"+entity2+"> "+entity2.ToLower()+"list = Enumerable.Range(0, "+entity2Quantity+").Select(x => new "+entity2+"()).ToList();\r\n\r\n            var ts = "+entity1.ToLower()+"list.Select(x => new Task(x.Dolgozik, TaskCreationOptions.LongRunning)).ToList();\r\n            ts.AddRange("+entity2.ToLower()+"list.Select(x => new Task(x.Dolgozik, TaskCreationOptions.LongRunning)).ToList());\r\n\r\n\r\n            ts.Add(new Task(x => {\r\n\r\n                while (true)\r\n                {\r\n                    Console.SetCursorPosition(0, 0);\r\n                    "+entity1.ToLower()+"list.ForEach(x => Console.WriteLine(x.ToString().PadRight(Console.WindowWidth)));\r\n                    Console.WriteLine(\"-\".PadRight(Console.WindowWidth));\r\n                    "+entity2.ToLower()+"list.ForEach(x => Console.WriteLine(x.ToString().PadRight(Console.WindowWidth)));\r\n                    Thread.Sleep(1000);\r\n                }\r\n\r\n                Console.SetCursorPosition(0, 0);\r\n                "+entity1.ToLower()+"list.ForEach(x => Console.WriteLine(x.ToString().PadRight(Console.WindowWidth)));\r\n                Console.WriteLine(\"-\".PadRight(Console.WindowWidth));\r\n                "+entity2.ToLower()+"list.ForEach(x => Console.WriteLine(x.ToString().PadRight(Console.WindowWidth)));\r\n\r\n            }, TaskCreationOptions.LongRunning));\r\n\r\n            ts.ForEach(t => t.Start());\r\n            Console.ReadKey();\r\n        }\r\n\r\n\r\n        enum "+entity1+"Allapot\r\n        {\r\n\r\n        }\r\n\r\n        class "+entity1+"\r\n        {\r\n            static int id = 0;\r\n            public int Id { get; set; }\r\n            public "+entity1+"Allapot Allapot;\r\n            public "+entity1+"()\r\n            {\r\n                Id = ++id;\r\n            }\r\n\r\n            public void Dolgozik()\r\n            {\r\n                while (true)\r\n                {\r\n\r\n                }\r\n            }\r\n            public override string ToString()\r\n            {\r\n                return $\"{Id.ToString().PadLeft(1,'0')} - Állapot: {Allapot}\";\r\n            }\r\n        }\r\n\r\n        enum "+entity2+"Allapot\r\n        {\r\n\r\n        }\r\n\r\n        class "+entity2+"\r\n        {\r\n            static int id = 0;\r\n            public int Id { get; set; }\r\n            public "+entity2+"Allapot Allapot;\r\n            public "+entity2+"()\r\n            {\r\n                Id = ++id;\r\n            }\r\n\r\n            public void Dolgozik()\r\n            {\r\n                while (true)\r\n                {\r\n\r\n                }\r\n            }\r\n\r\n            public override string ToString()\r\n            {\r\n                return $\"{Id.ToString().PadLeft(1, '0')} - Állapot: {Allapot}\";\r\n            }\r\n        }\r\n\r\n        class Util\r\n        {\r\n            public static Random Rand = new Random();\r\n            public static int RandomSleep(int minMs, int maxMs)\r\n            {\r\n                int sleepIdo = Rand.Next(minMs, maxMs);\r\n                Thread.Sleep(sleepIdo);\r\n                return sleepIdo;\r\n            }\r\n\r\n            public static int RandomTime(int minMs, int maxMs)\r\n            {\r\n                return Rand.Next(minMs,maxMs);\r\n            }\r\n        }\r\n\r\n    }\r\n}\r\n");


            Console.ReadKey();
        }
    }
      
}
