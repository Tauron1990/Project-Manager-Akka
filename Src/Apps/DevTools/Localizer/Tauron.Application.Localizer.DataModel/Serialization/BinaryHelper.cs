﻿using System;
using System.Collections.Immutable;
using System.IO;

namespace Tauron.Application.Localizer.DataModel.Serialization
{
    public static class BinaryHelper
    {
        public static ImmutableDictionary<TKey, TValue> Read<TKey, TValue>(
            BinaryReader reader,
            Func<BinaryReader, TKey> keyConversion, Func<BinaryReader, TValue> valueConversion)
            where TKey : notnull
        {
            int count = reader.ReadInt32();
            ImmutableDictionary<TKey, TValue>.Builder builder = ImmutableDictionary.CreateBuilder<TKey, TValue>();

            for (var i = 0; i < count; i++)
                builder.Add(keyConversion(reader), valueConversion(reader));

            return builder.ToImmutable();
        }

        public static void WriteDic(ImmutableDictionary<string, string> dic, BinaryWriter writer)
        {
            writer.Write(dic.Count);
            foreach ((string key, string value) in dic)
            {
                writer.Write(key);
                writer.Write(value);
            }
        }

        public static void WriteDic<TKey>(ImmutableDictionary<TKey, string> dic, BinaryWriter writer)
            where TKey : IWriteable
        {
            writer.Write(dic.Count);
            foreach ((TKey key, string value) in dic)
            {
                key.Write(writer);
                writer.Write(value);
            }
        }

        public static ImmutableList<TType> Read<TType>(BinaryReader reader, Func<BinaryReader, TType> converter)
        {
            ImmutableList<TType>.Builder builder = ImmutableList.CreateBuilder<TType>();
            int count = reader.ReadInt32();

            for (var i = 0; i < count; i++) builder.Add(converter(reader));

            return builder.ToImmutable();
        }

        public static ImmutableList<string> ReadString(BinaryReader reader) 
            => Read(reader, binaryReader => binaryReader.ReadString());

        public static void WriteList<TType>(ImmutableList<TType> list, BinaryWriter writer)
            where TType : IWriteable
        {
            writer.Write(list.Count);
            foreach (TType writeable in list)
                writeable.Write(writer);
        }

        public static void WriteList(ImmutableList<string> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (string writeable in list)
                writer.Write(writeable);
        }
    }
}