using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCManagement.Classes {
    public class DynamicEnum : Dictionary<uint, string>, IComparable, ISpanFormattable, IConvertible {
        private uint _enumValue;
        public uint Value => _enumValue;
        public DynamicEnum() { }
        public void AddRange(IEnumerable<KeyValuePair<uint, string>> enumerable) {
            enumerable = enumerable.OrderBy(x => x.Key);
            foreach (KeyValuePair<uint, string> kvp in enumerable) {
                if (kvp.Value is null)
                    continue;
                if (ContainsKey(kvp.Key) || ContainsValue(kvp.Value))
                    continue;
                Add(kvp.Key, kvp.Value);
            }
        }
        public void AddRange(IEnumerable<string> values) {
            foreach (string value in values) {
                if (ContainsValue(value))
                    continue;
                _ = Add(value);
            }
        }
        public new void Add(uint key, string value) {
            Add(key, value, false);
        }
        public void Add(uint key, string value, bool compositeOverride) {
            if (ContainsKey(key))
                throw new ArgumentException("Duplicate values are not permitted");
            if (ContainsValue(value))
                throw new ArgumentException("Duplicate names are not permitted");
            bool isPowOf2 = (key > 0) && ((key & (key - 1)) == 0);
            if (!isPowOf2 && !compositeOverride)
                throw new ArgumentException("New values must be powers of 2 unless compositeOverride is true");
            base.Add(key, value);
        }
        public uint Add(string value) {
            if (ContainsValue(value))
                throw new ArgumentException("Duplicate names are not permitted");
            uint highestVal = Keys.Max();
            uint maxFlag = 1u << (31 - (int)System.Numerics.BitOperations.LeadingZeroCount(highestVal));
            uint newVal;
            if (maxFlag <= uint.MaxValue >> 1)
                newVal = maxFlag << 1;
            else
                throw new InvalidOperationException("Maximum flag value reached");
            Add(newVal, value);
            return newVal;
        }
        public uint? GetValue(string name) {
            if (ContainsValue(name))
                return this.FirstOrDefault(kvp => kvp.Value == name).Key;
            return null;
        }
        public string? GetName(uint value) {
            if (TryGetValue(value, out string? name))
                return name;
            return null;
        }
        public int CompareTo(object? target) {
            if (target is null)
                return 1;
            ulong targetVal;
            if (target.GetType().IsEnum) {
                Type underlyingType = Enum.GetUnderlyingType(target.GetType());
                if (underlyingType == typeof(byte) ||
                        underlyingType == typeof(sbyte) ||
                        underlyingType == typeof(short) ||
                        underlyingType == typeof(ushort) ||
                        underlyingType == typeof(int) ||
                        underlyingType == typeof(uint) ||
                        underlyingType == typeof(long) ||
                        underlyingType == typeof(ulong)) {
                    targetVal = (ulong)target;
                } else {
                    throw new InvalidOperationException("target is not type SByte, Int16, Int32, Int64, Byte, UInt16, UInt32, or UInt64.");
                }
            } else if (target.GetType() == typeof(DynamicEnum))
                targetVal = ((DynamicEnum)target).Value;
            else
                throw new ArgumentException("target and this instance are not the same type");
            if (_enumValue < targetVal)
                return -1;
            else if (_enumValue > targetVal)
                return 1;
            else if (_enumValue == targetVal)
                return 0;
            else
                throw new System.Diagnostics.UnreachableException();
        }
        public TypeCode GetTypeCode() {
            return _enumValue.GetTypeCode();
        }
        public bool ToBoolean(IFormatProvider? provider) {
            return ((IConvertible)_enumValue).ToBoolean(provider);
        }
        public byte ToByte(IFormatProvider? provider) {
            return ((IConvertible)_enumValue).ToByte(provider);
        }
        public char ToChar(IFormatProvider? provider) {
            return ((IConvertible)_enumValue).ToChar(provider);
        }
        public DateTime ToDateTime(IFormatProvider? provider) {
            return ((IConvertible)_enumValue).ToDateTime(provider);
        }
        public decimal ToDecimal(IFormatProvider? provider) {
            return ((IConvertible)_enumValue).ToDecimal(provider);
        }
        public double ToDouble(IFormatProvider? provider) {
            return ((IConvertible)_enumValue).ToDouble(provider);
        }
        public short ToInt16(IFormatProvider? provider) {
            return ((IConvertible)_enumValue).ToInt16(provider);
        }
        public int ToInt32(IFormatProvider? provider) {
            return ((IConvertible)_enumValue).ToInt32(provider);
        }
        public long ToInt64(IFormatProvider? provider) {
            return ((IConvertible)_enumValue).ToInt64(provider);
        }
        public sbyte ToSByte(IFormatProvider? provider) {
            return ((IConvertible)_enumValue).ToSByte(provider);
        }
        public float ToSingle(IFormatProvider? provider) {
            return ((IConvertible)_enumValue).ToSingle(provider);
        }
        public override string ToString() {
            if (TryGetValue(_enumValue, out string? singleVal))
                return singleVal;
            List<uint> values = [.. Keys.Order()];
            List<string> output = [];
            foreach (uint value in values) {
                if ((value & _enumValue) == value)
                    output.Add(this[value]);
            }
            return string.Join(", ", output);
        }
        public string ToString(string? format) {
            return format switch {
                null or "G" or "g" or "F" or "f" => ToString(),
                "D" or "d" => _enumValue.ToString(),
                "X" or "x" => Convert.ToString(_enumValue, 16),
                _ => throw new FormatException("format contains an invalid specification"),
            };
        }
        public string ToString(string? format, IFormatProvider? provider) {
            return ToString(format);
        }
        public string ToString(IFormatProvider? provider) {
            return _enumValue.ToString(provider);
        }
        public object ToType(Type conversionType, IFormatProvider? provider) {
            return ((IConvertible)_enumValue).ToType(conversionType, provider);
        }
        public ushort ToUInt16(IFormatProvider? provider) {
            return ((IConvertible)_enumValue).ToUInt16(provider);
        }
        public uint ToUInt32(IFormatProvider? provider) {
            return ((IConvertible)_enumValue).ToUInt32(provider);
        }
        public ulong ToUInt64(IFormatProvider? provider) {
            return ((IConvertible)_enumValue).ToUInt64(provider);
        }
        public bool TryFormat (Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default) {
            string outputString = ToString(format.ToString());
            charsWritten = 0;
            if (outputString.Length > destination.Length)
                return false;
            charsWritten = outputString.Length;
            for (int i = 0; i < outputString.Length; i++) {
                destination[i] = outputString[i];
            }
            return true;
        }
        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default) {
            return TryFormat(destination, out charsWritten, format);
        }
    }
}
