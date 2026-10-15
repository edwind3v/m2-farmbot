namespace FarmBot.Core.Combat;

public static class CaptchaMap
{
    public sealed class Challenge
    {
        public Challenge(int id, int bg, string question, int button)
        {
            Id = id;
            Bg = bg;
            Question = question;
            Button = button;
        }

        public int Id { get; }
        public int Bg { get; }
        public string Question { get; }
        public int Button { get; }
    }

    public static IReadOnlyList<Challenge> Challenges { get; } = new Challenge[]
    {
        new(1, 10, "Click on the fish", 8),
        new(2, 3, "Click on the wolf's tail.", 5),
        new(3, 11, "Click on the book", 5),
        new(4, 6, "Click on the tree in the background", 2),
        new(5, 1, "Click on the hand of the General Store", 5),
        new(6, 11, "Click on the sword", 1),
        new(7, 9, "Click on the head of Baek-Go", 0),
        new(8, 4, "Click on the baby head.", 2),
        new(9, 10, "Click on the horse", 6),
        new(10, 5, "Click on the left half of the Storekeeper's face", 0),
        new(11, 11, "Click on the fish", 2),
        new(12, 2, "Click on the head of the fisherman.", 1),
        new(13, 11, "Click on the horse", 7),
        new(14, 5, "Click on the fire", 5),
        new(15, 10, "Click on the shield", 2),
        new(16, 6, "Click on Yonah head", 1),
        new(17, 3, "Click on the wolf's front paw.", 7),
        new(18, 11, "Click on the potion", 0),
        new(19, 7, "Click on the horse in the background", 2),
        new(20, 1, "Click on the Blacksmith head.", 0),
        new(21, 11, "Click on the shield", 4),
        new(22, 5, "Click on the Storekeeper's empty hand", 7),
        new(23, 2, "Click on the bag of the fisherman.", 6),
        new(24, 10, "Click on the sword", 4),
        new(25, 6, "Click on the top of the vase", 5),
        new(26, 4, "Click on the hair of the mom. (Ah-Yu)", 1),
        new(27, 10, "Click on the book", 0),
        new(28, 5, "Click on the right half of the Storekeeper's face", 1),
        new(29, 8, "Click on the head of the Stable Boy", 1),
        new(30, 12, "Click on the horse", 0),
        new(31, 12, "Click on the sword", 2),
        new(32, 12, "Click on the shield", 3),
        new(33, 12, "Click on the fish", 5),
        new(34, 12, "Click on the potion", 7),
        new(35, 12, "Click on the book", 8),
        new(36, 13, "Click on the sword", 0),
        new(37, 13, "Click on the horse", 2),
        new(38, 13, "Click on the book", 4),
        new(39, 13, "Click on the potion", 5),
        new(40, 13, "Click on the fish", 6),
        new(41, 13, "Click on the shield", 7),
        new(42, 14, "Click on the potion", 0),
        new(43, 14, "Click on the book", 1),
        new(44, 14, "Click on the fish", 3),
        new(45, 14, "Click on the horse", 5),
        new(46, 14, "Click on the sword", 6),
        new(47, 14, "Click on the shield", 8),
    };

    public static IReadOnlyDictionary<int, int> ButtonsById { get; } =
        Challenges.ToDictionary(item => item.Id, item => item.Button);

    public static bool TryGetButton(int challengeId, out int button) =>
        ButtonsById.TryGetValue(challengeId, out button);
}
