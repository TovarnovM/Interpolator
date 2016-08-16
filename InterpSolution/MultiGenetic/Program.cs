using GeneticSharp.Domain.Randomizations;
using System;
using System.Collections.Generic;
using System.Text;

namespace MultiGenetic {
    class Program {
        static void Main(string[] args) {
            for (int i = 0; i < 100; i++) {
                Console.WriteLine(RandomizationProvider.Current.GetInt(2,7));
            }
            Console.ReadLine();
        }
    }
}
