using UnityEditor;

namespace FastNoise2.Editor
{
	public static class LaunchFastNoiseToolMenuItem
	{
		[MenuItem("Tools/Launch FastNoiseNoiseTool")]
		static void DoLaunch()
		{
			NoiseToolProxy.NoiseToolProxy.LaunchNoiseTool();
		}
	}
}
