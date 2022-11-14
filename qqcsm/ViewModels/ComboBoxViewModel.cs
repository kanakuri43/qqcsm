using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qqcs.ViewModels
{
    public sealed class ComboBoxViewModel
    {
        public ComboBoxViewModel(int value, String displayValue)
        {
            Value = value;
            DisplayValue = displayValue;
        }

        public int Value { get; }
        public string DisplayValue { get; }
    }
}
