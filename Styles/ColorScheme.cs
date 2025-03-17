using System.Windows.Media;

namespace EnvironMint.Styles
{
    public static class ColorScheme
    {
        // primaries
        public static readonly Color PrimaryLight = (Color)ColorConverter.ConvertFromString("#4DB6AC");
        public static readonly Color Primary = (Color)ColorConverter.ConvertFromString("#009688");
        public static readonly Color PrimaryDark = (Color)ColorConverter.ConvertFromString("#00796B");

        // accents
        public static readonly Color AccentLight = (Color)ColorConverter.ConvertFromString("#FFD54F");
        public static readonly Color Accent = (Color)ColorConverter.ConvertFromString("#FFC107");
        public static readonly Color AccentDark = (Color)ColorConverter.ConvertFromString("#FFA000");

        // neutrals
        public static readonly Color Background = (Color)ColorConverter.ConvertFromString("#F5F5F5");
        public static readonly Color Surface = (Color)ColorConverter.ConvertFromString("#FFFFFF");
        public static readonly Color CardBackground = (Color)ColorConverter.ConvertFromString("#FFFFFF");
        public static readonly Color Border = (Color)ColorConverter.ConvertFromString("#E0E0E0");

        // text colors
        public static readonly Color TextPrimary = (Color)ColorConverter.ConvertFromString("#212121");
        public static readonly Color TextSecondary = (Color)ColorConverter.ConvertFromString("#757575");
        public static readonly Color TextOnPrimary = (Color)ColorConverter.ConvertFromString("#FFFFFF");
        public static readonly Color TextOnAccent = (Color)ColorConverter.ConvertFromString("#212121");

        // status'
        public static readonly Color Success = (Color)ColorConverter.ConvertFromString("#4CAF50");
        public static readonly Color Warning = (Color)ColorConverter.ConvertFromString("#FF9800");
        public static readonly Color Error = (Color)ColorConverter.ConvertFromString("#F44336");
        public static readonly Color Info = (Color)ColorConverter.ConvertFromString("#2196F3");

        // brushes
        public static readonly SolidColorBrush PrimaryLightBrush = new SolidColorBrush(PrimaryLight);
        public static readonly SolidColorBrush PrimaryBrush = new SolidColorBrush(Primary);
        public static readonly SolidColorBrush PrimaryDarkBrush = new SolidColorBrush(PrimaryDark);

        public static readonly SolidColorBrush AccentLightBrush = new SolidColorBrush(AccentLight);
        public static readonly SolidColorBrush AccentBrush = new SolidColorBrush(Accent);
        public static readonly SolidColorBrush AccentDarkBrush = new SolidColorBrush(AccentDark);

        public static readonly SolidColorBrush BackgroundBrush = new SolidColorBrush(Background);
        public static readonly SolidColorBrush SurfaceBrush = new SolidColorBrush(Surface);
        public static readonly SolidColorBrush CardBackgroundBrush = new SolidColorBrush(CardBackground);
        public static readonly SolidColorBrush BorderBrush = new SolidColorBrush(Border);

        public static readonly SolidColorBrush TextPrimaryBrush = new SolidColorBrush(TextPrimary);
        public static readonly SolidColorBrush TextSecondaryBrush = new SolidColorBrush(TextSecondary);
        public static readonly SolidColorBrush TextOnPrimaryBrush = new SolidColorBrush(TextOnPrimary);
        public static readonly SolidColorBrush TextOnAccentBrush = new SolidColorBrush(TextOnAccent);

        public static readonly SolidColorBrush SuccessBrush = new SolidColorBrush(Success);
        public static readonly SolidColorBrush WarningBrush = new SolidColorBrush(Warning);
        public static readonly SolidColorBrush ErrorBrush = new SolidColorBrush(Error);
        public static readonly SolidColorBrush InfoBrush = new SolidColorBrush(Info);
    }
}
