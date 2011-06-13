﻿/*
Copyright (c) 2011, Sean Kasun
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Terrafirma
{
    /// <summary>
    /// Interaction logic for SaveOptions.xaml
    /// </summary>
    public partial class SaveOptions : Window
    {
        public SaveOptions()
        {
            InitializeComponent();
        }

        public bool EntireMap
        {
            get;
            set;
        }
        public bool CanUseTexture
        {
            set
            {
                useTextures.IsEnabled=value;
            }
        }
        public bool UseTextures
        {
            get;
            set;
        }
        public bool UseZoom
        {
            get;
            set;
        }

        private void saveAll_Checked(object sender, RoutedEventArgs e)
        {
            if (currentOptions != null)
                currentOptions.IsEnabled = false;
            EntireMap = true;
        }

        private void saveCurrent_Checked(object sender, RoutedEventArgs e)
        {
            currentOptions.IsEnabled = true;
            EntireMap = false;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private void useTextures_Checked(object sender, RoutedEventArgs e)
        {
            UseTextures = useTextures.IsChecked == true;
        }

        private void useZoom_Checked(object sender, RoutedEventArgs e)
        {
            UseZoom = useZoom.IsChecked == true;
        }
    }
}
