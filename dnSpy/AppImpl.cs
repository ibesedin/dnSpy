﻿/*
    Copyright (C) 2014-2015 de4dot@gmail.com

    This file is part of dnSpy

    dnSpy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    dnSpy is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with dnSpy.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using dnSpy.Contracts;
using dnSpy.Contracts.Images;
using dnSpy.Contracts.Menus;
using dnSpy.Contracts.Themes;
using dnSpy.Contracts.ToolBars;
using dnSpy.Images;
using dnSpy.Menus;
using dnSpy.Themes;
using dnSpy.ToolBars;

namespace dnSpy {
	public sealed class AppImpl : IApp {//TODO: Shouldn't be public
		public Version Version {
			get { return GetType().Assembly.GetName().Version; }
		}

		public IMenuManager MenuManager {
			get { return menuManager; }
		}
		readonly MenuManager menuManager;

		public IToolBarManager ToolBarManager {
			get { return toolBarManager; }
		}
		readonly ToolBarManager toolBarManager;

		public IThemeManager ThemeManager {
			get { return themeManager; }
		}
		readonly ThemeManager themeManager;

		public IImageManager ImageManager {
			get { return imageManager; }
		}
		readonly ImageManager imageManager;

		public CompositionContainer CompositionContainer {
			get { return compositionContainer; }
		}
		CompositionContainer compositionContainer;

		public AppImpl() {
			DnSpy.App = this;
			this.menuManager = new MenuManager(this);
			this.toolBarManager = new ToolBarManager(this);
			this.themeManager = new ThemeManager(this);
			this.imageManager = new ImageManager(this);
		}

		public void InitializeCompositionContainer(IEnumerable<Assembly> asms, string pattern) {
			var aggregateCatalog = new AggregateCatalog();
			var ourAsm = GetType().Assembly;
			aggregateCatalog.Catalogs.Add(new AssemblyCatalog(ourAsm));
			foreach (var asm in asms) {
				if (ourAsm != asm)
					aggregateCatalog.Catalogs.Add(new AssemblyCatalog(asm));
			}
			AddFiles(aggregateCatalog, pattern);
			compositionContainer = new CompositionContainer(aggregateCatalog);
		}

		void AddFiles(AggregateCatalog aggregateCatalog, string pattern) {
			var dir = Path.GetDirectoryName(GetType().Assembly.Location);
			var random = new Random();
			var files = Directory.GetFiles(dir, pattern).OrderBy(a => random.Next()).ToArray();
			foreach (var file in files) {
				try {
					aggregateCatalog.Catalogs.Add(new AssemblyCatalog(Assembly.LoadFile(file)));
				}
				catch {
					Debug.Fail(string.Format("Failed to load file '{0}'", file));
				}
			}
		}

		public void InitializeThemes(string themeName) {
			themeManager.Initialize(themeName);
		}

		public void UpdateResources(ITheme theme, System.Windows.ResourceDictionary resources) {//TODO: REMOVE
			((Theme)theme).UpdateResources(resources);
		}
	}
}
