﻿using System;
using Gu.Wpf.NumericInput;
using Microsoft.Extensions.Logging;
using Stylet;

namespace MultiRobotSimulator.WPF.Pages
{
    public class NewFileDialogViewModel : Screen
    {
        private readonly ILogger<NewFileDialogViewModel> _logger;
        private int _height;
        private bool _heightValid;
        private int _width;
        private bool _widthValid;

        public NewFileDialogViewModel(ILogger<NewFileDialogViewModel> logger)
        {
            _logger = logger;
        }

        public bool CanClickOK => HeightValid && WidthValid;

        public int Height
        {
            get { return _height; }
            set { SetAndNotify(ref _height, value); }
        }

        public bool HeightValid
        {
            get { return _heightValid; }
            set
            {
                SetAndNotify(ref _heightValid, value);
                NotifyOfPropertyChange(() => CanClickOK);
            }
        }

        public int Width
        {
            get { return _width; }
            set { SetAndNotify(ref _width, value); }
        }

        public bool WidthValid
        {
            get { return _widthValid; }
            set
            {
                SetAndNotify(ref _widthValid, value);
                NotifyOfPropertyChange(() => CanClickOK);
            }
        }

        public void ClickCancel()
        {
            RequestClose(false);
        }

        public void ClickOK()
        {
            if (Height <= 0 || Width <= 0)
            {
                return;
            }
            RequestClose(true);
        }

        public void ValidationError(object sender, EventArgs e)
        {
            if (sender is IntBox intBox)
            {
                if (intBox.Name.Equals("IntBoxHeight", StringComparison.Ordinal))
                {
                    HeightValid = false;
                }

                if (intBox.Name.Equals("IntBoxWidth", StringComparison.Ordinal))
                {
                    WidthValid = false;
                }
            }
        }

        public void ValueChanged(object sender, ValueChangedEventArgs<int?> e)
        {
            if (sender is IntBox intBox)
            {
                if (intBox.Name.Equals("IntBoxHeight", StringComparison.Ordinal))
                {
                    HeightValid = e.NewValue > 0 && e.NewValue <= 1000;
                }

                if (intBox.Name.Equals("IntBoxWidth", StringComparison.Ordinal))
                {
                    WidthValid = e.NewValue > 0 && e.NewValue <= 1000;
                }
            }
        }

        #region Logging overrides

        protected override bool SetAndNotify<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            var result = base.SetAndNotify(ref field, value, propertyName);

            if (result)
            {
                _logger?.LogTrace("Property '{propertyName}' new value: '{value}'", propertyName, value);
            }

            return result;
        }

        #endregion Logging overrides
    }
}
