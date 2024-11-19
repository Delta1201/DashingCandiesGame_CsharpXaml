using GameLibrary; 
using System; 
using System.Collections.Generic; 
using System.IO; 
using System.Linq; 
using System.Threading.Tasks; 
using Windows.Foundation; 
using Windows.Storage; 
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups; 
using Windows.UI.Xaml; 
using Windows.UI.Xaml.Controls; 
using Windows.UI.Xaml.Media; 
using Windows.UI.Xaml.Media.Imaging; 
using static GameInterface.NotMain; 

/* UWP Game Template
 * Created By: Melissa VanderLely
 * Modified By: Dhaval Tailor
 * Note: this is an extension of 1A 
 * ChatGpt and StackOverflow were used in 
 */

namespace GameInterface
{
	public sealed partial class MainPage : Page // MainPage class inherits from Page
	{
		private NotMain nM; // Reference to NotMain instance
		public readonly Random placement = new Random(); // Random object for placement of game pieces
		public ScrollViewer practiceScrollViewer; // Declare a ScrollViewer variable to handle scrolling
		public MainPage() // Constructor for MainPage
		{
			this.InitializeComponent(); // Initialize UI components first
			nM = new NotMain(this); // Now initialize NotMain

			// Setting up event handlers and additional initialization
			Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown; // Handle key down events

			Loaded += async delegate
			{
				// Ensure to access ActualWidth and ActualHeight after Loaded event
				nM.screenHeight = gridMain.ActualHeight; // Set screen height
				nM.screenWidth = gridMain.ActualWidth; // Set screen width

				// Calculate sizes based on the screen dimensions
				nM.sizeOfTargets = Math.Max(nM.screenHeight, nM.screenWidth) * 0.02; // Calculate size of targets

				// Set fruit and player sizes based on target size
				nM.sizeFruits = nM.sizeOfTargets * 1.25; // Calculate size for fruits
				nM.sizePlayer = nM.sizeOfTargets * 2.0; // Calculate size for player

				// Fire up the dialogs or any other initialization needed
				await nM.ShowDialogs(); // Show initialization dialogs
			};
		}

		public GamePiece PieceUniqueSpot(string imgSrc, double size, List<GamePiece> g) // Method to place game pieces uniquely
		{
			if (g != null) // Ensure game piece list is not null
			{
				double left; // Left position for the game piece
				double top; // Top position for the game piece
				double space = size * 4; // Space around the piece to prevent overlapping

				do
				{
					// Generate random positions ensuring enough space
					left = placement.Next((int)(space), (int)(nM.screenWidth - (space + size))); // Random left position
					top = placement.Next((int)(space), (int)(nM.screenHeight - (space + size))); // Random top position
				}
				while (nM.SpotTaken(left, top, size, g)); // Pass left, top, and size to SpotTaken for accurate checking

				// Return the newly created game piece, positioned away from others
				return CreatePiece(imgSrc, size, (int)left, (int)top); // Create and return game piece
			}
			return null; // Return null if the game piece list is null
		}

		public void ResetGame() // Method to reset the game state
		{
			nM.StopAllTimers(); // Stop any active timers

			var gamePieceLists = new List<GamePiece>[] { nM.gamePieces, nM.manyDots, nM.manyEnemies, nM.manyFruits }; // List of all game piece lists

			foreach (var list in gamePieceLists) // Iterate through each game piece list
			{
				if (list != null) // Ensure the list is not null
				{
					// Remove all game pieces
					foreach (var piece in list.ToList()) // Create a copy to avoid modification during iteration
					{
						gridMain.Children.Remove(piece.Image); // Remove game piece image from grid
					}
					list?.Clear(); // Clear the list of game pieces
				}
			}
			// Remove all images especially the boom after collision with enemy
			gridMain.Children.Clear(); // Clear the grid of all children

			// Reset scores and counters
			nM.score = 0; // Reset score
						  // highScore = 0; // Uncomment to reset high score if needed
			nM.gameOn = false; // Set game status to not running
		}

		public void GameTimerEffet(object sender, object e) // Handle game timer effects
		{
			if (nM.currentMode == GameMode.Level3) // Check if the current mode is Level 3
			{
				// Just declaring that game has stopped
				nM.gameOn = false; // Stop the game

				// Remove player image
				gridMain.Children.Remove(player.Image); // Remove player from grid

				// Remove all enemy images
				if (nM.manyEnemies != null && nM.manyEnemies.Count > 0)
				{
					nM.manyEnemies.ToList().ForEach(enemy => gridMain.Children.Remove(enemy.Image)); // Remove each enemy image
				}
				// Remove all fruit images
				if (nM.manyFruits != null && nM.manyFruits.Count > 0)
				{
					nM.manyFruits.ToList().ForEach(fruit => gridMain.Children.Remove(fruit.Image)); // Remove each fruit image
				}
				// Remove all dot images
				if (nM.manyDots != null && nM.manyDots.Count > 0)
				{
					nM.manyDots.ToList().ForEach(dot => gridMain.Children.Remove(dot.Image)); // Remove each dot image
				}
			}
			nM.GameEndDialog(); // Show end game dialog
		}

		public GamePiece CreatePiece(string imgSrc, double size, int left, int top) // Method to create a game piece
		{
			Image img = new Image(); // Create a new Image instance
			img.Source = new BitmapImage(new Uri($"ms-appx:///Assets/{imgSrc}.png")); // Set the image source
			img.Width = size; // Set the image width
			img.Height = size; // Set the image height
			img.Name = $"img{imgSrc}"; // Set a unique name for the image
			img.Margin = new Thickness(left, top, 0, 0); // Set the position of the image
			img.VerticalAlignment = VerticalAlignment.Top; // Align the image to the top
			img.HorizontalAlignment = HorizontalAlignment.Left; // Align the image to the left

			gridMain.Children.Add(img); // Add the image to the grid

			return new GamePiece(img); // Return a new GamePiece instance with the created image
		}

		public async void FruitTimerEffect(object sender, object e) // Handle fruit timer effects
		{
			if (!nM.gameOn) return; // If the game is not running, exit the method

			await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Run the following code on the UI thread
			{
				// Delete the existing fruit if not used in collision and then place another at a unique spot
				if (nM.manyFruits.Count > 0) // Check if there are existing fruits
				{
					foreach (var fruit in nM.manyFruits.ToList()) // Create a copy of the list for iteration
					{
						gridMain.Children.Remove(fruit.Image); // Remove the fruit image from the grid
						nM.manyFruits.Remove(fruit); // Remove the fruit from the list
					}

					// Add a new fruit at a unique spot
					nM.manyFruits.Add(PieceUniqueSpot("fruit", nM.sizeFruits, nM.manyFruits.Concat(nM.gamePieces).ToList())); // Place a new fruit
				}
			});
		}

		public void CollideDots()
		{
			if (nM.manyDots != null) // Check if there are any dots
			{
				foreach (var dot in nM.manyDots.ToList()) // Iterate through each dot
				{
					if (nM.Collision(player, dot)) // Check for a collision between the player and the dot
					{
						// Remove the dot's image from the UI
						gridMain.Children.Remove(dot.Image);
						nM.manyDots.Remove(dot); // Remove the dot from the collection

						// Increment score if not in practice mode
						if (nM.currentMode != GameMode.Practice)
						{
							nM.score += dot.Points; // Add dot's points to the score
						}

						// Check if enlarging is enabled
						if (nM.enableEnlarger == true)
						{
							// Increase the player's size based on the dot's points
							player.Image.Width += dot.Points * nM.sizeEnlarger;
							player.Image.Height += dot.Points * nM.sizeEnlarger;
						}

						// Check if enemies are enabled
						if (nM.enableEnemy == true)
						{
							// Check if a new enemy should be added based on spawn rate
							if (nM.addedDotCount > nM.enemySpawnRate)
							{
								nM.manyEnemies.Add(PieceUniqueSpot("enemy", nM.sizeOfTargets, nM.manyEnemies));
								nM.addedDotCount = 0; // Reset the added dot count
							}
						}

						// If dots are not limited, add a new dot to the game
						if (nM.limitDots == false)
						{
							nM.manyDots.Add(PieceUniqueSpot("dot", nM.sizeOfTargets, nM.manyDots));
							nM.addedDotCount++; // Increment the added dot count
						}

						// Check for end of game conditions in practice mode
						if (nM.currentMode == GameMode.Practice && nM.manyDots.Count == 0)
						{
							player.Image.Visibility = Visibility.Collapsed; // Hide the player's image
							nM.GameEndDialog(); // Show the game end dialog
							break; // Exit the loop after handling the collision
						}
					}
				}
			}

			// Update the high score if the current score exceeds it
			if (nM.score > nM.highScore)
			{
				nM.highScore = nM.score;
			}
		}


		public void CollideFruits()
		{
			// Check if there are any fruits
			if (nM.manyFruits != null)
			{
				foreach (var fruit in nM.manyFruits.ToList()) // Iterate through each fruit
				{
					if (nM.Collision(player, fruit)) // Check for a collision between the player and the fruit
					{
						// Remove the fruit's image from the UI
						gridMain.Children.Remove(fruit.Image);
						nM.manyFruits.Remove(fruit); // Remove the fruit from the collection

						// Reset the player's size to the original size
						player.Image.Width = nM.sizePlayer;
						player.Image.Height = nM.sizePlayer;

						// If no fruits are left, add a new fruit to the game
						if (nM.manyFruits.Count == 0)
						{
							nM.manyFruits.Add(PieceUniqueSpot("fruit", nM.sizeFruits, nM.manyFruits));
						}
					}
				}
			}
		}


		public void CollideEnemy()
		{
			if (nM.manyEnemies != null && nM.gameOn == true) // Check if there are enemies and the game is running
			{
				foreach (var enemy in nM.manyEnemies.ToList()) // Iterate through each enemy
				{
					if (nM.Collision(player, enemy)) // Check for a collision between the player and the enemy
					{
						// Stop the game if there is a collision
						nM.gameOn = false;

						// Remove the player and enemy images from the UI
						gridMain.Children.Remove(player.Image);
						gridMain.Children.Remove(enemy.Image);

						// Create a "boom" effect at the enemy's location
						CreatePiece("boom", 100, (int)enemy.Location.Left, (int)enemy.Location.Top);

						// Hide the player's image
						player.Image.Visibility = Visibility.Collapsed;

						// Show the game end dialog
						nM.GameEndDialog();

						// Ensure the game is marked as not running
						nM.gameOn = false;
						break; // Exit the loop after handling the collision
					}
				}
			}
		}

		public void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs e)
		{
			if (nM.guideOpen == true)
			{
				// Handle arrow keys for scrolling
				if (e.VirtualKey == Windows.System.VirtualKey.Down)
				{
					// Scroll down
					practiceScrollViewer.ChangeView(null, practiceScrollViewer.VerticalOffset + 20, null); // Adjust the scroll amount as needed
					e.Handled = true; // Mark the event as handled
				}
				else if (e.VirtualKey == Windows.System.VirtualKey.Up)
				{
					// Scroll up
					practiceScrollViewer.ChangeView(null, practiceScrollViewer.VerticalOffset - 20, null); // Adjust the scroll amount as needed
					e.Handled = true; // Mark the event as handled
				}
				return; // Exit to prevent player movement
			}

			// Calculate new location for the player character
			player.Move(e.VirtualKey);

			var previousLocation = player.Location;

			// Collision with borders
			foreach (var fence in nM.borders.ToList())
			{
				if (nM.Collision(player, fence))
				{
					// player is colliding with the left border
					if (player.Location.Left <= 0)
					{
						// move the player slightly in opposite direction
						previousLocation.Left += 10;
					}
					// player is colliding with the right border
					else if (player.Location.Left >= nM.screenWidth - player.Image.Width)
					{
						// Move the player slightly in opposite direction
						previousLocation.Left -= 10;
					}
					// player is colliding with the top border
					else if (player.Location.Top <= 0)
					{
						// move the player slightly in opposite direction
						previousLocation.Top += 10;
					}
					// player is colliding with the bottom border
					else if (player.Location.Top >= nM.screenHeight - player.Image.Height)
					{
						// move the player slightly in opposite direction
						previousLocation.Top -= 10;
					}

					// Update player location to previous location
					player.NewLocation(previousLocation);
				}
			}

			// Collision with dots
			CollideDots();
			// Collision with fruits
			CollideFruits();
			// Collision with enemies
			CollideEnemy();
		}

		public void PlayGame(bool enableEnemy = false, bool enableFruits = false, bool enableGameTimer = false)
		{
			// Initializing lists
			nM.gamePieces = new List<GamePiece>();
			nM.borders = new List<GamePiece>();

			// Side borders
			for (int i = 0; i < nM.screenHeight; i++)
			{
				nM.borders.Add(CreatePiece("dot5", nM.sizeOfTargets / 2, 0, i)); // Left border
				nM.borders.Add(CreatePiece("dot5", nM.sizeOfTargets / 2, (int)(nM.screenWidth - (nM.sizeOfTargets / 2)), i)); // Right border
			}

			// Top and bottom borders
			for (int i = 0; i < nM.screenWidth; i++)
			{
				nM.borders.Add(CreatePiece("dot5", nM.sizeOfTargets / 2, i, 0)); // Top border
				nM.borders.Add(CreatePiece("dot5", nM.sizeOfTargets / 2, i, (int)(nM.screenHeight - (nM.sizeOfTargets / 2)))); // Bottom border
			}

			nM.gamePieces.AddRange(nM.borders);

			// Create player at a unique spot
			player = PieceUniqueSpot("player", nM.sizePlayer, new List<GamePiece>());
			nM.gamePieces.Add(player);

			// Initialize dots
			nM.manyDots = new List<GamePiece>();
			for (int i = 0; i < (nM.dotCount); i++)
			{
				nM.manyDots.Add(PieceUniqueSpot("dot", nM.sizeOfTargets, nM.gamePieces.Concat(nM.manyDots).ToList()));
			}

			// Check if enemies need to be generated
			if (enableEnemy == true && nM.enableFruit == false && enableGameTimer == false)
			{
				enableEnemy = true;
				nM.limitDots = false;
				// Generate enemies
				nM.manyEnemies = new List<GamePiece>();
				for (int i = 0; i < (int)Math.Floor(nM.dotCount / 2.0); i++)
				{
					nM.manyEnemies.Add(PieceUniqueSpot("enemy", nM.sizeOfTargets, nM.gamePieces.Concat(nM.manyEnemies).ToList()));
				}
				nM.currentMode = GameMode.Level1;
			}
			// Check if fruits need to be generated
			else if (enableFruits == true && nM.enableFruit == true && enableGameTimer == false)
			{
				nM.enableFruit = true;
				nM.enableEnlarger = true;
				// Generate enemies
				nM.manyEnemies = new List<GamePiece>();
				for (int i = 0; i < (int)Math.Floor(nM.dotCount / 2.0); i++)
				{
					nM.manyEnemies.Add(PieceUniqueSpot("enemy", nM.sizeOfTargets, nM.gamePieces.Concat(nM.manyEnemies).ToList()));
				}

				nM.FruitTimer((int)(double)nM.fT);
				// Generate fruits
				nM.manyFruits = new List<GamePiece>();
				for (int i = 0; i < (nM.fruitCount); i++)
				{
					nM.manyFruits.Add(PieceUniqueSpot("fruit", nM.sizeFruits, nM.gamePieces.Concat(nM.manyFruits).ToList()));
				}
				nM.currentMode = GameMode.Level2;
			}
			else if (enableGameTimer == true && nM.enableFruit == true && enableGameTimer == true)
			{
				nM.enableFruit = true;
				nM.enableEnlarger = true;
				enableEnemy = true;
				nM.limitDots = false;
				// Generate enemies
				nM.manyEnemies = new List<GamePiece>();
				for (int i = 0; i < (int)Math.Floor(nM.dotCount / 2.0); i++)
				{
					nM.manyEnemies.Add(PieceUniqueSpot("enemy", nM.sizeOfTargets, nM.gamePieces.Concat(nM.manyEnemies).ToList()));
				}
				nM.FruitTimer(10);
				// Generate fruits
				nM.manyFruits = new List<GamePiece>();
				for (int i = 0; i < (nM.fruitCount); i++)
				{
					nM.manyFruits.Add(PieceUniqueSpot("fruit", nM.sizeFruits, nM.gamePieces.Concat(nM.manyFruits).ToList()));
				}
				enableGameTimer = true;
				nM.GameTimer((int)(double)nM.gT);
				nM.currentMode = GameMode.Level3;
			}
			else if (enableEnemy == false)
			{
				enableEnemy = false;
				nM.limitDots = true;
				nM.currentMode = GameMode.Practice;
			}
			nM.gameOn = true; // Start the game
		}

	}

}
