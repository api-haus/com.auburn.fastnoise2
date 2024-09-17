using System.Linq;
using UnityEditor;
using UnityEngine;
using FastNoise2.Authoring;

namespace FastNoise2.Editor.NoiseAssets
{
	[CustomEditor(typeof(NoiseAsset))]
	public class NoiseAssetEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (GUILayout.Button("Bake"))
			{
				var targetAssets = targets.OfType<NoiseAsset>();

				foreach (var noiseAsset in targetAssets)
				{
					string assetPath = AssetDatabase.GetAssetPath(noiseAsset);
					noiseAsset.BakeIntoAsset(assetPath);
				}

				AssetDatabase.Refresh();
			}
		}
	}
}
