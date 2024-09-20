using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorShared;

public class LabeledItem<T> {
    public string Key { get; set; }
    public T Value { get; set; }
    public LabeledItem (string key, T value) {
        Key = key;
        Value = value;
    }
}
