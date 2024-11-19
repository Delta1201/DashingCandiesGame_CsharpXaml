using GameLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace GameInterface
{
	public class NotMain
	{
		private MainPage mP; // Reference to the main page of the game

		// Constructor that initializes the NotMain class with a MainPage instance
		public NotMain(MainPage mainPage)
		{
			this.mP = mainPage; // Assign the provided MainPage instance to the private member
								// gridNotMain = mainPage.GridMain; // Not currently in use
		}

		// Default constructor
		public NotMain()
		{
		}

		public CoreDispatcher dispatcher; // Dispatcher for handling UI updates
										  // Changing values will impact the difficulty level
		public int dotCount = 5; // Number of dots to display on the board
		public double gT = 3; // Timer limit for the game in minutes
		public double fT = 10; // Timer for fruit in seconds
		public double sizeEnlarger = 10; // Factor for enlarging the size of the player
		public double enemySpawnRate = 2; // Rate of enemy spawn; spawns occur after 3 due to counter timing
		public int difficultyPercentile = 15; // Percentage increase for game difficulty

		// Value for fruits that can be changed under certain conditions
		public int fruitCount = 1; // Number of fruits to spawn at a given moment
								   // Has a value that cannot be changed
		public int score = 0; // Points gained in each game
		public int highScore = 0; // Highest points gained in the game
		public int addedDotCount = 0; // Number of additional dots added to the game

		// Dependent values for the sizes of game pieces
		public double sizeOfTargets; // Base size of consumables (dots), adaptable to screen size
		public double sizeFruits; // Size for the fruit, based on the player's size
		public double sizePlayer; // Size for the player, based on the size of dots

		// Static game pieces
		public static GamePiece player; // Player character
		public static GamePiece border; // Game borders

		// Lists to hold different game pieces
		public List<GamePiece> manyDots; // List of collectible dots
		public List<GamePiece> manyEnemies; // List of enemy pieces
		public List<GamePiece> gamePieces; // General list of game pieces
		public List<GamePiece> manyFruits; // List of collectible fruits
		public List<GamePiece> borders; // List of game borders

		// Screen dimensions
		public double screenHeight; // Height of the game screen
		public double screenWidth; // Width of the game screen

		// Boolean flags to determine game status and options
		public bool gameOn = true; // Flag to indicate if the game is currently active
		public bool enableEnemy; // Flag to enable or disable enemies in the game
		public bool enableFruit; // Flag to enable or disable fruits in the game
		public bool enableGameTimer; // Flag to enable or disable the game timer
		public bool enableEnlarger; // Flag to enable or disable size increase, synced with enableFruit
		public bool limitDots; // Flag to limit the number of dots, inversely proportional to enableEnemy
		public bool futureGuide; // Flag to indicate if the user wants a guide
		public bool futureDifficulty; // Flag to indicate if the user wants to choose difficulty between levels
		public bool noToAll; // Flag to indicate if the user does not want customization options
		public bool guideOpen = false; // Flag to indicate if the guide is currently open

		// Game levels and difficulty
		public GameMode currentMode; // Current game mode
		public GameDifficulty currentDifficulty; // Current game difficulty setting

		// Timers for game functionality
		public DispatcherTimer fruitTimer = new DispatcherTimer(); // Timer for spawning fruits
		public DispatcherTimer gameTimer = new DispatcherTimer(); // Timer for the game duration

		// Getting the highest score from file
		public int highestScore; // Highest points gained ever (to be sourced from a file)
		public const string FileName = "highestscore.txt"; // Filename for storing the highest score

		// Method to show the user guide
		public async Task Guide() // ChatGPT: Reduced repetition and formated for me
		{
			guideOpen = true; // Set guide state to open

			// Create the content dialog for the user manual
			ContentDialog practiceDialog = new ContentDialog
			{
				Title = "User Guide", // Title of the dialog
				MaxHeight = screenHeight - 10, // Limit the dialog height to ensure scroll works
				Content = new Border
				{
					Background = new SolidColorBrush(Windows.UI.Colors.DarkGray), // Background color of the dialog
					Child = (mP.practiceScrollViewer = new ScrollViewer // Store reference to the ScrollViewer
					{
						VerticalScrollBarVisibility = ScrollBarVisibility.Auto, // Enable vertical scroll bar
						Content = new StackPanel
						{
							Orientation = Orientation.Vertical, // Set vertical orientation for stacking elements
							Children =
					{
						new TextBlock
						{
							Text = "DASHING CANDIES! \nLet's guide you through the levels.", // Main introduction text
                            Foreground = new SolidColorBrush(Windows.UI.Colors.Black), // Text color
                            FontSize = Math.Min(sizeOfTargets, 20), // Dynamic font size
                            FontWeight = Windows.UI.Text.FontWeights.Bold, // Font weight
                            Margin = new Thickness(0, 0, 0, 20), // Margin for spacing
                            TextWrapping = TextWrapping.Wrap // Enable text wrapping
                        },
						new TextBlock
						{
							Text = "\nUse the arrow keys to move the player around and capture candies. ",
							Foreground = new SolidColorBrush(Windows.UI.Colors.Black), // Text color
                            FontSize = Math.Min(sizeOfTargets - 2, 20), // Dynamic font size
                            Margin = new Thickness(0, 0, 0, 10), // Adds some spacing
                            TextWrapping = TextWrapping.Wrap // Enable text wrapping
                        },
						new Image
						{
							Source = new BitmapImage(new Uri("ms-appx:///Assets/player.png")), // Enemy image
                            Width = sizeOfTargets, // Set width based on target size
                            Height = sizeOfTargets, // Set height based on target size
                            Margin = new Thickness(0, 0, 0, 10) // Adds some spacing below enemy
                        },
						new Image
								{
									Source = new BitmapImage(new Uri("ms-appx:///Assets/dot.png")), // Dot image
                                    Width = sizeOfTargets, // Set width based on target size
                                    Height = sizeOfTargets, // Set height based on target size
                                    Margin = new Thickness(0, 0, 0, 10) // Adds spacing below dot
                                },
						new TextBlock
						{
							Text = "And these two will make the game more interesting by spawning at random loaction on board.",
							Foreground = new SolidColorBrush(Windows.UI.Colors.Black), // Text color
                            FontSize = Math.Min(sizeOfTargets - 2, 20), // Dynamic font size
                            Margin = new Thickness(0, 0, 0, 10), // Adds some spacing
                            TextWrapping = TextWrapping.Wrap // Enable text wrapping
                        },
						new StackPanel
						{
							Orientation = Orientation.Horizontal, // Set horizontal orientation for stacking images
                            Children =
							{
								new Image
								{
									Source = new BitmapImage(new Uri("ms-appx:///Assets/enemy.png")), // Player image
                                    Width = sizeOfTargets, // Set width based on target size
                                    Height = sizeOfTargets, // Set height based on target size
                                    Margin = new Thickness(0, 0, 10, 10) // Adds spacing between player and dot
                                },
								new Image
								{
									Source = new BitmapImage(new Uri("ms-appx:///Assets/fruit.png")), // Dot image
                                    Width = sizeOfTargets, // Set width based on target size
                                    Height = sizeOfTargets, // Set height based on target size
                                    Margin = new Thickness(0, 0, 0, 10) // Adds spacing below dot
                                }
							}
						},
						new TextBlock
						{
							Text = "Get familiar with the controls! \nUse the arrow keys to move the player around and capture candies. ",
							Foreground = new SolidColorBrush(Windows.UI.Colors.Black), // Text color
                            FontSize = Math.Min(sizeOfTargets - 2, 20), // Dynamic font size
                            Margin = new Thickness(0, 0, 0, 10), // Adds some spacing
                            TextWrapping = TextWrapping.Wrap // Enable text wrapping
                        },
                        // Difficulty Section
                        new TextBlock
						{
							Text = "GAME DIFFICULTY", // Title for the difficulty section
                            Foreground = new SolidColorBrush(Windows.UI.Colors.Black), // Text color
                            FontSize = Math.Min(sizeOfTargets - 2, 20), // Dynamic font size
                            FontWeight = Windows.UI.Text.FontWeights.Bold, // Font weight
                            Margin = new Thickness(0, 0, 0, 10), // Adds some spacing
                            TextWrapping = TextWrapping.Wrap // Enable text wrapping
                        },
						new TextBlock
						{
							Text = "1. Easy: Enemies spawn slowly, player grows at a slow rate, " +
							"and fruits stay on board before respawning at a different location.",
							Foreground = new SolidColorBrush(Windows.UI.Colors.Black), // Text color
                            FontSize = Math.Min(sizeOfTargets - 2, 20), // Dynamic font size
                            Margin = new Thickness(0, 0, 0, 5), // Adds some spacing
                            TextWrapping = TextWrapping.Wrap // Enable text wrapping
                        },
						new TextBlock
						{
							Text = "2. Medium: Enemies spawn at a medium rate, player grows moderately, " +
							"and fruits stay on board and respawn at a balanced rate.",
							Foreground = new SolidColorBrush(Windows.UI.Colors.Black), // Text color
                            FontSize = Math.Min(sizeOfTargets - 2, 20), // Dynamic font size
                            Margin = new Thickness(0, 0, 0, 5), // Adds some spacing
                            TextWrapping = TextWrapping.Wrap // Enable text wrapping
                        },
						new TextBlock
						{
							Text = "3. Hard: Enemies spawn quickly, player grows rapidly, " +
							"and fruits dont stay on board for long before they respawn. " +
							"You might have to rush to get fruits to stay shrinked",
							Foreground = new SolidColorBrush(Windows.UI.Colors.Black), // Text color
                            FontSize = Math.Min(sizeOfTargets - 2, 20), // Dynamic font size
                            Margin = new Thickness(0, 0, 0, 10), // Adds some spacing
                            TextWrapping = TextWrapping.Wrap // Enable text wrapping
                        },
						 new TextBlock
						{
							Text = "\nGAME LEVELS", // Title for the difficulty section
                            Foreground = new SolidColorBrush(Windows.UI.Colors.Black), // Text color
                            FontSize = Math.Min(sizeOfTargets - 2, 20), // Dynamic font size
                            FontWeight = Windows.UI.Text.FontWeights.Bold, // Font weight
                            Margin = new Thickness(0, 0, 0, 10), // Adds some spacing
                            TextWrapping = TextWrapping.Wrap // Enable text wrapping
                        },
                        // Practice Level Instructions with side-by-side images
                        new TextBlock
						{
							Text = "Practice Level", // Title for the practice level
                            Foreground = new SolidColorBrush(Windows.UI.Colors.Black), // Text color
                            FontSize = Math.Min(sizeOfTargets - 2, 20), // Dynamic font size
                            FontWeight = Windows.UI.Text.FontWeights.Bold, // Font weight
                            Margin = new Thickness(0, 0, 0, 10), // Adds some spacing
                            TextWrapping = TextWrapping.Wrap // Enable text wrapping
                        },
						new TextBlock
						{
							Text = "Get familiar with the controls! Don't worry, no points are counted here. It's just you and a few candies to practice with. " +
							"\nCan you catch them all?",
							Foreground = new SolidColorBrush(Windows.UI.Colors.Black), // Text color
                            FontSize = Math.Min(sizeOfTargets - 2, 20), // Dynamic font size
                            Margin = new Thickness(0, 0, 0, 10), // Adds some spacing
                            TextWrapping = TextWrapping.Wrap // Enable text wrapping
                        },
                        // Level 1 Instructions with image
                        new TextBlock
						{
							Text = "Level 1", // Title for Level 1
                            Foreground = new SolidColorBrush(Windows.UI.Colors.Black), // Text color
                            FontSize = Math.Min(sizeOfTargets - 2, 20), // Dynamic font size
                            FontWeight = Windows.UI.Text.FontWeights.Bold, // Font weight
                            Margin = new Thickness(0, 0, 0, 10) // Adds some spacing
                        },
						new TextBlock
						{
							Text = "Now the real challenge begins! \nWatch out for enemies—they're lurking around. " +
							"Your player stays the same size, but your maneuvering skills will be put to the test. \nStay sharp!",
							Foreground = new SolidColorBrush(Windows.UI.Colors.Black), // Text color
                            FontSize = Math.Min(sizeOfTargets - 2, 20), // Dynamic font size
                            Margin = new Thickness(0, 0, 0, 10), // Adds some spacing
                            TextWrapping = TextWrapping.Wrap // Enable text wrapping
                        },
						new Image
						{
							Source = new BitmapImage(new Uri("ms-appx:///Assets/enemy.png")), // Enemy image
                            Width = sizeOfTargets, // Set width based on target size
                            Height = sizeOfTargets, // Set height based on target size
                            Margin = new Thickness(0, 0, 0, 10) // Adds some spacing below enemy
                        },
                        // Level 2 Instructions with fruit image
                        new TextBlock
						{
							Text = "Level 2", // Title for Level 2
                            Foreground = new SolidColorBrush(Windows.UI.Colors.Black), // Text color
                            FontSize = Math.Min(sizeOfTargets - 2, 20), // Dynamic font size
                            FontWeight = Windows.UI.Text.FontWeights.Bold, // Font weight
                            Margin = new Thickness(0, 0, 0, 10) // Adds some spacing
                        },
						new TextBlock
						{
							Text = "In addition to Level 1, Player size will increase at a rate, collect fruits to stay shrinked! " +
							"\nUse the same controls to get fruits, " +
							"You've got a little more to manage. Fruits will also appear and disappear.",
							Foreground = new SolidColorBrush(Windows.UI.Colors.Black), // Text color
                            FontSize = Math.Min(sizeOfTargets - 2, 20), // Dynamic font size
                            Margin = new Thickness(0, 0, 0, 10), // Adds some spacing
                            TextWrapping = TextWrapping.Wrap // Enable text wrapping
                        },
						new Image
						{
							Source = new BitmapImage(new Uri("ms-appx:///Assets/fruit.png")), // Fruit image
                            Width = sizeOfTargets, // Set width based on target size
                            Height = sizeOfTargets, // Set height based on target size
                            Margin = new Thickness(0, 0, 0, 10) // Adds some spacing below fruit
                        },
						// Level 3 Instructions with fruit image
                        new TextBlock
						{
							Text = "Level 3", // Title for Level 3
                            Foreground = new SolidColorBrush(Windows.UI.Colors.Black), // Text color
                            FontSize = Math.Min(sizeOfTargets - 2, 20), // Dynamic font size
                            FontWeight = Windows.UI.Text.FontWeights.Bold, // Font weight
                            Margin = new Thickness(0, 0, 0, 10) // Adds some spacing
                        },
						new TextBlock
						{
							Text = $"A simple way to put it is that this is Level 2 on timer - {gT} mins."+
							"Grab as many candies as you can keeping yourself fit by consuming fruits in  limited time frame",
							Foreground = new SolidColorBrush(Windows.UI.Colors.Black), // Text color
                            FontSize = Math.Min(sizeOfTargets - 2, 20), // Dynamic font size
                            Margin = new Thickness(0, 0, 0, 10), // Adds some spacing
                            TextWrapping = TextWrapping.Wrap // Enable text wrapping
                        },
                        // End game instructions
						new TextBlock
						{
							Text = "\nGOOD LUCK!", // Title for GOOD LUCK
                            Foreground = new SolidColorBrush(Windows.UI.Colors.Black), // Text color
                            FontSize = Math.Min(sizeOfTargets - 2, 20), // Dynamic font size
                            FontWeight = Windows.UI.Text.FontWeights.Bold, // Font weight
                            Margin = new Thickness(0, 0, 0, 10) // Adds some spacing
                        },
						new TextBlock
						{
							Text = "Remember to have fun while you're at it. " +
							"Don’t get discouraged if you don't succeed right away. \nRemember to enjoy the game!",
							Foreground = new SolidColorBrush(Windows.UI.Colors.Black), // Text color
                            FontSize = Math.Min(sizeOfTargets - 2, 20), // Dynamic font size
                            Margin = new Thickness(0, 0, 0, 10), // Adds some spacing
                            TextWrapping = TextWrapping.Wrap // Enable text wrapping
                        },
					}
						}
					})
				},

				CloseButtonText = "Close"
				// Button configuration for closing the guide
				//PrimaryButtonText = "Close", // Primary button text
				//SecondaryButtonText = "Back to the Game", // Secondary button text for additional functionality
			};

			await practiceDialog.ShowAsync(); // Show the dialog to the user
			guideOpen = false; // Set guide state to closed
		}

		// Asynchronous method to show initial dialogs for user preferences
		public async Task ShowDialogs()
		{
			// First dialog: Ask if the user needs a guide
			var guideDialog = new MessageDialog("BEFORE WE BEGIN LET'S CUSTOMISE YOUR CHOICES: \n\nNeed a User Guide?");
			// Command for "No" option
			guideDialog.Commands.Add(new UICommand("No", new UICommandInvokedHandler(async (cmd) =>
			{
				await ShowFutureGuideDialog(); // Show dialog for future guide preferences
			})));
			// Command for "Yes" option
			guideDialog.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler(async (cmd) =>
			{
				await Guide();  // Show guide to the user
				guideOpen = false; // Mark guide as closed
				await ShowFutureGuideDialog(); // Proceed to future guide preferences
			})));
			// Command for "NO TO ALL" option
			guideDialog.Commands.Add(new UICommand("NO TO ALL", new UICommandInvokedHandler(async (cmd) =>
			{
				noToAll = true;  // Mark no future guides
				futureGuide = false; // Disable future guide
				futureDifficulty = false; // Disable future difficulty changes
				await ShowMainOptions(); // Show main options to the user
				await ShowSeverityOptions();  // Change difficulty options
			})));
			await guideDialog.ShowAsync(); // Display the guide dialog
		}

		// Asynchronous method to show future guide preferences
		public async Task ShowFutureGuideDialog()
		{
			// Dialog asking if the user wants a guide at each level later
			var futureGuideDialog = new MessageDialog("Would you want User Guide at each level later?");
			futureGuideDialog.Commands.Add(new UICommand("No", new UICommandInvokedHandler(async (cmd) =>
			{
				futureGuide = false; // User does not want future guides
				await ShowFutureSeverityDialog(); // Proceed to severity options
			})));
			futureGuideDialog.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler(async (cmd) =>
			{
				futureGuide = true; // User wants future guides
				await ShowFutureSeverityDialog(); // Proceed to severity options
			})));
			futureGuideDialog.Commands.Add(new UICommand("NO MORE CHOICES", new UICommandInvokedHandler(async (cmd) =>
			{
				noToAll = true;  // Mark no future guides
				futureGuide = false; // Disable future guide
				futureDifficulty = false; // Disable future difficulty changes
				await ShowMainOptions(); // Show main options to the user
				await ShowSeverityOptions();  // Change difficulty options
			})));
			await futureGuideDialog.ShowAsync(); // Display the future guide dialog
		}

		// Asynchronous method to ask if the user wants to change the difficulty later
		public async Task ShowFutureSeverityDialog()
		{
			// Dialog for changing the difficulty level during the game
			var futureSeverityDialog = new MessageDialog("Option to Challenge Level during the game?");
			futureSeverityDialog.Commands.Add(new UICommand("No", new UICommandInvokedHandler(async (cmd) =>
			{
				futureDifficulty = false; // User does not want future difficulty changes
				await ShowMainOptions(); // Show main options to the user
				await ShowSeverityOptions();  // Keep the current difficulty options
			})));
			futureSeverityDialog.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler(async (cmd) =>
			{
				futureDifficulty = true; // User wants to change difficulty
				await ShowMainOptions(); // Show main options to the user
				await ShowSeverityOptions();  // Change difficulty options
			})));
			await futureSeverityDialog.ShowAsync(); // Display the future severity dialog
		}

		// Asynchronous method to show difficulty options
		public async Task ShowSeverityOptions()
		{
			if (currentMode != GameMode.Practice) // Check if current mode is not Practice
			{
				string Msg = ""; // Message for the dialog
				var dialog = new MessageDialog($"{Msg}"); // Create a dialog with an empty message

				if (currentMode != GameMode.Practice && futureDifficulty == true) // Check for future difficulty changes
				{
					Msg = $"Your are on {currentDifficulty}, Like to change Challenge level?"; // Set message based on current difficulty
					dialog.Content = Msg;  // Set the dialog message

					// Show different options based on the current difficulty level
					switch (currentDifficulty)
					{
						case GameDifficulty.Easy:
							// Options for Easy difficulty
							dialog.Commands.Add(new UICommand("NO", new UICommandInvokedHandler((_) => Easy()))); // Stay at Easy
							dialog.Commands.Add(new UICommand("Medium", new UICommandInvokedHandler((_) => Medium()))); // Change to Medium
							dialog.Commands.Add(new UICommand("Hard", new UICommandInvokedHandler((_) => Hard())));   // Change to Hard
							break;

						case GameDifficulty.Medium:
							// Options for Medium difficulty
							dialog.Commands.Add(new UICommand("NO", new UICommandInvokedHandler((_) => Medium()))); // Stay at Medium
							dialog.Commands.Add(new UICommand("Lower to Easy", new UICommandInvokedHandler((_) => Easy())));   // Change to Easy
							dialog.Commands.Add(new UICommand("Challenge me", new UICommandInvokedHandler((_) => Hard())));   // Change to Hard
							break;

						case GameDifficulty.Hard:
							// Options for Hard difficulty
							dialog.Commands.Add(new UICommand("NO", new UICommandInvokedHandler((_) => Hard()))); // Stay at Hard
							dialog.Commands.Add(new UICommand("Go Easy", new UICommandInvokedHandler((_) => Easy())));   // Change to Easy
							dialog.Commands.Add(new UICommand("Lower a wee bit", new UICommandInvokedHandler((_) => Medium()))); // Change to Medium
							break;
					}
				}
				else // If user is in Practice mode or does not want to change difficulty
				{
					Msg = "Choose your CHALLENGE LEVEL:"; // Message prompting for difficulty choice
					dialog.Content = Msg;

					// Available options for Practice Mode
					dialog.Commands.Add(new UICommand("Easy", new UICommandInvokedHandler((_) => Easy())));   // Easy option
					dialog.Commands.Add(new UICommand("Medium", new UICommandInvokedHandler((_) => Medium()))); // Medium option
					dialog.Commands.Add(new UICommand("Hard", new UICommandInvokedHandler((_) => Hard())));   // Hard option
				}

				await dialog.ShowAsync();  // Display the severity options dialog
			}
		}

		// Asynchronous method to show main options for the game
		public async Task ShowMainOptions()
		{
			var dialog = new MessageDialog("Choose LEVEL to begin with:"); // Prompt for level selection

			// Commands for selecting game modes
			dialog.Commands.Add(new UICommand("Practice", new UICommandInvokedHandler(Practice))); // Command for Practice mode
			dialog.Commands.Add(new UICommand("Level 1", new UICommandInvokedHandler(Level1))); // Command for Level 1
			dialog.Commands.Add(new UICommand("Quit", new UICommandInvokedHandler(Quit))); // Command for quitting the game
			
			await dialog.ShowAsync(); // Display the main options dialog
		}

		// Asynchronous method to initialize the highest score
		public async Task InitializeHighestScore()
		{
			StorageFolder localFolder = ApplicationData.Current.LocalFolder; // Get the local storage folder
			StorageFile scoreFile; // Declare the score file variable

			try
			{
				// Try to get the file
				scoreFile = await localFolder.GetFileAsync(FileName);

				// Read score from the file
				string fileContent = await FileIO.ReadTextAsync(scoreFile);
				highestScore = int.TryParse(fileContent, out int score) ? score : 0; // Parse the score
			}
			catch (FileNotFoundException)
			{
				// If file not found, create and set default as 0
				highestScore = 0; // Initialize highest score

				scoreFile = await localFolder.CreateFileAsync(FileName, CreationCollisionOption.OpenIfExists); // Create the file
				await FileIO.WriteTextAsync(scoreFile, highestScore.ToString()); // Write the default score to the file
			}
		}

		// Asynchronous method to save the highest score to the file
		public async Task SaveHighestScore()
		{
			StorageFolder localFolder = ApplicationData.Current.LocalFolder; // Get the local storage folder
			StorageFile scoreFile = await localFolder.GetFileAsync(FileName); // Get the score file

			// Write the highest score to the file
			await FileIO.WriteTextAsync(scoreFile, highestScore.ToString());
		}

		// Enumeration for game modes
		public enum GameMode
		{
			Practice, // Only 5 candies spawn to practice, player size does not change, no points either, ask user to move to Level1 or practice again
			Level1,    // Player size does not change, enemies present
			Level2,    // Player size increments, enemies and fruits present
			Level3     // Similar to Level2 with a 3-minute time limit
		}

		// Enumeration for game difficulty levels
		public enum GameDifficulty
		{
			Easy, // Enemy spawn rate per dot is low (more points player can collect before new enemy spawn), player size does not increase at a greater rate (player won't get fat soon), cherry spawn rate is high so fruits are there for a longer time
			Medium,    // Enemy spawn rate per dot is medium, player size does increase at a medium rate, cherry spawn rate is medium
			Hard     // Enemy spawn rate per dot is high, player size increases fast, cherry spawn rate is low
		}

		public void Easy()
		{
			// Set the current difficulty to Easy
			currentDifficulty = GameDifficulty.Easy;

			// No change in size enlarger for Easy difficulty
			sizeEnlarger = sizeEnlarger * 1;

			// No change in enemy spawn rate for Easy difficulty
			enemySpawnRate = enemySpawnRate * 1;

			// No change in fT (game timer or related metric) for Easy difficulty
			fT = fT * 1;
		}

		public void Medium()
		{
			// Set the current difficulty to Medium
			currentDifficulty = GameDifficulty.Medium;

			// Increase size enlarger based on difficulty percentile
			sizeEnlarger = Math.Ceiling(sizeEnlarger + (sizeEnlarger * difficultyPercentile / 100));

			// Decrease enemy spawn rate based on difficulty percentile
			enemySpawnRate = Math.Floor(enemySpawnRate - difficultyPercentile / 10);

			// Decrease fT based on difficulty percentile
			fT = (fT - Math.Floor((double)(difficultyPercentile / 10)));
		}

		public void Hard()
		{
			// Set the current difficulty to Hard
			currentDifficulty = GameDifficulty.Hard;

			// Increase size enlarger based on difficulty percentile, doubled for Hard
			sizeEnlarger = Math.Ceiling(sizeEnlarger + (sizeEnlarger * (10 * 2) / 100));

			// Decrease enemy spawn rate based on difficulty percentile, doubled for Hard
			enemySpawnRate = Math.Floor(enemySpawnRate - difficultyPercentile / (10 * 2));

			// Decrease fT based on difficulty percentile, doubled for Hard
			fT = (fT - Math.Floor((double)(difficultyPercentile * 2 / 10)));
		}

		public void Practice(IUICommand command)
		{
			// Initialize all game elements for Practice mode
			InitializeAll();

			// Start the game without enemies, fruits, or game timer
			mP.PlayGame(enableEnemy = false, enableFruit = false, enableGameTimer = false);
		}

		public void Level1(IUICommand command)
		{
			// Show guide for Level 1 if the future guide is enabled
			if (futureGuide == true)
			{
				Level1Guide();
			}

			// Initialize all game elements for Level 1
			InitializeAll();

			// Start the game with enemies enabled and no fruits or game timer
			mP.PlayGame(enableEnemy = true, enableFruit = false, enableGameTimer = false);
		}

		public async void Level1Guide()
		{
			// Initialize the highest score before showing the guide
			await InitializeHighestScore();

			// Show a message dialog with Level 1 guidance
			await new MessageDialog($"Lookout for enemies as you navigate the game. Stay sharp and avoid them while you collect candies. Best luck!\n\n FYI: Highest score mark is {highestScore}").ShowAsync();
		}

		public void Level2(IUICommand command)
		{
			// Show guide for Level 2 if the future guide is enabled
			if (futureGuide == true)
			{
				Level2Guide();
			}

			// Initialize all game elements for Level 2
			InitializeAll();

			// Start the game with enemies and fruits enabled, and no game timer
			mP.PlayGame(enableEnemy = true, enableFruit = true, enableGameTimer = false);
		}

		public async void Level2Guide()
		{
			// Set gameOn to false before showing the guide
			gameOn = false;

			// Initialize the highest score before showing the guide
			await InitializeHighestScore();

			// Show a message dialog with Level 2 guidance
			await new MessageDialog($"With you growing in size and enemies spawning randomly, collect as many candies and fruits as possible. Stay shrinked!\n\n FYI: Highest score mark is {highestScore}").ShowAsync();
		}

		public void Level3(IUICommand command)
		{
			// Show guide for Level 3 if the future guide is enabled
			if (futureGuide == true)
			{
				Level3Guide();
			}

			// Initialize all game elements for Level 3
			InitializeAll();

			// Start the game with enemies and fruits enabled, and game timer active
			mP.PlayGame(enableEnemy = true, enableFruit = true, enableGameTimer = true);
		}

		public async void Level3Guide()
		{
			// Set gameOn to false before showing the guide
			gameOn = false;

			// Initialize the highest score before showing the guide
			await InitializeHighestScore();

			// Show a message dialog with Level 3 guidance
			await new MessageDialog($"Same as Level 2 with a time trial of {gT} min. \n Your timer begins upon closing this dialog box. Good luck!\n\n FYI: Highest score mark is {highestScore}").ShowAsync();
		}

		public void Quit(IUICommand command)
		{
			// Exit the application
			Application.Current.Exit();
		}

		public async void NextStepDialog()
		{
			// Show a dialog for selecting the next level or quitting
			var dialog = new MessageDialog($"Choose your LEVEL:");

			// Switch statement to provide options based on the current game mode
			// Create a ComboBox for level selection
			ComboBox levelComboBox = new ComboBox
			{
				PlaceholderText = "Select a Level",
				Width = 200 // Set width to ensure it's visible
			};
			switch (currentMode)
			{
				case GameMode.Practice:
					dialog.Commands.Add(new UICommand("LEVEL 1", new UICommandInvokedHandler(Level1)));
					dialog.Commands.Add(new UICommand("PRACTICE AGAIN", new UICommandInvokedHandler(Practice)));
					dialog.Commands.Add(new UICommand("Quit", new UICommandInvokedHandler(Quit)));
					
					
					break;

				case GameMode.Level1:
					dialog.Commands.Add(new UICommand("LEVEL 2", new UICommandInvokedHandler(Level2)));
					dialog.Commands.Add(new UICommand("Replay LEVEL 1", new UICommandInvokedHandler(Level1)));
					dialog.Commands.Add(new UICommand("Quit", new UICommandInvokedHandler(Quit)));
					// Show difficulty options if enabled
					if (futureDifficulty == true)
					{
						await ShowSeverityOptions();
					}
					break;

				case GameMode.Level2:
					gameOn = false;

					dialog.Commands.Add(new UICommand("LEVEL 3", new UICommandInvokedHandler(Level3)));
					dialog.Commands.Add(new UICommand("Replay LEVEL 2", new UICommandInvokedHandler(Level2)));
					//dialog.Commands.Add(new UICommand("Step back to LEVEL 1", new UICommandInvokedHandler(Level1)));
					dialog.Commands.Add(new UICommand("Quit", new UICommandInvokedHandler(Quit)));
					// Show difficulty options if enabled
					if (futureDifficulty == true)
					{
						await ShowSeverityOptions();
					}
					break;

				case GameMode.Level3:
					gameOn = false;
					string selectedLevel = levelComboBox.SelectedItem as string;
					dialog.Commands.Add(new UICommand("Replay LEVEL 3", new UICommandInvokedHandler(Level3)));
					//dialog.Commands.Add(new UICommand("Step back to LEVEL 2", new UICommandInvokedHandler(Level2)));
					dialog.Commands.Add(new UICommand("Quit", new UICommandInvokedHandler(Quit)));
					// Show difficulty options if enabled
					if (futureDifficulty == true)
					{
						await ShowSeverityOptions();
					}
					break;

				default:
					dialog.Commands.Add(new UICommand("Quit", new UICommandInvokedHandler(Quit)));
					break;
			}

			// Reset the game state before showing the dialog
			mP.ResetGame();
			await dialog.ShowAsync();
		}

		public void StopAllTimers()
		{
			// Stop the fruit timer if it is enabled
			if (fruitTimer.IsEnabled)
			{
				fruitTimer.Stop();
				fruitTimer.Tick -= mP.FruitTimerEffect; // Unsubscribe from the event
			}

			// Stop the game timer if it is enabled
			if (gameTimer.IsEnabled)
			{
				gameTimer.Stop();
				gameTimer.Tick -= mP.GameTimerEffet; // Unsubscribe from the event
			}
		}

		public void InitializeAll()
		{
			// Initialize lists for game pieces
			gamePieces = new List<GamePiece>();
			manyDots = new List<GamePiece>();
			manyEnemies = new List<GamePiece>();
			manyFruits = new List<GamePiece>();
			borders = new List<GamePiece>();
		}

		public void FruitTimer(int t)
		{
			// Set up the timer for fruit appearance
			fruitTimer.Tick += mP.FruitTimerEffect; // Subscribe to the timer's tick event
			fruitTimer.Interval = new TimeSpan(0, 0, t); // Set the timer interval (every t seconds)
			fruitTimer.Start(); // Start the timer
		}

		public void GameTimer(int t)
		{
			// Set up the timer for game time limit
			gameTimer.Tick += mP.GameTimerEffet; // Subscribe to the timer's tick event
			gameTimer.Interval = new TimeSpan(0, 0, t * 60); // Set the timer interval (every t minutes)
			gameTimer.Start(); // Start the timer
		}

		public async void GameEndDialog()
		{
			// Initialize the highest score before showing the game end dialog
			await InitializeHighestScore();

			// If the game mode is not Practice, check scores
			if (currentMode != GameMode.Practice)
			{
				// Check if the current score exceeds the highest score
				if (highScore <= highestScore)
				{
					await new MessageDialog($"Game Over! Score {score} " +
						$"\n This game High score is {highScore} " +
						$"\n\nHighest score mark is {highestScore}").ShowAsync();
				}
				else if (highScore > highestScore)
				{
					// Update highest score and save it to file
					highestScore = highScore; // Update highestScore
					await SaveHighestScore(); // Save it to the file
					await new MessageDialog($"GAME OVER WITH A NEW HIGH SCORE!!! " +
						$"Your score was {score} " +
						$"\n\nCongratulations! You have set a new High score: {highestScore}").ShowAsync();
				}
			}
			else
			{
				// If in Practice mode, show only the current score
				await new MessageDialog($"Congratulation! You have successfully completed practice level. " +
					$"\nNo score is recorded during practice. Going to further levels the score will be noted." +
					$"\n\nHighest score mark is {highestScore}").ShowAsync();
			}

			// Call NextStepDialog to determine the next course of action
			NextStepDialog();
		}

		public bool Collision(GamePiece a, GamePiece b)
		{
			// Create a rectangle for GamePiece 'a' using its location and dimensions
			var aPosition = new Rect(a.Location.Left, a.Location.Top, a.Image.Width, a.Image.Height);

			// Create a rectangle for GamePiece 'b' using its location and dimensions
			var bPosition = new Rect(b.Location.Left, b.Location.Top, b.Image.Width, b.Image.Height);

			// Check if the rectangles overlap, indicating a collision
			return aPosition.Left < bPosition.Right &&  // 'a' is to the left of 'b'
				   aPosition.Right > bPosition.Left &&  // 'a' is to the right of 'b'
				   aPosition.Top < bPosition.Bottom &&   // 'a' is above 'b'
				   aPosition.Bottom > bPosition.Top;     // 'a' is below 'b'
		}

		public bool SpotTaken(double left, double top, double size, List<GamePiece> g)
		{
			// Calculate the right and bottom boundaries of the new game piece
			double newRight = left + size;  // Right boundary of the new piece
			double newBottom = top + size;   // Bottom boundary of the new piece

			// Loop through each existing game piece to check for overlaps
			foreach (var piece in g)
			{
				// Get the position and size of the existing game piece
				double existingLeft = piece.Location.Left; // Actual property for the left position
				double existingTop = piece.Location.Top;   // Actual property for the top position
				double existingSize = piece.Size;          // Actual property for size (assumed square)

				// Calculate the right and bottom boundaries of the existing piece
				double existingRight = existingLeft + existingSize;
				double existingBottom = existingTop + existingSize;

				// Check for overlap between the new piece and the existing piece
				if (newRight > existingLeft && left < existingRight && // Check horizontal overlap
					newBottom > existingTop && top < existingBottom)   // Check vertical overlap
				{
					return true; // Overlap detected
				}
			}
			return false; // No overlap detected
		}

	}
}
