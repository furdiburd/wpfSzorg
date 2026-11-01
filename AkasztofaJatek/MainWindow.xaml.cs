using System.Text;
using System.Windows;

namespace AkasztofaJatek
{
    public partial class MainWindow : Window
    {
        private readonly Dictionary<string, string[]> words = new()
        {
            {
                "Állatok",
                new[]
                {
                    "kutya", "macska", "elefant", "zsiraf", "teknos",
                    "pillango", "krokodil", "delfin", "bagoly", "nyul"
                }
            },
            {
                "Informatika",
                new[]
                {
                    "programozas", "szamitogep", "monitor", "billentyuzet",
                    "algoritmus", "adatok", "halozat", "szoftver"
                }
            },
            {
                "Ételek",
                new[]
                {
                    "alma", "kenyer", "leves", "halaszle", "gulyas",
                    "torta", "sajt", "paradicsom", "banan", "csokolade"
                }
            },
            {
                "Iskola",
                new[]
                {
                    "iskola", "tanar", "diak", "toll", "fuzet",
                    "taska", "osztaly", "konyv", "pad", "terem"
                }
            }
        };

        private readonly Random random = new Random();
        private readonly List<char> usedLetters = new List<char>();

        private readonly string[] hangmanStages =
        {
            "  +---+\n      |\n      |\n      |\n      |\n      |\n=========",
            "  +---+\n  |   |\n  O   |\n      |\n      |\n      |\n=========",
            "  +---+\n  |   |\n  O   |\n  |   |\n      |\n      |\n=========",
            "  +---+\n  |   |\n  O   |\n /|   |\n      |\n      |\n=========",
            "  +---+\n  |   |\n  O   |\n /|\\  |\n      |\n      |\n=========",
            "  +---+\n  |   |\n  O   |\n /|\\  |\n /    |\n      |\n=========",
            "  +---+\n  |   |\n  O   |\n /|\\  |\n / \\  |\n      |\n========="
        };

        private string currentWord = "";
        private char[] displayedWord = new char[0];
        private int mistakes;
        private int maxMistakes = 6;
        private int score;
        private bool gameOver;

        public MainWindow()
        {
            InitializeComponent();

            comboCategory.ItemsSource = words.Keys;
            comboCategory.SelectedIndex = 0;
            comboDifficulty.SelectedIndex = 1;

            startNewGame();
        }

        private void startNewGame()
        {
            string category = comboCategory.SelectedItem as string ?? "Informatika";
            string[] categoryWords = words[category];

            currentWord = categoryWords[random.Next(categoryWords.Length)];
            maxMistakes = getMaxMistakes();
            mistakes = 0;
            score = 0;
            gameOver = false;

            usedLetters.Clear();

            displayedWord = new char[currentWord.Length];

            for (int i = 0; i < displayedWord.Length; i++)
            {
                displayedWord[i] = '_';
            }

            textGuess.Clear();
            setInputEnabled(true);

            updateWordDisplay();
            updateStatus();
        }

        private int getMaxMistakes()
        {
            switch (comboDifficulty.SelectedIndex)
            {
                case 0:
                    return 8;
                case 2:
                    return 4;
                default:
                    return 6;
            }
        }

        private void Tip_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (gameOver)
                {
                    return;
                }

                if (!isGuessValid())
                {
                    return;
                }

                char letter = char.ToLowerInvariant(textGuess.Text.Trim()[0]);

                usedLetters.Add(letter);
                checkGuess(letter);

                textGuess.Clear();
                textGuess.Focus();

                updateStatus();

                if (isGameWon())
                {
                    score += (maxMistakes - mistakes) * 5;
                    gameOver = true;
                    setInputEnabled(false);
                    updateStatus();
                    showResult(true);
                }
                else if (mistakes >= maxMistakes)
                {
                    gameOver = true;
                    setInputEnabled(false);
                    updateStatus();
                    showResult(false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool isGuessValid()
        {
            string input = textGuess.Text.Trim();

            if (input.Length == 0)
            {
                MessageBox.Show("Adj meg egy betűt!", "Hiányzó adat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (input.Length != 1)
            {
                MessageBox.Show("Pontosan egy betűt adj meg!", "Hibás adat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!char.IsLetter(input[0]))
            {
                MessageBox.Show("Csak betűt lehet megadni!", "Hibás adat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            char letter = char.ToLowerInvariant(input[0]);

            if (usedLetters.Contains(letter))
            {
                MessageBox.Show("Ezt a betűt már tippelted!", "Ismételt tipp", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void checkGuess(char letter)
        {
            int matches = 0;

            for (int i = 0; i < currentWord.Length; i++)
            {
                if (char.ToLowerInvariant(currentWord[i]) == letter)
                {
                    displayedWord[i] = char.ToUpperInvariant(currentWord[i]);
                    matches++;
                }
            }

            if (matches > 0)
            {
                score += matches * 10;
            }
            else
            {
                mistakes++;
            }

            updateWordDisplay();
        }

        private void updateWordDisplay()
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < displayedWord.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(" ");
                }

                builder.Append(displayedWord[i]);
            }

            textWord.Text = builder.ToString();
        }

        private bool isGameWon()
        {
            for (int i = 0; i < displayedWord.Length; i++)
            {
                if (displayedWord[i] == '_')
                {
                    return false;
                }
            }

            return true;
        }

        private void updateStatus()
        {
            textMistakes.Text = $"Hibák száma: {mistakes} / {maxMistakes}";
            textTries.Text = $"Hátralévő próbálkozások: {maxMistakes - mistakes}";
            textUsedLetters.Text = getUsedLettersText();
            textScore.Text = $"Pontszám: {score}";

            updateHangman();
        }

        private string getUsedLettersText()
        {
            if (usedLetters.Count == 0)
            {
                return "Használt betűk: –";
            }

            List<char> sortedLetters = new List<char>(usedLetters);
            sortedLetters.Sort();

            StringBuilder builder = new StringBuilder("Használt betűk: ");

            for (int i = 0; i < sortedLetters.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(char.ToUpperInvariant(sortedLetters[i]));
            }

            return builder.ToString();
        }

        private void updateHangman()
        {
            bool lost = mistakes >= maxMistakes;
            int stage = lost ? 6 : Math.Min(mistakes, 6);

            textHangman.Text = hangmanStages[stage];
        }

        private void setInputEnabled(bool enabled)
        {
            textGuess.IsEnabled = enabled;
            buttonTip.IsEnabled = enabled;
        }

        private void showResult(bool won)
        {
            if (won)
            {
                MessageBox.Show(
                    "Gratulálok!\n\n" +
                    $"Kitaláltad a szót: {currentWord.ToUpper()}\n" +
                    $"Hibák: {mistakes} / {maxMistakes}\n" +
                    $"Pontszám: {score}",
                    "Győzelem",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(
                    "Vesztettél!\n\n" +
                    $"A keresett szó: {currentWord.ToUpper()}\n" +
                    $"Hibák: {mistakes} / {maxMistakes}\n" +
                    $"Pontszám: {score}",
                    "Vereség",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            startNewGame();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "A játék szabályai:\n\n" +
                "- A program egy véletlenszerű szót választ a kiválasztott kategóriából.\n" +
                "- Egyszerre egy betűt lehet tippelni.\n" +
                "- A helyes betűk megjelennek a szóban, a hibás tippek számlálódnak.\n" +
                "- Könnyű: 8 hiba, Normál: 6 hiba, Nehéz: 4 hiba engedélyezett.\n" +
                "- Minden helyes betűért pont jár, kevesebb hibáért több bónuszpont.\n" +
                "- Az Új játék gombbal új szót kaphatsz.",
                "Súgó",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
