using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Gem
{
	public class EpisodeContentManager : ContentManager
	{
		private Dictionary<String, Texture2D> TextureTable = new Dictionary<string, Texture2D>();

		public Texture2D GetTexture(String name)
		{
			if (String.IsNullOrEmpty(name)) return null;
			if (!TextureTable.ContainsKey(name))
			{
				Texture2D Tex = null;
				try
				{
					Tex = this.Load<Texture2D>(name);
				}
				catch (ContentLoadException) { }
				TextureTable.Add(name, Tex);
				return Tex;
			}
			return TextureTable[name];
		}

		public EpisodeContentManager(IServiceProvider ServiceProvider, String RootDirectory)
			: base(ServiceProvider, RootDirectory)
		{
		}

		public System.IO.TextReader OpenTextStream(string assetName)
		{
			return new System.IO.StreamReader(OpenStream(assetName));
		}

		protected override System.IO.Stream OpenStream(string assetName)
		{
			return System.IO.File.OpenRead(
				System.IO.Path.Combine(Environment.CurrentDirectory, this.RootDirectory, assetName + ".xnb"));
		}

		public System.IO.TextReader OpenUnbuiltTextStream(string assetName)
		{
			try
			{
				return new System.IO.StreamReader(System.IO.Path.Combine(this.RootDirectory, assetName));
			}
			catch (ContentLoadException)
			{
				throw new ContentLoadException("Could not find raw asset " + assetName);
			}
		}

		public System.IO.StreamReader OpenUnbuiltStream(string assetName)
		{
			try
			{
				return new System.IO.StreamReader(System.IO.Path.Combine(this.RootDirectory, assetName));
			}
			catch (ContentLoadException)
			{
				throw new ContentLoadException("Could not find raw asset " + assetName);
			}
		}
	}
}
