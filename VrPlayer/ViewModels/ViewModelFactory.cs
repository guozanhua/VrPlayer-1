﻿using VrPlayer.Models;
using VrPlayer.Models.Config;
using VrPlayer.Models.Plugins;
using VrPlayer.Models.State;

namespace VrPlayer.ViewModels
{
    public class ViewModelFactory
    {
        private readonly IApplicationConfig _config;
        private readonly IPluginManager _pluginManager;
        private readonly IApplicationState _state;
        
        public ViewModelFactory(IApplicationConfig config, IPluginManager pluginManager, IApplicationState state)
        {
            _state = state;
            _config = config;
            _pluginManager = pluginManager;
        }

        public MainWindowViewModel CreateMainWindowViewModel()
        {
            return new MainWindowViewModel(_state);
        }

        public ViewPortViewModel CreateViewPortViewModel()
        {
            return new ViewPortViewModel(_state, _config);
        }

        public MenuViewModel CreateMenuViewModel()
        {
            return new MenuViewModel(_state, _pluginManager);
        }

        public MediaViewModel CreateMediaViewModel()
        {
            return new MediaViewModel(_state);
        }
    }
}