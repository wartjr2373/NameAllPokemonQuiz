using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NameAllPokemonQuiz
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal int NumberOfPokemon = 1025;
        internal List<Pokemon> PokemonList;
        internal List<Image> ImageList;
        internal List<Rectangle> RectangleList;
        internal bool IsDarkMode = false;
        internal bool IsAllOrigins = false;
        internal bool IsAllTypes = false;

        public MainWindow()
        {
            InitializeComponent();
            PokemonList = new();
            ImageList = new();
            RectangleList = new();
        }

        private void StartQuiz_Click( object sender, RoutedEventArgs e )
        {
            CompilePokemonList();

            if ( PokemonList.Count == 0 )
            {
                NoPokemonWarning.Visibility = Visibility.Visible;
                return;
            }
            else
            {
                NoPokemonWarning.Visibility = Visibility.Collapsed;
            }

            BrushConverter brushConverter = new();
            object? solidColorBrush = brushConverter.ConvertFrom( "#121212" );
            _ = solidColorBrush ?? throw new ArgumentNullException( nameof( solidColorBrush ) );

            foreach ( Pokemon p in PokemonList )
            {
                p.SNumber = p.Number.ToString();
                if ( p.Number < 100 )
                {
                    p.SNumber = $"0{p.SNumber}";
                    if ( p.Number < 10 )
                    {
                        p.SNumber = $"0{p.SNumber}";
                    }
                }


                p.ImgUri = string.Format( "Resources/{0}.png", p.AltVal != null ? $"{p.SNumber}-{p.AltVal}" : p.SNumber );
                p.ImgName = string.Format( "P{0}", p.AltVal != null ? $"{p.SNumber}_{p.AltVal}" : p.SNumber );
                Image image = new()
                {
                    Width = p.ImgWidth,
                    Height = p.ImgHeight,
                    Margin = new Thickness( p.MarginLeft, p.MarginTop, p.MarginRight, p.MarginBottom ),
                    Name = p.ImgName,
                    Source = new BitmapImage( new Uri( p.ImgUri, UriKind.Relative ) )
                };

                Rectangle? rectangle = new()
                {
                    Width = p.ImgWidth,
                    Height = p.ImgHeight,
                    Margin = new Thickness( p.MarginLeft, p.MarginTop, p.MarginRight, p.MarginBottom ),
                    Name = $"R_{p.ImgName}",
                    OpacityMask = new ImageBrush( new BitmapImage( new Uri( $"../../../{p.ImgUri}", UriKind.Relative ) ) )
                };
                if ( IsDarkMode )
                {
                    rectangle.Fill = Brushes.White;
                }
                else
                {
                    rectangle.Fill = (SolidColorBrush)solidColorBrush;
                }

                if ( U_Hidden.IsChecked == true )
                {
                    image.Visibility = Visibility.Hidden;
                    rectangle.Visibility = Visibility.Collapsed;
                }
                else if ( U_Shown.IsChecked == true )
                {
                    image.Visibility = Visibility.Visible;
                    rectangle.Visibility = Visibility.Collapsed;
                }
                else if ( U_Blurred.IsChecked == true )
                {
                    image.Visibility = Visibility.Visible;
                    BlurEffect blurEffect = new() { KernelType = KernelType.Gaussian, Radius = 15 };
                    image.Effect = blurEffect;
                    rectangle.Visibility = Visibility.Collapsed;
                }
                else if ( U_Silhouette.IsChecked == true )
                {
                    rectangle.Visibility = Visibility.Visible;
                    image.Visibility = Visibility.Collapsed;
                }

                ImgWP.Children.Add( image );
                ImageList.Add( image );

                ImgWP.Children.Add( rectangle );
                RectangleList.Add( rectangle );
            }

            LabelsSP.Visibility = Visibility.Visible;
            ImgWP.Visibility = Visibility.Visible;
            StartQuizWP.Visibility = Visibility.Collapsed;
            FiltersSP.Visibility = Visibility.Collapsed;
            PokemonTextBoxLabel.Content = "Name a Pokémon:";
            PokemonTextBox.Visibility = Visibility.Visible;
            PokemonTextBox.Text = string.Empty;
            GiveUpButton.Visibility = Visibility.Visible;

            KeyDown += new KeyEventHandler( RefreshGame );
        }

        private void CompilePokemonList()
        {
            var reader = new StreamReader( "PokemonList.csv" );
            var config = new CsvConfiguration( CultureInfo.InvariantCulture )
            {
                HeaderValidated = null,
                MissingFieldFound = null
            };
            using ( var csv = new CsvReader( reader, config ) )
            {
                int prevNumber = 0;
                foreach ( Pokemon p in csv.GetRecords<Pokemon>().ToList() )
                {
                    if ( CheckOrigin( p ) && CheckType( p ) && CheckDex( p ) )
                    {
                        if ( p.Number != prevNumber )
                        {
                            PokemonList.Add( p );
                            prevNumber = p.Number;
                        }
                    }
                }

                SortPokemonList();

                NumberOfPokemon = PokemonList.Count;
                ScoreLabel.Content = $"Total score: 0/{NumberOfPokemon}";
            }
        }

        private bool CheckOrigin( Pokemon p )
        {
            return ( KantoOrigin.IsChecked == true && p.Origin == "Kanto" ) ||
                   ( JohtoOrigin.IsChecked == true && p.Origin == "Johto" ) ||
                   ( HoennOrigin.IsChecked == true && p.Origin == "Hoenn" ) ||
                   ( SinnohOrigin.IsChecked == true && p.Origin == "Sinnoh" ) ||
                   ( UnovaOrigin.IsChecked == true && p.Origin == "Unova" ) ||
                   ( KalosOrigin.IsChecked == true && p.Origin == "Kalos" ) ||
                   ( AlolaOrigin.IsChecked == true && p.Origin == "Alola" ) ||
                   ( GalarOrigin.IsChecked == true && p.Origin == "Galar" ) ||
                   ( HisuiOrigin.IsChecked == true && p.Origin == "Hisui" ) ||
                   ( PaldeaOrigin.IsChecked == true && p.Origin == "Paldea" ) ||
                   ( UnknownOrigin.IsChecked == true && p.Origin == "Unknown" );
        }

        private bool CheckType( Pokemon p )
        {
            return ( Normal.IsChecked == true && ( p.Type1 == "Normal" || p.Type2 == "Normal" ) ) ||
                   ( Fighting.IsChecked == true && ( p.Type1 == "Fighting" || p.Type2 == "Fighting" ) ) ||
                   ( Flying.IsChecked == true && ( p.Type1 == "Flying" || p.Type2 == "Flying" ) ) ||
                   ( Poison.IsChecked == true && ( p.Type1 == "Poison" || p.Type2 == "Poison" ) ) ||
                   ( Ground.IsChecked == true && ( p.Type1 == "Ground" || p.Type2 == "Ground" ) ) ||
                   ( Rock.IsChecked == true && ( p.Type1 == "Rock" || p.Type2 == "Rock" ) ) ||
                   ( Bug.IsChecked == true && ( p.Type1 == "Bug" || p.Type2 == "Bug" ) ) ||
                   ( Ghost.IsChecked == true && ( p.Type1 == "Ghost" || p.Type2 == "Ghost" ) ) ||
                   ( Steel.IsChecked == true && ( p.Type1 == "Steel" || p.Type2 == "Steel" ) ) ||
                   ( Fire.IsChecked == true && ( p.Type1 == "Fire" || p.Type2 == "Fire" ) ) ||
                   ( Water.IsChecked == true && ( p.Type1 == "Water" || p.Type2 == "Water" ) ) ||
                   ( Grass.IsChecked == true && ( p.Type1 == "Grass" || p.Type2 == "Grass" ) ) ||
                   ( Electric.IsChecked == true && ( p.Type1 == "Electric" || p.Type2 == "Electric" ) ) ||
                   ( Psychic.IsChecked == true && ( p.Type1 == "Psychic" || p.Type2 == "Psychic" ) ) ||
                   ( Ice.IsChecked == true && ( p.Type1 == "Ice" || p.Type2 == "Ice" ) ) ||
                   ( Dragon.IsChecked == true && ( p.Type1 == "Dragon" || p.Type2 == "Dragon" ) ) ||
                   ( Dark.IsChecked == true && ( p.Type1 == "Dark" || p.Type2 == "Dark" ) ) ||
                   ( Fairy.IsChecked == true && ( p.Type1 == "Fairy" || p.Type2 == "Fairy" ) );
        }

        private bool CheckDex( Pokemon p )
        {
            return ( NatDex.IsChecked == true ||
                   ( RBYDex.IsChecked == true && ( p.RBYDex.HasValue ) ) ||
                   ( GSCDex.IsChecked == true && ( p.GSCDex.HasValue ) ) ||
                   ( RSEDex.IsChecked == true && ( p.RSEDex.HasValue ) ) ||
                   ( DPDex.IsChecked == true && ( p.DPDex.HasValue ) ) ||
                   ( PlatDex.IsChecked == true && ( p.PlatDex.HasValue ) ) ||
                   ( BWDex.IsChecked == true && ( p.BWDex.HasValue ) ) ||
                   ( BW2Dex.IsChecked == true && ( p.BW2Dex.HasValue ) ) ||
                   ( XYDex1.IsChecked == true && ( p.XYDex1.HasValue ) ) ||
                   ( XYDex2.IsChecked == true && ( p.XYDex2.HasValue ) ) ||
                   ( XYDex3.IsChecked == true && ( p.XYDex3.HasValue ) ) ||
                   ( ORASDex.IsChecked == true && ( p.ORASDex.HasValue ) ) ||
                   ( SMDex.IsChecked == true && ( p.SMDex.HasValue ) ) ||
                   ( USMDex.IsChecked == true && ( p.USMDex.HasValue ) ) ||
                   ( LGPEDex.IsChecked == true && ( p.LGPEDex.HasValue ) ) ||
                   ( SSDex1.IsChecked == true && ( p.SSDex1.HasValue ) ) ||
                   ( SSDex2.IsChecked == true && ( p.SSDex2.HasValue ) ) ||
                   ( SSDex3.IsChecked == true && ( p.SSDex3.HasValue ) ) ||
                   ( LADex.IsChecked == true && ( p.LADex.HasValue ) ) ||
                   ( SVDex.IsChecked == true && ( p.SVDex.HasValue ) ) ||
                   ( SVDex2.IsChecked == true && ( p.SVDex2.HasValue ) ) ||
                   ( SVDex3.IsChecked == true && ( p.SVDex3.HasValue ) ) );
        }

        private void SortPokemonList()
        {
            if ( NatDex.IsChecked == true )
            {
                PokemonList.Sort( ( p1, p2 ) => p1.Number.CompareTo( p2.Number ) );
            }
            else if ( RBYDex.IsChecked == true )
            {
                PokemonList.Sort( ( p1, p2 ) => p1.RBYDex.Value.CompareTo( p2.RBYDex.Value ) );
            }
            else if ( GSCDex.IsChecked == true )
            {
                PokemonList.Sort( ( p1, p2 ) => p1.GSCDex.Value.CompareTo( p2.GSCDex ) );
            }
            else if ( RSEDex.IsChecked == true )
            {
                PokemonList.Sort( ( p1, p2 ) => p1.RSEDex.Value.CompareTo( p2.RSEDex ) );
            }
            else if ( DPDex.IsChecked == true )
            {
                PokemonList.Sort( ( p1, p2 ) => p1.DPDex.Value.CompareTo( p2.DPDex ) );
            }
            else if ( PlatDex.IsChecked == true )
            {
                PokemonList.Sort( ( p1, p2 ) => p1.PlatDex.Value.CompareTo( p2.PlatDex ) );
            }
            else if ( BWDex.IsChecked == true )
            {
                PokemonList.Sort( ( p1, p2 ) => p1.BWDex.Value.CompareTo( p2.BWDex ) );
            }
            else if ( BW2Dex.IsChecked == true )
            {
                PokemonList.Sort( ( p1, p2 ) => p1.BW2Dex.Value.CompareTo( p2.BW2Dex ) );
            }
            else if ( XYDex1.IsChecked == true )
            {
                PokemonList.Sort( ( p1, p2 ) => p1.XYDex1.Value.CompareTo( p2.XYDex1 ) );
            }
            else if ( XYDex2.IsChecked == true )
            {
                PokemonList.Sort( ( p1, p2 ) => p1.XYDex2.Value.CompareTo( p2.XYDex2 ) );
            }
            else if ( XYDex3.IsChecked == true )
            {
                PokemonList.Sort( ( p1, p2 ) => p1.XYDex3.Value.CompareTo( p2.XYDex3 ) );
            }
            else if ( ORASDex.IsChecked == true )
            {
                PokemonList.Sort( ( p1, p2 ) => p1.ORASDex.Value.CompareTo( p2.ORASDex ) );
            }
            else if ( SMDex.IsChecked == true )
            {
                PokemonList.Sort( ( p1, p2 ) => p1.SMDex.Value.CompareTo( p2.SMDex ) );
            }
            else if ( USMDex.IsChecked == true )
            {
                PokemonList.Sort( ( p1, p2 ) => p1.USMDex.Value.CompareTo( p2.USMDex ) );
            }
            else if ( LGPEDex.IsChecked == true )
            {
                PokemonList.Sort( ( p1, p2 ) => p1.LGPEDex.Value.CompareTo( p2.LGPEDex ) );
            }
            else if ( SSDex1.IsChecked == true )
            {
                PokemonList.Sort( ( p1, p2 ) => p1.SSDex1.Value.CompareTo( p2.SSDex1 ) );
            }
            else if ( SSDex2.IsChecked == true )
            {
                PokemonList.Sort( ( p1, p2 ) => p1.SSDex2.Value.CompareTo( p2.SSDex2 ) );
            }
            else if ( SSDex3.IsChecked == true )
            {
                PokemonList.Sort( ( p1, p2 ) => p1.SSDex3.Value.CompareTo( p2.SSDex3 ) );
            }
            else if ( LADex.IsChecked == true )
            {
                PokemonList.Sort( ( p1, p2 ) => p1.LADex.Value.CompareTo( p2.LADex ) );
            }
            else if ( SVDex.IsChecked == true )
            {
                PokemonList.Sort( ( p1, p2 ) => p1.SVDex.Value.CompareTo( p2.SVDex ) );
            }
            else if (SVDex2.IsChecked == true)
            {
                PokemonList.Sort((p1, p2) => p1.SVDex2.Value.CompareTo(p2.SVDex2));
            }
            else if ( SVDex3.IsChecked == true )
            {
                PokemonList.Sort( ( p1, p2 ) => p1.SVDex3.Value.CompareTo( p2.SVDex3 ) );
            }
        }

        private void TextBox_TextChanged( object sender, TextChangedEventArgs e )
        {
            string compareText = PokemonTextBox.Text;
            compareText = compareText.Replace( " ", string.Empty )
                                     .Replace( "'", string.Empty )
                                     .Replace( ":", string.Empty )
                                     .Replace( ".", string.Empty )
                                     .Replace( "-", string.Empty )
                                     .Replace( "é", "e" )
                                     .Replace( "É", "E" )
                                     .Replace( "♂", "M" )
                                     .Replace( "♀", "F" )
                                     .ToLower();

            Pokemon? pokemon = PokemonList.FirstOrDefault( p => p.Name == compareText );
            if ( pokemon != null )
            {
                RemovePokemon( pokemon );
            }

            if ( compareText == "nidoran" )
            {
                Pokemon? nidoranf = PokemonList.FirstOrDefault( p => p.Name == "nidoranf" );
                if ( nidoranf != null )
                {
                    RemovePokemon( nidoranf );
                }
                Pokemon? nidoranm = PokemonList.FirstOrDefault( p => p.Name == "nidoranm" );
                if ( nidoranm != null )
                {
                    RemovePokemon( nidoranm );
                }
            }

            if ( PokemonList.Count == 0 )
            {
                PokemonTextBoxLabel.Content = "Congratulations! You scored 100%";
                PokemonTextBox.Visibility = Visibility.Hidden;
                GiveUpButton.Visibility = Visibility.Hidden;
            }
        }

        private void RemovePokemon( Pokemon pokemon )
        {
            PokemonList.Remove( pokemon );
            _ = pokemon.SNumber ?? throw new ArgumentNullException( nameof( pokemon.SNumber ) );

            UpdateImgWP( pokemon );

            ScoreLabel.Content = $"Total score: {NumberOfPokemon - PokemonList.Count}/{NumberOfPokemon}";
            PokemonTextBox.Text = null;
        }

        private void UpdateImgWP(Pokemon pokemon)
        {
            Image? image = ImageList.Where( i => i.Name == pokemon.ImgName ).FirstOrDefault();
            _ = image ?? throw new ArgumentNullException( nameof( image ) );

            Rectangle? rectangle = RectangleList.Where( i => i.Name == $"R_{pokemon.ImgName}" ).FirstOrDefault();
            _ = rectangle ?? throw new ArgumentNullException( nameof( rectangle ) );

            if ( G_Hidden.IsChecked == true )
            {
                if ( U_Silhouette.IsChecked == true )
                {
                    rectangle.Fill = Brushes.Transparent;
                }
                else
                {
                    image.Visibility = Visibility.Hidden;
                }
            }
            else if ( G_Shown.IsChecked == true )
            {
                if ( U_Silhouette.IsChecked == true )
                {
                    rectangle.Visibility = Visibility.Hidden;
                    rectangle.Visibility = Visibility.Collapsed;
                }

                if ( U_Blurred.IsChecked == true )
                {
                    image.Effect = null;
                }

                image.Visibility = Visibility.Visible;
            }
            else if ( G_Blurred.IsChecked == true )
            {
                if ( U_Silhouette.IsChecked == true )
                {
                    rectangle.Visibility = Visibility.Collapsed;
                }

                image.Visibility = Visibility.Visible;
                BlurEffect blurEffect = new() { KernelType = KernelType.Gaussian, Radius = 15 };
                image.Effect = blurEffect;
            }
            else if ( G_Silhouette.IsChecked == true )
            {
                if ( U_Silhouette.IsChecked == false )
                {
                    image.Visibility = Visibility.Collapsed;
                    rectangle.Visibility = Visibility.Visible;
                }

                if ( U_Blurred.IsChecked == true )
                {
                    image.Effect = null;
                }
            }
        }

        private void AllOrigins_Checked( object sender, RoutedEventArgs e )
        {
            IsAllOrigins = !IsAllOrigins;

            if (IsAllOrigins)
            {
                KantoOrigin.IsChecked = true;
                JohtoOrigin.IsChecked = true;
                HoennOrigin.IsChecked = true;
                SinnohOrigin.IsChecked = true;
                UnovaOrigin.IsChecked = true;
                KalosOrigin.IsChecked = true;
                AlolaOrigin.IsChecked = true;
                GalarOrigin.IsChecked = true;
                HisuiOrigin.IsChecked = true;
                PaldeaOrigin.IsChecked = true;
                UnknownOrigin.IsChecked = true;

                AllOrigins.Content = "None";
            }
            else
            {
                KantoOrigin.IsChecked = false;
                JohtoOrigin.IsChecked = false;
                HoennOrigin.IsChecked = false;
                SinnohOrigin.IsChecked = false;
                UnovaOrigin.IsChecked = false;
                KalosOrigin.IsChecked = false;
                AlolaOrigin.IsChecked = false;
                GalarOrigin.IsChecked = false;
                HisuiOrigin.IsChecked = false;
                PaldeaOrigin.IsChecked = false;
                UnknownOrigin.IsChecked = false;

                AllOrigins.Content = "All";
            }

            AllOrigins.IsChecked = false;
        }
        private void AllTypes_Checked( object sender, RoutedEventArgs e )
        {
            IsAllTypes = !IsAllTypes;

            if ( IsAllTypes )
            {
                Normal.IsChecked = true;
                Fighting.IsChecked = true;
                Flying.IsChecked = true;
                Poison.IsChecked = true;
                Ground.IsChecked = true;
                Rock.IsChecked = true;
                Bug.IsChecked = true;
                Ghost.IsChecked = true;
                Steel.IsChecked = true;
                Fire.IsChecked = true;
                Water.IsChecked = true;
                Grass.IsChecked = true;
                Electric.IsChecked = true;
                Psychic.IsChecked = true;
                Ice.IsChecked = true;
                Dragon.IsChecked = true;
                Dark.IsChecked = true;
                Fairy.IsChecked = true;

                AllTypes.Content = "None";
            }
            else
            {
                Normal.IsChecked = false;
                Fighting.IsChecked = false;
                Flying.IsChecked = false;
                Poison.IsChecked = false;
                Ground.IsChecked = false;
                Rock.IsChecked = false;
                Bug.IsChecked = false;
                Ghost.IsChecked = false;
                Steel.IsChecked = false;
                Fire.IsChecked = false;
                Water.IsChecked = false;
                Grass.IsChecked = false;
                Electric.IsChecked = false;
                Psychic.IsChecked = false;
                Ice.IsChecked = false;
                Dragon.IsChecked = false;
                Dark.IsChecked = false;
                Fairy.IsChecked = false;

                AllTypes.Content = "All";
            }

            AllTypes.IsChecked = false;
        }

        private void RefreshGame( object sender, KeyEventArgs e )
        {
            if ( ( Keyboard.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control )
            {
                if ( e.Key == Key.F5 )
                {
                    LabelsSP.Visibility = Visibility.Collapsed;
                    ImgWP.Visibility = Visibility.Collapsed;
                    foreach ( UIElement element in ImgWP.Children )
                    {
                        element.Visibility = Visibility.Collapsed;
                    }
                    PokemonList = new();
                    ImageList = new();
                    RectangleList = new();
                    FiltersSP.Visibility = Visibility.Visible;
                    StartQuizWP.Visibility = Visibility.Visible;
                    return;
                }
            }
        }

        private void GiveUp_Click( object sender, RoutedEventArgs e )
        {
            foreach ( Pokemon p in PokemonList )
            {
                UpdateImgWP( p );
            }

            double percentage = (double)( NumberOfPokemon - PokemonList.Count ) / NumberOfPokemon * 100;
            PokemonTextBoxLabel.Content = $"You scored {Math.Truncate( percentage * 100 ) / 100}%";
            PokemonTextBox.Visibility = Visibility.Hidden;
            GiveUpButton.Visibility = Visibility.Hidden;
        }

        private void DarkMode_Checked( object sender, RoutedEventArgs e )
        {
            IsDarkMode = !IsDarkMode;

            if ( IsDarkMode )
            {
                #region Dark Mode adjustments

                BrushConverter brushConverter = new();
                object? solidColorBrush = brushConverter.ConvertFrom( "#121212" );
                object? solidColorBrush2 = brushConverter.ConvertFrom( "#FF2F2F2F" );
                if ( solidColorBrush != null )
                {
                    Background = (SolidColorBrush)solidColorBrush;
                    PokemonTextBox.Background = (SolidColorBrush)solidColorBrush;
                }
                if (solidColorBrush2 != null)
                {
                    StartQuiz.Background = (SolidColorBrush)solidColorBrush2;
                }
                PokemonTextBoxLabel.Foreground = Brushes.White;
                PokemonTextBox.Foreground = Brushes.White;
                ScoreLabel.Foreground = Brushes.White;

                PokemonTextBoxLabel.Opacity = 0.87;
                PokemonTextBox.Opacity = 0.87;
                ScoreLabel.Opacity = 0.87;

                OriginLabel.Foreground = Brushes.White;
                AllOrigins.Foreground = Brushes.White;
                KantoOrigin.Foreground = Brushes.White;
                JohtoOrigin.Foreground = Brushes.White;
                HoennOrigin.Foreground = Brushes.White;
                SinnohOrigin.Foreground = Brushes.White;
                UnovaOrigin.Foreground = Brushes.White;
                KalosOrigin.Foreground = Brushes.White;
                AlolaOrigin.Foreground = Brushes.White;
                GalarOrigin.Foreground = Brushes.White;
                HisuiOrigin.Foreground = Brushes.White;
                PaldeaOrigin.Foreground = Brushes.White;
                UnknownOrigin.Foreground = Brushes.White;

                OriginLabel.Opacity = 0.87;
                AllOrigins.Opacity = 0.87;
                KantoOrigin.Opacity = 0.87;
                JohtoOrigin.Opacity = 0.87;
                HoennOrigin.Opacity = 0.87;
                SinnohOrigin.Opacity = 0.87;
                UnovaOrigin.Opacity = 0.87;
                KalosOrigin.Opacity = 0.87;
                AlolaOrigin.Opacity = 0.87;
                GalarOrigin.Opacity = 0.87;
                HisuiOrigin.Opacity = 0.87;
                PaldeaOrigin.Opacity = 0.87;
                UnknownOrigin.Opacity = 0.87;

                TypeLabel.Foreground = Brushes.White;
                AllTypes.Foreground = Brushes.White;
                Normal.Foreground = Brushes.White;
                Fighting.Foreground = Brushes.White;
                Flying.Foreground = Brushes.White;
                Poison.Foreground = Brushes.White;
                Ground.Foreground = Brushes.White;
                Rock.Foreground = Brushes.White;
                Bug.Foreground = Brushes.White;
                Ghost.Foreground = Brushes.White;
                Steel.Foreground = Brushes.White;
                Fire.Foreground = Brushes.White;
                Water.Foreground = Brushes.White;
                Grass.Foreground = Brushes.White;
                Electric.Foreground = Brushes.White;
                Psychic.Foreground = Brushes.White;
                Ice.Foreground = Brushes.White;
                Dragon.Foreground = Brushes.White;
                Dark.Foreground = Brushes.White;
                Fairy.Foreground = Brushes.White;

                TypeLabel.Opacity = 0.87;
                AllTypes.Opacity = 0.87;
                Normal.Opacity = 0.87;
                Fighting.Opacity = 0.87;
                Flying.Opacity = 0.87;
                Poison.Opacity = 0.87;
                Ground.Opacity = 0.87;
                Rock.Opacity = 0.87;
                Bug.Opacity = 0.87;
                Ghost.Opacity = 0.87;
                Steel.Opacity = 0.87;
                Fire.Opacity = 0.87;
                Water.Opacity = 0.87;
                Grass.Opacity = 0.87;
                Electric.Opacity = 0.87;
                Psychic.Opacity = 0.87;
                Ice.Opacity = 0.87;
                Dragon.Opacity = 0.87;
                Dark.Opacity = 0.87;
                Fairy.Opacity = 0.87;

                PokedexLabel.Foreground = Brushes.White;
                NatDex.Foreground = Brushes.White;
                RBYDex.Foreground = Brushes.White;
                GSCDex.Foreground = Brushes.White;
                RSEDex.Foreground = Brushes.White;
                DPDex.Foreground = Brushes.White;
                PlatDex.Foreground = Brushes.White;
                BWDex.Foreground = Brushes.White;
                BW2Dex.Foreground = Brushes.White;
                XYDex1.Foreground = Brushes.White;
                XYDex2.Foreground = Brushes.White;
                XYDex3.Foreground = Brushes.White;
                ORASDex.Foreground = Brushes.White;
                SMDex.Foreground = Brushes.White;
                USMDex.Foreground = Brushes.White;
                LGPEDex.Foreground = Brushes.White;
                SSDex1.Foreground = Brushes.White;
                SSDex2.Foreground = Brushes.White;
                SSDex3.Foreground = Brushes.White;
                LADex.Foreground = Brushes.White;
                SVDex.Foreground = Brushes.White;
                SVDex2.Foreground = Brushes.White;
                SVDex3.Foreground = Brushes.White;

                PokedexLabel.Opacity = 0.87;
                NatDex.Opacity = 0.87;
                RBYDex.Opacity = 0.87;
                GSCDex.Opacity = 0.87;
                RSEDex.Opacity = 0.87;
                DPDex.Opacity = 0.87;
                PlatDex.Opacity = 0.87;
                BWDex.Opacity = 0.87;
                BW2Dex.Opacity = 0.87;
                XYDex1.Opacity = 0.87;
                XYDex2.Opacity = 0.87;
                XYDex3.Opacity = 0.87;
                ORASDex.Opacity = 0.87;
                SMDex.Opacity = 0.87;
                USMDex.Opacity = 0.87;
                LGPEDex.Opacity = 0.87;
                SSDex1.Opacity = 0.87;
                SSDex2.Opacity = 0.87;
                SSDex3.Opacity = 0.87;
                LADex.Opacity = 0.87;
                SVDex.Opacity = 0.87;
                SVDex2.Opacity = 0.87;
                SVDex3.Opacity = 0.87;

                OptionsLabel.Foreground = Brushes.White;
                DarkMode.Foreground = Brushes.White;
                UnguessedLabel.Foreground = Brushes.White;
                U_Hidden.Foreground = Brushes.White;
                U_Shown.Foreground = Brushes.White;
                U_Blurred.Foreground = Brushes.White;
                U_Silhouette.Foreground = Brushes.White;
                GuessedLabel.Foreground = Brushes.White;
                G_Hidden.Foreground = Brushes.White;
                G_Shown.Foreground = Brushes.White;
                G_Blurred.Foreground = Brushes.White;
                G_Silhouette.Foreground = Brushes.White;

                OptionsLabel.Opacity = 0.87;
                DarkMode.Opacity = 0.87;
                UnguessedLabel.Opacity = 0.87;
                U_Hidden.Opacity = 0.87;
                U_Shown.Opacity = 0.87;
                U_Blurred.Opacity = 0.87;
                U_Silhouette.Opacity = 0.87;
                GuessedLabel.Opacity = 0.87;
                G_Hidden.Opacity = 0.87;
                G_Shown.Opacity = 0.87;
                G_Blurred.Opacity = 0.87;
                G_Silhouette.Opacity = 0.87;

                StartQuiz.Foreground = Brushes.White;
                StartQuiz.Opacity = 0.87;
                NoPokemonWarning.Opacity = 0.87;

                #endregion
                DarkMode.Content = "Light Mode";
            }
            else
            {
                #region Light Mode adjustments

                BrushConverter brushConverter = new();
                object? solidColorBrush = brushConverter.ConvertFrom( "#FFDDDDDD" );
                if ( solidColorBrush != null )
                {
                    StartQuiz.Background = (SolidColorBrush)solidColorBrush;
                }

                Background = Brushes.White;
                PokemonTextBoxLabel.Foreground = Brushes.Black;
                PokemonTextBox.Background = Brushes.White;
                PokemonTextBox.Foreground = Brushes.Black;
                ScoreLabel.Foreground = Brushes.Black;

                PokemonTextBoxLabel.Opacity = 1;
                PokemonTextBox.Opacity = 1;
                ScoreLabel.Opacity = 1;

                OriginLabel.Foreground = Brushes.Black;
                AllOrigins.Foreground = Brushes.Black;
                KantoOrigin.Foreground = Brushes.Black;
                JohtoOrigin.Foreground = Brushes.Black;
                HoennOrigin.Foreground = Brushes.Black;
                SinnohOrigin.Foreground = Brushes.Black;
                UnovaOrigin.Foreground = Brushes.Black;
                KalosOrigin.Foreground = Brushes.Black;
                AlolaOrigin.Foreground = Brushes.Black;
                GalarOrigin.Foreground = Brushes.Black;
                HisuiOrigin.Foreground = Brushes.Black;
                PaldeaOrigin.Foreground = Brushes.Black;
                UnknownOrigin.Foreground = Brushes.Black;

                OriginLabel.Opacity = 1;
                AllOrigins.Opacity = 1;
                KantoOrigin.Opacity = 1;
                JohtoOrigin.Opacity = 1;
                HoennOrigin.Opacity = 1;
                SinnohOrigin.Opacity = 1;
                UnovaOrigin.Opacity = 1;
                KalosOrigin.Opacity = 1;
                AlolaOrigin.Opacity = 1;
                GalarOrigin.Opacity = 1;
                HisuiOrigin.Opacity = 1;
                PaldeaOrigin.Opacity = 1;
                UnknownOrigin.Opacity = 1;

                TypeLabel.Foreground = Brushes.Black;
                AllTypes.Foreground = Brushes.Black;
                Normal.Foreground = Brushes.Black;
                Fighting.Foreground = Brushes.Black;
                Flying.Foreground = Brushes.Black;
                Poison.Foreground = Brushes.Black;
                Ground.Foreground = Brushes.Black;
                Rock.Foreground = Brushes.Black;
                Bug.Foreground = Brushes.Black;
                Ghost.Foreground = Brushes.Black;
                Steel.Foreground = Brushes.Black;
                Fire.Foreground = Brushes.Black;
                Water.Foreground = Brushes.Black;
                Grass.Foreground = Brushes.Black;
                Electric.Foreground = Brushes.Black;
                Psychic.Foreground = Brushes.Black;
                Ice.Foreground = Brushes.Black;
                Dragon.Foreground = Brushes.Black;
                Dark.Foreground = Brushes.Black;
                Fairy.Foreground = Brushes.Black;

                TypeLabel.Opacity = 1;
                AllTypes.Opacity = 1;
                Normal.Opacity = 1;
                Fighting.Opacity = 1;
                Flying.Opacity = 1;
                Poison.Opacity = 1;
                Ground.Opacity = 1;
                Rock.Opacity = 1;
                Bug.Opacity = 1;
                Ghost.Opacity = 1;
                Steel.Opacity = 1;
                Fire.Opacity = 1;
                Water.Opacity = 1;
                Grass.Opacity = 1;
                Electric.Opacity = 1;
                Psychic.Opacity = 1;
                Ice.Opacity = 1;
                Dragon.Opacity = 1;
                Dark.Opacity = 1;
                Fairy.Opacity = 1;

                PokedexLabel.Foreground = Brushes.Black;
                NatDex.Foreground = Brushes.Black;
                RBYDex.Foreground = Brushes.Black;
                GSCDex.Foreground = Brushes.Black;
                RSEDex.Foreground = Brushes.Black;
                DPDex.Foreground = Brushes.Black;
                PlatDex.Foreground = Brushes.Black;
                BWDex.Foreground = Brushes.Black;
                BW2Dex.Foreground = Brushes.Black;
                XYDex1.Foreground = Brushes.Black;
                XYDex2.Foreground = Brushes.Black;
                XYDex3.Foreground = Brushes.Black;
                ORASDex.Foreground = Brushes.Black;
                SMDex.Foreground = Brushes.Black;
                USMDex.Foreground = Brushes.Black;
                LGPEDex.Foreground = Brushes.Black;
                SSDex1.Foreground = Brushes.Black;
                SSDex2.Foreground = Brushes.Black;
                SSDex3.Foreground = Brushes.Black;
                LADex.Foreground = Brushes.Black;
                SVDex.Foreground = Brushes.Black;
                SVDex2.Foreground = Brushes.Black;
                SVDex3.Foreground = Brushes.Black;

                PokedexLabel.Opacity = 1;
                NatDex.Opacity = 1;
                RBYDex.Opacity = 1;
                GSCDex.Opacity = 1;
                RSEDex.Opacity = 1;
                DPDex.Opacity = 1;
                PlatDex.Opacity = 1;
                BWDex.Opacity = 1;
                BW2Dex.Opacity = 1;
                XYDex1.Opacity = 1;
                XYDex2.Opacity = 1;
                XYDex3.Opacity = 1;
                ORASDex.Opacity = 1;
                SMDex.Opacity = 1;
                USMDex.Opacity = 1;
                LGPEDex.Opacity = 1;
                SSDex1.Opacity = 1;
                SSDex2.Opacity = 1;
                SSDex3.Opacity = 1;
                LADex.Opacity = 1;
                SVDex.Opacity = 1;
                SVDex2.Opacity = 1;
                SVDex3.Opacity = 1;

                OptionsLabel.Foreground = Brushes.Black;
                DarkMode.Foreground = Brushes.Black;
                UnguessedLabel.Foreground = Brushes.Black;
                U_Hidden.Foreground = Brushes.Black;
                U_Shown.Foreground = Brushes.Black;
                U_Blurred.Foreground = Brushes.Black;
                U_Silhouette.Foreground = Brushes.Black;
                GuessedLabel.Foreground = Brushes.Black;
                G_Hidden.Foreground = Brushes.Black;
                G_Shown.Foreground = Brushes.Black;
                G_Blurred.Foreground = Brushes.Black;
                G_Silhouette.Foreground = Brushes.Black;

                OptionsLabel.Opacity = 1;
                DarkMode.Opacity = 1;
                UnguessedLabel.Opacity = 1;
                U_Hidden.Opacity = 1;
                U_Shown.Opacity = 1;
                U_Blurred.Opacity = 1;
                U_Silhouette.Opacity = 1;
                GuessedLabel.Opacity = 1;
                G_Hidden.Opacity = 1;
                G_Shown.Opacity = 1;
                G_Blurred.Opacity = 1;
                G_Silhouette.Opacity = 1;

                StartQuiz.Foreground = Brushes.Black;
                StartQuiz.Opacity = 1;
                NoPokemonWarning.Opacity = 1;

                #endregion
                DarkMode.Content = "Dark Mode";
            }

            DarkMode.IsChecked = false;
        }

        private void Credits_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show( $"All Pokemon Quiz (C) 2023 Dylan Mead.{Environment.NewLine}{Environment.NewLine}" + 
                $"Additional sprites created by:{Environment.NewLine}\u2022 LarryTurbo (DeviantArt){Environment.NewLine}" +
                $"\u2022 SilSinn9801 (DeviantArt){Environment.NewLine}" +
                $"\u2022 Minority (Smogon){Environment.NewLine}" +
                $"\u2022 leParagon, Megax Rocker, Vent, Cesare_CBass (PokeCommunity){Environment.NewLine}" +
                $"\u2022 Arclart (DeviantArt){Environment.NewLine}" +
                $"\u2022 Ezerart (DeviantArt){Environment.NewLine}" +
                $"\u2022 u/Jordanos11 (Reddit)"
                , "Credits" );
                
        }
    }
}