using System;

namespace StacjePogodowe
{
    public class Pomiar
    {
        public string Stacja { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
        public double Temperatura { get; set; }
        public int Cisnienie { get; set; }

        // Właściwości pomocnicze do podświetlania wierszy w DataGrid
        public bool CzyMax { get; set; }
        public bool CzyMin { get; set; }

        // Formatowanie do zapisu z nietypowym separatorem ";)"
        public string DoLiniiPliku()
        {
            return $"{Stacja};{Data};{Temperatura.ToString(System.Globalization.CultureInfo.InvariantCulture)};{Cisnienie}";
        }
    }
}