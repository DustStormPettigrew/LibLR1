using System.Collections.Generic;

namespace LibLR1.Contracts
{
	public class TrackGradientStop
	{
		public float Position { get; set; }
		public TrackColor Color { get; set; }
		public Dictionary<string, string> Metadata { get; private set; }

		public TrackGradientStop()
		{
			Color = new TrackColor(1f, 1f, 1f, 1f);
			Metadata = new Dictionary<string, string>();
		}
	}
}
