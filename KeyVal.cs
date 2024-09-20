using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorShared;
public class KeyVal<K,V> {
    public K Key;
    public V Value;

    public KeyVal() {}

    public KeyVal(K key, V value) {
        Key = key;
        Value = value;
    }
}