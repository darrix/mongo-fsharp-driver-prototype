(* Copyright (c) 2013 MongoDB, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *)

namespace FSharp.MongoDB.Bson.Serialization.Serializers

open System.Collections.Generic

open MongoDB.Bson.Serialization
open MongoDB.Bson.Serialization.Serializers

/// <summary>
/// A serializer for F# maps.
/// </summary>
type FSharpMapSerializer<'KeyType, 'ValueType when 'KeyType : comparison>() =
    inherit SerializerBase<Map<'KeyType, 'ValueType>>()

    let serializer = DictionaryInterfaceImplementerSerializer<Dictionary<'KeyType, 'ValueType>>()

    override __.Serialize(context, args, value) =
        let dictValue = Dictionary()
        value |> Map.iter (fun k v -> dictValue.Add(k, v))

        serializer.Serialize(context, args, dictValue)

    override __.Deserialize(context, args) =
        serializer.Deserialize(context, args)
        |> Seq.map (|KeyValue|)
        |> Map.ofSeq
