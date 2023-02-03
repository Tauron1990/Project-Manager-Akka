﻿using System.Windows.Media;
using Tauron.Application.Localizer.UIModels;

namespace Tauron.Application.Localizer.Views
{
    /// <summary>
    ///     Interaktionslogik für CenterView.xaml
    /// </summary>
    public partial class CenterView
    {
        public CenterView(IViewModel<CenterViewModel> model)
            : base(model)
        {
            InitializeComponent();
            Background = Brushes.Transparent;
        }
    }
}