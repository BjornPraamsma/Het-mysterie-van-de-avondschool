using System;
using System.Windows;
using System.Windows.Controls;
using Game.Models;

namespace Game.Services
{
    public class QuizManager
    {
        private readonly Grid _overlay;
        private readonly TextBlock _questionText;
        private readonly TextBox _answerInput;
        private readonly TextBlock _feedbackText;

        public int ActiveDoorTileId { get; private set; } = -1;
        public bool IsQuizActive { get; private set; }

        public QuizManager(Grid overlay, TextBlock questionText, TextBox answerInput, TextBlock feedbackText)
        {
            _overlay = overlay;
            _questionText = questionText;
            _answerInput = answerInput;
            _feedbackText = feedbackText;
        }

        public void TriggerQuiz(int doorTileId, LevelMap map)
        {
            ActiveDoorTileId = doorTileId;
            IsQuizActive = true;

            var door = map.DoorQuestions[doorTileId];
            _questionText.Text = door.Question;
            _answerInput.Text = "";
            _feedbackText.Text = "";
            _overlay.Visibility = Visibility.Visible;

            _overlay.Dispatcher.BeginInvoke(new Action(() => _answerInput.Focus()));
        }

        public bool SubmitAnswer(LevelMap map, Action onUnlocked)
        {
            if (ActiveDoorTileId == -1) return false;

            var door = map.DoorQuestions[ActiveDoorTileId];

            if (int.TryParse(_answerInput.Text, out int userAnswer) && userAnswer == door.CorrectAnswer)
            {
                door.IsUnlocked = true;
                UnlockDoorInMap(ActiveDoorTileId, map);

                CloseQuiz();
                onUnlocked?.Invoke();
                return true;
            }

            _feedbackText.Text = "Fout antwoord, probeer opnieuw!";
            return false;
        }

        public void CloseQuiz()
        {
            _overlay.Visibility = Visibility.Collapsed;
            IsQuizActive = false;
            ActiveDoorTileId = -1;
        }

        private void UnlockDoorInMap(int doorTileId, LevelMap map)
        {
            for (int r = 0; r < map.Height; r++)
            {
                for (int c = 0; c < map.Width; c++)
                {
                    if (map.Layout[r, c] == doorTileId)
                    {
                        map.Layout[r, c] = 0;
                    }
                }
            }
        }
    }
}