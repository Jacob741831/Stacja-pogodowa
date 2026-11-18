using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StacjePogodowe
{
    /// <summary>
    /// Logika interakcji dla okna głównego aplikacji MainWindow.xaml.
    /// Odpowiada za obsługę interfejsu, operacje na plikach oraz logikę biznesową stacji pogodowych.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Globalna lista przechowująca wszystkie poprawne, wczytane lub dodane pomiary stacji pogodowych.
        /// </summary>
        private List<Pomiar> _wszystkiePomiary = new List<Pomiar>();

        /// <summary>
        /// Znak separatora używany do oddzielania kolumn i pól w pliku tekstowym (domyślnie średnik).
        /// </summary>
        private const string Separator = ";";

        /// <summary>
        /// Tekstowa etykieta reprezentująca opcję wyświetlenia wszystkich stacji w elemencie ComboBox (SELECT-OPTION).
        /// </summary>
        private const string DomyslnaOpcja = "Wszystkie stacje";

        /// <summary>
        /// Inicjalizuje nowe wystąpienie klasy MainWindow, konfiguruje kontrolki, 
        /// ustawia aktualną datę w formularzu oraz generuje początkową listę rozwijaną.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            TxtData.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            OdswiezListeStacji();
        }

        /// <summary>
        /// Obsługuje zdarzenie kliknięcia przycisku wczytywania danych z pliku tekstowego lub CSV.
        /// Posiada pełną walidację struktury pliku, obsługę pustych linii, błędnych formatów oraz "dziwnych" plików.
        /// </summary>
        /// <param name="sender">Obiekt wywołujący zdarzenie (przycisk wczytywania).</param>
        /// <param name="e">Argumenty zdarzenia routowanego.</param>
        private void BtnWczytaj_Click(object sender, RoutedEventArgs e)
        {
            // Konfiguracja okna dialogowego wyboru pliku
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Pliki tekstowe (*.txt;*.csv)|*.txt;*.csv|Wszystkie pliki (*.*)|*.*"
            };

            // Jeśli użytkownik wybrał plik i kliknął OK
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Odczyt wszystkich linii z wybranego pliku
                    string[] linie = File.ReadAllLines(openFileDialog.FileName);

                    // Walidacja: Sprawdzenie czy plik nie jest pusty
                    if (linie.Length == 0)
                    {
                        MessageBox.Show("Wybrany plik jest pusty!", "Błąd pliku", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Tymczasowa lista do walidacji danych przed nadpisaniem głównej kolekcji
                    var tymczasowePomiary = new List<Pomiar>();
                    int bledneLinie = 0;

                    // Iteracja po każdej linii w pliku tekstowym
                    foreach (string linia in linie)
                    {
                        // Pomijanie pustych linii w pliku
                        if (string.IsNullOrWhiteSpace(linia)) continue;

                        // Rozdzielenie linii na kolumny za pomocą zdefiniowanego separatora
                        string[] czesci = linia.Split(Convert.ToChar(Separator));

                        // Walidacja struktury: linijka musi mieć co najmniej 4 kolumny (Stacja, Data, Temp, Ciśnienie)
                        if (czesci.Length < 4)
                        {
                            bledneLinie++;
                            continue;
                        }

                        // Pobranie i oczyszczenie wartości temperatury (zamiana przecinka na kropkę dla poprawności parsowania)
                        string tempStr = czesci[2].Trim().Replace(',', '.');

                        // Próba skonwertowania tekstów na właściwe typy liczbowe
                        if (double.TryParse(tempStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double temp) &&
                            int.TryParse(czesci[3].Trim(), out int cisnienie))
                        {
                            // Dodanie poprawnego rekordu do listy tymczasowej
                            tymczasowePomiary.Add(new Pomiar
                            {
                                Stacja = czesci[0].Trim(),
                                Data = czesci[1].Trim(),
                                Temperatura = temp,
                                Cisnienie = cisnienie
                            });
                        }
                        else
                        {
                            // Zliczenie linii, które zawierają błędne dane liczbowe
                            bledneLinie++;
                        }
                    }

                    // Walidacja: Jeśli w pliku nie ma ani jednego poprawnego rekordu (np. wrzucono losowy plik tekstowy ze śmieciami)
                    if (tymczasowePomiary.Count == 0)
                    {
                        MessageBox.Show("Wybrany plik ma nieprawidłowy format, błędne separatory lub nie zawiera poprawnych danych pomiarowych!", "Błąd formatu", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Nadpisanie głównej listy dopiero po pomyślnym zweryfikowaniu danych
                    _wszystkiePomiary = tymczasowePomiary;
                    OdswiezListeStacji();

                    // Przygotowanie komunikatu końcowego z informacją o wczytaniu i ewentualnych błędnych wierszach
                    string komunikat = $"Wczytano pomyślnie {_wszystkiePomiary.Count} pomiarów.";
                    if (bledneLinie > 0)
                    {
                        komunikat += $"\n(Uwaga: pominięto {bledneLinie} uszkodzonych lub niepoprawnych wierszy).";
                    }

                    MessageBox.Show(komunikat, "Informacja o wczytaniu", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    // Obsługa nieoczekiwanych błędów systemowych (np. brak dostępu do pliku)
                    MessageBox.Show($"Wystąpił krytyczny błąd podczas odczytu pliku: {ex.Message}", "Błąd krytyczny", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Obsługuje zdarzenie kliknięcia przycisku zapisu aktualnej listy pomiarów do pliku tekstowego.
        /// </summary>
        /// <param name="sender">Obiekt wywołujący zdarzenie (przycisk zapisu).</param>
        /// <param name="e">Argumenty zdarzenia routowanego.</param>
        private void BtnZapisz_Click(object sender, RoutedEventArgs e)
        {
            // Konfiguracja okna zapisu pliku
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Plik tekstowy (*.txt)|*.txt",
                FileName = "pomiary_wyjscie.txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Konwersja każdego obiektu pomiaru na sformatowaną linię tekstową z separatorem
                    var linie = _wszystkiePomiary.Select(p => p.DoLiniiPliku());
                    
                    // Zapis wszystkich linii do wybranego pliku
                    File.WriteAllLines(saveFileDialog.FileName, linie);
                    
                    MessageBox.Show("Zapisano pomiary do pliku.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd podczas zapisu pliku: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Obsługuje dodawanie nowego pomiaru z pól tekstowych formularza do kolekcji.
        /// Zawiera walidację pustych pól, poprawności typów liczbowych oraz wykrywanie duplikatów.
        /// Po dodaniu automatycznie podświetla i przewija widok do nowego rekordu.
        /// </summary>
        /// <param name="sender">Obiekt wywołujący zdarzenie (przycisk dodawania).</param>
        /// <param name="e">Argumenty zdarzenia routowanego.</param>
        private void BtnDodaj_Click(object sender, RoutedEventArgs e)
        {
            // Pobranie danych z kontrolek tekstowych formularza
            string stacja = TxtStacja.Text.Trim();
            string data = TxtData.Text.Trim();
            string tempStr = TxtTemp.Text.Trim().Replace(',', '.');
            string cisnienieStr = TxtCisnienie.Text.Trim();

            // Krok 1: Walidacja czy wymagane pola tekstowe nie są puste
            if (string.IsNullOrEmpty(stacja) || string.IsNullOrEmpty(data))
            {
                MessageBox.Show("Uzupełnij nazwę stacji oraz datę pomiaru.", "Uwaga", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Krok 2: Walidacja poprawności formatu liczb (temperatura i ciśnienie)
            if (!double.TryParse(tempStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double temp) ||
                !int.TryParse(cisnienieStr, out int cisnienie))
            {
                MessageBox.Show("Podaj poprawne wartości liczbowe dla temperatury oraz ciśnienia.", "Uwaga", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Krok 3: Sprawdzenie czy taki sam rekord (ta sama stacja oraz dokładnie ta sama data/godzina) już istnieje
            bool istniejeJuż = _wszystkiePomiary.Any(p => p.Stacja.Equals(stacja, StringComparison.OrdinalIgnoreCase) && p.Data == data);
            if (istniejeJuż)
            {
                MessageBox.Show($"Błąd: Pomiar dla stacji '{stacja}' w czasie '{data}' już istnieje w wykazie!", "Duplikat rekordu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Utworzenie nowego obiektu pomiaru
            var nowyPomiar = new Pomiar
            {
                Stacja = stacja,
                Data = data,
                Temperatura = temp,
                Cisnienie = cisnienie
            };

            // Dodanie obiektu do głównej listy
            _wszystkiePomiary.Add(nowyPomiar);

            // Wyczyszczenie pól formularza po udanym dodaniu
            TxtStacja.Clear();
            TxtTemp.Clear();
            TxtCisnienie.Clear();

            // Ustawienie filtru w ComboBox na nowo dodaną stację, aby użytkownik od razu ją zobaczył
            CmbStacje.SelectedItem = stacja;
            OdswiezListeStacji();

            // Automatyczne zaznaczenie i przewinięcie tabeli (DataGrid) do nowo dodanego wiersza
            DgPomiary.SelectedItem = nowyPomiar;
            DgPomiary.ScrollIntoView(nowyPomiar);
        }

        /// <summary>
        /// Odświeża elementy dostępne w liście rozwijanej ComboBox (SELECT-OPTION) 
        /// na podstawie unikalnych nazw stacji występujących w zebranych danych.
        /// </summary>
        private void OdswiezListeStacji()
        {
            // Zapamiętanie aktualnie wybranej stacji (jeśli taka była)
            string aktualnyWybor = CmbStacje.SelectedItem as string ?? DomyslnaOpcja;

            // Pobranie unikalnych nazw stacji i posortowanie ich alfabetycznie
            var stacje = _wszystkiePomiary
                .Select(p => p.Stacja)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            // Wstawienie opcji ogólnej na sam początek listy
            stacje.Insert(0, DomyslnaOpcja);

            // Przypisanie listy do kontrolki ComboBox
            CmbStacje.ItemsSource = stacje;

            // Przywrócenie poprzedniego wyboru lub ustawienie domyślnego
            if (stacje.Contains(aktualnyWybor))
                CmbStacje.SelectedItem = aktualnyWybor;
            else
                CmbStacje.SelectedIndex = 0;

            // Wywołanie filtrowania i przeliczenia rekordów MAX/MIN
            ZastosujFiltrIRekordy();
        }

        /// <summary>
        /// Reaguje na zmianę wybranej stacji w elemencie ComboBox (SELECT-OPTION).
        /// </summary>
        /// <param name="sender">Obiekt wywołujący zdarzenie (ComboBox).</param>
        /// <param name="e">Argumenty zmiany zaznaczenia.</param>
        private void CmbStacje_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ZastosujFiltrIRekordy();
        }

        /// <summary>
        /// Filtruje widok tabeli na podstawie aktualnie wybranej stacji w ComboBox,
        /// a następnie wyznacza wartości maksymalne (MAX) i minimalne (MIN) temperatury
        /// i ustawia odpowiednie flagi w celu ich podświetlenia w interfejsie.
        /// </summary>
        private void ZastosujFiltrIRekordy()
        {
            if (CmbStacje.SelectedItem == null) return;

            string wybranaStacja = CmbStacje.SelectedItem.ToString()!;

            // Krok 1: Reset flag podświetlenia (MAX i MIN) dla wszystkich rekordów w bazie
            foreach (var p in _wszystkiePomiary)
            {
                p.CzyMax = false;
                p.CzyMin = false;
            }

            // Krok 2: Filtrowanie listy – jeśli wybrano "Wszystkie stacje", bierzemy wszystko, w przeciwnym razie filtrujemy po nazwie
            List<Pomiar> przefiltrowane = (wybranaStacja == DomyslnaOpcja)
                ? _wszystkiePomiary.ToList()
                : _wszystkiePomiary.Where(p => p.Stacja == wybranaStacja).ToList();

            // Krok 3: Wyznaczanie temperatur MAX i MIN w obrębie aktualnie widocznych wierszy
            if (przefiltrowane.Count > 0)
            {
                double maxTemp = przefiltrowane.Max(p => p.Temperatura);
                double minTemp = przefiltrowane.Min(p => p.Temperatura);

                // Oznaczenie flagami rekordów spełniających warunek maksimum lub minimum
                foreach (var p in przefiltrowane)
                {
                    if (p.Temperatura == maxTemp) p.CzyMax = true;
                    if (p.Temperatura == minTemp) p.CzyMin = true;
                }
            }

            // Krok 4: Odświeżenie źródła danych tabeli DataGrid, aby odświeżyć widok i style podświetlenia
            DgPomiary.ItemsSource = null;
            DgPomiary.ItemsSource = przefiltrowane;
        }
    }
}