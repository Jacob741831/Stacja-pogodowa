# 🌦️ Stacje Pogodowe (C# / WPF)

Aplikacja okienkowa napisana w języku **C#** z wykorzystaniem technologii **WPF (Windows Presentation Foundation)**. Program umożliwia zarządzanie pomiarami meteorologicznymi, analizę ekstremalnych temperatur oraz filtrowanie danych dla poszczególnych stacji pogodowych.

---

## ✨ Funkcjonalności

* **Odczyt i zapis plików:** Wczytywanie danych pomiarowych z plików `.txt` / `.csv` oraz osobny eksport zmodyfikowanej bazy do nowego pliku.
* **Walidacja danych wejściowych:** Zabezpieczenie przed wczytaniem pustych lub uszkodzonych plików oraz wykrywanie duplikatów przy dodawaniu nowych wpisów.
* **Dodawanie pomiarów:** Wbudowany formularz pozwalający na dopisywanie nowych odczytów (stacja, data, temperatura, ciśnienie) z automatycznym zaznaczeniem nowego rekordu w tabeli.
* **Analiza rekordów MAX i MIN:** Automatyczne wyszukiwanie i wizualne podświetlanie wierszy z najwyższą (**jasnoczerwony**) oraz najniższą (**jasnoniebieski**) temperaturą w aktualnym widoku.
* **Filtrowanie po stacji:** Dynamicznie aktualizowana lista rozwijana (`ComboBox`), umożliwiająca wyświetlenie danych dla wszystkich stacji jednocześnie lub zawężenie widoku do wybranej lokalizacji.

---

## 📄 Format pliku z danymi

Dane w pliku wejściowym (np. `pomiary.txt`) są rozdzielane średnikiem (`;`) w następującej kolejności kolumn:

`NazwaStacji;DataIGodzina;Temperatura;Cisnienie`

### Przykładowa zawartość pliku `pomiary.txt`:
```text
Warszawa;2026-11-18 10:00;4.5;1015
Kraków;2026-11-18 10:00;-2.1;1020
Gdańsk;2026-11-18 10:00;7.2;1012
Warszawa;2026-11-18 12:00;6.8;1014
Kraków;2026-11-18 12:00;0.5;1019
```

---

## 🛠️ Wymagania i uruchomienie

Projekt został stworzony i przetestowany w środowisku **Visual Studio Code** z zainstalowanym **.NET SDK**.

### Wymagania:
* System operacyjny Windows (wymagany przez WPF)
* [.NET SDK](https://dotnet.microsoft.com/) (wersja 8.0 lub 10.0)
* Edytor **Visual Studio Code** z rozszerzeniem *C# Dev Kit* (opcjonalnie)

### Jak uruchomić projekt:
1. Sklonuj repozytorium lub pobierz pliki projektu na dysk.
2. Otwórz folder projektu w **Visual Studio Code**.
3. Otwórz zintegrowany terminal (`Ctrl + ~`) i wpisz polecenie:
   ```bash
   dotnet run
   ```
4. Po uruchomieniu okna aplikacji kliknij przycisk **"Wczytaj z pliku"** i wskaż dołączony do repozytorium plik `pomiary.txt`.

---

## 📂 Struktura projektu

* `MainWindow.xaml` – definicja interfejsu graficznego (układ okna, formularz, style podświetlania wierszy w tabeli `DataGrid`).
* `MainWindow.xaml.cs` – logika aplikacji, obsługa zdarzeń, walidacja plików, filtrowanie oraz wyznaczanie temperatur MAX/MIN.
* `Pomiar.cs` – model danych reprezentujący pojedynczy rekord pomiarowy oraz metodę formatującą zapis do pliku.
* `pomiary.txt` – przykładowy plik z danymi wejściowymi.
