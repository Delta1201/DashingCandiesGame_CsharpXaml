using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace GameLibrary
{
	// GamePiece class represents a game object that can be moved on the board with an associated image and location
	public class GamePiece : IComparable<GamePiece>, IEquatable<GamePiece>
	{
		// Private fields
		private Thickness objectMargins;            // Represents the location (margins) of the game piece on the board
		private Image onScreen;                     // The image that represents the game piece on the screen
		private RotateTransform rotate;             // Handles rotation of the game piece's image

		// Public properties
		public Thickness Location => onScreen.Margin; // Provides read-only access to the location (image margins)
		public Image Image => onScreen;               // Provides access to the Image displayed on the screen

		public int Points { get; set; }  // Points associated with the game piece (for scoring purposes)
		public int Size { get; set; }    // Size of the game piece (could be used for scaling or collision logic)

		private static Random random = new Random();  // Static random instance for any randomness needed in the game (e.g., spawn location)

		// Constructor that initializes the game piece with an image and points
		public GamePiece(Image img, int point = 1)
		{
			onScreen = img;               // Assigns the image to the onScreen variable
			objectMargins = img.Margin;    // Sets the initial location (margins) of the game piece
			Points = point;                // Initializes points for the game piece

			// Initialize rotation of the image (centered pivot point)
			rotate = new RotateTransform()
			{
				CenterX = img.Width / 2,   // Set the X-axis rotation center to the middle of the image
				CenterY = img.Height / 2   // Set the Y-axis rotation center to the middle of the image
			};
			onScreen.RenderTransform = rotate;  // Apply the rotation to the image

			// Subscribe to the SizeChanged event of the image to adjust rotation when the size changes
			onScreen.SizeChanged += OnImageSizeChanged;
		}

		// Event handler to adjust the rotation center when the image size changes
		private void OnImageSizeChanged(object sender, SizeChangedEventArgs e)
		{
			rotate.CenterX = onScreen.ActualWidth / 2;   // Update the X-axis rotation center
			rotate.CenterY = onScreen.ActualHeight / 2;  // Update the Y-axis rotation center
		}

		// Destructor to unsubscribe from events (currently commented out coz it was breaking)
		//~GamePiece()
		//{
		//	onScreen.SizeChanged -= OnImageSizeChanged;
		//}

		// Moves the game piece based on the direction of key pressed
		public bool Move(Windows.System.VirtualKey direction)
		{
			switch (direction)
			{
				case Windows.System.VirtualKey.Up:
					objectMargins.Top -= 10;  // Move upwards by decreasing the Top margin
					rotate.Angle = -90;       // Rotate image to face upwards
					break;
				case Windows.System.VirtualKey.Down:
					objectMargins.Top += 10;  // Move downwards by increasing the Top margin
					rotate.Angle = 90;        // Rotate image to face downwards
					break;
				case Windows.System.VirtualKey.Left:
					objectMargins.Left -= 10; // Move left by decreasing the Left margin
					rotate.Angle = -180;      // Rotate image to face left
					break;
				case Windows.System.VirtualKey.Right:
					objectMargins.Left += 10; // Move right by increasing the Left margin
					rotate.Angle = 0;         // Rotate image to face right
					break;
				default:
					return false; // If direction is not recognized, do nothing
			}
			onScreen.Margin = objectMargins; // Apply the new position (margins) to the image
			return true;
		}

		// Implements the IComparable interface to compare the locations of two game pieces (based on Top and Left margins)
		public int CompareTo(GamePiece other)
		{
			if (other == null) return 1;

			// Compare the Top margin first
			int topComparison = this.Location.Top.CompareTo(other.Location.Top);

			if (topComparison != 0)
				return topComparison;

			// If Top is the same, compare the Left margin
			return this.Location.Left.CompareTo(other.Location.Left);
		}

		// Implements the IEquatable interface to check if two game pieces are located in the same position
		public bool Equals(GamePiece other)
		{
			if (other == null) return false;
			return this.Location.Equals(other.Location);  // Check if their locations are the same
		}

		// Updates the game piece's location (margins) to a new position
		public void NewLocation(Thickness newLocation)
		{
			objectMargins = newLocation;    // Set the new location (margins)
			onScreen.Margin = objectMargins; // Apply the new margins to the image
		}
	}
}

